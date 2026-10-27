#nullable enable
using System;
namespace Ogs.Telemetry
{

/// <summary>
/// Unit conversion table for the bind path: senders declare the unit they
/// natively measure (<see cref="Unit"/> source units), the publisher
/// converts to the channel's target unit before it reaches the wire. All
/// factors are exact where physically defined. The table is shared by every
/// C# engine plugin; UE mirrors it in C++ and both are exercised by the
/// standard's conformance vectors so the engines provably agree.
/// </summary>
public static class Conversions
{
    /// <summary>
    /// Returns a precomputed converter source→target, or null when the pair
    /// is not convertible (e.g. km/h → rad). Identity is a valid converter.
    /// </summary>
    public static Func<double, double>? TryGetConverter(Unit source, Unit target)
    {
        if (source == target) return static v => v;
        var factor = TryGetFactor(source, target);
        if (factor.HasValue)
        {
            var f = factor.Value;
            return v => v * f;
        }
        if (source == Unit.Fahrenheit && target == Unit.Celsius)
            return v => (v - 32.0) * (5.0 / 9.0);
        if (source == Unit.Celsius && target == Unit.Fahrenheit)
            return v => v * (9.0 / 5.0) + 32.0;
        return null;
    }

    /// <summary>Scalar factor for uniform conversions, null otherwise.</summary>
    public static double? TryGetFactor(Unit source, Unit target) => (source, target) switch
    {
        // speed
        (Unit.KilometrePerHour, Unit.MetrePerSecond) => 1.0 / 3.6,
        (Unit.MilePerHour, Unit.MetrePerSecond) => 0.44704,
        (Unit.FootPerSecond, Unit.MetrePerSecond) => 0.3048,
        (Unit.Knot, Unit.MetrePerSecond) => 1852.0 / 3600.0,
        // length
        (Unit.Foot, Unit.Metre) => 0.3048,
        // angle
        (Unit.Degree, Unit.Radian) => Math.PI / 180.0,
        (Unit.DegreePerSecond, Unit.RadianPerSecond) => Math.PI / 180.0,
        // rotation rate
        (Unit.RevPerSecond, Unit.Rpm) => 60.0,
        // acceleration
        (Unit.GForce, Unit.MetrePerSecondSquared) => 9.80665,
        (Unit.FootPerSecondSquared, Unit.MetrePerSecondSquared) => 0.3048,
        // pressure
        (Unit.Bar, Unit.Pascal) => 1e5,
        (Unit.Psi, Unit.Pascal) => 6894.757293168361,
        (Unit.Kilopascal, Unit.Pascal) => 1000.0,
        // mass
        (Unit.Gram, Unit.Kilogram) => 1e-3,
        // ratio
        (Unit.Percent, Unit.Scalar) => 1e-2,
        _ => null,
    };
}
}
