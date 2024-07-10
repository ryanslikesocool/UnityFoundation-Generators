using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Foundation.Generators {
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	internal sealed class ThrowsGeneratorAnalyzer : DiagnosticAnalyzer {
		private static readonly DiagnosticDescriptor DeclarationRule = new DiagnosticDescriptor(
			id: "ThrowsAttributeDeclarationAnalyzer",
			title: $"The declaration is marked [Throws], but does not throw any uncaught exceptions",
			messageFormat: $"The declaration is marked [Throws], but does not throw any uncaught exceptions",
			category: "RuntimeSafety",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: $"The declaration is marked [Throws], but does not throw any uncaught exceptions",
			helpLinkUri: string.Empty
		);


		private static readonly DiagnosticDescriptor InvocationRule = new DiagnosticDescriptor(
			id: "ThrowsAttributeInvocationAnalyzer",
			title: $"The invocation may throw, but the exception is not caught or passed upward",
			messageFormat: $"The invocation may throw, but the exception is not caught or passed upward",
			category: "RuntimeSafety",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: $"The invocation may throw, but the exception is not caught or passed upward",
			helpLinkUri: string.Empty
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(DeclarationRule, InvocationRule);

		public override void Initialize(AnalysisContext context) {
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
			context.RegisterSyntaxNodeAction(AnalyzeMethodInvocation, SyntaxKind.InvocationExpression);
		}

		private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context) {
			MethodDeclarationSyntax methodDeclarationSyntax = context.Node as MethodDeclarationSyntax;
			IMethodSymbol methodSymbol = context.ContainingSymbol as IMethodSymbol;

			if (
				!HasThrowsAttribute(methodSymbol)
				|| !ChildHasThrowStatement(methodDeclarationSyntax)
				|| !ChildInvokesThrows(methodDeclarationSyntax, context)
			) {
				return;
			}

			Diagnostic diagnostic = Diagnostic.Create(DeclarationRule, methodDeclarationSyntax.GetLocation());
			context.ReportDiagnostic(diagnostic);
		}

		private void AnalyzeMethodInvocation(SyntaxNodeAnalysisContext context) {
			InvocationExpressionSyntax invocationExpressionSyntax = context.Node as InvocationExpressionSyntax;
			IMethodSymbol methodSymbol = context.ContainingSymbol as IMethodSymbol;
			IMethodSymbol methodDeclaration = GetMethodDeclaration(invocationExpressionSyntax, context);

			if (
				!HasThrowsAttribute(methodDeclaration) ||
				HasThrowsAttribute(methodSymbol) ||
				AncestorHasTryStatement(invocationExpressionSyntax)
			) {
				return;
			}

			Diagnostic diagnostic = Diagnostic.Create(InvocationRule, invocationExpressionSyntax.GetLocation());
			context.ReportDiagnostic(diagnostic);
		}

		private static bool HasThrowsAttribute(ISymbol method)
			=> method.GetAttributes().Any(ad => ad?.AttributeClass?.ToDisplayString() == ThrowsGenerator.ATTRIBUTE_NAME);

		private static bool AncestorHasTryStatement(SyntaxNode syntax)
		 	=> syntax.Ancestors().Any(a => a.IsKind(SyntaxKind.TryStatement));

		private static bool ChildHasThrowStatement(SyntaxNode syntax)
			=> syntax.ChildNodes().Any(a => a.IsKind(SyntaxKind.ThrowKeyword));

		private static IMethodSymbol GetMethodDeclaration(InvocationExpressionSyntax syntax, SyntaxNodeAnalysisContext context)
			=> context
				.SemanticModel
				.GetSymbolInfo(syntax)
				.Symbol as IMethodSymbol;

		private static bool ChildInvokesThrows(SyntaxNode syntax, SyntaxNodeAnalysisContext context)
			=> syntax.ChildNodes()
				.Where(a => a.IsKind(SyntaxKind.InvocationExpression))
				.Select(a => GetMethodDeclaration(a as InvocationExpressionSyntax, context))
				.Any(a => HasThrowsAttribute(a));
	}
}