using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Foundation.Generators {
	[Generator]
	internal sealed partial class PropertyGenerators : ISourceGenerator {
		public void Initialize(GeneratorInitializationContext context) {
			AttributeFileGenerator.Register(ref context);
			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context) {
			if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver)) {
				return;
			}

			ContextSymbols contextSymbols = new ContextSymbols(context);

			foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> group in receiver.Fields.GroupBy<IFieldSymbol, INamedTypeSymbol>(f => f.ContainingType, SymbolEqualityComparer.Default)) {
				string classSource = ProcessContainingType(
					context: context,
					typeSymbol: group.Key,
					fields: group,
					contextSymbols: contextSymbols
				);
				string fileName = Extensions.FormatGeneratedCSFileName($"{group.Key.Name}_GeneratedProperties");
				context.AddSource(fileName, SourceText.From(classSource, Encoding.UTF8));
			}
		}

		private string ProcessContainingType(
			GeneratorExecutionContext context,
			INamedTypeSymbol typeSymbol,
			IEnumerable<IFieldSymbol> fields,
			ContextSymbols contextSymbols
		)
			=> SourceBuilder.Run(instance => {
				instance.ExtendType(typeSymbol, _ => {
					foreach (IFieldSymbol fieldSymbol in fields) {
						ProcessField(
							context: context,
							source: instance.source,
							fieldSymbol: fieldSymbol,
							contextSymbols: contextSymbols
						);
					}
				});
			});

		private void ProcessField(
			GeneratorExecutionContext context,
			StringBuilder source,
			IFieldSymbol fieldSymbol,
			ContextSymbols contextSymbols
		) {
			string fieldName = fieldSymbol.Name;
			ITypeSymbol fieldType = fieldSymbol.Type;

			ImmutableArray<AttributeData> attributes = fieldSymbol.GetAttributes();
			AttributeData getAttributeData = Extensions.TryReturnClass(() => attributes.GetAttribute(contextSymbols.getAttributeSymbol));
			AttributeData setAttributeData = Extensions.TryReturnClass(() => attributes.GetAttribute(contextSymbols.setAttributeSymbol));

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
				AttributeData propertyNameAttributeData = attributes.GetAttribute(contextSymbols.propertyNameAttributeSymbol);
				propertyName = propertyNameAttributeData.GetConstructorArgumentClass<string>(typeName: null);
			} catch {
				propertyName = Extensions.PromoteFieldName(fieldName);
			}

			AttributeData propertyAccessibilityAttributeData = Extensions.TryReturnClass(() => attributes.GetAttribute(contextSymbols.propertyAccessibilityAttributeSymbol));
			AttributeData propertyModifierAttributeData = Extensions.TryReturnClass(() => attributes.GetAttribute(contextSymbols.propertyModifierAttributeSymbol));

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

			// attributes
			string attributesString = string.Empty;
			{
				StringBuilder attributeStringBuilder = new StringBuilder();

				//INamedTypeSymbol obsoleteAttributeSymbol = contextSymbols.obsoleteAttributeSymbol;
				//foreach (AttributeData attributeData in attributes.Where(ad => ad.AttributeClass.Equals(obsoleteAttributeSymbol, SymbolEqualityComparer.Default))) {
				//	attributeStringBuilder.AppendLine($"// attr: {attributeData.AttributeClass}");
				//}

				// obsolete
				if (attributes.TryGetAttribute(contextSymbols.obsoleteAttributeSymbol, out AttributeData obsoleteAttributeData)) {
					string obsoleteContstructorArgument = obsoleteAttributeData.GetConstructorArgument(TypeName.STRING)?.GetStringValue();
					if (obsoleteContstructorArgument.IsNullOrEmptyOrWhiteSpace()) {
						attributeStringBuilder.AppendLine("[System.Obsolete]");
					} else {
						attributeStringBuilder.AppendLine($"[System.Obsolete(\"{obsoleteContstructorArgument}\")]");
					}
				}

				attributesString = attributeStringBuilder.ToString();
			}

			if (setAttributeData != null) {
				if (attributes.TryGetAttribute(contextSymbols.onChangeAttributeSymbol, out AttributeData onChangeAttributeData)) {
					onChangeFunction = onChangeAttributeData.GetConstructorArgument(TypeName.STRING)?.GetStringValue() ?? $"OnChange{propertyName}";
					processor |= ProcessorFlag.OnChange;
				}

				if (attributes.TryGetAttribute(contextSymbols.willSetAttributeSymbol, out AttributeData willSetAttributeData)) {
					willSetFunction = willSetAttributeData.GetConstructorArgument(TypeName.STRING)?.GetStringValue() ?? $"WillSet{propertyName}";
					processor |= ProcessorFlag.WillSet;
				}

				if (attributes.TryGetAttribute(contextSymbols.didSetAttributeSymbol, out AttributeData didSetAttributeData)) {
					didSetFunction = didSetAttributeData.GetConstructorArgument(TypeName.STRING)?.GetStringValue() ?? $"DidSet{propertyName}";
					processor |= ProcessorFlag.DidSet;
				}
			}

			//source.AppendLine($"public const string {fieldName}_PropertyGeneratorInfo = \"Creating property with flags {processor} for {fieldName} -> {accessibilityString} {modifierString} {fieldType} {propertyName}\";");

			switch (processor) {
				case ProcessorFlag.Get:
					source.AppendFormat(FORMAT_GET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName);
					break;
				case ProcessorFlag.Set:
					source.AppendFormat(FORMAT_SET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName);
					break;
				case ProcessorFlag.Get_Set:
					source.AppendFormat(FORMAT_GET_SET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName);
					break;
				case ProcessorFlag.Get_Set_OnChange:
					source.AppendFormat(FORMAT_GET_SET_ONCHANGE, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, onChangeFunction);
					break;
				case ProcessorFlag.Get_Set_WillSet:
					source.AppendFormat(FORMAT_GET_SET_WILLSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction);
					break;
				case ProcessorFlag.Get_Set_DidSet:
					source.AppendFormat(FORMAT_GET_SET_DIDSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, didSetFunction);
					break;
				case ProcessorFlag.Set_OnChange:
					source.AppendFormat(FORMAT_SET_ONCHANGE, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, onChangeFunction);
					break;
				case ProcessorFlag.Set_WillSet:
					source.AppendFormat(FORMAT_SET_WILLSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction);
					break;
				case ProcessorFlag.Set_DidSet:
					source.AppendFormat(FORMAT_SET_DIDSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, didSetFunction);
					break;
				case ProcessorFlag.Get_Set_OnChange_WillSet:
					source.AppendFormat(FORMAT_GET_SET_ONCHANGE_WILLSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, onChangeFunction);
					break;
				case ProcessorFlag.Get_Set_OnChange_DidSet:
					source.AppendFormat(FORMAT_GET_SET_ONCHANGE_DIDSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, onChangeFunction, didSetFunction);
					break;
				case ProcessorFlag.Get_Set_WillSet_DidSet:
					source.AppendFormat(FORMAT_GET_SET_WILLSET_DIDSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, didSetFunction);
					break;
				case ProcessorFlag.Get_Set_OnChange_WillSet_DidSet:
					source.AppendFormat(FORMAT_GET_SET_ONCHANGE_WILLSET_DIDSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, onChangeFunction, didSetFunction);
					break;
				case ProcessorFlag.Set_OnChange_WillSet:
					source.AppendFormat(FORMAT_SET_ONCHANGE_WILLSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, onChangeFunction);
					break;
				case ProcessorFlag.Set_OnChange_DidSet:
					source.AppendFormat(FORMAT_SET_ONCHANGE_DIDSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, onChangeFunction, didSetFunction);
					break;
				case ProcessorFlag.Set_WillSet_DidSet:
					source.AppendFormat(FORMAT_SET_WILLSET_DIDSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, didSetFunction);
					break;
				case ProcessorFlag.Set_OnChange_WillSet_DidSet:
					source.AppendFormat(FORMAT_SET_ONCHANGE_WILLSET_DIDSET, attributesString, accessibilityString, modifierString, fieldType, propertyName, fieldName, willSetFunction, onChangeFunction, didSetFunction);
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
							return displayString == TypeName.GET_ATTRIBUTE || displayString == TypeName.SET_ATTRIBUTE;
						})) {
							Fields.Add(fieldSymbol);
						}
					}
				}
			}
		}

		// MARK: - Supporting Data

		private sealed class ContextSymbols {
			public readonly GeneratorExecutionContext context;

			private readonly Dictionary<string, INamedTypeSymbol> attributeSymbols;

			public ContextSymbols(
				GeneratorExecutionContext context
			) {
				this.context = context;
				this.attributeSymbols = new Dictionary<string, INamedTypeSymbol>();
			}

			//public INamedTypeSymbol getAttributeSymbol => attributes[TypeName.GET_ATTRIBUTE];
			//public INamedTypeSymbol setAttributeSymbol => attributes[TypeName.SET_ATTRIBUTE];
			//public INamedTypeSymbol propertyNameAttributeSymbol => attributes[TypeName.PROPERTY_NAME_ATTRIBUTE];
			//public INamedTypeSymbol propertyAccessibilityAttributeSymbol => attributes[TypeName.PROPERTY_ACCESSIBILITY_ATTRIBUTE];
			//public INamedTypeSymbol propertyModifierAttributeSymbol => attributes[TypeName.PROPERTY_MODIFIER_ATTRIBUTE];
			//public INamedTypeSymbol onChangeAttributeSymbol => attributes[TypeName.ON_CHANGE_ATTRIBUTE];
			//public INamedTypeSymbol willSetAttributeSymbol => attributes[TypeName.WILL_SET_ATTRIBUTE];
			//public INamedTypeSymbol didSetAttributeSymbol => attributes[TypeName.DID_SET_ATTRIBUTE];
			//public INamedTypeSymbol obsoleteAttributeSymbol => attributes[TypeName.OBSOLETE_ATTRIBUTE];

			private INamedTypeSymbol GetOrFindAttributeSymbol(string attributeName) {
				if (attributeSymbols.TryGetValue(attributeName, out INamedTypeSymbol existingValue)) {
					return existingValue;
				} else {
					INamedTypeSymbol newValue = context.Compilation.GetTypeByMetadataName(attributeName);
					attributeSymbols.Add(attributeName, newValue);
					return newValue;
				}
			}

			public INamedTypeSymbol getAttributeSymbol => GetOrFindAttributeSymbol(TypeName.GET_ATTRIBUTE);
			public INamedTypeSymbol setAttributeSymbol => GetOrFindAttributeSymbol(TypeName.SET_ATTRIBUTE);
			public INamedTypeSymbol propertyNameAttributeSymbol => GetOrFindAttributeSymbol(TypeName.PROPERTY_NAME_ATTRIBUTE);
			public INamedTypeSymbol propertyAccessibilityAttributeSymbol => GetOrFindAttributeSymbol(TypeName.PROPERTY_ACCESSIBILITY_ATTRIBUTE);
			public INamedTypeSymbol propertyModifierAttributeSymbol => GetOrFindAttributeSymbol(TypeName.PROPERTY_MODIFIER_ATTRIBUTE);
			public INamedTypeSymbol onChangeAttributeSymbol => GetOrFindAttributeSymbol(TypeName.ON_CHANGE_ATTRIBUTE);
			public INamedTypeSymbol willSetAttributeSymbol => GetOrFindAttributeSymbol(TypeName.WILL_SET_ATTRIBUTE);
			public INamedTypeSymbol didSetAttributeSymbol => GetOrFindAttributeSymbol(TypeName.DID_SET_ATTRIBUTE);
			public INamedTypeSymbol obsoleteAttributeSymbol => GetOrFindAttributeSymbol(TypeName.OBSOLETE_ATTRIBUTE);
		}
	}
}