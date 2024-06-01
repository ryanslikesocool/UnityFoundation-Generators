using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Foundation.Generators {
	internal static partial class Extensions {
		// MARK: - Constructor Argument

		public static bool TryGetConstructorArgumentValue(this AttributeData attributeData, string typeName, out TypedConstant argument)
			=> attributeData.ConstructorArguments.FirstOrNull(arg => arg.Type.ToDisplayString() == typeName).TryUnwrap(out argument);

		public static bool TryGetConstructorArgumentValue<T>(this AttributeData attributeData, string typeName, out TypedConstant argument)
			=> attributeData.TryGetConstructorArgumentValue(typeName ?? typeof(T).ToString(), out argument);

		public static T? GetConstructorArgumentStructValue<T>(this AttributeData attributeData, string typeName = null) where T : struct {
			if (attributeData.TryGetConstructorArgumentValue<T>(typeName ?? typeof(T).ToString(), out TypedConstant argumentValue)) {
				return argumentValue.GetStructValue<T>();
			} else {
				return null;
			}
		}

		public static T GetConstructorArgumentClassValue<T>(this AttributeData attributeData, string typeName = null) where T : class {
			if (attributeData.TryGetConstructorArgumentValue<T>(typeName ?? typeof(T).ToString(), out TypedConstant argumentValue)) {
				return argumentValue.GetClassValue<T>();
			} else {
				return null;
			}
		}

		// MARK: - Named Argument

		public static bool TryGetNamedArgument(this AttributeData attributeData, string argumentName, out KeyValuePair<string, TypedConstant> argument)
			=> attributeData.NamedArguments.FirstOrNull(arg => arg.Key == argumentName).TryUnwrap(out argument);

		public static bool TryGetNamedArgumentValue(this AttributeData attributeData, string argumentName, out TypedConstant result) {
			if (attributeData.TryGetNamedArgument(argumentName, out KeyValuePair<string, TypedConstant> namedArgument)) {
				result = namedArgument.Value;
				return true;
			} else {
				result = default;
				return false;
			}
		}

		public static bool TryGetNamedArgumentStructValue<T>(this AttributeData attributeData, string argumentName, out T value) where T : struct {
			if (
				attributeData.TryGetNamedArgumentValue(argumentName, out TypedConstant parameterValue)
				&& parameterValue.TryGetStructValue(out value)
			) {
				return true;
			} else {
				value = default;
				return false;
			}
		}

		public static T? GetNamedArgumentStructValue<T>(this AttributeData attributeData, string argumentName) where T : struct {
			if (attributeData.TryGetNamedArgumentValue(argumentName, out TypedConstant argumentValue)) {
				return argumentValue.GetStructValue<T>();
			} else {
				return null;
			}
		}

		public static bool TryGetNamedArgumentClassValue<T>(this AttributeData attributeData, string argumentName, out T value) where T : class {
			if (
				attributeData.TryGetNamedArgumentValue(argumentName, out TypedConstant parameterValue)
				&& parameterValue.TryGetClassValue(out value)
			) {
				return true;
			}
			value = default;
			return false;
		}

		public static T GetNamedArgumentClassValue<T>(this AttributeData attributeData, string argumentName) where T : class {
			if (attributeData.TryGetNamedArgumentValue(argumentName, out TypedConstant argumentValue)) {
				return argumentValue.GetClassValue<T>();
			} else {
				return null;
			}
		}
	}
}