using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Foundation.Generators {
	internal static partial class Extensions {
		// MARK: - Bool

		public static bool TryGetBoolValue(this TypedConstant value, out bool result) {
			if (!value.IsNull) {
				return bool.TryParse(value.Value.ToString(), out result);
			} else {
				result = default;
				return false;
			}
		}

		public static bool TryGetBoolValue(this KeyValuePair<string, TypedConstant> value, out bool result)
			=> value.Value.TryGetBoolValue(out result);

		public static bool? GetBoolValue(this TypedConstant value) {
			if (TryGetBoolValue(value, out bool result)) {
				return result;
			} else {
				return null;
			}
		}

		public static bool GetBoolValue(this TypedConstant value, bool defaultValue)
			=> value.GetBoolValue() ?? defaultValue;

		public static bool? GetBoolValue(this KeyValuePair<string, TypedConstant> value)
			=> value.Value.GetBoolValue();

		public static bool GetBoolValue(this KeyValuePair<string, TypedConstant> value, bool defaultValue)
			=> value.Value.GetBoolValue() ?? defaultValue;

		// MARK: - Int

		public static bool TryGetIntValue(this TypedConstant value, out int result) {
			if (!value.IsNull) {
				return int.TryParse(value.Value.ToString(), out result);
			} else {
				result = default;
				return false;
			}
		}

		public static bool TryGetIntValue(this KeyValuePair<string, TypedConstant> value, out int result)
			=> value.Value.TryGetIntValue(out result);

		public static int? GetIntValue(this TypedConstant value) {
			if (TryGetIntValue(value, out int result)) {
				return result;
			} else {
				return null;
			}
		}

		public static int GetIntValue(this TypedConstant value, int defaultValue)
			=> value.GetIntValue() ?? defaultValue;

		public static int? GetIntValue(this KeyValuePair<string, TypedConstant> value)
			=> value.Value.GetIntValue();

		public static int GetIntValue(this KeyValuePair<string, TypedConstant> value, int defaultValue)
			=> value.GetIntValue() ?? defaultValue;

		// MARK: - Float

		public static bool TryGetFloatValue(this TypedConstant value, out float result) {
			if (!value.IsNull) {
				return float.TryParse(value.Value.ToString(), out result);
			} else {
				result = default;
				return false;
			}
		}

		public static bool TryGetFloatValue(this KeyValuePair<string, TypedConstant> value, out float result)
			=> value.Value.TryGetFloatValue(out result);

		public static float? GetFloatValue(this TypedConstant value) {
			if (TryGetFloatValue(value, out float result)) {
				return result;
			} else {
				return null;
			}
		}

		public static float? GetFloatValue(this TypedConstant value, float defaultValue)
			=> value.GetFloatValue() ?? defaultValue;

		public static float? GetFloatValue(this KeyValuePair<string, TypedConstant> value)
			=> value.Value.GetFloatValue();

		public static float? GetFloatValue(this KeyValuePair<string, TypedConstant> value, float defaultValue)
			=> value.GetFloatValue() ?? defaultValue;

		// MARK: - String

		public static bool TryGetStringValue(this TypedConstant value, out string result) {
			if (!value.IsNull) {
				result = (string)value.Value;
				return result != null;
			} else {
				result = null;
				return false;
			}
		}

		public static bool TryGetStringValue(this KeyValuePair<string, TypedConstant> value, out string result)
			=> value.Value.TryGetStringValue(out result);

		public static string GetStringValue(this TypedConstant value) {
			if (TryGetStringValue(value, out string result)) {
				return result;
			} else {
				return null;
			}
		}

		public static string GetStringValue(this KeyValuePair<string, TypedConstant> value)
			=> value.Value.GetStringValue();

		// MARK: - Generic

		public static T? GetStructValue<T>(this TypedConstant value) where T : struct {
			if (typeof(T) == typeof(bool)) {
				return value.GetBoolValue() as T?;
			} else if (typeof(T) == typeof(int)) {
				return value.GetIntValue() as T?;
			} else if (typeof(T) == typeof(float)) {
				return value.GetFloatValue() as T?;
			} else {
				throw new NotImplementedException();
			}
		}

		public static T GetClassValue<T>(this TypedConstant value) where T : class {
			if (typeof(T) == typeof(string)) {
				return value.GetStringValue() as T;
			} else {
				throw new NotImplementedException();
			}
		}

		public static bool TryUnwrap<T>(this T? optional, out T value) where T : struct {
			value = optional.GetValueOrDefault();
			return optional.HasValue;
		}
	}
}