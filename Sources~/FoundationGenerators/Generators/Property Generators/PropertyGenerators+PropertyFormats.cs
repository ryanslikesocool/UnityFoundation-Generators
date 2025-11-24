namespace Foundation.Generators {
	internal sealed partial class PropertyGenerators {
		private const string FORMAT_GET = @"
/// <summary>
/// An auto-generated property providing read-only access to <see cref=""{5}""/>.
/// </summary>
/// <seealso cref=""{5}""/>
{0}
{1} {2} {3} {4} => {5};
		";

		private const string FORMAT_SET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{5}""/>.
/// </summary>
/// <seealso cref=""{5}""/>
{0}
{1} {2} {3} {4} {{
	set => {5} = value;
}}
		";

		private const string FORMAT_GET_SET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{5}""/>.
/// </summary>
/// <seealso cref=""{5}""/>
{0}
{1} {2} {3} {4} {{
	get => {5};
	set => {5} = value;
}}
		";

		private const string FORMAT_SET_ONCHANGE = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called if the underlying value changes when set.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0}
{1} {2} {3} {4} {{
	set {{
		{3} oldValue = {5};
		{5} = value;
		if (!oldValue.Equals(value)) {{
			{6}();
		}}
	}}
}}
		";

		private const string FORMAT_GET_SET_ONCHANGE = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called if the underlying value changes when set.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0}
{1} {2} {3} {4} {{
	get => {5};
	set {{
		{3} oldValue = {5};
		{5} = value;
		if (!oldValue.Equals(value)) {{
			{6}();
		}}
	}}
}}
		";

		private const string FORMAT_SET_WILLSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called immediately before setting the underlying value.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0}
{1} {2} {3} {4} {{
	set {{
		{6}({5}, ref value);
		{5} = value;
	}}
}}
		";

		private const string FORMAT_GET_SET_WILLSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called immediately before setting the underlying value.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0}
{1} {2} {3} {4} {{
	get => {5};
	set {{
		{6}({5}, ref value);
		{5} = value;
	}}
}}
		";

		private const string FORMAT_SET_DIDSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0}
{1} {2} {3} {4} {{
	set {{
		{3} oldValue = {5};
		{5} = value;
		{6}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_GET_SET_DIDSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
{0}
{1} {2} {3} {4} {{
	get => {5};
	set {{
		{3} oldValue = {5};
		{5} = value;
		{6}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_SET_ONCHANGE_DIDSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called if the underlying value changes when set.
/// The function <see cref=""{7}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
/// <seealso cref=""{7}""/>
{0}
{1} {2} {3} {4} {{
	set {{
		{3} oldValue = {5};
		{5} = value;
		{6}();
		{7}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_GET_SET_ONCHANGE_DIDSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called if the underlying value changes when set.
/// The function <see cref=""{7}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
/// <seealso cref=""{7}""/>
{0}
{1} {2} {3} {4} {{
	get => {5};
	set {{
		{3} oldValue = {5};
		{5} = value;
		if (!oldValue.Equals(value)) {{
			{6}();
		}}
		{7}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_SET_ONCHANGE_WILLSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{7}""/> will be called if the underlying value changes when set.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
/// <seealso cref=""{7}""/>
{0}
{1} {2} {3} {4} {{
	set {{
		{3} oldValue = {5};
		{6}(oldValue, ref value);
		{5} = value;
		if (!oldValue.Equals(value)) {{
			{7}();
		}}
	}}
}}
		";

		private const string FORMAT_GET_SET_ONCHANGE_WILLSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{7}""/> will be called if the underlying value changes when set.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
/// <seealso cref=""{7}""/>
{0}
{1} {2} {3} {4} {{
	get => {5};
	set {{
		{3} oldValue = {5};
		{6}(oldValue, ref value);
		{5} = value;
		if (!oldValue.Equals(value)) {{
			{7}();
		}}
	}}
}}
		";

		private const string FORMAT_SET_WILLSET_DIDSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{7}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
/// <seealso cref=""{7}""/>
{0}
{1} {2} {3} {4} {{
	set {{
		{3} oldValue = {5};
		{6}(oldValue, ref value);
		{5} = value;
		{7}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_GET_SET_WILLSET_DIDSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{7}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
/// <seealso cref=""{7}""/>
{0}
{1} {2} {3} {4} {{
	get => {5};
	set {{
		{3} oldValue = {5};
		{6}(oldValue, ref value);
		{5} = value;
		{7}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_SET_ONCHANGE_WILLSET_DIDSET = @"
/// <summary>
/// An auto-generated property providing write-only access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{7}""/> will be called if the underlying value changes when set.
/// The function <see cref=""{8}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
/// <seealso cref=""{7}""/>
/// <seealso cref=""{8}""/>
{0}
{1} {2} {3} {4} {{
	set {{
		{3} oldValue = {5};
		{6}(oldValue, ref value);
		{5} = value;
		if (!oldValue.Equals(value)) {{
			{7}();
		}}
		{8}(oldValue, value);
	}}
}}
		";

		private const string FORMAT_GET_SET_ONCHANGE_WILLSET_DIDSET = @"
/// <summary>
/// An auto-generated property providing read and write access to <see cref=""{5}""/>.
/// The function <see cref=""{6}""/> will be called immediately before setting the underlying value.
/// The function <see cref=""{7}""/> will be called if the underlying value changes when set.
/// The function <see cref=""{8}""/> will be called immediately after setting the underlying value.
/// </summary>
/// <seealso cref=""{5}""/>
/// <seealso cref=""{6}""/>
/// <seealso cref=""{7}""/>
/// <seealso cref=""{8}""/>
{0}
{1} {2} {3} {4} {{
	get => {5};
	set {{
		{3} oldValue = {5};
		{6}(oldValue, ref value);
		{5} = value;
		if (!oldValue.Equals(value)) {{
			{7}();
		}}
		{8}(oldValue, value);
	}}
}}
		";
	}
}