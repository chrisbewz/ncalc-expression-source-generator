using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCalcExpressionGenerator.Syntax.Receivers;

/// <summary>
/// Syntax receiver class to validate method declarations marked with MethodExpressionAttribute for MethodExpressionSourceGenerator class
/// </summary>
public class MethodExpressionSyntaxReceiver : ISyntaxReceiver
{
    public List<MethodDeclarationSyntax> Methods { get; } = new List<MethodDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        // Check for methods with the specified attribute
        if (syntaxNode is MethodDeclarationSyntax methodSyntax &&
            methodSyntax.AttributeLists.SelectMany(attrList => attrList.Attributes)
                .Any(attribute => attribute.Name.ToString() == "MethodExpression"))
        {
            Methods.Add(methodSyntax);
        }
    }
}