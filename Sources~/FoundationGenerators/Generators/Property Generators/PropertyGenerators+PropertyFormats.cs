namespace Foundation.Generators {
	internal sealed partial class PropertyGenerators {
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