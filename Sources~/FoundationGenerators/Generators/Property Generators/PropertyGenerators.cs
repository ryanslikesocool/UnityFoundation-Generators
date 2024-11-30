using System.Linq;
using System.Collections.Immutable;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators {
	[Generator]
	internal sealed partial class PropertyGenerators : ISourceGenerator {
		private const string GET_ATTRIBUTE_NAME = "GetAttribute";
		private const string SET_ATTRIBUTE_NAME = "SetAttribute";
		private const string PROPERTY_NAME_ATTRIBUTE_NAME = "PropertyNameAttribute";
		private const string PROPERTY_ACCESSIBILITY_ATTRIBUTE_NAME = "PropertyAccessibilityAttribute";
		private const string PROPERTY_MODIFIER_ATTRIBUTE_NAME = "PropertyModifierAttribute";
		private const string ON_CHANGE_ATTRIBUTE_NAME = "OnChangeAttribute";
		private const string WILL_SET_ATTRIBUTE_NAME = "WillSetAttribute";
		private const string DID_SET_ATTRIBUTE_NAME = "DidSetAttribute";

		public void Initialize(GeneratorInitializationContext context) {
			AttributeFileGenerator.Register(ref context);
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
				string fileName = Extensions.FormatGeneratedCSFileName($"{group.Key.Name}_GeneratedProperties");
				context.AddSource(fileName, SourceText.From(classSource, Encoding.UTF8));
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
					// TODO: figure out a better way to log this to the user.  log to the Unity console or something?
					// maybe this can be revealed through an Analyzer
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
	}
}