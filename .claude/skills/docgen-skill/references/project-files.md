# Project files reference

Three files run a docgen project: `project.proj` (MSBuild), `project.xml` (docgen config), `doc.bat` (one-liner). When the project also auto-generates from source, two more files appear: `prepareproject.xml` and (for C#) `preparesettings.xml`. This document describes each in full.

The schema for `project.xml` is bundled with docgen at `source/parser/resource/project.xsd` inside the docgen install.

## project.xml — the docgen config

This is the *only* file docgen itself reads. It's an XML document in the namespace `http://www.gehtsoft.com/docgen/project`, with three top-level sections.

```xml
<?xml version="1.0" encoding="utf-8"?>
<dg:help-project xmlns:dg="http://www.gehtsoft.com/docgen/project">
  <dg:common> ... </dg:common>      <!-- optional: parser-wide defines -->
  <dg:source> ... </dg:source>      <!-- required: which .ds files to read -->
  <dg:output> ... </dg:output>      <!-- one or more: where output goes -->
</dg:help-project>
```

### `<dg:common>` — global defines

Holds `<dg:define name="..." value="..."/>` entries that affect parsing for all outputs. The most universally useful one:

```xml
<dg:define name="simplified-text-syntax" value="yes" />
```

Every Gehtsoft project sets this. It enables a more forgiving text parser. Leave it on unless you have a specific reason not to.

### `<dg:source>` — input files

Two mutually exclusive forms:

**Form A — list of files and folders** (the common case):

```xml
<dg:source>
  <dg:file name="src/index.ds" encoding="utf-8" />
  <dg:folder name="src/ns" encoding="utf-8" />
  <dg:folder name="src/raw" encoding="utf-8" />
</dg:source>
```

`<dg:folder>` defaults: `recursive="false"`, `mask="^.+\.ds$"`. The default mask matches every `.ds` file. The default encoding in the schema is `windows-1252` — always set `encoding="utf-8"` explicitly for new projects to avoid surprises.

**Order matters.** First definition of any `@key` wins. Hand-written files go first; auto-generated `src/raw/` goes last so it fills in voids without overwriting hand work. See SKILL.md "merge mechanism" for details.

**Form B — single pre-built XML model**:

```xml
<dg:source>
  <dg:xml-file name="model.xml" />
</dg:source>
```

Used when the model has been precomputed (e.g., by an earlier docgen pass with the `null` template). Rare in normal projects.

### `<dg:output>` — output renderers

You can have multiple `<dg:output>` blocks, one per output format. Each runs the same parsed model through a different XSLT template.

The two attributes that aren't optional:
- `template` — path to an XSLT file. Use the `%docgen%` token to reference the bundled templates: `%docgen%/template/html/main.xsl`, `%docgen%/template/markdown/main.xsl`, `%docgen%/template/null/null.xsl`, `%docgen%/template/xmldoc/main.xsl`. The token resolves to the docgen install directory.
- `file` — primary output path. For multi-file output (HTML), this is a placeholder; the template emits many files into the same directory.

Inside each `<dg:output>` you put `<dg:define>` entries the template reads. Common defines for the HTML template:

| Define                                             | Purpose                                                          |
|----------------------------------------------------|------------------------------------------------------------------|
| `text-language`                                    | UI language code (`en`, `ru`, …).                                |
| `help-title`                                       | Title shown in the header and TOC.                               |
| `chm-file`                                         | Output CHM filename (set if you want a `.chm` alongside HTML).   |
| `write-hhp`                                        | `yes` to emit HTML Help Workshop `.hhp` project file.            |
| `advanced-web-content`                             | `yes` for the modern HTML layout (default for new projects).     |
| `default-transform`                                | `yes` for the standard XSL transform pipeline.                   |
| `enable-highlighter`                               | `yes` to include the syntax highlighter assets.                  |
| `external-resources`                               | `yes` to link to CSS/JS as separate files.                       |
| `web-content-file-name`                            | Base filename for the entry page (`index` is the convention).    |
| `web-content-file-name-backward-compatibility`     | `yes` to also emit legacy filenames.                             |

For the `xmldoc` template (re-emits XML doc comments after enrichment):

| Define          | Purpose                                                 |
|-----------------|---------------------------------------------------------|
| `assembly`      | Assembly name to scope the XML doc to.                  |
| `namespace`     | Regex (XSLT 1.0 flavor) for namespace filtering.        |
| `write-summary` | `yes` to include `<summary>` blocks.                    |

A complete `project.xml` for a single-stage C# project named `MyLibrary`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<dg:help-project xmlns:dg="http://www.gehtsoft.com/docgen/project">
 <dg:common>
  <dg:define name="simplified-text-syntax" value="yes" />
 </dg:common>
 <dg:source>
  <dg:file name="src/index.ds" encoding="utf-8" />
  <dg:file name="src/ns/MyLibrary.ds" encoding="utf-8" />
  <dg:file name="src/ns/MyLibrary.Submodule.ds" encoding="utf-8" />
  <dg:folder name="src/raw" encoding="utf-8" />
 </dg:source>
 <dg:output template="%docgen%/template/html/main.xsl" file="dst/null-file" encoding="utf-8">
  <dg:define name="text-language" value="en" />
  <dg:define name="help-title" value="MyLibrary API Reference" />
  <dg:define name="advanced-web-content" value="yes" />
  <dg:define name="default-transform" value="yes" />
  <dg:define name="enable-highlighter" value="yes" />
  <dg:define name="external-resources" value="yes" />
  <dg:define name="web-content-file-name" value="index" />
 </dg:output>
</dg:help-project>
```

Listing `ns/` files individually (rather than as a folder) is intentional — it gives explicit ordering control. The `raw/` folder comes after, so auto-generated content fills voids only.

## project.proj — the MSBuild wrapper

The build wiring. Two targets are universal: `MakeDoc` (build the docs) and `CleanDoc` (delete output). Projects with auto-generation add `Scan` and `Raw`.

### Skeleton

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup>
  <TargetFramework>netstandard2.0</TargetFramework>
  <DocGenVersion>0.1.34</DocGenVersion>
 </PropertyGroup>

 <ItemGroup>
  <PackageReference Include="Gehtsoft.Build.DocGen" Version="$(DocGenVersion)" IncludeAssets="build" />
  <PackageReference Include="Gehtsoft.Build.ContentDelivery" Version="0.1.10" IncludeAssets="build" />
 </ItemGroup>

 <PropertyGroup>
  <DocTargetDir>$(MSBuildProjectDirectory)/dst</DocTargetDir>
 </PropertyGroup>

 <ItemGroup>
  <CustomFile Include="$(MSBuildProjectDirectory)/img/**/*.png;$(MSBuildProjectDirectory)/html/**/*.*" />
  <CurrentDocTargetDir Include="$(DocTargetDir)" />
 </ItemGroup>

 <Target Name="CleanDoc">
  <RemoveDir Directories="@(CurrentDocTargetDir)" />
 </Target>

 <Target Name="MakeDoc">
  <RemoveDir Directories="@(CurrentDocTargetDir)" />
  <MakeDir Directories="$(DocTargetDir)" />
  <DocGen Project="project.xml" />
  <Copy SourceFiles="@(CustomFile)" DestinationFolder="$(DocTargetDir)" />
  <ContentFromPackage Package="Gehtsoft.Build.DocGen" Source="Content/template/html/res"         Version="$(DocGenVersion)" Destination="$(DocTargetDir)/res" />
  <ContentFromPackage Package="Gehtsoft.Build.DocGen" Source="Content/template/html/highlighter" Version="$(DocGenVersion)" Destination="$(DocTargetDir)/highlighter" />
  <ContentFromPackage Package="Gehtsoft.Build.DocGen" Source="Content/template/html/menu"        Version="$(DocGenVersion)" Destination="$(DocTargetDir)/menu" />
  <ContentFromPackage Package="Gehtsoft.Build.DocGen" Source="Content/template/html/pageImages"  Version="$(DocGenVersion)" Destination="$(DocTargetDir)/pageImages" />
 </Target>
</Project>
```

### Why each piece is there

- **`<TargetFramework>netstandard2.0</TargetFramework>`** — required for the MSBuild SDK to load; the actual docgen task is .NET-version-agnostic.
- **`Gehtsoft.Build.DocGen` package** — provides the `<DocGen>` MSBuild task and bundles the templates under `%docgen%`.
- **`Gehtsoft.Build.ContentDelivery` package** — provides `<ContentFromPackage>` for copying template assets (CSS, JS, icons) into the output. Without these, the HTML pages render but look broken.
- **`<DocGenVersion>` property** — single source of truth for the package version. New projects should use this pattern rather than hardcoding the version on each `PackageReference` and `ContentFromPackage` line.
- **`<CustomFile>` item group** — extra files to copy into output (custom images, hand-written HTML pages). Adjust to your project.
- **`MakeDoc` clean-then-build sequence** — `RemoveDir` then `MakeDir` ensures stale files don't survive between builds. The order of `<DocGen>` then `<Copy>` then `<ContentFromPackage>` is required: docgen writes into the empty directory, then assets are layered on top.

### Adding auto-generation targets

For C#, add the assemblies you want to extract from and two targets:

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

`Mode` on `Asm2Xml` matches the assembly's runtime. As of docgen 0.1.34 the supported values are `net472`, `net50`, `net60`, `net70`, `net80`, `net90`, `net10.0`. Note the spelling: pre-10 modes have no dot (`net90`, not `net9.0`); 10 and later do (`net10.0`). Pick the one matching your DLL's `<TargetFramework>`; if your assembly targets `netstandard2.0`, use the lowest supported full-framework Mode that loads cleanly (typically `net60` or `net80`).

For doxygen, replace the `Scan` target with a doxygen invocation (typically via `<Exec Command="doxygen Doxyfile" />`), and `Raw` runs docgen with `doxygen2ds/main.xsl`. See `references/source-extraction.md`.

## prepareproject.xml — the auto-gen pass

A docgen project whose only purpose is to run the `cs2ds` or `doxygen2ds` XSLT and emit `.ds` files. It has the same shape as `project.xml` but a near-empty source (often a stub `null.ds`) and a single output that runs the conversion template.

C# version:

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

The `cs2ds` template reads `obj/raw.xml` (produced by `Asm2Xml`) and writes one `.ds` per type into `src/raw/`. The `default-group` define is the `@ingroup` value applied to the root namespace's auto-generated group block.

`null.ds` is a one-line stub that just exists so `<dg:source>` isn't empty:

```
@group
    @key=null
    @title=
@end
```

## preparesettings.xml — C# extraction settings

Used by `cs2ds`. Names assemblies and namespaces that should be stripped (treated as external/system types, not documented).

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

- `<assembly name xmldoc>` — name matches the assembly being scanned; `xmldoc` points at the XML doc file produced by the C# compiler (the one alongside the DLL).
- `<strip-namespace>` — types in these namespaces appear as plain names in cross-references but don't get pages of their own. Always include `System`, the system collection namespaces, and any third-party namespaces the user shouldn't have to host docs for.

If the user adds a new dependency that shows up as orphan cross-reference targets in the output, add a `<strip-namespace>` for it.

## doc.bat / prepare.bat — convenience wrappers

```batch
@echo off
dotnet build project.proj /t:MakeDoc
```

```batch
@echo off
dotnet build project.proj /t:Scan,Raw
```

Two targets in `prepare.bat` because they're a pipeline: `Scan` produces `obj/raw.xml`, `Raw` consumes it. They run in the order listed.

A `clean.bat` is also common:

```batch
@echo off
dotnet build project.proj /t:CleanDoc
```

## .gitignore for doc/

The standard set:

```
dst/
obj/
bin/
```

`src/raw/` is *not* gitignored — auto-generated files are checked in. This is intentional: it means the documentation build doesn't require the source assemblies to be present. Re-running `prepare.bat` updates them.

## Common pitfalls

- **Forgetting `IncludeAssets="build"`** on the package references — without it, the MSBuild tasks aren't loaded and the build silently does nothing useful.
- **Wrong `Mode` on `Asm2Xml`** — extraction succeeds but member signatures may end up garbled.
- **Missing `xmldoc` path** in `preparesettings.xml` — the prepare step runs but produces empty `@brief` and `@param` content because there's no XML doc to merge.
- **Source order putting `raw/` before `ns/`** — auto-generated content "wins" and your hand-written namespace pages get silently dropped. Always put hand-written first.
- **Forgetting to re-run prepare after source changes** — `MakeDoc` does not invoke `Raw`. If a method was renamed in source, `src/raw/` still has the old name until you re-prepare.
