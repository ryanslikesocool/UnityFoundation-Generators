using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

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
			// Can't use inline switch because sln C# version isn't up-to-date...
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

		public static bool TryGetArgumentIndex(this AttributeData attributeData, string name, out int index) {
			index = -1;

			for (int i = 0; i < attributeData.NamedArguments.Length; i++) {
				if (attributeData.NamedArguments[i].Key == name) {
					index = i;
					return true;
				}
			}

			return false;
		}

		public static bool TryGetArgument(this AttributeData attributeData, string name, out KeyValuePair<string, TypedConstant> argument) {
			argument = default;

			if (attributeData.TryGetArgumentIndex(name, out int index)) {
				argument = attributeData.NamedArguments[index];
				return true;
			}

			return false;
		}

		public static KeyValuePair<string, TypedConstant>? GetArgument(this AttributeData attributeData, string name) {
			if (attributeData.TryGetArgument(name, out KeyValuePair<string, TypedConstant> result)) {
				return result;
			} else {
				return null;
			}
		}

		public static bool TryGetArgumentValue(this AttributeData attributeData, string name, out TypedConstant argumentValue) {
			argumentValue = default;

			if (attributeData.TryGetArgument(name, out KeyValuePair<string, TypedConstant> namedArgument)) {
				argumentValue = namedArgument.Value;
				return true;
			}

			return false;
		}

		public static T? GetArgumentStruct<T>(this AttributeData attributeData, string argumentName) where T : struct {
			if (attributeData.TryGetArgumentValue(argumentName, out TypedConstant argumentValue)) {
				if (typeof(T) == typeof(bool)) {
					return argumentValue.BoolValue() as T?;
				} else {
					throw new NotImplementedException();
				}
			} else {
				return null;
			}
		}

		public static T GetArgumentClass<T>(this AttributeData attributeData, string argumentName) where T : class {
			if (attributeData.TryGetArgumentValue(argumentName, out TypedConstant argumentValue)) {
				if (typeof(T) == typeof(string)) {
					return argumentValue.StringValue() as T;
				} else {
					throw new NotImplementedException();
				}
			} else {
				return null;
			}
		}

		public static bool? BoolValue(this TypedConstant argument) {
			if (!argument.IsNull) {
				return bool.Parse(argument.Value.ToString());
			} else {
				return null;
			}
		}

		public static Optional<string> StringValue(this TypedConstant argument) {
			if (!argument.IsNull) {
				return argument.Value.ToString();
			} else {
				return null;
			}
		}
	}
}
