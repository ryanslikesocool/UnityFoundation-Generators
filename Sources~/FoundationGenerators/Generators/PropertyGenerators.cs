using System.Linq;
using System.Collections.Immutable;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Foundation.Generators {
	[Generator]
	internal sealed class PropertyGenerators : ISourceGenerator {
		private const string GET_ATTRIBUTE_NAME = "GetAttribute";
		private const string SET_ATTRIBUTE_NAME = "SetAttribute";
		private const string PROPERTY_NAME_ATTRIBUTE_NAME = "PropertyNameAttribute";
		private const string PROPERTY_ACCESSIBILITY_ATTRIBUTE_NAME = "PropertyAccessibilityAttribute";
		private const string PROPERTY_MODIFIER_ATTRIBUTE_NAME = "PropertyModifierAttribute";
		private const string ON_CHANGE_ATTRIBUTE_NAME = "OnChangeAttribute";
		private const string WILL_SET_ATTRIBUTE_NAME = "WillSetAttribute";
		private const string DID_SET_ATTRIBUTE_NAME = "DidSetAttribute";

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
internal sealed class GetAttribute : Attribute {
	public GetAttribute() { }
}

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
internal sealed class SetAttribute : Attribute {
	public SetAttribute() { }
}

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

		public void Initialize(GeneratorInitializationContext context) {
			context.RegisterForPostInitialization(i => i.AddSource($"PropertyGeneratorAttributes_gen.cs", FILE_TEXT));
			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context) {
			if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver)) {
				return;
			}

			INamedTypeSymbol getAttributeSymbol = context.Compilation.GetTypeByMetadataName(GET_ATTRIBUTE_NAME);
			INamedTypeSymbol setAttributeSymbol = context.Compilation.GetTypeByMetadataName(SET_ATTRIBUTE_NAME);
			INamedTypeSymbol propertyNameAttributeSymbol = context.Compilation.GetTypeByMetadataName(PROPERTY_NAME_ATTRIBUTE_NAME);
			INamedTypeSymbol propertyAccessibilityAttributeSymbol = context.Compilation.GetTypeByMetadataName(PROPERTY_ACCESSIBILITY_ATTRIBUTE_NAME);
			INamedTypeSymbol propertyModifierAttributeSymbol = context.Compilation.GetTypeByMetadataName(PROPERTY_MODIFIER_ATTRIBUTE_NAME);
			INamedTypeSymbol onChangeAttributeSymbol = context.Compilation.GetTypeByMetadataName(ON_CHANGE_ATTRIBUTE_NAME);
			INamedTypeSymbol willSetAttributeSymbol = context.Compilation.GetTypeByMetadataName(WILL_SET_ATTRIBUTE_NAME);
			INamedTypeSymbol didSetAttributeSymbol = context.Compilation.GetTypeByMetadataName(DID_SET_ATTRIBUTE_NAME);

			foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> group in receiver.Fields.GroupBy<IFieldSymbol, INamedTypeSymbol>(f => f.ContainingType, SymbolEqualityComparer.Default)) {
				string classSource = ProcessContainingType(
					group.Key,
					group,
					getAttributeSymbol,
					setAttributeSymbol,
					propertyNameAttributeSymbol,
					propertyAccessibilityAttributeSymbol,
					propertyModifierAttributeSymbol,
					onChangeAttributeSymbol,
					willSetAttributeSymbol,
					didSetAttributeSymbol
				);
				context.AddSource($"{group.Key.Name}_GeneratedProperties_gen.cs", SourceText.From(classSource, Encoding.UTF8));
			}
		}

		private string ProcessContainingType(
			INamedTypeSymbol typeSymbol,
			IEnumerable<IFieldSymbol> fields,
			ISymbol getAttributeSymbol,
			ISymbol setAttributeSymbol,
			ISymbol propertyNameAttributeSymbol,
			ISymbol propertyAccessibilityAttributeSymbol,
			ISymbol propertyModifierAttributeSymbol,
			ISymbol onChangeAttributeSymbol,
			ISymbol willSetAttributeSymbol,
			ISymbol didSetAttributeSymbol
		)
			=> SourceBuilder.Run(instance => {
				instance.ExtendType(typeSymbol, _ => {
					foreach (IFieldSymbol fieldSymbol in fields) {
						ProcessField(
							instance.source,
							fieldSymbol,
							getAttributeSymbol,
							setAttributeSymbol,
							propertyNameAttributeSymbol,
							propertyAccessibilityAttributeSymbol,
							propertyModifierAttributeSymbol,
							onChangeAttributeSymbol,
							willSetAttributeSymbol,
							didSetAttributeSymbol
						);
					}
				});
			});

		private void ProcessField(
			StringBuilder source,
			IFieldSymbol fieldSymbol,
			ISymbol getAttributeSymbol,
			ISymbol setAttributeSymbol,
			ISymbol propertyNameAttributeSymbol,
			ISymbol propertyAccessibilityAttributeSymbol,
			ISymbol propertyModifierAttributeSymbol,
			ISymbol onChangeAttributeSymbol,
			ISymbol willSetAttributeSymbol,
			ISymbol didSetAttributeSymbol
		) {
			string fieldName = fieldSymbol.Name;
			ITypeSymbol fieldType = fieldSymbol.Type;

			ImmutableArray<AttributeData> attributes = fieldSymbol.GetAttributes();
			AttributeData getAttributeData = Extensions.TryReturnClass(() => attributes.GetAttribute(getAttributeSymbol));
			AttributeData setAttributeData = Extensions.TryReturnClass(() => attributes.GetAttribute(setAttributeSymbol));

			ProcessorFlag processor = ProcessorFlag.None;

			if (getAttributeData != null) {
				processor |= ProcessorFlag.Get;
			}
			if (setAttributeData != null) {
				processor |= ProcessorFlag.Set;
			}

			if (processor == ProcessorFlag.None) {
				return;
			}

			string propertyName;
			try {
				AttributeData propertyNameAttributeData = attributes.GetAttribute(propertyNameAttributeSymbol);
				propertyName = propertyNameAttributeData.GetConstructorArgumentClass<string>(typeName: null);
			} catch {
				propertyName = Extensions.PromoteFieldName(fieldName);
			}

			AttributeData propertyAccessibilityAttributeData = Extensions.TryReturnClass(() => attributes.GetAttribute(propertyAccessibilityAttributeSymbol));
			AttributeData propertyModifierAttributeData = Extensions.TryReturnClass(() => attributes.GetAttribute(propertyModifierAttributeSymbol));

			PropertyAccessibility propertyAccessibility = PropertyAccessibility.Public;
			if (propertyAccessibilityAttributeData != null) {
				propertyAccessibility = (PropertyAccessibility)(propertyAccessibilityAttributeData.GetConstructorArgument("PropertyAccessibilityAttribute.Accessibility")?.GetIntValue() ?? (int)propertyAccessibility);
			}
			PropertyModifier propertyModifier = PropertyModifier.None;
			if (propertyModifierAttributeData != null) {
				propertyModifier = (PropertyModifier)(propertyModifierAttributeData.GetConstructorArgument("PropertyModifierAttribute.Modifier")?.GetIntValue() ?? (int)propertyModifier);
			}

			string accessibilityString = propertyAccessibility.Description();
			string modifierString = propertyModifier.Description();

			string onChangeFunction = null;
			string willSetFunction = null;
			string didSetFunction = null;

			if (setAttributeData != null) {
				try {
					AttributeData onChangeAttributeData = attributes.GetAttribute(onChangeAttributeSymbol);
					onChangeFunction = onChangeAttributeData.GetConstructorArgument("string")?.GetStringValue() ?? $"OnChange{propertyName}";
				} catch {
					onChangeFunction = null;
				}

				try {
					AttributeData willSetAttributeData = attributes.GetAttribute(willSetAttributeSymbol);
					willSetFunction = willSetAttributeData.GetConstructorArgument("string")?.GetStringValue() ?? $"WillSet{propertyName}";
				} catch {
					willSetFunction = null;
				}

				try {
					AttributeData didSetAttributeData = attributes.GetAttribute(didSetAttributeSymbol);
					didSetFunction = didSetAttributeData.GetConstructorArgument("string")?.GetStringValue() ?? $"DidSet{propertyName}";
				} catch {
					didSetFunction = null;
				}

				if (!onChangeFunction.IsNullOrEmptyOrWhiteSpace()) {
					processor |= ProcessorFlag.OnChange;
				}
				if (!willSetFunction.IsNullOrEmptyOrWhiteSpace()) {
					processor |= ProcessorFlag.WillSet;
				}
				if (!didSetFunction.IsNullOrEmptyOrWhiteSpace()) {
					processor |= ProcessorFlag.DidSet;
				}
			}

			//source.AppendLine($"public const string {fieldName}_PropertyGeneratorInfo = \"Creating property with flags {processor} for {fieldName} -> {accessibilityString} {modifierString} {fieldType} {propertyName}\";");

			switch (processor) {
				case ProcessorFlag.Get:
					source.AppendFormat(FORMAT_GET, accessibilityString, modifierString, fieldType, propertyName, fieldName);
					break;
				case ProcessorFlag.Set:
					source.AppendFormat(FORMAT_SET, accessibilityString, modifierString, fieldType, propertyName, fieldName);
					break;
				case ProcessorFlag.Get_Set:
					source.AppendFormat(FORMAT_GET_SET, accessibilityString, modifierString, fieldType, propertyName, fieldName);
					break;
				case ProcessorFlag.Get_Set_OnChange:
					source.AppendFormat(FORMAT_GET_SET_ONCHANGE, accessibilityString, modifierString, fieldType, propertyName, fieldName, onChangeFunction);
					break;
				case ProcessorFlag.Get_Set_WillSet:
					source.AppendFormat(FORMAT_GET_SET_WILLSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction);
					break;
				case ProcessorFlag.Get_Set_DidSet:
					source.AppendFormat(FORMAT_GET_SET_DIDSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, didSetFunction);
					break;
				case ProcessorFlag.Set_OnChange:
					source.AppendFormat(FORMAT_SET_ONCHANGE, accessibilityString, modifierString, fieldType, propertyName, fieldName, onChangeFunction);
					break;
				case ProcessorFlag.Set_WillSet:
					source.AppendFormat(FORMAT_SET_WILLSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction);
					break;
				case ProcessorFlag.Set_DidSet:
					source.AppendFormat(FORMAT_SET_DIDSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, didSetFunction);
					break;
				case ProcessorFlag.Get_Set_OnChange_WillSet:
					source.AppendFormat(FORMAT_GET_SET_ONCHANGE_WILLSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, onChangeFunction);
					break;
				case ProcessorFlag.Get_Set_OnChange_DidSet:
					source.AppendFormat(FORMAT_GET_SET_ONCHANGE_DIDSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, onChangeFunction, didSetFunction);
					break;
				case ProcessorFlag.Get_Set_WillSet_DidSet:
					source.AppendFormat(FORMAT_GET_SET_WILLSET_DIDSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, didSetFunction);
					break;
				case ProcessorFlag.Get_Set_OnChange_WillSet_DidSet:
					source.AppendFormat(FORMAT_GET_SET_ONCHANGE_WILLSET_DIDSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, onChangeFunction, didSetFunction);
					break;
				case ProcessorFlag.Set_OnChange_WillSet:
					source.AppendFormat(FORMAT_SET_ONCHANGE_WILLSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, onChangeFunction);
					break;
				case ProcessorFlag.Set_OnChange_DidSet:
					source.AppendFormat(FORMAT_SET_ONCHANGE_DIDSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, onChangeFunction, didSetFunction);
					break;
				case ProcessorFlag.Set_WillSet_DidSet:
					source.AppendFormat(FORMAT_SET_WILLSET_DIDSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, didSetFunction);
					break;
				case ProcessorFlag.Set_OnChange_WillSet_DidSet:
					source.AppendFormat(FORMAT_SET_ONCHANGE_WILLSET_DIDSET, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, onChangeFunction, didSetFunction);
					break;
				default:
					source.AppendLine($"public const string {fieldName}_PropertyGenerationError = \"Invalid processor {processor} for {fieldName}\";");
					break;
			}
		}

		// MARK: - Syntax

		private sealed class SyntaxReceiver : ISyntaxContextReceiver {
			public List<IFieldSymbol> Fields { get; } = new List<IFieldSymbol>();

			public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
				if (
					context.Node is FieldDeclarationSyntax fieldDeclarationSyntax
					&& fieldDeclarationSyntax.AttributeLists.Count > 0
				) {
					foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables) {
						IFieldSymbol fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
						if (fieldSymbol.GetAttributes().Any(ad => {
							string displayString = ad.AttributeClass.ToDisplayString();
							return displayString == GET_ATTRIBUTE_NAME || displayString == SET_ATTRIBUTE_NAME;
						})) {
							Fields.Add(fieldSymbol);
						}
					}
				}
			}
		}

		// MARK: - Supporting Data

		[Flags]
		private enum ProcessorFlag {
			None = 0,

			Get = 1 << 0,
			Set = 1 << 1,
			OnChange = 1 << 2,
			WillSet = 1 << 3,
			DidSet = 1 << 4,

			Get_Set = Get | Set,
			Get_Set_OnChange = Get | Set | OnChange,
			Get_Set_WillSet = Get | Set | WillSet,
			Get_Set_DidSet = Get | Set | DidSet,
			Get_Set_OnChange_WillSet = Get | Set | OnChange | WillSet,
			Get_Set_OnChange_DidSet = Get | Set | OnChange | DidSet,
			Get_Set_WillSet_DidSet = Get | Set | WillSet | DidSet,
			Get_Set_OnChange_WillSet_DidSet = Get | Set | OnChange | WillSet | DidSet,
			Set_OnChange = Set | OnChange,
			Set_WillSet = Set | WillSet,
			Set_DidSet = Set | DidSet,
			Set_OnChange_WillSet = Set | OnChange | WillSet,
			Set_OnChange_DidSet = Set | OnChange | DidSet,
			Set_WillSet_DidSet = Set | WillSet | DidSet,
			Set_OnChange_WillSet_DidSet = Set | OnChange | WillSet | DidSet,
		}

		// MARK: - Formats

		private const string FORMAT_GET = @"
/// <summary>
/// An auto-generated property providing read-only access to <see cref=""{4}""/>.
/// </summary>
/// <seealso cref=""{4}""/>
{0} {1} {2} {3} => {4};
		";

		private const string FORMAT_SET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{4}""/>.
/// </summary>
/// <seealso cref=""{4}""/>
{0} {1} {2} {3} {{
	set => {4} = value;
}}
		";

		private const string FORMAT_GET_SET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// </summary>
/// <seealso cref=""{4}""/>
{0} {1} {2} {3} {{
	get => {4};
	set => {4} = value;
}}
		";

		private const string FORMAT_SET_ONCHANGE = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called if the underlying value changes when set.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
{0} {1} {2} {3} {{
	set {{
		{2} oldValue = {4};
		{4} = value;
		if (!oldValue.Equals(value)) {{
			{5}();
		}}
	}}
}}
		";

		private const string FORMAT_GET_SET_ONCHANGE = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called if the underlying value changes when set.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
{0} {1} {2} {3} {{
	get => {4};
	set {{
		{2} oldValue = {4};
		{4} = value;
		if (!oldValue.Equals(value)) {{
			{5}();
		}}
	}}
}}
		";

		private const string FORMAT_SET_WILLSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately before setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
{0} {1} {2} {3} {{
	set {{
		{5}({4}, ref value);
		{4} = value;
	}}
}}
		";

		private const string FORMAT_GET_SET_WILLSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately before setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
{0} {1} {2} {3} {{
	get => {4};
	set {{
		{5}({4}, ref value);
		{4} = value;
	}}
}}
		";

		private const string FORMAT_SET_DIDSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
{0} {1} {2} {3} {{
	set {{
		{2} oldValue = {4};
		{4} = value;
		{5}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_GET_SET_DIDSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
{0} {1} {2} {3} {{
	get => {4};
	set {{
		{2} oldValue = {4};
		{4} = value;
		{5}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_SET_ONCHANGE_DIDSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called if the underlying value changes when set.
/// The function <see cref=""{6}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0} {1} {2} {3} {{
	set {{
		{2} oldValue = {4};
		{4} = value;
		{5}();
		{6}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_GET_SET_ONCHANGE_DIDSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called if the underlying value changes when set.
/// The function <see cref=""{6}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0} {1} {2} {3} {{
	get => {4};
	set {{
		{2} oldValue = {4};
		{4} = value;
		if (!oldValue.Equals(value)) {{
			{5}();
		}}
		{6}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_SET_ONCHANGE_WILLSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{6}""/> will be called if the underlying value changes when set.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0} {1} {2} {3} {{
	set {{
		{2} oldValue = {4};
		{5}(oldValue, ref value);
		{4} = value;
		if (!oldValue.Equals(value)) {{
			{6}();
		}}
	}}
}}
		";

		private const string FORMAT_GET_SET_ONCHANGE_WILLSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{6}""/> will be called if the underlying value changes when set.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0} {1} {2} {3} {{
	get => {4};
	set {{
		{2} oldValue = {4};
		{5}(oldValue, ref value);
		{4} = value;
		if (!oldValue.Equals(value)) {{
			{6}();
		}}
	}}
}}
		";

		private const string FORMAT_SET_WILLSET_DIDSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{6}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0} {1} {2} {3} {{
	set {{
		{2} oldValue = {4};
		{5}(oldValue, ref value);
		{4} = value;
		{6}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_GET_SET_WILLSET_DIDSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{6}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0} {1} {2} {3} {{
	get => {4};
	set {{
		{2} oldValue = {4};
		{5}(oldValue, ref value);
		{4} = value;
		{6}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_SET_ONCHANGE_WILLSET_DIDSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{6}""/> will be called if the underlying value changes when set.
/// The function <see cref=""{7}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
/// <seealso cref=""{7}""/>
{0} {1} {2} {3} {{
	set {{
		{2} oldValue = {4};
		{5}(oldValue, ref value);
		{4} = value;
		if (!oldValue.Equals(value)) {{
			{6}();
		}}
		{7}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_GET_SET_ONCHANGE_WILLSET_DIDSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{6}""/> will be called if the underlying value changes when set.
/// The function <see cref=""{7}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
/// <seealso cref=""{7}""/>
{0} {1} {2} {3} {{
	get => {4};
	set {{
		{2} oldValue = {4};
		{5}(oldValue, ref value);
		{4} = value;
		if (!oldValue.Equals(value)) {{
			{6}();
		}}
		{7}(oldValue, value);
	}}
}}
		";
	}
}