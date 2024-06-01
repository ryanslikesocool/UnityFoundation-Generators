using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	internal static partial class Extensions {
		public static string Description(this Accessibility accessibility) {
			// Can't use inline switch because C# 7.3 doesn't support it...
			switch (accessibility) {
				case Accessibility.Private:
					return ACCESS_LEVEL_PRIVATE;
				case Accessibility.Protected:
					return ACCESS_LEVEL_PROTECTED;
				case Accessibility.Internal:
					return ACCESS_LEVEL_INTERNAL;
				case Accessibility.Public:
					return ACCESS_LEVEL_PUBLIC;
				default:
					return string.Empty;
			}
		}
	}
}