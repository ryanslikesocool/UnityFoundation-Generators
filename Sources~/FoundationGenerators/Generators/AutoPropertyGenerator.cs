using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators {
	[Generator]
	public sealed class AutoPropertyGenerator : ISourceGenerator {
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
	public enum AccessModifier {
		None = 0,
		Static = 1 << 0,
		ReadOnly = 1 << 1,
		New = 1 << 2
	}

	[Flags]
	public enum Mutability {
		Get = 1 << 0,
		Set = 1 << 1,
		GetSet = Get | Set
	}

	/// <param name=""accessLevel"">The access level of the generated property.</param>
	/// <param name=""mutability"">The mutability of the generated property.</param>
	public AutoPropertyAttribute(AccessLevel accessLevel = AccessLevel.Public, AccessModifier accessModifier = AccessModifier.None, Mutability mutability = Mutability.Get) { }
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
				context.AddSource($"{group.Key.Name}_{ATTRIBUTE_NAME}_gen.cs", SourceText.From(classSource, Encoding.UTF8));
			}
		}

		private string ProcessClass(INamedTypeSymbol typeSymbol, IEnumerable<IFieldSymbol> fields, ISymbol attributeSymbol)
			=> SourceBuilder.Run(instance => {
				instance.ExtendType(typeSymbol, _ => {
					foreach (IFieldSymbol fieldSymbol in fields) {
						ProcessField(instance.source, fieldSymbol, attributeSymbol);
					}
				});
			});

		private void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol, ISymbol attributeSymbol) {
			string fieldName = fieldSymbol.Name;
			ITypeSymbol fieldType = fieldSymbol.Type;

			AttributeData attributeData = fieldSymbol.GetAttributes().Single(ad
				=> ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)
			);

			ProcessAttribute(attributeData, out AccessLevel accessLevel, out AccessModifier accessModifier, out Mutability mutability);
			string publicFieldName = Extensions.PromoteFieldName(fieldName);

			source.AppendFormat(mutability.GetPropertyFormat(), accessLevel.Description(), accessModifier.Description(), fieldType, publicFieldName, fieldName);
		}


		private void ProcessAttribute(AttributeData attributeData, out AccessLevel accessLevel, out AccessModifier accessModifier, out Mutability mutability) {
			accessLevel = AccessLevel.Public;
			accessModifier = AccessModifier.None;
			mutability = Mutability.Get;

			string[] argumentTypes = new string[3] {
				"AutoPropertyAttribute.AccessLevel",
				"AutoPropertyAttribute.AccessModifier",
				"AutoPropertyAttribute.Mutability"
			};

			for (int i = 0; i < attributeData.ConstructorArguments.Length; i++) {
				TypedConstant argument = attributeData.ConstructorArguments[i];
				string typeDisplayName = argument.Type.ToDisplayString();

				if (typeDisplayName == argumentTypes[0]) {
					accessLevel = argument.ProcessAccessLevel() ?? accessLevel;
				}
				if (typeDisplayName == argumentTypes[1]) {
					accessModifier = argument.ProcessAccessModifier() ?? accessModifier;
				}
				if (typeDisplayName == argumentTypes[2]) {
					mutability = argument.ProcessArgumentMutability() ?? mutability;
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