using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PlantUmlClassDiagramGenerator.Attributes;

namespace PlantUmlClassDiagramGenerator.Library;

public class RelationshipCollection : IEnumerable<Relationship>
{
    private readonly IList<Relationship> items = new List<Relationship>();

    public void AddInheritanceFrom(TypeDeclarationSyntax syntax, SemanticModel semanticModel)
    {
        if (syntax.BaseList == null) return;

        var subTypeName = TypeNameText.From(syntax, semanticModel);

        foreach (var typeStntax in syntax.BaseList.Types)
        {
            if (typeStntax.Type is not SimpleNameSyntax typeNameSyntax) continue;
            var baseTypeName = TypeNameText.From(typeNameSyntax, semanticModel);
            items.Add(new Relationship(baseTypeName, subTypeName, "<|--", baseTypeName.TypeArguments));
        }
    }

    public void AddInnerclassRelationFrom(SyntaxNode node, SemanticModel semanticModel)
    {
        if (node.Parent is not BaseTypeDeclarationSyntax outerTypeNode 
            || node is not BaseTypeDeclarationSyntax innerTypeNode) return;

        var outerTypeName = TypeNameText.From(outerTypeNode, semanticModel);
        var innerTypeName = TypeNameText.From(innerTypeNode, semanticModel);
        items.Add(new Relationship(outerTypeName, innerTypeName, "+--"));
    }

    public void AddAssociationFrom(FieldDeclarationSyntax node, VariableDeclaratorSyntax field, SemanticModel semanticModel)
    {
        var leafNode = node.Declaration.Type as SimpleNameSyntax ??
            (node.Declaration.Type as QualifiedNameSyntax)?.Right;

        if (leafNode is null || node.Parent is not BaseTypeDeclarationSyntax rootNode) return;

        var symbol = field.Initializer == null ? "-->" : "o->";
        var fieldIdentifier = field.Identifier.ToString();
        var leafName = TypeNameText.From(leafNode, semanticModel);
        var rootName = TypeNameText.From(rootNode, semanticModel);
        AddRelationship(leafName, rootName, symbol, fieldIdentifier);
    }

    public void AddAssociationFrom(PropertyDeclarationSyntax node, TypeSyntax typeIgnoringNullable, SemanticModel semanticModel)
    {
        if (typeIgnoringNullable is not SimpleNameSyntax leafNode 
            || node.Parent is not BaseTypeDeclarationSyntax rootNode) return;

        var symbol = node.Initializer == null ? "-->" : "o->";
        var nodeIdentifier = node.Identifier.ToString();
        var leafName = TypeNameText.From(leafNode, semanticModel);
        var rootName = TypeNameText.From(rootNode, semanticModel);
        AddRelationship(leafName, rootName, symbol, nodeIdentifier);
    }

    public void AddAssociationFrom(ParameterSyntax node, RecordDeclarationSyntax parent, SemanticModel semanticModel)
    {
        if (node.Type is not SimpleNameSyntax leafNode 
            || parent is not BaseTypeDeclarationSyntax rootNode) return;

        var symbol = node.Default == null ? "-->" : "o->";
        var nodeIdentifier = node.Identifier.ToString();
        var leafName = TypeNameText.From(leafNode, semanticModel);
        var rootName = TypeNameText.From(rootNode, semanticModel);
        AddRelationship(leafName, rootName, symbol, nodeIdentifier);
    }

    public void AddAssociationFrom(PropertyDeclarationSyntax node, PlantUmlAssociationAttribute attribute, SemanticModel semanticModel)
    {
        if (node.Parent is not BaseTypeDeclarationSyntax rootNode) return;
        var leafName = GetLeafName(attribute.Name, node.Type, semanticModel);
        if (leafName is null) { return; }
        var rootName = TypeNameText.From(rootNode, semanticModel);
        AddeRationship(attribute, leafName, rootName);

    }

    public void AddAssociationFrom(MethodDeclarationSyntax node, ParameterSyntax parameter, PlantUmlAssociationAttribute attribute, SemanticModel semanticModel)
    {
        if (node.Parent is not BaseTypeDeclarationSyntax rootNode) return;
        var leafName = GetLeafName(attribute.Name, parameter.Type, semanticModel);
        if (leafName is null) { return; }
        var rootName = TypeNameText.From(rootNode, semanticModel);
        AddeRationship(attribute, leafName, rootName);
    }

    public void AddAssociationFrom(RecordDeclarationSyntax node, ParameterSyntax parameter, PlantUmlAssociationAttribute attribute, SemanticModel semanticModel)
    {
        if (node is not BaseTypeDeclarationSyntax rootNode) { return; }
        var leafName = GetLeafName(attribute.Name, parameter.Type, semanticModel);
        if (leafName is null) { return; }
        var rootName = TypeNameText.From(rootNode, semanticModel);
        AddeRationship(attribute, leafName, rootName);
    }

    public void AddAssociationFrom(ConstructorDeclarationSyntax node, ParameterSyntax parameter, PlantUmlAssociationAttribute attribute, SemanticModel semanticModel)
    {
        if (node.Parent is not BaseTypeDeclarationSyntax rootNode) { return; }
        var leafName = GetLeafName(attribute.Name, parameter.Type, semanticModel);
        if (leafName is null) { return; }
        var rootName = TypeNameText.From(rootNode, semanticModel);
        AddeRationship(attribute, leafName, rootName);
    }

    public void AddAssociationFrom(FieldDeclarationSyntax node, PlantUmlAssociationAttribute attribute, SemanticModel semanticModel)
    {
        if (node.Parent is not BaseTypeDeclarationSyntax rootNode) { return; }
        var leafName = GetLeafName(attribute.Name, node.Declaration.Type, semanticModel);
        if(leafName is null) { return; }
        var rootName = TypeNameText.From(rootNode, semanticModel);
        AddeRationship(attribute, leafName, rootName);
    }

    private static TypeNameText GetLeafName(string attributeName, TypeSyntax typeSyntax, SemanticModel semanticModel)
    {
        if (!string.IsNullOrWhiteSpace(attributeName))
        {
            return new TypeNameText() { Identifier = attributeName };
        }
        else if (typeSyntax is SimpleNameSyntax simpleNode)
        {
            return TypeNameText.From(simpleNode, semanticModel);
        }
        return null;
        
    }

    private void AddeRationship(PlantUmlAssociationAttribute attribute, TypeNameText leafName, TypeNameText rootName)
    {
        var symbol = string.IsNullOrEmpty(attribute.Association) ? "--" : attribute.Association;
        items.Add(new Relationship(rootName, leafName, symbol, attribute.RootLabel, attribute.LeafLabel, attribute.Label));
    }

    private void AddRelationship(TypeNameText leafName, TypeNameText rootName, string symbol, string nodeIdentifier)
    {
        items.Add(new Relationship(rootName, leafName, symbol, "", nodeIdentifier + leafName.TypeArguments));
    }

    public IEnumerator<Relationship> GetEnumerator()
    {
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
