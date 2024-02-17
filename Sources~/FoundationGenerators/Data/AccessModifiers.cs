using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	[Flags]
	internal enum AccessModifier {
		None = 0,
		Static = 1 << 0,
		ReadOnly = 1 << 1,
		New = 1 << 2
	}

	internal static partial class Extensions {
		private const string ACCESS_MODIFIER_STATIC = "static";
		private const string ACCESS_MODIFIER_READONLY = "readonly";
		private const string ACCESS_MODIFIER_NEW = "new";

		public static AccessModifier? ProcessAccessModifier(this TypedConstant argument) {
			if (int.TryParse(argument.Value.ToString(), out int enumValue)) {
				return (AccessModifier)enumValue;
			} else {
				return null;
			}
		}

		public static string Description(this AccessModifier accessModifier) {
			List<string> modifiers = new List<string>(3);

			if (accessModifier.HasFlag(AccessModifier.Static)) {
				modifiers.Add(ACCESS_MODIFIER_STATIC);
			}
			if (accessModifier.HasFlag(AccessModifier.ReadOnly)) {
				modifiers.Add(ACCESS_MODIFIER_READONLY);
			}
			if (accessModifier.HasFlag(AccessModifier.New)) {
				modifiers.Add(ACCESS_MODIFIER_NEW);
			}

			return string.Join(" ", modifiers);
		}
	}
}