using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace PlantUmlClassDiagramGenerator.Library;

public class TypeNameTextSemanticModel
{
    private static readonly SymbolDisplayFormat symbolDisplayFormat;
    private static readonly SymbolDisplayFormat symbolDisplayFormatNameOnly;
    private static readonly SymbolDisplayFormat symbolDisplayFormatNestedNameOnly;

    static TypeNameTextSemanticModel()
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

    public static string GetSafeNameAndNameSpaceText(SemanticModel semanticModel, SyntaxNode syntax)
    {
        if (semanticModel == null)
        {
            return null;
        }

        var symbol = semanticModel.GetSymbolInfo(syntax).Symbol ?? semanticModel.GetDeclaredSymbol(syntax);

        if (symbol == null)
        {
            return null;
        }

        string name = symbol.ToDisplayString(symbolDisplayFormatNameOnly);
        string nestedName = symbol.ToDisplayString(symbolDisplayFormatNestedNameOnly);
        string fullName = symbol.ToDisplayString(symbolDisplayFormat);

        if (name.Equals(nestedName, StringComparison.Ordinal))
        {
            return fullName;
        }
        else
        {
            string[] parts = nestedName.Split('.');
            string safeName = $"{fullName.Substring(0, fullName.Length - nestedName.Length - 1)}";
            for (int i = 0; i < parts.Length - 1; i++)
            {
                string part = parts[i];
                if (part == "") continue;
                else safeName += $".@{part}";
            }
            safeName += $".{parts.Last()}";

            return safeName;
        }
    }

    public static string GetSafeTypeParametersText(SyntaxNode syntax, SemanticModel semanticModel)
    {
        if (semanticModel == null)
        {
            return null;
        }

        if ((semanticModel.GetSymbolInfo(syntax).Symbol ?? semanticModel.GetDeclaredSymbol(syntax)) is not INamedTypeSymbol symbol)
        {
            return null;
        }

        var paramaters = symbol.TypeParameters;

        if (paramaters == null || paramaters.Length == 0)
        {
            return string.Empty;
        }

        return $"<{string.Join(",", paramaters.Select(t => t.Name))}>";
    }

    public static string GetTypeDefineText(SemanticModel semanticModel, SyntaxNode syntax)
    {
        var fullyQualifiedName = GetSafeNameAndNameSpaceText(semanticModel, syntax);
        if (fullyQualifiedName == null) return null;

        return GetTypeDefineText(semanticModel, fullyQualifiedName);
    }

    public static string GetTypeDefineText(SemanticModel semanticModel, string fullyQualifiedName)
    {
        if (semanticModel == null)
        {
            return GetTypeDefineText(fullyQualifiedName);
        }

        var csFullyQualifiedName = fullyQualifiedName.Replace("@", "").Trim('"');

        var symbol = semanticModel.Compilation.GetTypeByMetadataName(csFullyQualifiedName);

        if (symbol == null)
        {
            return GetTypeDefineText(fullyQualifiedName);
        }

        string abstractKeyword = (symbol.TypeKind is TypeKind.Class && symbol.IsAbstract) ? "abstract " : "";
        string typeKeyword = symbol.TypeKind.ToString().ToLower();
        string typeParameters = symbol.IsGenericType ? $"<{string.Join(",", symbol.TypeParameters)}>" : "";
        var modifiers = "";
        string record = symbol.IsRecord ? "<<record>>" : "";

        return $"{abstractKeyword}{typeKeyword} {fullyQualifiedName}{typeParameters} {modifiers}{record}";
    }

    private static string GetTypeDefineText(string fullyQualifiedName)
    {
        var parts = fullyQualifiedName.Trim('"').Split('`');
        if (parts.Length == 1)
        {
            return $"class {fullyQualifiedName}";
        }

        var n = int.Parse(parts[1]);

        if (n == 1)
        {
            return $"class {fullyQualifiedName}<T>";
        }
        else
        {
            return $"class {fullyQualifiedName}<{string.Join(",", Enumerable.Range(0, n).Select(n => $"T{n}"))}>";
        }
    }
}

