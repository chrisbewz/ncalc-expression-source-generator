namespace NCalcExpressionGenerator.Models;

/// <summary>
/// Record class to store basic methods parameters information for generators
/// </summary>
/// <param name="Name">Name of method argument.</param>
/// <param name="Type">Fully qualified type name of argument.</param>
public record ArgumentInfo(
    string Name,
    string Type);