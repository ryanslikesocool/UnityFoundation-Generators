using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators {
	[Generator]
	internal sealed class GetComponentGenerator : ISourceGenerator {
		public const string ATTRIBUTE_NAME = "GetComponentAttribute";
		public const string REQUIRED_FUNCTION_NAME = "InitializeComponents";

		private const string FILE_TEXT = @"
using System;

/// <summary>
/// Automatically assign components on a <c>MonoBehaviour</c>.
/// <remarks>
/// You must call <c>InitializeComponents()</c> on the <c>MonoBehaviour</c> for components to be assigned.
/// </remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class GetComponentAttribute : Attribute {
	public enum TargetType {
		This = 0,
		Parent = 1,
		Child = 2
	}

	/// <param name=""targetType"">The object the component is attached to, relative to <c>this.gameObject</c>.</param>
	public GetComponentAttribute(TargetType targetType = TargetType.This) { }
}
		";

		public void Initialize(GeneratorInitializationContext context) {
			context.RegisterForPostInitialization(i => i.AddSource($"{ATTRIBUTE_NAME}_gen.cs", FILE_TEXT));
			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context) {
			if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver)) {
				return;
			}

			INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName(ATTRIBUTE_NAME);

			foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> group in receiver.Fields.GroupBy<IFieldSymbol, INamedTypeSymbol>(f => f.ContainingType, SymbolEqualityComparer.Default)) {
				string classSource = ProcessClass(group.Key, group, attributeSymbol);
				context.AddSource($"{group.Key.Name}_{ATTRIBUTE_NAME}_gen.cs", SourceText.From(classSource, Encoding.UTF8));
			}
		}

		private string ProcessClass(INamedTypeSymbol typeSymbol, IEnumerable<IFieldSymbol> fields, ISymbol attributeSymbol)
			=> SourceBuilder.Run(instance => {
				instance.ExtendType(typeSymbol, _ => {
					instance.source.AppendLine($@"
/// <summary>
/// Retrieve necessary components.
/// </summary>
private void {REQUIRED_FUNCTION_NAME}() {{
");

					foreach (IFieldSymbol fieldSymbol in fields) {
						ProcessField(instance.source, fieldSymbol, attributeSymbol);
					}

					instance.source.Append("}");
				});
			});

		private void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol, ISymbol attributeSymbol) {
			string fieldName = fieldSymbol.Name;
			ITypeSymbol fieldType = fieldSymbol.Type;

			AttributeData attributeData = fieldSymbol.GetAttributes().Single(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

			if (fieldType is IArrayTypeSymbol arrayType) {
				string methodType = ProcessAttribute(attributeData, true);
				source.AppendLine($"{fieldName} = {methodType}<{arrayType.ElementType}>();");
			} else {
				string methodType = ProcessAttribute(attributeData, false);
				source.AppendLine($"{fieldName} = {methodType}<{fieldType}>();");
			}
		}

		private string ProcessAttribute(AttributeData attributeData, bool isArray) {
			StringBuilder stringBuilder = new StringBuilder("GetComponent");
			if (isArray) {
				stringBuilder.Append("s");
			}
			if (
				attributeData.ConstructorArguments.Length > 0
				&& int.TryParse(attributeData.ConstructorArguments[0].Value.ToString(), out int enumValue)
			) {
				switch (enumValue) {
					case 1:
						stringBuilder.Append("InParent");
						break;
					case 2:
						stringBuilder.Append("InChildren");
						break;
				}
			}

			return stringBuilder.ToString();
		}

		private sealed class SyntaxReceiver : ISyntaxContextReceiver {
			public List<IFieldSymbol> Fields { get; } = new List<IFieldSymbol>();

			public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
				if (
					context.Node is FieldDeclarationSyntax fieldDeclarationSyntax
					&& fieldDeclarationSyntax.AttributeLists.Count > 0
				) {
					foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables) {
						IFieldSymbol fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;

						if (
							// TODO: figure out a way to check if base type is an interface
							//(fieldSymbol?.ContainingType.BaseType.IsDerivedFrom("Component") ?? false)
							//&&
							fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == ATTRIBUTE_NAME)
						) {
							//if (fieldSymbol?.Type.BaseType.IsDerivedFrom("Component") ?? false) {
							//	Fields.Add(fieldSymbol);
							//} else if (
							//	 fieldSymbol?.Type.TypeKind == TypeKind.Array
							// //fieldSymbol is IArrayTypeSymbol arraySymbol
							// //&& arraySymbol.ElementType.BaseType.IsDerivedFrom("Component")
							// ) {
							//	// TODO: validate array element type
							//	Fields.Add(fieldSymbol);
							//}
							Fields.Add(fieldSymbol);
						}
					}
				}
			}
		}
	}
}