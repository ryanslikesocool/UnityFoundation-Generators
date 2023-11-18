using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FoundationGenerators {
	[Generator]
	public class EditorMutableGenerator : ISourceGenerator {
		private const string ATTRIBUTE_NAME = "EditorMutableAttribute";

		private const string ATTRIBUTE_TEXT = @"
using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class EditorMutableAttribute : Attribute {
	public EditorMutableAttribute() { }
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
				context.AddSource($"{group.Key.Name}_EditorMutable_gen.cs", SourceText.From(classSource, Encoding.UTF8));
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
			var fieldName = fieldSymbol.Name;
			ITypeSymbol fieldType = fieldSymbol.Type;

			AttributeData attributeData = fieldSymbol.GetAttributes().Single(ad
				=> ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

			var publicFieldName = fieldName;
			if (fieldName[0] == '_') {
				// _field -> field
				publicFieldName = fieldName.Substring(1);
			} else {
				// field -> Field
				publicFieldName = $"{fieldName[0].ToString().ToUpper()}{fieldName.Substring(1)}";
			}

			source.AppendLine($"public {fieldType} {publicFieldName} => {fieldName};");
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