using System;

namespace Foundation.Generators {
	[Flags]
	internal enum PropertyMutability {
		None = 0,
		Get = 1 << 0,
		Set = 1 << 1,
		GetSet = Get | Set
	}
}