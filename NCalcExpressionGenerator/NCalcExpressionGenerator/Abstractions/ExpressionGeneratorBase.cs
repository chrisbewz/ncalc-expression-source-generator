using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NCalcExpressionGenerator.Diagnostics;
using NCalcExpressionGenerator.Models;
using NCalcExpressionGenerator.Structures.Enumerations;
using NCalcExpressionGenerator.Syntax.Receivers;

namespace NCalcExpressionGenerator.Abstractions;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TDeclarations"></typeparam>
public abstract partial class ExpressionGeneratorBase<TDeclarations> : IIncrementalGenerator
{
    #region Properties

    /// <summary>
    /// 
    /// </summary>
    private protected ISyntaxReceiver MethodExpressionSyntaxReceiver;

    /// <summary>
    /// Stores the regular expression template for matching enclosed parameters on a string expression.
    /// </summary>
    private protected string ParameterRegexPattern = (@"\<left_placeholder>([^\]]+)\<right_placeholder>");

    /// <summary>
    /// Stores information about expression parameter identifiers
    /// </summary>
    private protected SeparatorInfo ExpressionParametersEnclosureInfo;

    /// <summary>
    /// Regular expression pattern used to find parameter declarations on string expressions
    /// </summary>
    private protected Regex ParameterIdentifierRegx;

    /// <summary>
    /// Fully description name of marker attribute used to find for method declarations
    /// </summary>
    private protected string FullyQualifiedMarkerAttributeName;

    /// <summary>
    /// Namespace of target attribute to look from code
    /// </summary>
    private protected string Namespace;

    /// <summary>
    /// Class name of the attribute generator has to look for without namespace
    /// </summary>
    private protected string AttributeName;
    
    /// <summary>
    /// Identifer used to find parameters inside a string expression
    /// </summary>
    private ParameterSeparatorOptions DefaultParameterEnclosure;

    #endregion

    #region Constructors

    protected ExpressionGeneratorBase(
        string fullyQualifiedAttributeMetadataName,
        ISyntaxReceiver expressionSyntaxReceiver,
        ParameterSeparatorOptions expressionDefaultParameterEnclosure
        )
    {
        this.FullyQualifiedMarkerAttributeName = fullyQualifiedAttributeMetadataName;
        string[] split = fullyQualifiedAttributeMetadataName.Split('.');
        this.AttributeName = split.Last();
        this.Namespace = string.Join(".", split.Take(split.Length - 1).ToArray());
        this.DefaultParameterEnclosure = expressionDefaultParameterEnclosure;
        this.MethodExpressionSyntaxReceiver = expressionSyntaxReceiver;
    }

    #endregion

    #region ISourceGenerator
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        PostInitializationCallBack(context);
        IncrementalValuesProvider<TDeclarations> source = GatherGenerationInfo(context);
        
        context.RegisterSourceOutput(context.CompilationProvider.Combine(source.Collect()), RegisterSource);
    }
        
    #endregion

    #region Methods

    /// <summary>
    /// Fetch all valid member declarations to generate members from
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private protected abstract IncrementalValuesProvider<TDeclarations> GatherGenerationInfo(
        IncrementalGeneratorInitializationContext context);
    
    /// <summary>
    /// Finds and returns all valid method declaration based on marker attribute
    /// </summary>
    /// <returns></returns>
    private protected abstract TDeclarations FilterDeclarations(GeneratorSyntaxContext context);

    /// <summary>
    /// TODO: DOCUMENT
    /// </summary>
    /// <param name="context"></param>
    /// <param name="values"></param>
    private protected abstract void RegisterSource(SourceProductionContext context,
        (Compilation Left, ImmutableArray<TDeclarations> Right) values);
    
    /// <summary>
    /// Method called for generated code syntax tree building and registering as output
    /// </summary>
    /// <param name="context"></param>
    /// <param name="compilation"></param>
    /// <param name="classDeclarations"></param>
    private protected abstract void GenerateSource(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<TDeclarations> methodDeclarations);

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

    /// <summary>
    /// Replaces the parameter raw string removing it's string identifiers as the values present in <see cref="ExpressionParametersEnclosureInfo"/>
    /// </summary>
    /// <param name="parameterContent"></param>
    /// <returns></returns>
    private protected string ReplaceParameterPlaceHolders(string parameterContent)
    => parameterContent.Replace(this.ExpressionParametersEnclosureInfo.Left, string.Empty)
        .Replace(this.ExpressionParametersEnclosureInfo.Right, string.Empty);

    /// <summary>
    /// Returns the value contained on marker MethodExpressionAttribute given a method declaration 
    /// </summary>
    /// <param name="methodSyntax"></param>
    /// <returns></returns>
    private protected string GetMethodExpression(MethodDeclarationSyntax methodSyntax)
    {
        // Retrieve the MethodExpressionAttribute
        // Only the first attribute found is considered since only one expression 
        // is considered valid to generate the annotated method implementation
        AttributeSyntax attribute = methodSyntax.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .FirstOrDefault(attribute => attribute.Name.ToString() == AttributeName.Replace("Attribute",String.Empty));

        // Retrieve the expression string from the attribute found if any
        string? expression = attribute?.ArgumentList?.Arguments.FirstOrDefault()?.ToString();

        // TODO: Emit a error diagnostic and interrupt source generator pipeline execution since none expression was found and consequentily, no implementation will be provided to annotated method if the execution continue
        return expression ?? "/* No valid expression found */";
    }

    /// <summary>
    /// Returns method declaration arguments information for syntax tree parameters validation before code generation
    /// </summary>
    /// <param name="methodSyntax"></param>
    /// <returns></returns>
    private protected IList<ArgumentInfo> GetMethodParameterInfo(MethodDeclarationSyntax methodSyntax)
    {
        // Retrieve parameter information about its name and declared type to perform validation with the attribute expression provided
        IEnumerable<ParameterSyntax> parameters = methodSyntax.ParameterList?.Parameters ?? Enumerable.Empty<ParameterSyntax>();

        return parameters.Select(param => new ArgumentInfo(param.Identifier.Text, param.Type.ToString())).ToList();
    }
    
    /// <summary>
    /// Callback called after generator initialization for registering the attribute source code to find for valid declarations.
    /// </summary>
    /// <param name="context"></param>
    private protected virtual void PostInitializationCallBack(
        IncrementalGeneratorInitializationContext context)
    {
        // Configuring the regex pattern to capture expression parameters on code generation pipeline
        this.ParameterIdentifierRegx = AdjustParameterRegularExpression();
        
        // context.RegisterPostInitializationOutput(
        //     ctx =>
        //         ctx.AddSource(
        //             $"{AttributeName}.g.cs",
        //             SourceText.From(this.AttributeClassDeclaration.OfType<ClassDeclarationSyntax>().First().ToFullString(), Encoding.UTF8)));
    }

    #endregion
}