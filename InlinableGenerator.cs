using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Foundation.CodeGen {
		public class InlinableReplacer: CSharpSyntaxRewriter {

	}

//	[Generator]
//	public class InlinableGenerator : ISourceGenerator {
//		private const string attributeText = @"
//using System;

	//namespace Foundation {
	//	[AttributeUsage(System.AttributeTargets.Constructor | System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	//	public sealed class InlinableAttribute : Attribute {
	//		public InlinableAttribute() { }
	//	}
	//}";
	//		public void Initialize(GeneratorInitializationContext context) {
	//			// Register the attribute source
	//			context.RegisterForPostInitialization(i => i.AddSource("InlinableAttribute.generated.cs", attributeText));

	//			// Register a syntax receiver that will be created for each generation pass
	//			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
	//		}

	//		public void Execute(GeneratorExecutionContext context) {
	//			// retrieve the populated receiver 
	//			if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver)) {
	//				return;
	//			}

	//			// get the added attribute, and INotifyPropertyChanged
	//			INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName("Foundation.InlinableAttribute");
	//		}

	//		/// <summary>
	//		/// Created on demand before each generation pass
	//		/// </summary>
	//		internal class SyntaxReceiver : ISyntaxContextReceiver {
	//			public List<IMethodSymbol> Methods { get; } = new List<IMethodSymbol>();

	//			/// <summary>
	//			/// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
	//			/// </summary>
	//			public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
	//				// any field with at least one attribute is a candidate for property generation
	//				if (context.Node is MethodDeclarationSyntax methodDeclarationSyntax && methodDeclarationSyntax.AttributeLists.Count > 0) {
	//					// Get the symbol being declared by the method, and keep it if its annotated
	//					IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;
	//					if (methodSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "Foundation.InlinableAttribute")) {
	//						Methods.Add(methodSymbol);
	//					}
	//				}
	//			}
	//		}
	//	}
}