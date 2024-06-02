using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	internal enum PropertyAccessibility {
		Private = 0,
		Protected = 1,
		Internal = 2,
		Public = 3
	}

	internal static partial class Extensions {
		private const string ACCESS_LEVEL_PRIVATE = "private";
		private const string ACCESS_LEVEL_PROTECTED = "protected";
		private const string ACCESS_LEVEL_INTERNAL = "internal";
		private const string ACCESS_LEVEL_PUBLIC = "public";

		public static string Description(this PropertyAccessibility accessLevel) {
			// Can't use inline switch because C# 7.3 doesn't support it...
			switch (accessLevel) {
				case PropertyAccessibility.Private:
					return ACCESS_LEVEL_PRIVATE;
				case PropertyAccessibility.Protected:
					return ACCESS_LEVEL_PROTECTED;
				case PropertyAccessibility.Internal:
					return ACCESS_LEVEL_INTERNAL;
				case PropertyAccessibility.Public:
					return ACCESS_LEVEL_PUBLIC;
				default:
					return string.Empty;
			}
		}

		public static PropertyAccessibility? ProcessAccessLevel(this TypedConstant argument) {
			if (int.TryParse(argument.Value.ToString(), out int enumValue)) {
				return (PropertyAccessibility)enumValue;
			} else {
				return null;
			}
		}
	}
}