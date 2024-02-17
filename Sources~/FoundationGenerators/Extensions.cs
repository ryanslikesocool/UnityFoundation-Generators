using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	internal static partial class Extensions {
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
