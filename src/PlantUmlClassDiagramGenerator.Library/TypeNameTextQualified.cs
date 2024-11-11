using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace PlantUmlClassDiagramGenerator.Library;

public class TypeNameTextQualified
{
    public static string GetQualifiedTypeName(SimpleNameSyntax syntax)
    {
        return GetQualifiedTypeName(syntax, syntax.Identifier.Text);
    }

    public static string GetQualifiedTypeName(GenericNameSyntax syntax)
    {
        return GetQualifiedTypeName(syntax, syntax.Identifier.Text);
    }

    public static string GetQualifiedTypeName(BaseTypeDeclarationSyntax syntax)
    {
        return GetQualifiedTypeName(syntax, syntax.Identifier.Text);
    }

    public static string GetQualifiedTypeName(SyntaxNode syntax, string typeName)
    {
        if (syntax.Parent is QualifiedNameSyntax qualifiedName)
        {
            var namespacePart = qualifiedName.Left;
            var identifier = typeName;
            while (true)
            {
                identifier = $"{namespacePart}.{identifier}";
                if (namespacePart is QualifiedNameSyntax namespacePartHasLeft)
                {
                    namespacePart = namespacePartHasLeft.Left;
                }
                else
                {
                    break;
                }
            }

            return identifier;
        }
        else
        {
            return typeName;
        }
    }
}
