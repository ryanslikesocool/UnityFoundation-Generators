using Microsoft.CodeAnalysis;

namespace Foundation.Generators {
	internal sealed partial class PropertyGenerators {
		private static class AttributeFileGenerator {
			public static void Register(ref GeneratorInitializationContext context)
				=> context.RegisterPostInitializationCSFileGeneration(FILE_NAME_PREFIX, FILE_TEXT);

			// MARK: - Constants

			private const string FILE_NAME_PREFIX = "PropertyGeneratorAttributes";

			private const string FILE_TEXT = @"
using System;

/// <summary>
/// Generate a getter property.
/// </summary>
/// <seealso cref=""SetAttribute""/>
/// <seealso cref=""PropertyNameAttribute""/>
/// <seealso cref=""PropertyAccessibilityAttribute""/>
/// <seealso cref=""PropertyModifierAttribute""/>
/// <seealso cref=""OnChangeAttribute""/>
/// <seealso cref=""WillSetAttribute""/>
/// <seealso cref=""DidSetAttribute""/>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class GetAttribute : Attribute { }

/// <summary>
/// Generate a setter property.
/// </summary>
/// <seealso cref=""GetAttribute""/>
/// <seealso cref=""PropertyNameAttribute""/>
/// <seealso cref=""PropertyAccessibilityAttribute""/>
/// <seealso cref=""PropertyModifierAttribute""/>
/// <seealso cref=""OnChangeAttribute""/>
/// <seealso cref=""WillSetAttribute""/>
/// <seealso cref=""DidSetAttribute""/>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class SetAttribute : Attribute { }

/// <summary>
/// Indicates that a property created with <see cref=""GetAttribute""/> and/or <see cref=""SetAttribute""/> should use a custom name.
/// </summary>
/// <remarks>
/// If the default ""promoted"" name is acceptable, this attribute may be omitted.
/// </remarks>
/// <seealso cref=""GetAttribute""/>
/// <seealso cref=""SetAttribute""/>
/// <seealso cref=""PropertyAccessibilityAttribute""/>
/// <seealso cref=""PropertyModifierAttribute""/>
/// <seealso cref=""OnChangeAttribute""/>
/// <seealso cref=""WillSetAttribute""/>
/// <seealso cref=""DidSetAttribute""/>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class PropertyNameAttribute :  Attribute {
	public PropertyNameAttribute(string name = null) { }
}

/// <summary>
/// Indicates that a property created with <see cref=""GetAttribute""/> and/or <see cref=""SetAttribute""/> should restrict accessibility.
/// </summary>
/// <remarks>
/// If the property accessibility should be <c>public</c>, this attribute may be omitted.
/// </remarks>
/// <seealso cref=""GetAttribute""/>
/// <seealso cref=""SetAttribute""/>
/// <seealso cref=""PropertyNameAttribute""/>
/// <seealso cref=""PropertyModifierAttribute""/>
/// <seealso cref=""OnChangeAttribute""/>
/// <seealso cref=""WillSetAttribute""/>
/// <seealso cref=""DidSetAttribute""/>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class PropertyAccessibilityAttribute : Attribute {
	/// <summary>
	/// The access level of the generated property.
	/// </summary>
	public enum Accessibility {
		Private = 0,
		Protected = 1,
		Internal = 2,
		Public = 3
	}

	public PropertyAccessibilityAttribute(Accessibility accessibility = Accessibility.Public) { }
}

/// <summary>
/// Indicates that a property created with <see cref=""GetAttribute""/> and/or <see cref=""SetAttribute""/> should have add additional modifiers to the declaration.
/// </summary>[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
/// <remarks>
/// If no modifiers are needed, this attribute may be omitted.
/// </remarks>
/// <seealso cref=""GetAttribute""/>
/// <seealso cref=""SetAttribute""/>
/// <seealso cref=""PropertyNameAttribute""/>
/// <seealso cref=""PropertyAccessibilityAttribute""/>
/// <seealso cref=""OnChangeAttribute""/>
/// <seealso cref=""WillSetAttribute""/>
/// <seealso cref=""DidSetAttribute""/>
internal sealed class PropertyModifierAttribute : Attribute {
	/// <summary>
	/// Accessibility modifiers applied to the generated property.
	/// </summary>
	[Flags]
	public enum Modifier {
		None = 0,
		Static = 1 << 0,
		ReadOnly = 1 << 1,
		New = 1 << 2
	}

	public PropertyModifierAttribute(Modifier modifier = Modifier.None) { }
}

/// <summary>
/// Indicates that a property created with <see cref=""SetAttribute""/> should call a method when the underlying value is changed.
/// </summary>
/// <remarks>
/// The default method that will be called is <c>OnChange[your property name]</c>.
///
/// The function signature should be formatted as such, where <c>MyProperty</c> is the property name:
/// <code>
/// void OnChangeMyProperty() { /* ... */ }
/// </code>
/// </remarks>
/// <seealso cref=""GetAttribute""/>
/// <seealso cref=""SetAttribute""/>
/// <seealso cref=""PropertyNameAttribute""/>
/// <seealso cref=""PropertyAccessibilityAttribute""/>
/// <seealso cref=""PropertyModifierAttribute""/>
/// <seealso cref=""WillSetAttribute""/>
/// <seealso cref=""DidSetAttribute""/>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class OnChangeAttribute : Attribute {
	public OnChangeAttribute(string methodName = null) { }
}

/// <summary>
/// Indicates that a property created with <see cref=""SetAttribute""/> should call a method immediately before setting the underlying value.
/// </summary>
/// <remarks>
/// The default method that will be called is <c>WillSet[your property name]</c>.
///
/// The function signature should be formatted as such, where <c>MyProperty</c> is the property name and <c>T</c> is the property type:
/// <code>
/// void WillSetMyProperty(T oldValue, ref T newValue) { /* ... */ }
/// </code>
/// </remarks>
/// <seealso cref=""GetAttribute""/>
/// <seealso cref=""SetAttribute""/>
/// <seealso cref=""PropertyNameAttribute""/>
/// <seealso cref=""PropertyAccessibilityAttribute""/>
/// <seealso cref=""PropertyModifierAttribute""/>
/// <seealso cref=""OnChangeAttribute""/>
/// <seealso cref=""DidSetAttribute""/>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class WillSetAttribute : Attribute {
	public WillSetAttribute(string methodName = null) { }
}

/// <summary>
/// Indicates that a property created with <see cref=""SetAttribute""/> should call a method immediately after setting the underlying value.
/// </summary>
/// <remarks>
/// The default method that will be called is <c>DidSet[your property name]</c>.
///
/// The function signature should be formatted as such, where <c>MyProperty</c> is the property name and <c>T</c> is the property type:
/// <code>
/// void DidSetMyProperty(T oldValue, T newValue) { /* ... */ }
/// </code>
/// </remarks>
/// <seealso cref=""GetAttribute""/>
/// <seealso cref=""SetAttribute""/>
/// <seealso cref=""PropertyNameAttribute""/>
/// <seealso cref=""PropertyAccessibilityAttribute""/>
/// <seealso cref=""PropertyModifierAttribute""/>
/// <seealso cref=""OnChangeAttribute""/>
/// <seealso cref=""WillSetAttribute""/>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class DidSetAttribute : Attribute {
	public DidSetAttribute(string methodName = null) { }
}
			";
		}
	}
}