using Microsoft.CodeAnalysis;
using System;

namespace Foundation.Generators {
	internal static partial class Extensions {
		public static bool TryGetBoolValue(this TypedConstant value, out bool result) {
			if (!value.IsNull) {
				return bool.TryParse(value.Value.ToString(), out result);
			} else {
				result = default;
				return false;
			}
		}

		public static bool? GetBoolValue(this TypedConstant value) {
			if (TryGetBoolValue(value, out bool result)) {
				return result;
			} else {
				return null;
			}
		}

		public static bool TryGetIntValue(this TypedConstant value, out int result) {
			if (!value.IsNull) {
				return int.TryParse(value.Value.ToString(), out result);
			} else {
				result = default;
				return false;
			}
		}

		public static int? GetIntValue(this TypedConstant value) {
			if (TryGetIntValue(value, out int result)) {
				return result;
			} else {
				return null;
			}
		}

		public static bool TryGetFloatValue(this TypedConstant value, out float result) {
			if (!value.IsNull) {
				return float.TryParse(value.Value.ToString(), out result);
			} else {
				result = default;
				return false;
			}
		}

		public static float? GetFloatValue(this TypedConstant value) {
			if (TryGetFloatValue(value, out float result)) {
				return result;
			} else {
				return null;
			}
		}

		public static bool TryGetStringValue(this TypedConstant value, out string result) {
			if (!value.IsNull) {
				result = (string)value.Value;
				return result != null;
			} else {
				result = null;
				return false;
			}
		}

		public static string GetStringValue(this TypedConstant value) {
			if (TryGetStringValue(value, out string result)) {
				return result;
			} else {
				return null;
			}
		}

		// MARK: - Generic

		public static bool TryGetStructValue<T>(this TypedConstant value, out T result) where T : struct {
			T? optionalResult = value.GetStructValue<T>();
			if (optionalResult.HasValue) {
				result = optionalResult.Value;
				return true;
			} else {
				result = default;
				return false;
			}
		}

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

		public static bool TryGetClassValue<T>(this TypedConstant value, out T result) where T : class {
			result = value.GetClassValue<T>();
			return result != null;
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