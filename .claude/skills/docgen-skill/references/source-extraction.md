# Source extraction

How docgen turns C#, C++, or Java source code into `.ds` files that get merged with hand-written content.

## The shape (it's the same for all three)

```
source code  --[extractor]-->  raw XML  --[XSLT template]-->  .ds files
```

| Language | Extractor                         | XSLT template                                 |
|----------|-----------------------------------|-----------------------------------------------|
| C#       | `Asm2Xml.exe` (Mono.Cecil)        | `%docgen%/template/cs2ds/main.xsl`            |
| C++      | `doxygen` (with `GENERATE_XML=YES`) | `%docgen%/template/doxygen2ds/main.xsl`     |
| Java     | `doxygen` (with `GENERATE_XML=YES`) | `%docgen%/template/doxygen2ds/main.xsl`     |

Both pipelines run as a `prepareproject.xml` — a docgen project whose only output is to invoke the conversion XSLT and write `.ds` files. The MSBuild project orchestrates: extractor first, then docgen on the prepare project.

The output `.ds` files then sit alongside hand-written ones, and the main `project.xml` reads them all in `<dg:source>` order. Hand-written first, auto-generated last (see SKILL.md "merge mechanism").

## C# pipeline

### What you write in source

Standard C# XML doc comments. Nothing custom. The richer the comments, the richer the generated `.ds`.

```csharp
/// <summary>
/// Calculates the trajectory for a given launch angle.
/// </summary>
/// <param name="angle">Launch angle in radians.</param>
/// <param name="velocity">Initial velocity in m/s.</param>
/// <returns>Populated trajectory result.</returns>
/// <exception cref="ArgumentOutOfRangeException">
/// Thrown if angle is outside [-π/2, π/2].
/// </exception>
/// <example>
/// <code>
/// var calc = new TrajectoryCalculator();
/// var t = calc.Calculate(0.05, 800.0);
/// </code>
/// </example>
public TrajectoryResult Calculate(double angle, double velocity) { ... }
```

Recognized tags: `<summary>`, `<remarks>`, `<param>`, `<paramref>`, `<typeparam>`, `<typeparamref>`, `<returns>`, `<exception cref>`, `<see cref>`, `<seealso cref>`, `<code>`, `<example>`, `<value>`, `<para>`, `<list>`, `<c>`. Standard set; nothing custom. Two caveats that bite in practice: `<remarks>` is parsed but **emitted nowhere** (its content is silently dropped), and the formatting tag `<c>` renders poorly — see "Authoring C# XML comments for docgen" below for what to write instead.

`cref` arguments take a member identifier with overload signature (`Foo(int)`, `Foo`, `Bar.Baz<T>`) — never a call expression like `Foo(5)`. A bad `cref` extracts as a literal string and won't render as a link.

### Authoring C# XML comments for docgen

How `cs2ds` transforms a `///` comment is not obvious, and the transform has surprises that are invisible in the `.cs` source. Author with these rules so the generated `@brief`, formatting, and prose come out right. After regenerating `src/raw/`, verify against the *generated `.ds`* (and ideally the rendered HTML), not just the comment source.

**1. `<summary>` → `@brief`: first paragraph is the brief, plain text only.**

`cs2ds` maps the **whole `<summary>`** to `@brief`. Paragraphs inside `<summary>` are separated by a **blank `///` line**: the first paragraph becomes the one-line `@brief` (used in indexes, tooltips, member lists), and subsequent paragraphs become the detailed description body. So a multi-line summary with no blank-line break dumps *all* of it into `@brief` — noise everywhere `@brief` is shown.

```csharp
/// <summary>
/// One short line — becomes @brief.
///
/// First detail paragraph. May use [c]...[/c] and &lt;see cref&gt; freely.
///
/// Second detail paragraph.
/// </summary>
```

- Keep the **first line plain text** — no `[c]`, no `<see cref>`. Inline markup in the first paragraph gets pulled onto its own line and truncates the `@brief` mid-sentence. A `<see cref>` to a type outside the doc model (a BCL type like `System.Linq.Expressions.LambdaExpression`, or `bool`) renders **empty**, so the brief silently loses words. Put markup and cross-references in the detail paragraphs only.
- **Do not use `<remarks>`** for docgen targets — `cs2ds` emits it nowhere, so the content vanishes with no warning. Fold long-form detail into summary paragraphs instead.

**2. Use docgen BBCode for *formatting*, XML for *references*.** The C# XML formatting tags render poorly: `<c>...</c>` is emitted as a `[c]...[/c]` block on its own indented line (breaking the sentence across three lines), and `<i>`/`<b>` emphasis is dropped entirely. Write the docgen BBCode directly in the comment instead — it passes through `cs2ds` verbatim, stays inline, and is plain text to the C# compiler (no doc warnings):

| Instead of    | Write         |
|---------------|---------------|
| `<c>code</c>` | `[c]code[/c]` |
| `<i>text</i>` | `[i]text[/i]` |
| `<b>text</b>` | `[b]text[/b]` |

Keep angle brackets *inside* BBCode as XML entities so the comment stays valid XML: `[c]&lt;script&gt;[/c]`, not `[c]<script>[/c]`. `cs2ds` decodes the entities, so the output is `[c]<script>[/c]`. **Keep `<see cref>` / `<paramref>` / `<typeparamref>` as XML** — those are *references*, not formatting, and `cs2ds` turns `<see cref>` into a desirable `[clink=...]...[/clink]` link. Only the formatting tags become BBCode.

**3. Never start a `///` line with `- ` or `* `.** `cs2ds` preserves your source line breaks, and docgen treats a line whose first non-whitespace character is `-` or `*` (plus a space) as a list bullet. A parenthetical dash that happens to wrap to the start of a line —

```csharp
/// ... For deeper control
/// - notably how a parameter renders - derive a subclass and override the
/// [c]protected virtual[/c] emit methods.
```

— renders as a stray bullet dropped into the middle of the paragraph. Keep the dash on the **end of the previous line** (`... For deeper control -` then `notably ...`) or reword. (Same gotcha applies to hand-written `.ds` — see `ds-format.md`.)

The C# compiler must be set to emit the XML doc file. In the library's `.csproj`:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
</PropertyGroup>
```

The XML doc file path needs to be reachable from `preparesettings.xml`.

### What the prepare step looks like

Three artifacts in `doc/`:

**1. `prepareproject.xml`** — the docgen project that runs the C# converter:

```xml
<?xml version="1.0" encoding="utf-8"?>
<dg:help-project xmlns:dg="http://www.gehtsoft.com/docgen/project">
 <dg:source>
  <dg:file name="src/null.ds" encoding="utf-8" />
 </dg:source>
 <dg:output template="%docgen%/template/cs2ds/main.xsl" file="src/raw/null-file" encoding="utf-8">
  <dg:define name="settings" value="preparesettings.xml" />
  <dg:define name="source" value="obj/raw.xml" />
  <dg:define name="default-group" value="main" />
 </dg:output>
</dg:help-project>
```

Defines:
- `settings` — path to `preparesettings.xml`.
- `source` — path to the XML output of `Asm2Xml`.
- `default-group` — `@ingroup` for the root namespace's group block. Use `main` (or whatever the root group's `@key` is in `index.ds`).

**2. `preparesettings.xml`** — extraction tuning:

```xml
<?xml version="1.0" encoding="utf-8"?>
<settings>
 <assembly name="MyLibrary" xmldoc="../MyLibrary/bin/Debug/netstandard2.0/MyLibrary.xml" />

 <strip-namespace name="System" />
 <strip-namespace name="System.Collections" />
 <strip-namespace name="System.Collections.Generic" />
 <strip-namespace name="System.Linq" />
</settings>
```

- `<assembly name xmldoc>` — `name` matches the assembly being processed (without `.dll`); `xmldoc` is the path to the C# compiler's XML doc output, used to enrich extracted types with summaries/params.
- `<strip-namespace>` — types in these namespaces are referenced (so `<see cref>` resolves) but don't get pages of their own. Always strip `System.*` and any third-party namespaces. Also strip your own namespaces if they're documented in `ns/` already, to prevent the auto-gen from creating a duplicate group with no description.

**3. `MakeDoc`-companion targets in `project.proj`**:

```xml
<ItemGroup>
  <DocSource Include="$(MSBuildProjectDirectory)/../MyLibrary/bin/Debug/netstandard2.0/MyLibrary.dll" />
</ItemGroup>

<PropertyGroup>
  <RawDir>$(MSBuildProjectDirectory)/src/raw</RawDir>
</PropertyGroup>

<ItemGroup>
  <CurrentRawDir Include="$(RawDir)" />
</ItemGroup>

<Target Name="Scan">
  <Asm2Xml Assemblies="@(DocSource)" OutputXml="obj/raw.xml" Mode="net90" />
</Target>

<Target Name="Raw">
  <RemoveDir Directories="@(CurrentRawDir)" />
  <MakeDir Directories="$(RawDir)" />
  <DocGen Project="prepareproject.xml" />
</Target>
```

`Asm2Xml` parameters:
- `Assemblies` — one or more DLLs to scan.
- `OutputXml` — output file (read by `prepareproject.xml`'s `source` define).
- `Mode` — runtime profile. Supported as of docgen 0.1.34: `net472`, `net50`, `net60`, `net70`, `net80`, `net90`, `net10.0`. Note the spelling: pre-10 modes have no dot (`net90`, not `net9.0`); 10 and later do (`net10.0`). Pick the one matching the DLL's `<TargetFramework>`. The wrong mode produces garbled signatures.

A `prepare.bat` runs the pair:

```batch
@echo off
dotnet build project.proj /t:Scan,Raw
```

### What gets generated

`Asm2Xml` produces an XML model (`obj/raw.xml`) containing every type, member, parameter, and the merged XML doc commentary. The `cs2ds` template iterates that model and writes one `.ds` file per type:

```
src/raw/
├── MyLibrary.ds                       # @group for the namespace
├── MyLibrary.Calculator.ds            # @class
├── MyLibrary.CalculatorOptions.ds     # @class
└── ...
```

A generated class has a structure like:

```
@class
    @name=Calculator
    @key=MyLibrary.Calculator
    @ingroup=MyLibrary
    @sig=T:MyLibrary.Calculator
    @type=class
    @parent=object
    @brief=Performs calculations.

    @member
        @type=constructor
        @name=Calculator
        @key=.ctor.Rf5
        @sig=M:MyLibrary.Calculator.#ctor(System.Double,System.Double)
        @visibility=public
        @scope=instance
        @brief=Initializes a new instance.
        ...
    @end
    ...
@end
```

The CRC suffix on `@key` (e.g., `.ctor.Rf5`) disambiguates overloads. Don't try to predict it for hand-written members; if you want to override an auto-generated member, copy the key from the generated file.

## C++ / Java pipeline (doxygen)

### What you write in source

Doxygen-style comments. The minimum is a brief sentence; the richer the comment, the richer the output.

C++ Javadoc-style:

```cpp
/**
 * @brief Computes the trajectory for a given launch angle.
 *
 * @param angle    Launch angle in radians.
 * @param velocity Initial velocity in m/s.
 * @return Populated trajectory result.
 *
 * @throws std::invalid_argument if angle is out of range.
 *
 * @code
 * Calculator c;
 * auto t = c.compute(0.05, 800.0);
 * @endcode
 */
TrajectoryResult compute(double angle, double velocity);
```

Java with standard JavaDoc:

```java
/**
 * Computes the trajectory for a given launch angle.
 *
 * @param angle    Launch angle in radians.
 * @param velocity Initial velocity in m/s.
 * @return Populated trajectory result.
 * @throws IllegalArgumentException if angle is out of range.
 */
public TrajectoryResult compute(double angle, double velocity) { ... }
```

Doxygen-recognized tags: `@brief`, `@param`, `@return`, `@throws` / `@exception`, `@see`, `@code` / `@endcode`, `@note`, `@warning`, `@deprecated`, `@since`, etc. Both `@`-style and `\`-style are accepted.

### Doxyfile

Doxygen reads a `Doxyfile`. Generate a fresh one with `doxygen -g` and adjust. The settings that matter for docgen:

```
PROJECT_NAME           = MyLibrary
INPUT                  = ../../src/include \
                         ../../src/core/include
RECURSIVE              = YES
FILE_PATTERNS          = *.h *.hpp *.cpp

OUTPUT_DIRECTORY       = ./doxygen
GENERATE_HTML          = NO
GENERATE_LATEX         = NO
GENERATE_XML           = YES
XML_OUTPUT             = xml
JAVADOC_AUTOBRIEF      = NO
```

The crucial part: `GENERATE_XML = YES`, with `OUTPUT_DIRECTORY` and `XML_OUTPUT` aligned with the path you give to `prepareproject.xml`'s `xml-path` define. Turn off HTML and LaTeX — they're noise.

### prepare project for doxygen

```xml
<?xml version="1.0" ?>
<dg:help-project xmlns:dg="http://www.gehtsoft.com/docgen/project">
    <dg:source>
        <dg:file name="./src/null.ds" encoding="utf-8" />
    </dg:source>
    <dg:output template="%docgen%/template/doxygen2ds/main.xsl" file="./null-file" encoding="utf-8">
        <dg:define name="xml-path" value="./doxygen/xml/" />
        <dg:define name="ds-path"  value="./dst/" />
        <dg:define name="codepage" value="65001" />
        <dg:define name="group"    value="cpp" />
        <dg:define name="language" value="cpp" />
        <dg:define name="file-list" value="files.xml" />
    </dg:output>
</dg:help-project>
```

Defines for `doxygen2ds`:

| Define              | Purpose                                                                          |
|---------------------|----------------------------------------------------------------------------------|
| `xml-path`          | Where doxygen wrote its XML.                                                     |
| `ds-path`           | Where to write generated `.ds` files.                                            |
| `codepage`          | Output encoding (`65001` = UTF-8, `1252` = Windows-1252).                        |
| `group`             | `@ingroup` for the top-level namespace group (point at the group in `index.ds`).|
| `language`          | `cpp` or `java`. Drives signature formatting.                                    |
| `divisor`           | Separator between class name and member name. Default `.` (Java); use `::` if you want C++ style. |
| `exclude-namespace` | Regex of namespaces to skip. For Java, exclude `java:.*` to drop JDK types.     |
| `file-list`         | Optional output XML listing files for downstream processing.                     |

### MSBuild orchestration

```xml
<PropertyGroup>
  <DoxyDir>$(MSBuildProjectDirectory)\doxygen</DoxyDir>
  <DestDir>$(MSBuildProjectDirectory)\dst</DestDir>
</PropertyGroup>

<ItemGroup>
  <CurrentDoxyDir Include="$(DoxyDir)" />
  <CurrentDestDir Include="$(DestDir)" />
</ItemGroup>

<Target Name="ClearDoc">
  <RemoveDir Directories="@(CurrentDoxyDir)" />
  <RemoveDir Directories="@(CurrentDestDir)" />
</Target>

<Target Name="Doxygen">
  <Exec Command="doxygen.exe" />
  <Exec Command="del $(DoxyDir)\xml\*8h.xml" />
  <Exec Command="del $(DoxyDir)\xml\*8cpp.xml" />
  <Exec Command="del $(DoxyDir)\xml\dir_*.xml" />
</Target>

<Target Name="Prepare">
  <RemoveDir Directories="@(CurrentDestDir)" />
  <MakeDir Directories="$(DestDir)" />
  <DocGen Project="prepareproject.xml" />
</Target>
```

The `del *8h.xml` / `del *8cpp.xml` / `del dir_*.xml` lines remove doxygen's per-file and per-directory XMLs which are noise — `doxygen2ds` only wants the per-namespace and per-class files. Without these deletions you'll get spurious extra `.ds` files.

`prepare.bat`:

```batch
@echo off
dotnet build project.proj /t:Doxygen,Prepare
```

## Multi-language projects

The same docs can cover C++, C#, Java, and JavaScript in one HTML output. The pattern:

```
doc/
├── project.proj       # main build
├── project.xml        # main config: lists src/cpp/, src/cs/, src/java/, src/js/
├── src/
│   ├── index.ds       # top-level group "main" with per-language subgroups
│   ├── cpp/           # hand-written C++ overviews + auto-generated .ds copied from prepare/cpp/dst/
│   ├── cs/            # same for C#
│   ├── java/          # same for Java
│   └── js/            # hand-written only (no extractor for JS)
└── prepare/
    ├── cpp/           # own project.proj + Doxyfile + dst/
    ├── cs/            # own project.proj + preparesettings.xml + dst/
    └── java/          # own project.proj + Doxyfile + dst/
```

Each `prepare/<lang>/` is independent. To refresh one language's auto-generated docs:

```
cd doc/prepare/cpp && prepare.bat
```

Then either copy `dst/*.ds` into `doc/src/cpp/` (the typical workflow) or change the main `project.xml` to reference the `prepare/<lang>/dst/` directory directly.

In `index.ds` the layout is one `@group key=cpp ingroup=main` per language:

```
@group
    @key=cpp
    @title=C++ API
    @ingroup=main
    @brief=Native client library for Win/Linux/macOS/iOS/Android.
@end

@group
    @key=java
    @title=Java API
    @ingroup=main
    @brief=Java client library.
@end
```

Per-language auto-generated namespace groups then have `@ingroup=cpp` or `@ingroup=java`, slotting under the right top-level branch.

## When to use which assembly pattern

**Single-stage.** The main `project.xml` references both `src/ns/` and `src/raw/`. The `Scan` and `Raw` targets live in the same `project.proj` as `MakeDoc`. Auto-generated files are committed into the repo. Refreshing requires running `prepare.bat` separately.

- Pro: simpler, one `MakeDoc` target builds everything.
- Pro: docs build without source code present (CI-friendly).
- Con: running `MakeDoc` does *not* refresh `src/raw/`; you must remember to run prepare first when source changed.

**Two-stage.** The prepare runs in a sub-project under `doc/prepare/` with its own `project.proj`. The main build doesn't include `prepare/.../dst/` directly — generated files are copied into `src/raw/` (or wherever) on refresh.

- Pro: clearer separation; prepare and main builds can use different docgen versions if needed.
- Pro: scales naturally to multiple languages (each `prepare/<lang>/`).
- Con: more moving parts.

For new single-language projects, prefer **single-stage**. For multi-language projects, **two-stage** is cleaner. Document the "rerun prepare.bat after source changes" step in the project README.

## Common gotchas

- **`Mode` mismatch on `Asm2Xml`**. The DLL's runtime must match the Mode. Use `net80` for `net8.0`, `net90` for `net9.0`, `net10.0` for `net10.0`, etc. Note the spelling boundary: modes for runtimes before .NET 10 omit the dot (`net8.0` → `net80`, `net9.0` → `net90`); from .NET 10 the dot is kept (`net10.0`, not `net100`). For `netstandard2.0` libraries, pick a full-framework Mode that loads cleanly — `net80` is a safe default.
- **Missing `<GenerateDocumentationFile>true</GenerateDocumentationFile>`** in the library's `.csproj`. `Asm2Xml` runs without error but every `@brief` is empty.
- **Wrong `xmldoc` path** in `preparesettings.xml`. Points to the relative location of the C# XML doc file from the prepare project's working directory, *not* from the doc root. Failure mode: silent — extraction succeeds, briefs are missing.
- **Doxygen output not deleted between runs.** Stale XML produces stale `.ds`. Always include the `RemoveDir` step on `dst/` and consider clearing `doxygen/xml/` too.
- **Forgetting to add `<strip-namespace>` for new dependencies**. They show up as orphan link targets in the rendered docs.
- **`raw/` listed before `ns/` in `<dg:source>`**. Auto-generated content shadows your hand-written namespace pages silently.
- **Generated members losing their CRC suffix** if you rename a hand-written member to match. The auto-generated key has a CRC; if you write `@key=Frobnicate` in `ns/` but the generated key is `@key=Frobnicate.A1B2C3D4`, they're different keys and the merge doesn't happen. Open the generated file to read off the exact key before overriding.

## Source-of-truth pointers

Inside the docgen install (resolvable as `%docgen%` from project files):

- `cs2ds` template entry point: `template/cs2ds/main.xsl`
- `doxygen2ds` template entry point: `template/doxygen2ds/main.xsl`
- `Asm2Xml` MSBuild task: bundled with the `Gehtsoft.Build.DocGen` NuGet package.
- Project schema: `source/parser/resource/project.xsd`.
