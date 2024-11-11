using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlantUmlClassDiagramGenerator.Library;

public class TypeNameText
{
    public string Identifier { get; set; }

    public string TypeArguments { get; set; }
    
    public static TypeNameText From(SimpleNameSyntax syntax, SemanticModel semanticModel)
    {
        var identifier = TypeNameTextSemanticModel.GetSafeNameAndNameSpaceText(semanticModel, syntax);
        identifier ??= TypeNameTextQualified.GetQualifiedTypeName(syntax, syntax.Identifier.Text);

        var typeArgs = string.Empty;
        if (syntax is GenericNameSyntax genericName && genericName.TypeArgumentList != null)
        {
            var count = genericName.TypeArgumentList.Arguments.Count;
            identifier = $"\"{identifier}`{count}\"";
            typeArgs = "<" + string.Join(",", genericName.TypeArgumentList.Arguments) + ">";
        }
        else if (identifier.Contains("@"))
        {
            identifier = $"\"{identifier}\"";
        }

        return new TypeNameText
        {
            Identifier = identifier,
            TypeArguments = typeArgs
        };
    }

    public static TypeNameText From(GenericNameSyntax syntax, SemanticModel semanticModel)
    { 
        var identifier = TypeNameTextSemanticModel.GetSafeNameAndNameSpaceText(semanticModel, syntax);
        identifier ??= TypeNameTextQualified.GetQualifiedTypeName(syntax, syntax.Identifier.Text);

        int paramCount = syntax.TypeArgumentList.Arguments.Count;
        var typeArguments = TypeNameTextSemanticModel.GetSafeTypeParametersText(syntax, semanticModel);
        if(typeArguments is null)
        {
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


                typeArguments = "<" + string.Join(",", parameters) + ">";
            }
        }

        return new TypeNameText
        {
            Identifier = $"\"{identifier}`{paramCount}\"",
            TypeArguments = typeArguments,
        };
    }

    public static TypeNameText From(BaseTypeDeclarationSyntax syntax, SemanticModel semanticModel)
    {
        var identifier = TypeNameTextSemanticModel.GetSafeNameAndNameSpaceText(semanticModel, syntax);
        identifier ??= TypeNameTextQualified.GetQualifiedTypeName(syntax, syntax.Identifier.Text);

        var typeArgs = string.Empty;
        if (syntax is TypeDeclarationSyntax typeDeclaration && typeDeclaration.TypeParameterList != null)
        {
            var count = typeDeclaration.TypeParameterList.Parameters.Count;
            identifier = $"\"{identifier}`{count}\"";
            typeArgs = "<" + string.Join(",", typeDeclaration.TypeParameterList.Parameters) + ">";
        }
        else if (identifier.Contains("@"))
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
