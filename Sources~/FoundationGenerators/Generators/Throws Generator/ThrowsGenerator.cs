using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	[Generator]
	internal sealed class ThrowsGenerator : ISourceGenerator {
		public const string ATTRIBUTE_NAME = "ThrowsAttribute";

		private const string FILE_TEXT = @"
using System;

/// <summary>
/// Indicates that a function has the potential to throw an exception that should be caught.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
internal sealed class ThrowsAttribute : Attribute { }
		";

		public void Initialize(GeneratorInitializationContext context) {
			context.RegisterForPostInitialization(i => i.AddSource($"{ATTRIBUTE_NAME}_gen.cs", FILE_TEXT));
		}

		public void Execute(GeneratorExecutionContext context) { }
	}
}