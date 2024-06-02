using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Foundation.Generators {
	[Generator]
	internal sealed class AutoPropertyGenerator : ISourceGenerator {
		private const string ATTRIBUTE_NAME = "AutoPropertyAttribute";

		private const string FILE_TEXT = @"
using System;

/// <summary>
/// Generate a property.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class AutoPropertyAttribute : Attribute {
	/// <summary>
	/// The access level of the generated property.
	/// </summary>
	public enum AccessLevel {
		Private = 0,
		Protected = 1,
		Internal = 2,
		Public = 3
	}

	/// <summary>
	/// Accessibility modifiers applied to the generated property.
	/// </summary>
	[Flags]
	public enum AccessModifier {
		None = 0,
		Static = 1 << 0,
		ReadOnly = 1 << 1,
		New = 1 << 2
	}

	/// <summary>
	/// The mutability of the generated property.
	/// </summary>
	[Flags]
	public enum Mutability {
		Get = 1 << 0,
		Set = 1 << 1,
		GetSet = Get | Set
	}

	/// <summary>
	/// The access level of the generated property.
	/// </summary>
	public AutoPropertyAttribute.AccessLevel accessLevel { get; set; }

	/// <summary>
	/// Additional access modifiers attached to the generated property.
	/// </summary>
	public AutoPropertyAttribute.AccessModifier accessModifier { get; set; }

	/// <summary>
	/// The mutability of the generated property.
	/// </summary>
	public AutoPropertyAttribute.Mutability mutability { get; set; }

	/// <summary>
	/// The name of the method to call when the property value changes after being set.
	/// </summary>
	/// <remark>
	/// Leave this as <see langword=""null""/> to not observe changes.
	/// Mutability will automatically be set to <see cref=""Mutability.GetSet""/> if <c>OnChange</c> is non-null.
	/// </remark>
	public string onChange { get; set; }

	/// <summary>
	/// The name of the function to be called immediately before the property value is set.
	/// </summary>
	/// <remark>
	/// Leave this as <see langword=""null""/> to not observe changes.
	/// Mutability will automatically be set to <see cref=""Mutability.GetSet""/> if <c>OnChange</c> is non-null.
	/// </remark>
	public string willSet { get; set; }

	/// <summary>
	/// The name of the function to be called immediately after the property value is set.
	/// </summary>
	/// <remark>
	/// Leave this as <see langword=""null""/> to not observe changes.
	/// Mutability will automatically be set to <see cref=""Mutability.GetSet""/> if <c>OnChange</c> is non-null.
	/// </remark>
	public string didSet { get; set; }

	public AutoPropertyAttribute() { }
}
		";

		public void Initialize(GeneratorInitializationContext context) {
			context.RegisterForPostInitialization(i => i.AddSource($"{ATTRIBUTE_NAME}_gen.cs", FILE_TEXT));
			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context) {
			if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver)) {
				return;
			}

			INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName(ATTRIBUTE_NAME);

			foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> group in receiver.Fields.GroupBy<IFieldSymbol, INamedTypeSymbol>(f => f.ContainingType, SymbolEqualityComparer.Default)) {
				string classSource = ProcessContainingType(group.Key, group, attributeSymbol);
				context.AddSource($"{group.Key.Name}_{ATTRIBUTE_NAME}_gen.cs", SourceText.From(classSource, Encoding.UTF8));
			}
		}

		private string ProcessContainingType(INamedTypeSymbol typeSymbol, IEnumerable<IFieldSymbol> fields, ISymbol attributeSymbol)
			=> SourceBuilder.Run(instance => {
				instance.ExtendType(typeSymbol, _ => {
					foreach (IFieldSymbol fieldSymbol in fields) {
						ProcessField(instance.source, fieldSymbol, attributeSymbol);
					}
				});
			});

		private void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol, ISymbol attributeSymbol) {
			string fieldName = fieldSymbol.Name;
			ITypeSymbol fieldType = fieldSymbol.Type;

			AttributeData attributeData = fieldSymbol.GetAttributes().Single(ad
				=> ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)
			);

			PropertyAccessibility accessLevel = (PropertyAccessibility)attributeData.GetNamedArgumentStruct("accessLevel", (int)PropertyAccessibility.Public);
			PropertyModifier accessModifier = (PropertyModifier)attributeData.GetNamedArgumentStruct("accessModifier", (int)PropertyModifier.None);
			PropertyMutability mutability = (PropertyMutability)attributeData.GetNamedArgumentStruct("mutability", (int)PropertyMutability.Get);

			string onChangeFunction = attributeData.GetNamedArgumentClass<string>("onChange");
			string willSetFunction = attributeData.GetNamedArgumentClass<string>("willSet");
			string didSetFunction = attributeData.GetNamedArgumentClass<string>("didSet");

			string promotedFieldName = Extensions.PromoteFieldName(fieldName);
			string accessLevelString = accessLevel.Description();
			string accessModifierString = accessModifier.Description();

			ProcessorFlag processor = ProcessorFlag.None;

			if (!onChangeFunction.IsNullOrEmptyOrWhiteSpace()) {
				processor |= ProcessorFlag.OnChange;
			}
			if (!willSetFunction.IsNullOrEmptyOrWhiteSpace()) {
				processor |= ProcessorFlag.WillSet;
			}
			if (!didSetFunction.IsNullOrEmptyOrWhiteSpace()) {
				processor |= ProcessorFlag.DidSet;
			}

			//source.AppendLine($"public const string {fieldName}_AutoPropertyInfo = \"Creating [AutoProperty] for {fieldName} -> {accessLevelString} {accessModifierString} {fieldType} {promotedFieldName}\";");

			switch (processor) {
				case ProcessorFlag.None:
					switch (mutability) {
						case PropertyMutability.Get:
							source.AppendFormat(FORMAT_GET, accessLevelString, accessModifierString, fieldType, promotedFieldName, fieldName);
							break;
						case PropertyMutability.Set:
							source.AppendFormat(FORMAT_SET, accessLevelString, accessModifierString, fieldType, promotedFieldName, fieldName);
							break;
						case PropertyMutability.GetSet:
							source.AppendFormat(FORMAT_GET_SET, accessLevelString, accessModifierString, fieldType, promotedFieldName, fieldName);
							break;
						default:
							source.AppendLine($"public const string {fieldName}ProcessorError = \"Invalid mutability {mutability} for [AutoProperty] {fieldName}\";");
							break;
					}
					break;
				case ProcessorFlag.OnChange:
					source.AppendFormat(FORMAT_ONCHANGE, accessLevelString, accessModifierString, fieldType, promotedFieldName, fieldName, onChangeFunction);
					break;
				case ProcessorFlag.WillSet:
					source.AppendFormat(FORMAT_WILLSET, accessLevelString, accessModifierString, fieldType, promotedFieldName, fieldName, willSetFunction);
					break;
				case ProcessorFlag.DidSet:
					source.AppendFormat(FORMAT_DIDSET, accessLevelString, accessModifierString, fieldType, promotedFieldName, fieldName, didSetFunction);
					break;
				case ProcessorFlag.OnChange_WillSet:
					source.AppendFormat(FORMAT_ONCHANGE_WILLSET, accessLevelString, accessModifierString, fieldType, promotedFieldName, fieldName, willSetFunction, onChangeFunction);
					break;
				case ProcessorFlag.OnChange_DidSet:
					source.AppendFormat(FORMAT_ONCHANGE_DIDSET, accessLevelString, accessModifierString, fieldType, promotedFieldName, fieldName, onChangeFunction, didSetFunction);
					break;
				case ProcessorFlag.WillSet_DidSet:
					source.AppendFormat(FORMAT_WILLSET_DIDSET, accessLevelString, accessModifierString, fieldType, promotedFieldName, fieldName, willSetFunction, didSetFunction);
					break;
				case ProcessorFlag.OnChange_WillSet_DidSet:
					source.AppendFormat(FORMAT_ONCHANGE_WILLSET_DIDSET, accessLevelString, accessModifierString, fieldType, promotedFieldName, fieldName, willSetFunction, onChangeFunction, didSetFunction);
					break;
				default:
					source.AppendLine($"public const string {fieldName}ProcessorError = \"Invalid processor {processor} for [AutoProperty] on {fieldName}\";");
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

						if (fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == ATTRIBUTE_NAME)) {
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

			OnChange = 1 << 0,
			WillSet = 1 << 1,
			DidSet = 1 << 2,

			OnChange_WillSet = OnChange | WillSet,
			OnChange_DidSet = OnChange | DidSet,
			WillSet_DidSet = WillSet | DidSet,
			OnChange_WillSet_DidSet = OnChange | WillSet | DidSet
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
{0} {1} {2} {3} {
	set => {4} = value;
}
		";

		private const string FORMAT_GET_SET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// </summary>
/// <seealso cref=""{4}""/>
{0} {1} {2} {3} {
	get => {4};
	set => {4} = value;
}
		";

		private const string FORMAT_ONCHANGE = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called if the underlying value changes when set.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
{0} {1} {2} {3} {
	get => {4};
	set {
		{2} oldValue = {4};
		{4} = value;
		if (!oldValue.Equals(value)) {
			{5}();
		}
	}
}
		";

		private const string FORMAT_WILLSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately before setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
{0} {1} {2} {3} {
	get => {4};
	set {
		{5}({4}, ref value);
		{4} = value;
	}
}
		";

		private const string FORMAT_DIDSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
{0} {1} {2} {3} {
	get => {4};
	set {
		{2} oldValue = {4};
		{4} = value;
		{5}(oldValue, value);
	}
}
	";

		private const string FORMAT_ONCHANGE_DIDSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called if the underlying value changes when set.
/// The function <see cref=""{6}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0} {1} {2} {3} {
	get => {4};
	set {
		{2} oldValue = {4};
		{4} = value;
		{5}();
		{6}(oldValue, value);
	}
}
	";

		private const string FORMAT_ONCHANGE_WILLSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{6}""/> will be called if the underlying value changes when set.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0} {1} {2} {3} {
	get => {4};
	set {
		{2} oldValue = {4};
		{5}(oldValue, ref value);
		{4} = value;
		{6}();
	}
}
	";

		private const string FORMAT_WILLSET_DIDSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{4}""/>.
/// The function <see cref=""{5}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{6}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{4}""/>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0} {1} {2} {3} {
	get => {4};
	set {
		{2} oldValue = {4};
		{5}(oldValue, ref value);
		{4} = value;
		{6}(oldValue, value);
	}
}
";

		private const string FORMAT_ONCHANGE_WILLSET_DIDSET = @"
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
{0} {1} {2} {3} {
	get => {4};
	set {
		{2} oldValue = {4};
		{5}(oldValue, ref value);
		{4} = value;
		if (!oldValue.Equals(value)) {
			{6}();
		}
		{7}(oldValue, value);
	}
}
";
	}
}