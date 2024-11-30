using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	internal sealed partial class SingletonComponentGenerator {
		private static class AttributeFileGenerator {
			public static void Register(ref GeneratorInitializationContext context)
				=> context.RegisterPostInitializationCSFileGeneration(FILE_NAME_PREFIX, FILE_TEXT);

			// MARK: - Constants

			private const string FILE_NAME_PREFIX = SingletonComponentGenerator.ATTRIBUTE_NAME;

			private const string FILE_TEXT = @"
using System;

/// <summary>
/// Mark a component as a singleton, generating a shared instance accessible from anywhere in C#.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
internal sealed class SingletonComponentAttribute : Attribute {
	/// <summary>
	/// Should the component and GameObject persist between scenes?
	/// </summary>
	/// <remarks>
	/// This uses <c>DontDestroyOnLoad(this.gameObject);</c> internally.
	/// </remarks>
	public bool persistent { get; set; }

	/// <summary>
	/// Should a GameObject with this component automatically be created if a shared instance cannot be found?
	/// </summary>
	public bool auto { get; set; }

	public SingletonComponentAttribute() { }
}
			";
		}
	}
}