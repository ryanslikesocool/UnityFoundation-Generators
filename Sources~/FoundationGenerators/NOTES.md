# Unity
You must add the `RoslynAnalyzer` asset tag to the DLL.

# Namespaces and access
~~Use namespaces in generators to only generate code in that namespace.  This will break any usage outside of that namespace.~~
i have no idea how namespaces work with this.

Make generated objects internal to avoid naming conflicts, since the code will be generated for every assembly.

# Code Gen
You CANNOT use files created manually inside generated code, which really sucks.

Generate whole files with
```cs
context.RegisterForPostInitialization(i
	=> i.AddSource("filename_gen.cs", someFileTextString)
);
```
