//using System.Linq;
//using System.Text;
//using System.Collections.Generic;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Text;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System;
//
//namespace Foundation.Generators {
//	[Generator]
//	public class PropertyObserverGenerator : ISourceGenerator {
//		private const string ATTRIBUTE_NAME = "PropertyObserver";
//
//		private const string ATTRIBUTE_TEXT = @"
//using System;
//
//namespace Foundation {
//	using Generators;
//
//	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
//	public sealed class PropertyObserverAttribute : Attribute {
//		public PropertyObserverAttribute(string willSet, string didSet, AccessLevel accessLevel = AccessLevel.Public) { }
//	}
//}
//";
//
//		public void Initialize(GeneratorInitializationContext context) {
//			context.RegisterForPostInitialization(i
//				=> i.AddSource($"{ATTRIBUTE_NAME}_gen.cs", ATTRIBUTE_TEXT)
//			);
//			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
//		}
//
//		public void Execute(GeneratorExecutionContext context) {
//			if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver)) {
//				return;
//			}
//
//			INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName(ATTRIBUTE_NAME);
//
//			foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> group in receiver.Fields.GroupBy<IFieldSymbol, INamedTypeSymbol>(f => f.ContainingType, SymbolEqualityComparer.Default)) {
//				var classSource = ProcessClass(group.Key, group, attributeSymbol);
//				context.AddSource($"{group.Key.Name}_PropertyObserver_gen.cs", SourceText.From(classSource, Encoding.UTF8));
//			}
//		}
//
//		private string ProcessClass(INamedTypeSymbol classSymbol, IEnumerable<IFieldSymbol> fields, ISymbol attributeSymbol) {
//			var source = new StringBuilder();
//			bool hasNamespace = classSymbol.ContainingNamespace != null;
//
//			if (hasNamespace) {
//				source.AppendLine($"namespace {classSymbol.ContainingNamespace} {{");
//			}
//
//			source.AppendLine($@"
//	public partial class {classSymbol.Name} {{
//");
//
//			foreach (IFieldSymbol fieldSymbol in fields) {
//				ProcessField(source, fieldSymbol, attributeSymbol);
//			}
//
//			source.Append("\n}");
//
//			if (hasNamespace) {
//				source.Append("\n}");
//			}
//
//			return source.ToString();
//		}
//
//		private void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol, ISymbol attributeSymbol) {
//			string fieldName = fieldSymbol.Name;
//			ITypeSymbol fieldType = fieldSymbol.Type;
//
//			AttributeData attributeData = fieldSymbol.GetAttributes().Single(ad
//				=> ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
//
//			ProcessAttribute(attributeData, out string accessLevel, out string willSetFunction, out string didSetFunction);
//			string publicFieldName = Extensions.PromoteFieldName(fieldName);
//
//			source.AppendLine($@"{accessLevel} {fieldType} {publicFieldName} {{
//				get => {fieldName};
//				set {{
//					{fieldType} oldValue = {fieldName};
//");
//
//			if (willSetFunction != null) {
//				source.AppendLine($"{willSetFunction}(oldValue, ref value);");
//			}
//
//			source.AppendLine($"{fieldName} = value;");
//
//			if (didSetFunction != null) {
//				source.AppendLine($"{didSetFunction}(oldValue, value);");
//			}
//
//			source.AppendLine("}\n}");
//		}
//
//		private void ProcessAttribute(AttributeData attributeData, out string accessLevel, out string willSetFunction, out string didSetFunction) {
//			accessLevel = ProcessAccessLevel();
//			willSetFunction = ProcessFunctionName(attributeData.ConstructorArguments[1]);
//			didSetFunction = ProcessFunctionName(attributeData.ConstructorArguments[2]);
//
//			string ProcessAccessLevel() {
//				if (int.TryParse(attributeData.ConstructorArguments[0].Value.ToString(), out var enumValue)) {
//					switch (enumValue) {
//						case 0:
//							return "private";
//						case 1:
//							return "protected";
//						case 2:
//							return "internal";
//						case 3:
//							return "public";
//					}
//				}
//				return string.Empty;
//			}
//
//			string ProcessFunctionName(TypedConstant argument) {
//				if (!argument.IsNull) {
//					return argument.Value.ToString();
//				} else {
//					return null;
//				}
//			}
//		}
//
//		internal class SyntaxReceiver : ISyntaxContextReceiver {
//			public List<IFieldSymbol> Fields { get; } = new List<IFieldSymbol>();
//
//			public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
//				if (
//					context.Node is FieldDeclarationSyntax fieldDeclarationSyntax
//					&& fieldDeclarationSyntax.AttributeLists.Count > 0
//				) {
//					foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables) {
//						IFieldSymbol fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
//
//						if (fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == ATTRIBUTE_NAME)) {
//							Fields.Add(fieldSymbol);
//						}
//					}
//				}
//			}
//		}
//	}
//}