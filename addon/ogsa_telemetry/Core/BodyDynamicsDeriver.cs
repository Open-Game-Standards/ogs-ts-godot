#nullable enable
using System.Numerics;

// BodyDynamicsDeriver — see the class doc below; the C++ twin lives in
// ogsa-ts-unreal's OgsaTelemetryFrame.h (both must stay behaviourally
// identical; the tests here pin the semantics).

namespace Ogsa.Telemetry
{

/// <summary>
/// Derives OGSA-TS body dynamics (specific force + angular velocity) from
/// a sampled world pose, for senders whose game does not expose
/// accelerometer-class data natively. Semantics per the schema's
/// bodyDynamics variant (convention A, accelerometer-style):
///
///   specificForce = a_world − g_world  (f = a − g; +9.80665 on +heave
///                                       at rest — what an IMU reads),
///   expressed in the C-4 body frame by rotating with the inverse of the
///   reported orientation (v_world = q · v_body — Unity/Unreal actor
///   rotations in the standard frame).
///   angularVelocity from quaternion deltas: the body-frame rotation
///   log(q_t⁻¹ ⊗ q_t₊₁)/dt — artifact-free at attitude representation
///   flips, unlike receiver-side Euler differentiation (GameConn
///   docs/cueing.md).
///
/// C-4 component sense (the subtle part): the receiver's two paths must
/// agree — native ω versus d(euler)/dt of GameConn's EulerC4 extraction
/// (pitch = asin(f.y) nose-up+, yaw = atan2(f.x, f.z) nose-right+,
/// roll = atan2(u.x, u.y) right-side-down+). Working the pure-axis cases
/// through that extraction shows: a positive stored-Hamilton rotation
/// about +y is nose-right (matches C-4 +yaw), but about +x it tilts the
/// nose DOWN and about +z it rolls left-side-down — so C-4 pitch/roll
/// rates are the NEGATED stored-math components. ω is therefore mapped
/// component-wise as (−x, +y, −z); this makes a sender-derived ω equal
/// the Euler-differentiated rate the same receiver would otherwise
/// compute (the anchors are pinned by GameConn's NativeRatesTests).
///
/// Velocity may be supplied by the caller when the engine measures it
/// (Unreal GetVelocity, Unity Rigidbody); otherwise it is derived from
/// position deltas. Acceleration is low-pass filtered (τ = 0.1 s) to tame
/// sampling jitter; angular velocity is NOT filtered (onset cues need its
/// edges). Teleports are detected as moves the best available velocity
/// estimate cannot explain (engine-measured velocity when supplied,
/// else the previously derived one) — NOT by a fixed per-sample
/// displacement cap, which conflated "teleport" with "fast mover":
/// flight and space sims routinely exceed 2.5 m per 20 ms sample
/// (125 m/s) in perfectly normal flight, which reset the estimator
/// every tick. Without a velocity measurement a single inconsistent
/// sample is acceleration onset (or the first sample after a reset),
/// so only a sustained inconsistency is treated as a teleport. The
/// first sample after a reset yields the exact static value: gravity
/// reaction rotated into the body frame, zero rates.
/// </summary>
public sealed class BodyDynamicsDeriver
{
    /// <summary>Standard gravity, m/s² (the value the schema's tilt-coordination consumers assume; matches TelemetryPublisher's discovery Environment.Gravity).</summary>
    public const float Gravity = 9.80665f;

    /// <summary>Acceleration low-pass time constant, seconds.</summary>
    public const double AccelFilterTau = 0.1;

    /// <summary>Base teleport-inconsistency tolerance, metres: the part of
    /// a per-sample move the velocity estimate cannot explain before it is
    /// a candidate teleport (a proportional slack on top covers velocity
    /// change mid-sample). See the class doc for the full rule.</summary>
    public const float TeleportThresholdMetres = 2.5f;

    public Vector3 SpecificForce { get; private set; }
    public Vector3 AngularVelocity { get; private set; }

    Vector3 _prevPosition;
    Vector3 _prevVelocity;
    Quaternion _prevOrientation;
    Vector3 _filteredAccel;
    double _prevTime = double.NaN;
    bool _hasHistory;
    bool _pendingTeleport; // no-velocity path: one strike is accel onset, two is a teleport


    /// <summary>Feeds one pose sample (standard frame: metres, unit
    /// quaternion, world space; dt in seconds). Call every host tick, not
    /// just on publish cadence, so the derivative denominators stay small.
    /// hasVelocity/engineVelocity: caller-measured world velocity if the
    /// engine exposes one (m/s); otherwise position deltas are used.</summary>
    public void Sample(double time, Vector3 position, Quaternion orientation,
                       bool hasVelocity = false, Vector3 engineVelocity = default)
    {
        if (!_hasHistory || double.IsNaN(_prevTime))
        {
            Reset(position, orientation, time);
            return;
        }
        double dt = time - _prevTime;
        if (dt <= 0)
        {
            Reset(position, orientation, time);
            return;
        }

        Vector3 displacement = position - _prevPosition;
        Vector3 velocity;
        if (hasVelocity)
        {
            // engine-measured velocity is authoritative: a move it cannot
            // explain (respawn, level change, floating-origin shift) is a
            // teleport, however fast the ship legitimately flies
            velocity = engineVelocity;
            float expected = (float)dt * engineVelocity.Length();
            float inconsistency = (displacement - engineVelocity * (float)dt).Length();
            if (inconsistency > TeleportThresholdMetres + 0.5f * expected)
            {
                Reset(position, orientation, time);
                return;
            }
        }
        else
        {
            velocity = displacement / (float)dt;
            Vector3 expected = _prevVelocity * (float)dt;
            float inconsistency = (displacement - expected).Length();
            if (inconsistency > TeleportThresholdMetres + 0.5f * expected.Length())
            {
                // without a velocity measurement one inconsistent sample is
                // acceleration onset (or the first sample after a reset); a
                // sustained one is a teleport
                if (_pendingTeleport)
                {
                    Reset(position, orientation, time);
                    return;
                }
                _pendingTeleport = true;
            }
            else _pendingTeleport = false;
        }

        Vector3 accel = (velocity - _prevVelocity) / (float)dt;
        // one-pole low-pass: y += (x − y)·dt/τ, stable for any dt
        double blend = System.Math.Min(1.0, dt / AccelFilterTau);
        _filteredAccel += (accel - _filteredAccel) * (float)blend;

        AngularVelocity = AngularVelocityFromDelta(_prevOrientation, orientation, dt);
        SpecificForce = RotateByInverse(orientation,
            _filteredAccel + new Vector3(0f, Gravity, 0f));

        _prevPosition = position;
        _prevVelocity = velocity;
        _prevOrientation = orientation;
        _prevTime = time;
    }

    /// <summary>Static value after any reset: gravity reaction in the body
    /// frame, zero angular velocity.</summary>
    public void Reset(Vector3 position, Quaternion orientation, double time)
    {
        SpecificForce = RotateByInverse(orientation, new Vector3(0f, Gravity, 0f));
        AngularVelocity = default;
        _prevPosition = position;
        _prevVelocity = default;
        _prevOrientation = orientation;
        _filteredAccel = default;
        _prevTime = time;
        _hasHistory = true;
        _pendingTeleport = false;
    }

    /// <summary>Body-frame angular velocity (rad/s, C-4 component sense:
    /// +x nose-up, +y nose-right, +z right-side-down) from the world-frame
    /// orientation delta. Computes the body-frame rotation
    /// dq = q_prev⁻¹ ⊗ q_cur via its log (stored-Hamilton math), then maps
    /// components to C-4 as (−x, +y, −z) — see the class doc for why that
    /// mapping makes native rates equal the receiver's Euler-differentiated
    /// rates.</summary>
    public static Vector3 AngularVelocityFromDelta(Quaternion previous,
                                                   Quaternion current, double dt)
    {
        // dq = q_prev⁻¹ ⊗ q_cur  (body-frame rotation over dt).
        // NOTE: System.Numerics Quaternion.Concatenate(a, b) returns b ⊗ a
        // ("the value1 rotation followed by value2"), so the operand order
        // here is (current, inverse).
        Quaternion dq = Quaternion.Concatenate(current, Quaternion.Conjugate(previous));
        // sign canonicalization: either hemisphere represents the same
        // rotation; take w ≥ 0 so the log stays in [−π, π]
        if (dq.W < 0f) { dq = new Quaternion(-dq.X, -dq.Y, -dq.Z, -dq.W); }
        Vector3 vec = new(dq.X, dq.Y, dq.Z);
        float vecLen = vec.Length();
        if (vecLen < 1e-8f || dt <= 0) return default;
        float angle = 2f * (float)System.Math.Atan2(vecLen, dq.W); // |θ| ∈ [0, π]
        Vector3 stored = vec / vecLen * (float)(angle / dt);
        return StoredToC4Rates(stored);
    }

    /// <summary>Maps a body-frame angular velocity from the engine's
    /// stored-Hamilton component sense to C-4 (−x, +y, −z) — see the class
    /// doc for why that makes native rates equal the receiver's
    /// Euler-differentiated rates. For senders whose engine measures
    /// angular velocity natively (Unity/Unreal rigidbody world rate):
    /// rotate the reading into the body frame with
    /// <see cref="RotateByInverse"/> first, then apply this.</summary>
    public static Vector3 StoredToC4Rates(Vector3 storedBodyAngularVelocity)
        => new(-storedBodyAngularVelocity.X, storedBodyAngularVelocity.Y,
               -storedBodyAngularVelocity.Z);

    /// <summary>Rotates a world-frame vector into the body frame:
    /// v_body = q⁻¹ · v_world for the actor-style orientation q
    /// (v_world = q · v_body — the convention BindPose feeds).</summary>
    public static Vector3 RotateByInverse(Quaternion orientation, Vector3 world)
        => Vector3.Transform(world, Quaternion.Conjugate(orientation));
}
}
