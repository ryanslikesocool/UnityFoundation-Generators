using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators {
	[Generator]
	public sealed class SingletonGenerator : ISourceGenerator {
		private const string ATTRIBUTE_NAME = "SingletonAttribute";

		private const string FILE_TEXT = @"
using System;

/// <summary>
/// Mark a component as a singleton, accessible from anywhere in C#.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
internal sealed class SingletonAttribute : Attribute {
	/// <summary>
	/// Should the singleton persist between scenes?
	/// </summary>
	/// <remarks>
	/// This uses <c>DontDestroyOnLoad(this.gameObject);</c> internally.
	/// </remarks>
	public bool Persistent { get; set; }

	/// <summary>
	/// Should a GameObject with this component automatically be created if a shared instance cannot be found?
	/// </summary>
	public bool Auto { get; set; }

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
				instance.UsingNamespaces("UnityEngine");
				instance.ExtendType(typeSymbol, _ => {
					AttributeData attributeData = typeSymbol.GetAttributes().Single(ad
						=> ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

					ProcessAttribute(attributeData, out bool persistent, out bool auto);

					{ // getter
						instance.source.AppendLine($@"
	private static {typeSymbol.Name} _shared = default;

	public static {typeSymbol.Name} Shared {{
		get {{
			if (_shared == null) {{
				{typeSymbol.Name} newShared = GameObject.FindObjectOfType<{typeSymbol.Name}>();
				if (newShared != null) {{
					newShared.InitializeSingleton();
				}}
			}}
						");

						if (auto) {
							instance.source.AppendLine($@"
	if (_shared == null) {{
		GameObject singletonObject = new GameObject();
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
							instance.source.AppendLine(@"
		GameObject.DontDestroyOnLoad(this.gameObject);
						");
						}

						instance.source.AppendLine(@"
	}
					");
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
							instance.source.AppendLine(@"
		GameObject.Destroy(this.gameObject);
							");
						}

						instance.source.AppendLine(@"
	}
						");
					}
				});
			});

		private void ProcessAttribute(AttributeData attributeData, out bool persistent, out bool auto) {
			persistent = false;
			auto = false;

			string[] argumentNames = new string[2] {
				"Persistent",
				"Auto"
			};

			for (int i = 0; i < attributeData.NamedArguments.Length; i++) {
				if (attributeData.NamedArguments[i].Key == argumentNames[0]) {
					persistent = ProcessBoolean(attributeData.NamedArguments[i].Value);
				}
				if (attributeData.NamedArguments[i].Key == argumentNames[1]) {
					auto = ProcessBoolean(attributeData.NamedArguments[i].Value);
				}
			}
		}

		private bool ProcessBoolean(TypedConstant argument) {
			if (!argument.IsNull) {
				return bool.Parse(argument.Value.ToString());
			} else {
				return false;
			}
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
