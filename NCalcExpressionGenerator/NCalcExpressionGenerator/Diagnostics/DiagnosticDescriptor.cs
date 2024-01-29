using System.ComponentModel;
using Microsoft.CodeAnalysis;
using NCalcExpressionGenerator.Syntax.Receivers;

namespace NCalcExpressionGenerator.Diagnostics;

/// <summary>
/// A container for all <see cref="DiagnosticDescriptor"/> instances for errors reported by analyzers in this project.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string MethodExpressionSourceGeneratorDiagnosticsIdentifier = "MEGEN";
    
    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a duplicate declaration of <see cref="INotifyPropertyChanged"/> would happen.
    /// <para>
    /// Format: <c>"Cannot apply [INotifyPropertyChangedAttribute] to type {0}, as it already declares the INotifyPropertyChanged interface"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor EmptyValidMemberDeclarationsDescriptor = new DiagnosticDescriptor(
        id: $"{MethodExpressionSourceGeneratorDiagnosticsIdentifier}0001",
        title: $"None valid method declarations found",
        messageFormat: $"The generator {nameof(MethodExpressionSourceGenerator)} cannot find any annotated method declation marked as valid from {nameof(MethodExpressionSyntaxReceiver)}.",
        category: typeof(MethodExpressionSourceGenerator).Name,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: $"The generator {nameof(MethodExpressionSourceGenerator)} cannot find any annotated method declation marked as valid from {nameof(MethodExpressionSyntaxReceiver)}.");

    
}