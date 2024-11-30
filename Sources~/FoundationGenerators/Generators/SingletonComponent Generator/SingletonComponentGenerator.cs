using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators {
	[Generator]
	internal sealed partial class SingletonComponentGenerator : ISourceGenerator {
		private const string ATTRIBUTE_NAME = "SingletonComponentAttribute";

		public void Initialize(GeneratorInitializationContext context) {
			AttributeFileGenerator.Register(ref context);
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
				instance.UsingNamespaces("UnityEngine");
				instance.ExtendType(typeSymbol, _ => {
					AttributeData attributeData = typeSymbol.GetAttributes().Single(ad
						=> ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

					bool persistent = attributeData.GetNamedArgumentStruct("persistent", false);
					bool auto = attributeData.GetNamedArgumentStruct("auto", false);

					{ // getter
						instance.source.AppendLine($@"
	private static {typeSymbol.Name} _shared = default;

	/// <summary>
	/// The shared instance of the Component.
	/// </summary>
	public static {typeSymbol.Name} Shared {{
		get {{
			if (_shared == null) {{
#if UNITY_2023_1_OR_NEWER
				{typeSymbol.Name} newShared = GameObject.FindAnyObjectByType<{typeSymbol.Name}>();
#else
				{typeSymbol.Name} newShared = GameObject.FindObjectOfType<{typeSymbol.Name}>();
#endif
				if (newShared != null) {{
					newShared.InitializeSingleton();
				}}
			}}
						");

						if (auto) {
							instance.source.AppendLine($@"
	if (_shared == null) {{
		GameObject singletonObject = new GameObject();
		singletonObject.hideFlags = HideFlags.HideAndDontSave;
		singletonObject.name = ""Singleton<{typeSymbol.Name}>"";

		_shared = singletonObject.AddComponent<{typeSymbol.Name}>();
	}}
							");
						}

						instance.source.AppendLine(@"
			return _shared;
		}
	}
						");
					}

					{ // init
						instance.source.AppendLine(@"
	private void InitializeSingleton() {
		if (_shared != null && _shared != this) {
			GameObject.Destroy(this);
			return;
		}

		_shared = this;
					");

						if (persistent) {
							instance.source.AppendLine("GameObject.DontDestroyOnLoad(this.gameObject);");
						}

						instance.source.AppendLine("}");
					}

					{ // deinit
						instance.source.AppendLine(@"
	/// <summary>
	/// Perform any necessary cleanup.
	/// </summary>
	public void DeinitializeSingleton() {
		_shared = default;
					");

						if (auto) {
							instance.source.AppendLine("GameObject.Destroy(this.gameObject);");
						}

						instance.source.AppendLine("}");
					}
				});
			});

		private sealed class SyntaxReceiver : ISyntaxContextReceiver {
			public List<INamedTypeSymbol> Types { get; } = new List<INamedTypeSymbol>();

			public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
				if (
					context.Node is TypeDeclarationSyntax typeDeclarationSyntax
					&& typeDeclarationSyntax.AttributeLists.Count > 0
				) {
					INamedTypeSymbol typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax) as INamedTypeSymbol;

					if (
						(typeSymbol?.BaseType.IsDerivedFrom("MonoBehaviour") ?? false)
						&& typeSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == ATTRIBUTE_NAME)
					) {
						Types.Add(typeSymbol);
					}
				}
			}
		}
	}
}
