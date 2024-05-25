using System;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	[Flags]
	internal enum Mutability {
		Get = 1 << 0,
		Set = 1 << 1,
		GetSet = Get | Set
	}

	internal static partial class Extensions {
		public static Mutability? ProcessArgumentMutability(this TypedConstant argument) {
			if (int.TryParse(argument.Value.ToString(), out int flagsValue)) {
				return (Mutability)flagsValue;
			} else {
				return null;
			}
		}

		/// <summary>
		/// 0: Access level
		/// 1: Access modifiers
		/// 2: Type
		/// 3: Exposed name
		/// 4: Underlying name
		/// </summary>
		public static string GetPropertyFormat(this Mutability mutability) {
			// Can't use inline switch because sln C# version isn't up-to-date...
			switch (mutability) {
				case Mutability.Get:
					return GET_FORMAT;
				case Mutability.Set:
					return SET_FORMAT;
				case Mutability.GetSet:
					return GET_SET_FORMAT;
				default:
					return string.Empty;
			}
		}

		private const string GET_SET_FORMAT = @"
{0} {1} {2} {3} {
	get => {4};
	set => {4} = value;
}
		";
		private const string GET_FORMAT = @"
{0} {1} {2} {3} => {4};
		";
		private const string SET_FORMAT = @"
{0} {1} {2} {3} {
	set => {4} = value;
}
		";
	}
}