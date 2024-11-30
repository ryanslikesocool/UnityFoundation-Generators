using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	internal sealed partial class GetComponentGenerator {
		private static class AttributeFileGenerator {
			public static void Register(ref GeneratorInitializationContext context)
				=> context.RegisterPostInitializationCSFileGeneration(FILE_NAME_PREFIX, FILE_TEXT);

			// MARK: - Constants

			private const string FILE_NAME_PREFIX = GetComponentGenerator.ATTRIBUTE_NAME;

			private const string FILE_TEXT = @"
using System;

/// <summary>
/// Automatically assign components on a <c>MonoBehaviour</c>.
/// <remarks>
/// You must call <c>InitializeComponents()</c> on the <c>MonoBehaviour</c> for components to be assigned.
/// </remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class GetComponentAttribute : Attribute {
	public enum TargetType {
		This = 0,
		Parent = 1,
		Child = 2
	}

	/// <param name=""targetType"">The object the component is attached to, relative to <c>this.gameObject</c>.</param>
	public GetComponentAttribute(TargetType targetType = TargetType.This) { }
}
			";
		}
	}
}