using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Foundation.Generators {
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	internal sealed class GetComponentAnalyzer : DiagnosticAnalyzer {
		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
			id: "GetComponentAttributeAnalyzer",
			title: $"{GetComponentGenerator.REQUIRED_FUNCTION_NAME} method should be called",
			messageFormat: $"{GetComponentGenerator.REQUIRED_FUNCTION_NAME} method should be called",
			category: "InitializationSafety",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: $"{GetComponentGenerator.REQUIRED_FUNCTION_NAME} method should be called",
			helpLinkUri: string.Empty
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context) {
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
		}

		private void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context) {
			FieldDeclarationSyntax fieldDeclarationSyntax = (FieldDeclarationSyntax)context.Node;
			IFieldSymbol fieldSymbol = (IFieldSymbol)context.ContainingSymbol;

			if (!HasGetComponentAttribute(fieldSymbol)) {
				return;
			}

			SyntaxNode classNode = fieldSymbol.ContainingType.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

			foreach (InvocationExpressionSyntax expressionSyntax in classNode.DescendantNodes().OfType<InvocationExpressionSyntax>()) {
				IMethodSymbol methodSymbol = context.SemanticModel.GetSymbolInfo(expressionSyntax).Symbol as IMethodSymbol;

				if (methodSymbol == null) {
					continue;
				}
				if (methodSymbol.Name == GetComponentGenerator.REQUIRED_FUNCTION_NAME) {
					return;
				}
			}

			Diagnostic diagnostic = Diagnostic.Create(Rule, fieldDeclarationSyntax.GetLocation());
			context.ReportDiagnostic(diagnostic);
		}

		private static bool HasGetComponentAttribute(ISymbol fieldSymbol)
			=> fieldSymbol.GetAttributes().Any(ad => ad?.AttributeClass?.ToDisplayString() == GetComponentGenerator.ATTRIBUTE_NAME);
	}
}