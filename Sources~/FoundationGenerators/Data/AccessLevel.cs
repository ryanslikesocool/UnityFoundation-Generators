using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	internal enum AccessLevel {
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

		public static string Description(this AccessLevel accessLevel) {
			switch (accessLevel) {
				case AccessLevel.Private:
					return ACCESS_LEVEL_PRIVATE;
				case AccessLevel.Protected:
					return ACCESS_LEVEL_PROTECTED;
				case AccessLevel.Internal:
					return ACCESS_LEVEL_INTERNAL;
				case AccessLevel.Public:
					return ACCESS_LEVEL_PUBLIC;
				default:
					return string.Empty;
			}
		}

		public static AccessLevel? ProcessAccessLevel(this TypedConstant argument) {
			if (int.TryParse(argument.Value.ToString(), out int enumValue)) {
				return (AccessLevel)enumValue;
			} else {
				return null;
			}
		}
	}
}