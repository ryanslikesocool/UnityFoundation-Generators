using System;

namespace Foundation.Generators {
	internal sealed partial class PropertyGenerators {
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
	}
}