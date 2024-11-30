using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	internal sealed partial class SingletonGenerator {
		private static class AttributeFileGenerator {
			public static void Register(ref GeneratorInitializationContext context)
				=> context.RegisterPostInitializationCSFileGeneration(FILE_NAME_PREFIX, FILE_TEXT);

			// MARK: - Constants

			private const string FILE_NAME_PREFIX = SingletonGenerator.ATTRIBUTE_NAME;

			private const string FILE_TEXT = @"
using System;

/// <summary>
/// Mark a type as a singleton, generating a shared instance accessible from anywhere in C#.
/// </summary>
/// <remarks>
/// To mark a Unity component as a singleton, use the <see cref=""SingletonComponent""/> attribute instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
internal sealed class SingletonAttribute : Attribute {
	/// <remarks>
	/// Maps to UnityEngine.RuntimeInitializeLoadType
	/// </remarks>
	public enum RuntimeInitializeLoadType {
		AfterSceneLoad,
		BeforeSceneLoad,
		AfterAssembliesLoaded,
		BeforeSplashScreen,
		SubsystemRegistration
	}

	/// <summary>
	/// Declares a lazily-loaded singleton.
	/// </summary>
	public SingletonAttribute() { }

	/// <summary>
	/// Declares an explicitly-loaded singleton.
	/// </summary>
	/// <param name=""loadType"">
	/// The stage in which to initialize the singleton.
	/// </param>
	public SingletonAttribute(RuntimeInitializeLoadType loadType) { }
}
			";
		}
	}
}