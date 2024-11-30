using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	internal static partial class Extensions {
		public static bool IsNullOrEmptyOrWhiteSpace(this string text)
			=> string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text);

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
			} else if (fieldName[0] == 'm' && fieldName[1] == '_') {
				// m_Field -> field
				return $"{fieldName[2].ToString().ToLower()}{fieldName.Substring(3)}";
			} else {
				// field -> Field
				return $"{fieldName[0].ToString().ToUpper()}{fieldName.Substring(1)}";
			}
		}

		public static string FormatGeneratedCSFileName(string prefix) {
			const string generatedFileNameFormat = "{0}_gen.cs";
			return string.Format(generatedFileNameFormat, prefix);
		}

		public static void RegisterPostInitializationCSFileGeneration(this ref GeneratorInitializationContext context, in string fileNamePrefix, string fileContent) {
			string attributeFileName = FormatGeneratedCSFileName(fileNamePrefix);
			context.RegisterForPostInitialization(i => i.AddSource(attributeFileName, fileContent));
		}
	}
}
