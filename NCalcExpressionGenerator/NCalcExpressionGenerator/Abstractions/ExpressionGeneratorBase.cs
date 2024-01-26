using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NCalcExpressionGenerator.Structures.Enumerations;

using SeparatorInfo = (string Left, string Right);

namespace NCalcExpressionGenerator.Abstractions;

/// <summary>
/// Base abstraction for method extending expression source generators
/// </summary>
public abstract partial class ExpressionGeneratorBase : ISourceGenerator
{

    #region Properties

    private protected string ParameterRegexPattern = (@"\<left_placeholder>([^\]]+)\<right_placeholder>");

    /// <summary>
    /// 
    /// </summary>
    private protected SeparatorInfo ExpressionParametersEnclosureInfo;

    /// <summary>
    /// Regular expression pattern used to find parameter declarations on string expressions
    /// </summary>
    private Regex ParameterIdentifierRegx;

    /// <summary>
    /// Fully description name of marker attribute used to find for method declarations
    /// </summary>
    private string FullyQualifiedMarkerAttributeName;
    
    /// <summary>
    /// Identifer used to find parameters inside a string expression
    /// </summary>
    private ParameterSeparatorOptions DefaultParameterEnclosure;

    #endregion

    protected ExpressionGeneratorBase(
        ParameterSeparatorOptions expressionDefaultParameterEnclosure,
        string expressionFullyQualifiedMarkerAttributeName
        )
    {
        FullyQualifiedMarkerAttributeName = expressionFullyQualifiedMarkerAttributeName;
        DefaultParameterEnclosure = expressionDefaultParameterEnclosure;
    }

    public void Initialize(GeneratorInitializationContext context)
    {

    }

    public void Execute(GeneratorExecutionContext context)
    {

    }

    /// <summary>
    /// Finds and returns all valid method declaration based on marker attribute
    /// </summary>
    /// <returns></returns>
    private protected abstract IEnumerable<MethodDeclarationSyntax> FilterMethodDeclarations();


    /// <summary>
    /// Returns an adjusted parameter finding regular expression given the selected parameter separator
    /// </summary>
    /// <returns></returns>
    private Regex AdjustParameterRegularExpression()
    {
        Regex result = default;

        switch(DefaultParameterEnclosure)
        {
            case ParameterSeparatorOptions.SquareBrackets:
                this.ExpressionParametersEnclosureInfo = new SeparatorInfo("[","]");
                break;

            case ParameterSeparatorOptions.CurlyBraces:
                this.ExpressionParametersEnclosureInfo = new SeparatorInfo("{","}");
                break;

            case ParameterSeparatorOptions.DoubleCurlyBraces:
                this.ExpressionParametersEnclosureInfo = new SeparatorInfo("{{","}}");
                break;

            default:
                this.ExpressionParametersEnclosureInfo = new SeparatorInfo("[","]");
                break;
            
        }

        result = new Regex(ParameterRegexPattern
            .Replace("<left_placeholder>",ExpressionParametersEnclosureInfo.Left)
            .Replace("<right_placeholder>",ExpressionParametersEnclosureInfo.Right)
        );

        return result;

    }
}