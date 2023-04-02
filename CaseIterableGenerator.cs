/*
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Foundation.CodeGen {
	[Generator]
	public class CaseIterableGenerator : ISourceGenerator {
		private const string attributeText = @"
using System;

namespace Foundation {
	[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
	public sealed class CaseIterableAttribute : Attribute {
		public CaseIterableAttribute() { }
	}
}";

		private const string caseIterableExtension = @"
public static partial class {{0}}Extensions {
	public static {{0}}[] AllCases
}
";
		// "this" keyword doesn't work on types 🙃

		public void Initialize(GeneratorInitializationContext context) {
			// Register the attribute source
			context.RegisterForPostInitialization(i => i.AddSource("CaseIterableAttribute.generated.cs", attributeText));

			// Register a syntax receiver that will be created for each generation pass
			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context) {
			// retrieve the populated receiver 
			if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver)) {
				return;
			}

			// get the added attribute, and INotifyPropertyChanged
			INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName("Foundation.CaseIterableAttribute");
		}

		/// <summary>
		/// Created on demand before each generation pass
		/// </summary>
		internal class SyntaxReceiver : ISyntaxContextReceiver {
			public List<ITypeSymbol> Enums { get; } = new List<ITypeSymbol>();

			/// <summary>
			/// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
			/// </summary>
			public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
				// any field with at least one attribute is a candidate for property generation
				if (context.Node is EnumDeclarationSyntax enumDeclarationSyntax && enumDeclarationSyntax.AttributeLists.Count > 0) {
					ITypeSymbol typeSymbol = context.SemanticModel.GetDeclaredSymbol(enumDeclarationSyntax) as ITypeSymbol;
					if (typeSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "Foundation.CaseIterableAttribute")) {
						Enums.Add(typeSymbol);
					}
				}
			}
		}
	}
}
*/