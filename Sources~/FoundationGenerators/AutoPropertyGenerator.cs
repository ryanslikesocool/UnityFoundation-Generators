using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators {
	[Generator]
	public class AutoPropertyGenerator : ISourceGenerator {
		private const string ATTRIBUTE_NAME = "AutoPropertyAttribute";

		private const string FILE_TEXT = @"
using System;

/// <summary>
/// Generate a property with the given access level.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class AutoPropertyAttribute : Attribute {
	public enum AccessLevel {
		Private = 0,
		Protected = 1,
		Internal = 2,
		Public = 3
	}

	[Flags]
	public enum Mutability {
		Get = 1 << 0,
		Set = 1 << 1
	}

	public AutoPropertyAttribute(AccessLevel accessLevel = AccessLevel.Public, Mutability mutability = Mutability.Get) { }
}
		";

		public void Initialize(GeneratorInitializationContext context) {
			context.RegisterForPostInitialization(i
				=> i.AddSource($"{ATTRIBUTE_NAME}_gen.cs", FILE_TEXT)
			);
			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context) {
			if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver)) {
				return;
			}

			INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName(ATTRIBUTE_NAME);

			foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> group in receiver.Fields.GroupBy<IFieldSymbol, INamedTypeSymbol>(f => f.ContainingType, SymbolEqualityComparer.Default)) {
				string classSource = ProcessClass(group.Key, group, attributeSymbol);
				context.AddSource($"{group.Key.Name}_AutoProperty_gen.cs", SourceText.From(classSource, Encoding.UTF8));
			}
		}

		private string ProcessClass(INamedTypeSymbol classSymbol, IEnumerable<IFieldSymbol> fields, ISymbol attributeSymbol)
			=> Extensions.WrapNamespace(classSymbol, (StringBuilder source) => {
				source.AppendLine($"public partial class {classSymbol.Name} {{");

				foreach (IFieldSymbol fieldSymbol in fields) {
					ProcessField(source, fieldSymbol, attributeSymbol);
				}

				source.AppendLine("}");
			});

		private void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol, ISymbol attributeSymbol) {
			string fieldName = fieldSymbol.Name;
			ITypeSymbol fieldType = fieldSymbol.Type;

			AttributeData attributeData = fieldSymbol.GetAttributes().Single(ad
				=> ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)
			);

			ProcessAttribute(attributeData, out string accessLevel, out bool hasGetter, out bool hasSetter);
			string publicFieldName = Extensions.PromoteFieldName(fieldName);

			//for (int i = 0; i < attributeData.ConstructorArguments.Length; i++) {
			//	source.AppendLine(attributeData.ConstructorArguments[i].Type.ToDisplayString());
			//}

			if (hasGetter && hasSetter) {
				source.AppendLine($@"{accessLevel} {fieldType} {publicFieldName} {{
					get => {fieldName};
					set => {fieldName} = value;
				}}");
			} else if (hasGetter) {
				source.AppendLine($"{accessLevel} {fieldType} {publicFieldName} => {fieldName};");
			} else if (hasSetter) {
				source.AppendLine($@"{accessLevel} {fieldType} {publicFieldName} {{
					set => {fieldName} = value;
				}}");
			}
		}

		private void ProcessAttribute(AttributeData attributeData, out string accessLevel, out bool hasGetter, out bool hasSetter) {
			accessLevel = "public";
			hasGetter = true;
			hasSetter = false;

			string[] argumentTypes = new string[2] {
				"AutoPropertyAttribute.AccessLevel",
				"AutoPropertyAttribute.Mutability"
			};

			for (int i = 0; i < attributeData.ConstructorArguments.Length; i++) {
				if (attributeData.ConstructorArguments[i].Type.ToDisplayString() == argumentTypes[0]) {
					accessLevel = ProcessAccessLevel(attributeData.ConstructorArguments[i]);
				}
				if (attributeData.ConstructorArguments[i].Type.ToDisplayString() == argumentTypes[1]) {
					ProcessArgumentMutability(attributeData.ConstructorArguments[i], out hasGetter, out hasSetter);
				}
			}
		}

		private string ProcessAccessLevel(TypedConstant argument) {
			if (int.TryParse(argument.Value.ToString(), out int enumValue)) {
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

		private void ProcessArgumentMutability(TypedConstant argument, out bool hasGetter, out bool hasSetter) {
			hasGetter = false;
			hasSetter = false;

			if (int.TryParse(argument.Value.ToString(), out int flagsValue)) {
				if ((flagsValue & (1 << 0)) != 0) {
					hasGetter = true;
				}
				if ((flagsValue & (1 << 1)) != 0) {
					hasSetter = true;
				}
			}
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

						if (fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == ATTRIBUTE_NAME)) {
							Fields.Add(fieldSymbol);
						}
					}
				}
			}
		}
	}
}