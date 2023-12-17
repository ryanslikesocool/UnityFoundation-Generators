using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Foundation.Generators {
	[Generator]
	public sealed class PropertyObserverGenerator : ISourceGenerator {
		private const string ATTRIBUTE_NAME = "PropertyObserverAttribute";

		private const string FILE_TEXT = @"
using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class PropertyObserverAttribute : Attribute {
	public enum AccessLevel {
		Private = 0,
		Protected = 1,
		Internal = 2,
		Public = 3
	}

	/// <summary>
	/// The name of the function to be called immediately before the property value will be set.
	/// </summary>
	public string WillSet { get; set;}

	/// <summary>
	/// The name of the function to be called immediately after the property value is set.
	/// </summary>
	public string DidSet { get; set;}

	/// <param name=""accessLevel"">The access level of the generated property.</param>
	public PropertyObserverAttribute(AccessLevel accessLevel = AccessLevel.Public) { }
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
				var classSource = ProcessClass(group.Key, group, attributeSymbol);
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
				=> ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

			ProcessAttribute(attributeData, out string accessLevel, out string willSetFunction, out string didSetFunction);
			string publicFieldName = Extensions.PromoteFieldName(fieldName);

			source.AppendLine($@"
			{accessLevel} {fieldType} {publicFieldName} {{
				get => {fieldName};
				set {{
					{fieldType} oldValue = {fieldName};
			");

			if (willSetFunction != null) {
				source.AppendLine($"{willSetFunction}(oldValue, ref value);");
			}

			source.AppendLine($"{fieldName} = value;");

			if (didSetFunction != null) {
				source.AppendLine($"{didSetFunction}(oldValue, value);");
			}

			source.AppendLine("}\n}");

			//for (int i = 0; i < attributeData.NamedArguments.Length; i++) {
			//	source.AppendLine(attributeData.NamedArguments[i].Key);
			//}
		}

		private void ProcessAttribute(AttributeData attributeData, out string accessLevel, out string willSetFunction, out string didSetFunction) {
			accessLevel = "public";
			willSetFunction = null;
			didSetFunction = null;

			string[] argumentTypes = new string[1] {
				"PropertyObserverAttribute.AccessLevel"
			};

			string[] argumentNames = new string[2] {
				"WillSet",
				"DidSet"
			};

			for (int i = 0; i < argumentTypes.Length; i++) {
				if (attributeData.ConstructorArguments[i].Type.ToDisplayString() == argumentTypes[0]) {
					accessLevel = attributeData.ConstructorArguments[i].ProcessAccessLevel();
				}
			}

			for (int i = 0; i < attributeData.NamedArguments.Length; i++) {
				if (attributeData.NamedArguments[i].Key == argumentNames[0]) {
					willSetFunction = ProcessFunctionName(attributeData.NamedArguments[i].Value);
				}
				if (attributeData.NamedArguments[i].Key == argumentNames[1]) {
					didSetFunction = ProcessFunctionName(attributeData.NamedArguments[i].Value);
				}
			}
		}

		private string ProcessFunctionName(TypedConstant argument) {
			if (!argument.IsNull) {
				return argument.Value.ToString();
			} else {
				return null;
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