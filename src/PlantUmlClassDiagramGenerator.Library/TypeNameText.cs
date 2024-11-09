﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace PlantUmlClassDiagramGenerator.Library;

public class TypeNameText
{
    private static readonly SymbolDisplayFormat symbolDisplayFormat;
    private static readonly SymbolDisplayFormat symbolDisplayFormatNameOnly;
    private static readonly SymbolDisplayFormat symbolDisplayFormatNestedNameOnly;

    static TypeNameText()
    {
        symbolDisplayFormat = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.None,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        symbolDisplayFormatNameOnly = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
            genericsOptions: SymbolDisplayGenericsOptions.None,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        symbolDisplayFormatNestedNameOnly = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
            genericsOptions: SymbolDisplayGenericsOptions.None,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
    }

    public string Identifier { get; set; }

    public string TypeArguments { get; set; }
    
    public static TypeNameText From(SimpleNameSyntax syntax, SemanticModel semanticModel)
    {
        if (semanticModel == null)
        {
            var identifier = syntax.Identifier.Text;
            var typeArgs = string.Empty;
            if (syntax is GenericNameSyntax genericName && genericName.TypeArgumentList != null)
            {
                var count = genericName.TypeArgumentList.Arguments.Count;
                identifier = $"\"{identifier}`{count}\"";
                typeArgs = "<" + string.Join(",", genericName.TypeArgumentList.Arguments) + ">";
            }
            else if (identifier.StartsWith("@"))
            {
                identifier = $"\"{identifier}\"";
            }
            return new TypeNameText
            {
                Identifier = identifier,
                TypeArguments = typeArgs
            };
        }
        else
        {
            var identifier = GetText(syntax, semanticModel) ?? syntax.Identifier.Text;
            var typeArgs = string.Empty;
            if (syntax is GenericNameSyntax genericName && genericName.TypeArgumentList != null)
            {
                var count = genericName.TypeArgumentList.Arguments.Count;
                identifier = $"\"{identifier}`{count}\"";
                typeArgs = "<" + string.Join(",", genericName.TypeArgumentList.Arguments) + ">";
            }
            else if (identifier.StartsWith("@"))
            {
                identifier = $"\"{identifier}\"";
            }

            return new TypeNameText
            {
                Identifier = identifier,
                TypeArguments = typeArgs
            };
        }
    }

    public static TypeNameText From(GenericNameSyntax syntax, SemanticModel semanticModel)
    {
        if (semanticModel == null)
        {
            int paramCount = syntax.TypeArgumentList.Arguments.Count;
            string[] parameters = new string[paramCount];
            if (paramCount > 1)
            {
                for (int i = 0; i < paramCount; i++)
                {
                    parameters[i] = $"T{i + 1}";
                }
            }
            else
            {
                parameters[0] = "T";
            }
            return new TypeNameText
            {
                Identifier = $"\"{syntax.Identifier.Text}`{paramCount}\"",
                TypeArguments = "<" + string.Join(",", parameters) + ">",
            };
        }
        else
        {
            var identifier = GetText(syntax, semanticModel) ?? syntax.Identifier.Text;

            int paramCount = syntax.TypeArgumentList.Arguments.Count;
            string[] parameters = new string[paramCount];
            if (paramCount > 1)
            {
                for (int i = 0; i < paramCount; i++)
                {
                    parameters[i] = $"T{i + 1}";
                }
            }
            else
            {
                parameters[0] = "T";
            }

            return new TypeNameText
            {
                Identifier = $"\"{identifier}`{paramCount}\"",
                TypeArguments = "<" + string.Join(",", parameters) + ">",
            };
        }

    }

    public static TypeNameText From(BaseTypeDeclarationSyntax syntax, SemanticModel semanticModel)
    {
        if (semanticModel == null)
        {
            var identifier = syntax.Identifier.Text;
            var typeArgs = string.Empty;
            if (syntax is TypeDeclarationSyntax typeDeclaration && typeDeclaration.TypeParameterList != null)
            {
                var count = typeDeclaration.TypeParameterList.Parameters.Count;
                identifier = $"\"{identifier}`{count}\"";
                typeArgs = "<" + string.Join(",", typeDeclaration.TypeParameterList.Parameters) + ">";
            }
            else if (identifier.StartsWith("@"))
            {
                identifier = $"\"{identifier}\"";
            }
            return new TypeNameText
            {
                Identifier = identifier,
                TypeArguments = typeArgs
            };
        }
        else
        {
            var identifier = GetText(syntax, semanticModel) ?? syntax.Identifier.Text;
            var typeArgs = string.Empty;
            if (syntax is TypeDeclarationSyntax typeDeclaration && typeDeclaration.TypeParameterList != null)
            {
                var count = typeDeclaration.TypeParameterList.Parameters.Count;
                identifier = $"\"{identifier}`{count}\"";
                typeArgs = "<" + string.Join(",", typeDeclaration.TypeParameterList.Parameters) + ">";
            }
            else if (identifier.StartsWith("@"))
            {
                identifier = $"\"{identifier}\"";
            }

            return new TypeNameText
            {
                Identifier = identifier,
                TypeArguments = typeArgs
            };
        }
    }

    public static string GetText(SyntaxNode syntax, SemanticModel semanticModel)
    {
        if ( semanticModel == null )
        {
            return null;
        }

        var symbol = syntax is SimpleNameSyntax ? semanticModel.GetSymbolInfo(syntax).Symbol 
            : syntax is BaseTypeDeclarationSyntax ? semanticModel.GetDeclaredSymbol(syntax) 
            : null;

        if (symbol == null)
        {
            return null;
        }

        string name = symbol.ToDisplayString(symbolDisplayFormatNameOnly);
        string nestedName = symbol.ToDisplayString(symbolDisplayFormatNestedNameOnly);
        string fullName = symbol.ToDisplayString(symbolDisplayFormat);

        if(name.Equals(nestedName, StringComparison.Ordinal))
        {
            return fullName;
        }
        else
        {
            return fullName.Substring(0, fullName.Length - nestedName.Length) + name;
        }
    }
}