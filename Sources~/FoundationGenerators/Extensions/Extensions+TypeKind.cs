using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	internal static partial class Extensions {
		public static string Description(this TypeKind kind) {
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
	}
}