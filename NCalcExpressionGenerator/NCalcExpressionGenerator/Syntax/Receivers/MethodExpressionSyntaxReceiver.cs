using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        var methodSyntax = (MethodDeclarationSyntax)syntaxNode;

        // Check for methods with the specified attribute
        if (!(methodSyntax.AttributeLists.SelectMany(attrList => attrList.Attributes)
                .Any(attribute => attribute.Name.ToString() == "MethodExpression")))
                return;
        
        // Since the generated code implementation will basically be a extension of annotated method
        // The syntax receiver is only interested in methods declared as partial methods
        // Also considering that methods must be created as public in order to be a valid declaration for code generation
        if(!(methodSyntax.Modifiers.AsEnumerable().Any(modf => modf.IsKind(SyntaxKind.PartialKeyword)
         || methodSyntax.Modifiers.AsEnumerable().Any(modf => modf.IsKind(SyntaxKind.PublicKeyword)))))
            return;

        
        // As last check on method syntax, since the generator purpose is to parse methods
        // That represents math expressions, a value type need to be defined as standard 
        // in order to generate code, so only methods with return type defined as double will be considered as valid
        // This way since NCalc expression returns a nullable object as result, it can be casted to double 
        // using a single call to the class method Convert.ToDouble()
        // NOTE: This return choice is only to not make necessary write logic to check the annotated method return type in order to find the write way to cast and return the evaluated expression
        // as well makes simple the choice of expression statement to build the generated method syntax tree
        PredefinedTypeSyntax? predefinedType = (PredefinedTypeSyntax)methodSyntax.ReturnType;

        if(!(predefinedType is not null && predefinedType.Keyword.IsKind(SyntaxKind.DoubleKeyword)))
            return;

        Methods.Add(methodSyntax);
        
    }
}