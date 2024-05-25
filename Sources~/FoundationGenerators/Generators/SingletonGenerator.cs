using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Generators;

namespace Foundation.Generators {
	[Generator]
	public sealed class SingletonGenerator : ISourceGenerator {
		private const string ATTRIBUTE_NAME = "SingletonAttribute";

		private const string FILE_TEXT = @"
using UnityEngine;
using System;

/// <summary>
/// Mark a type as a singleton, accessible from anywhere in C#.
/// </summary>
/// <remarks>
/// To mark a Unity component as a singleton, use the <see cref=""SingletonComponent""/> attribute instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
internal sealed class SingletonAttribute : Attribute {
	/// <summary>
	/// The stage in which to initialize the singleton.
	/// </summary>
	/// <remarks>
	/// Set this value to <see langword=""null""/> to load lazily.
	/// </remarks>
	public RuntimeInitializeLoadType? LoadType { get; set; }

	public SingletonAttribute() { }
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

			foreach (INamedTypeSymbol typeSymbol in receiver.Types) {
				string classSource = ProcessClass(typeSymbol, attributeSymbol);
				context.AddSource($"{typeSymbol.Name}_{ATTRIBUTE_NAME}_gen.cs", SourceText.From(classSource, Encoding.UTF8));
			}
		}

		private string ProcessClass(INamedTypeSymbol typeSymbol, ISymbol attributeSymbol)
			=> SourceBuilder.Run(instance => {
				AttributeData attributeData = typeSymbol.GetAttributes().Single(ad
					=> ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
				ProcessAttribute(attributeData, out RuntimeInitializeLoadType? loadType);

				if (loadType.HasValue) {
					instance.UsingNamespaces("UnityEngine");
				}

				instance.ExtendType(typeSymbol, _ => {

					{ // getter
						instance.source.AppendLine($@"
	private static {typeSymbol.Name} _shared = default;

	/// <summary>
	/// The shared instance of the object.
	/// </summary>
	public static {typeSymbol.Name} Shared
		=> _shared ??= new {typeSymbol.Name}();
						");
					}

					if (loadType.HasValue) {
						instance.source.AppendLine($@"
	[RuntimeInitializeOnLoadMethod({loadType.HasValue})]
	private static void FoundationGenerators_RuntimeInitializeOnLoadMethod() {{
		_shared = Shared;
	}}
						");
					}
				});
			});

		private void ProcessAttribute(AttributeData attributeData, out RuntimeInitializeLoadType? loadType) {
			loadType = null;
		}

		private sealed class SyntaxReceiver : ISyntaxContextReceiver {
			public List<INamedTypeSymbol> Types { get; } = new List<INamedTypeSymbol>();

			public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
				if (
					context.Node is TypeDeclarationSyntax typeDeclarationSyntax
					&& typeDeclarationSyntax.AttributeLists.Count > 0
				) {
					INamedTypeSymbol typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax) as INamedTypeSymbol;

					if (
						!(typeSymbol?.BaseType.IsDerivedFrom("MonoBehaviour") ?? true)
						&& typeSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == ATTRIBUTE_NAME)
					) {
						Types.Add(typeSymbol);
					}
				}
			}
		}
	}
}
