using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace FoundationGenerators {
	[Generator]
	public class GetterGenerator : ISourceGenerator {
		private const string ATTRIBUTE_NAME = "GetterAttribute";

		private const string ATTRIBUTE_TEXT = @"
using System;

/// <summary>
/// Generate a property getter with the given access level.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class GetterAttribute : Attribute {
	public enum AccessLevel {
		Private = 0,
		Protected = 1,
		Internal = 2,
		Public = 3
	}

	public GetterAttribute(AccessLevel accessLevel = AccessLevel.Public) { }
}
";

		public void Initialize(GeneratorInitializationContext context) {
			context.RegisterForPostInitialization(i
				=> i.AddSource($"{ATTRIBUTE_NAME}_gen.cs", ATTRIBUTE_TEXT)
			);
			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context) {
			if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver)) {
				return;
			}

			INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName(ATTRIBUTE_NAME);

			foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> group in receiver.Fields.GroupBy<IFieldSymbol, INamedTypeSymbol>(f => f.ContainingType, SymbolEqualityComparer.Default)) {
				var classSource = ProcessClass(group.Key, group, attributeSymbol);
				context.AddSource($"{group.Key.Name}_Getter_gen.cs", SourceText.From(classSource, Encoding.UTF8));
			}
		}

		private string ProcessClass(INamedTypeSymbol classSymbol, IEnumerable<IFieldSymbol> fields, ISymbol attributeSymbol) {
			var source = new StringBuilder();
			bool hasNamespace = classSymbol.ContainingNamespace != null;

			if (hasNamespace) {
				source.AppendLine($"namespace {classSymbol.ContainingNamespace} {{");
			}

			source.AppendLine($@"
	public partial class {classSymbol.Name} {{
");

			foreach (IFieldSymbol fieldSymbol in fields) {
				ProcessField(source, fieldSymbol, attributeSymbol);
			}

			source.Append("\n}");

			if (hasNamespace) {
				source.Append("\n}");
			}

			return source.ToString();
		}

		private void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol, ISymbol attributeSymbol) {
			string fieldName = fieldSymbol.Name;
			ITypeSymbol fieldType = fieldSymbol.Type;

			AttributeData attributeData = fieldSymbol.GetAttributes().Single(ad
				=> ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

			string accessLevel = ProcessAttribute(attributeData);
			string publicFieldName = ProcessFieldName(fieldName);

			source.AppendLine($"{accessLevel} {fieldType} {publicFieldName} => {fieldName};");
		}

		private string ProcessFieldName(in string fieldName) {
			if (fieldName[0] == '_') {
				// _field -> field
				return fieldName.Substring(1);
			} else {
				// field -> Field
				return $"{fieldName[0].ToString().ToUpper()}{fieldName.Substring(1)}";
			}
		}

		private string ProcessAttribute(AttributeData attributeData) {
			if (
				attributeData.ConstructorArguments.Length > 0
				&& int.TryParse(attributeData.ConstructorArguments[0].Value.ToString(), out var enumValue)
			) {
				switch (enumValue) {
					case 0:
						return "private";
					case 1:
						return "protected";
					case 2:
						return "internal";
					case 3:
						return "public";
				}
			}

			return string.Empty;
		}

		internal class SyntaxReceiver : ISyntaxContextReceiver {
			public List<IFieldSymbol> Fields { get; } = new List<IFieldSymbol>();

			public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
				if (
					context.Node is FieldDeclarationSyntax fieldDeclarationSyntax
					&& fieldDeclarationSyntax.AttributeLists.Count > 0
				) {
					foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables) {
						IFieldSymbol fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;

						if (fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == ATTRIBUTE_NAME)) {
							Fields.Add(fieldSymbol);
						}
					}
				}
			}
		}
	}
}