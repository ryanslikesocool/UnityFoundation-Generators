using System;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Foundation.Generators {
	internal static class Extensions {
		public static bool IsDerivedFrom(this INamedTypeSymbol baseType, string targetType) {
			while (baseType != null) {
				if (baseType.Name == targetType) {
					return true;
				}

				baseType = baseType.BaseType;
			}

			return false;
		}

		public static string PromoteFieldName(in string fieldName) {
			if (fieldName[0] == '_') {
				// _field -> field
				return fieldName.Substring(1);
			} else {
				// field -> Field
				return $"{fieldName[0].ToString().ToUpper()}{fieldName.Substring(1)}";
			}
		}

		public static string WrapNamespace(
			this INamedTypeSymbol classSymbol,
			Action<StringBuilder> content
		) {
			var source = new StringBuilder();
			bool hasNamespace = classSymbol.ContainingNamespace != null;

			if (hasNamespace) {
				source.AppendLine($"namespace {classSymbol.ContainingNamespace} {{");
			}

			content(source);

			if (hasNamespace) {
				source.Append("\n}");
			}

			return source.ToString();
		}

		public static string ProcessAccessLevel(this TypedConstant argument) {
			if (int.TryParse(argument.Value.ToString(), out var enumValue)) {
				switch (enumValue) {
					case 0:
						return "private";
					case 1:
						return "protected";
					case 2:
						return "internal";
					case 3:
						return "public";
				}
			}
			return string.Empty;
		}

		public static string AsString(this TypeKind kind) {
			switch (kind) {
				case TypeKind.Array:
					return "Array";
				case TypeKind.Class:
					return "Class";
				case TypeKind.Delegate:
					return "Delegate";
				case TypeKind.Dynamic:
					return "Dynamic";
				case TypeKind.Enum:
					return "Enum";
				case TypeKind.Error:
					return "Error";
				case TypeKind.FunctionPointer:
					return "Function Pointer";
				case TypeKind.Interface:
					return "Interface";
				case TypeKind.Module:
					return "Module";
				case TypeKind.Pointer:
					return "Pointer";
				case TypeKind.Struct:
					return "Struct";
				case TypeKind.Submission:
					return "Submission";
				default:
					return "Unknown";
			}
		}
	}
}
