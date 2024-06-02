using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;

namespace Foundation.Generators {
	internal static partial class Extensions {
		public static AttributeData GetAttribute(this System.Collections.Immutable.ImmutableArray<AttributeData> attributes, ISymbol symbol)
			=> attributes.Single(ad => ad.AttributeClass.Equals(symbol, SymbolEqualityComparer.Default));

		// MARK: - Constructor Argument

		public static TypedConstant? GetConstructorArgument(this AttributeData attributeData, string typeName)
		 	=> attributeData.ConstructorArguments.FirstOrNull(arg => arg.Type.ToDisplayString() == typeName);

		public static bool TryGetConstructorArgument(this AttributeData attributeData, string typeName, out TypedConstant argument)
			=> attributeData.GetConstructorArgument(typeName).TryUnwrap(out argument);

		public static T? GetConstructorArgumentStruct<T>(this AttributeData attributeData, string typeName) where T : struct
			=> attributeData.GetConstructorArgument(typeName ?? typeof(T).ToString())?.GetStructValue<T>();

		public static T GetConstructorArgumentStruct<T>(this AttributeData attributeData, string typeName, T defaultValue) where T : struct
			=> attributeData.GetConstructorArgumentStruct<T>(typeName) ?? defaultValue;

		public static T GetConstructorArgumentClass<T>(this AttributeData attributeData, string typeName) where T : class
			=> attributeData.GetConstructorArgument(typeName ?? typeof(T).ToString())?.GetClassValue<T>();

		// MARK: - Named Argument

		public static KeyValuePair<string, TypedConstant>? GetNamedArgument(this AttributeData attributeData, string argumentName)
			=> attributeData.NamedArguments.FirstOrNull(arg => arg.Key == argumentName);

		public static bool TryGetNamedArgument(this AttributeData attributeData, string argumentName, out KeyValuePair<string, TypedConstant> argument)
			=> attributeData.GetNamedArgument(argumentName).TryUnwrap(out argument);

		public static TypedConstant? GetNamedArgumentValue(this AttributeData attributeData, string argumentName)
			=> attributeData.GetNamedArgument(argumentName)?.Value;

		public static T? GetNamedArgumentStruct<T>(this AttributeData attributeData, string argumentName) where T : struct
			=> attributeData.GetNamedArgumentValue(argumentName)?.GetStructValue<T>();

		public static T GetNamedArgumentStruct<T>(this AttributeData attributeData, string argumentName, T defaultValue) where T : struct
			=> attributeData.GetNamedArgumentStruct<T>(argumentName) ?? defaultValue;

		public static T GetNamedArgumentClass<T>(this AttributeData attributeData, string argumentName) where T : class
			=> attributeData.GetNamedArgumentValue(argumentName)?.GetClassValue<T>();
	}
}