# Future

Unity is a bit slow to update language support, but that doesn't prevent me from having ideas for generators that incorporate recent C# language features!

## Better `[GetComponent]`
The current `[GetComponent]` attribute is applied to a field and requires an additional method call (usually in `Awake()`) to actually perform the `GetComponent<T>()`.
C# 13 adds
[`partial` properties](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#more-partial-members)
and the
[`field` keyword](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#the-field-keyword)
which can streamline the generator's usage.

```cs
// MyBehaviour.cs

using UnityEngine;

// This is assuming that Unity keeps up with language features
// and adds generic attributes introduced in C# 11.
[RequireComponent<Canvas>()]
internal partial class MyBehaviour : MonoBehaviour {
	[GetComponent] public partial Canvas MyCanvasReference { get; }
}

```
```cs
// MyBehaviour_gen.cs

using UnityEngine;

internal partial class MyBehaviour {
	public partial Canvas MyCanvasReference {
		get {
			// Ideally, this would be further condensed using the null-coalescing assignment operator,
			// but Unity overrides the equality operators for `UnityEngine.Object`.
			if (field == null) {
				field = GetComponent<Canvas>();
			}
			return field;
		}
	}
}
```

There is a(n unlikely) potential issue that could arise in regards to *when* the component is accessed.
The new implementation would access the component lazily, instead of when the developer calls `InitializeComponents();`
I can't actually think of an example where it actually causes a problem, but it's something to think about.
This could always be remedied by offering a `bool Lazy` property to the attribute and using the old implementation if `false`.