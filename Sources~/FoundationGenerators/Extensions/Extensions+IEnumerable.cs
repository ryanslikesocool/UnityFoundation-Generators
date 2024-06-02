using System;
using System.Collections.Generic;

namespace Foundation.Generators {
	internal static partial class Extensions {
		public static int? FirstIndex<Element>(this IEnumerable<Element> collection, Func<Element, bool> predicate) {
			int index = 0;

			foreach (Element element in collection) {
				if (predicate(element)) {
					return index;
				}
				index++;
			}

			return null;
		}

		public static Element? FirstOrNull<Element>(this IEnumerable<Element> collection, Func<Element, bool> predicate) where Element : struct {
			foreach (Element element in collection) {
				if (predicate(element)) {
					return element;
				}
			}
			return null;
		}
	}
}