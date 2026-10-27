#nullable enable
using System;
namespace Ogs.Telemetry
{

/// <summary>
/// Closed unit set for OGS-TS channels (CONVENTIONS.md C-3: SI base units
/// plus a closed whitelist). Members in the first block are target units —
/// legal for declaring channels and carried on the wire in discovery.
/// Members in the second block are source units only: accepted by
/// <see cref="TelemetryPublisher.BindChannel"/> so senders can declare what
/// they natively measure; the publisher converts. Never declared on a
/// channel — a channel whose unit matches a ratified CHANNELS.md entry must
/// use that entry's unit exactly.
/// </summary>
public enum Unit
{
    // -- target units (declarable, on-wire) --
    Scalar,                 // ""       unitless ratio, 0..1 inputs (C-3)
    Metre,                  // m
    MetrePerSecond,         // m/s
    MetrePerSecondSquared,  // m/s^2
    Radian,                 // rad      (C-4: rotations are radians)
    RadianPerSecond,        // rad/s
    RadianPerSecondSquared, // rad/s^2
    Rpm,                    // rpm
    Newton,                 // N
    NewtonMetre,            // Nm
    Pascal,                 // Pa
    Kilogram,               // kg
    Second,                 // s
    Kelvin,                 // K
    Celsius,                // degC
    Volt,                   // V
    Ampere,                 // A
    MetreCubedPerSecond,    // m3/s

    // -- source units (convert on bind; never declared) --
    Degree,                 // deg
    DegreePerSecond,        // deg/s
    KilometrePerHour,       // km/h
    MilePerHour,            // mph
    Foot,                   // ft
    FootPerSecond,          // ft/s
    FootPerSecondSquared,   // ft/s^2
    Knot,                   // kn
    GForce,                 // g
    Bar,                    // bar
    Psi,                    // psi
    Kilopascal,             // kPa
    Fahrenheit,             // degF
    Percent,                // %
    Gram,                   // g
    RevPerSecond,           // rps
}

public static class UnitInfo
{
    /// <summary>Wire string for discovery descriptors; empty for Scalar.</summary>
    public static string ToWireString(this Unit unit) => unit switch
    {
        Unit.Scalar => "",
        Unit.Metre => "m",
        Unit.MetrePerSecond => "m/s",
        Unit.MetrePerSecondSquared => "m/s^2",
        Unit.Radian => "rad",
        Unit.RadianPerSecond => "rad/s",
        Unit.RadianPerSecondSquared => "rad/s^2",
        Unit.Rpm => "rpm",
        Unit.Newton => "N",
        Unit.NewtonMetre => "Nm",
        Unit.Pascal => "Pa",
        Unit.Kilogram => "kg",
        Unit.Second => "s",
        Unit.Kelvin => "K",
        Unit.Celsius => "degC",
        Unit.Volt => "V",
        Unit.Ampere => "A",
        Unit.MetreCubedPerSecond => "m3/s",
        _ => throw new ArgumentOutOfRangeException(nameof(unit),
            $"{unit} is a source-only unit and cannot appear in discovery"),
    };

    /// <summary>True if the unit is legal as a declared channel unit.</summary>
    public static bool IsTargetUnit(this Unit unit) => unit switch
    {
        Unit.Degree or Unit.DegreePerSecond or Unit.KilometrePerHour
            or Unit.MilePerHour or Unit.Foot or Unit.FootPerSecond
            or Unit.FootPerSecondSquared or Unit.Knot or Unit.GForce
            or Unit.Bar or Unit.Psi or Unit.Kilopascal or Unit.Fahrenheit
            or Unit.Percent or Unit.Gram or Unit.RevPerSecond => false,
        _ => true,
    };
}
}
