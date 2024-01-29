namespace NCalcExpressionGenerator.Models;

/// <summary>
/// Record to provide expression generators parameters capturing separators
/// </summary>
/// <param name="Left">Left sided parameter identifier</param>
/// <param name="Right">Right sided parameter identifier</param>
public record SeparatorInfo(
    string Left,
    string Right
);