using System.Text;
using System;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	internal static class SourceBuilder {
		public static string Run(Action<Instance> builder) {
			Instance instance = new Instance();
			builder(instance);
			return instance.Finalize();
		}

		public static Instance UsingNamespaces(
			this Instance instance,
		 	params string[] namespaces
		) {
			foreach (string element in namespaces) {
				instance.source.AppendLine($"using {element};");
			}
			instance.source.AppendLine(string.Empty);

			return instance;
		}

		public static Instance MatchNamespace(
			this Instance instance,
			INamedTypeSymbol typeSymbol,
			Action<StringBuilder> contentBuilder
		) {
			bool hasNamespace = typeSymbol.ContainingNamespace != null;

			if (hasNamespace) {
				instance.source.AppendLine($"namespace {typeSymbol.ContainingNamespace} {{");
			}

			contentBuilder(instance.source);

			if (hasNamespace) {
				instance.source.AppendLine("}");
			}

			return instance;
		}

		public static Instance ExtendType(
			this Instance instance,
			INamedTypeSymbol typeSymbol,
			Action<StringBuilder> contentBuilder
		) {
			instance.MatchNamespace(typeSymbol, _ => {
				instance.source.AppendLine($"{typeSymbol.DeclaredAccessibility.Description()} partial {typeSymbol.TypeKind.Description().ToLower()} {typeSymbol.Name} {{");
				contentBuilder(instance.source);
				instance.source.AppendLine("}");
			});

			return instance;
		}

		public sealed class Instance {
			public StringBuilder source;

			public delegate void Builder(StringBuilder source);

			public Instance() {
				source = new StringBuilder();
			}

			public string Finalize() {
				string result = source.ToString();
				source = null;
				return result;
			}
		}
	}
}