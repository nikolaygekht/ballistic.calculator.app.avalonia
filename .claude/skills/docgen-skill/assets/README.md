# Asset templates

Five scaffold trees that match the docgen workflows. Copy the appropriate one into the user's repository at `doc/`, then replace the `REPLACE_WITH_*` placeholders.

## Picking a scaffold

| If the project is...                                  | Use                       |
|-------------------------------------------------------|---------------------------|
| Pure narrative — articles, tutorials, no API ref      | `manual-only/`            |
| C# library, single assembly or a few related ones     | `csharp-single-stage/`    |
| C++ only                                              | `doxygen-cpp-only/`       |
| Java only                                             | `doxygen-java-only/`      |
| Both C++ and Java (same docs site)                    | `doxygen-multilang/`      |

## `manual-only/`

100% hand-written docs. No `prepare/` step, no auto-generation.

Files:
- `project.proj` — MSBuild with `MakeDoc` and `CleanDoc` targets.
- `project.xml` — docgen config; `<dg:source>` lists `src/index.ds` plus `src/articles/`.
- `index.ds` — top-level group + a stub article.
- `doc.bat`, `clean.bat` — convenience wrappers.
- `.gitignore` — ignores `dst/`, `obj/`, `bin/`.

## `csharp-single-stage/`

C# library docs with auto-generation via `Asm2Xml`, single MSBuild project. Recommended for new C# projects.

Files (in addition to the manual set):
- `prepareproject.xml` — invokes the `cs2ds` template to convert `obj/raw.xml` into `src/raw/*.ds`.
- `preparesettings.xml` — points at the assembly's compiled XML doc file and lists namespaces to strip.
- `null.ds` — stub source file required by `prepareproject.xml`.
- `prepare.bat` — runs the `Scan,Raw` pipeline: `Asm2Xml` then docgen with `cs2ds`.

The user must:
1. Replace `REPLACE_WITH_PROJECT_NAME` and `REPLACE_WITH_PROJECT_TITLE`.
2. Adjust the assembly path in `project.proj` (`<DocSource Include=...>`).
3. Adjust `Mode` on `Asm2Xml` to match the assembly's runtime. Supported as of docgen 0.1.34: `net472`, `net50`, `net60`, `net70`, `net80`, `net90`, `net10.0`. Note: pre-10 modes have no dot (`net90`, not `net9.0`); 10 and later do (`net10.0`).
4. Ensure the C# project's `.csproj` has `<GenerateDocumentationFile>true</GenerateDocumentationFile>`.
5. Run `prepare.bat` once to populate `src/raw/`, then `doc.bat` to build.

## `doxygen-cpp-only/`

C++ library docs with doxygen auto-generation. Single language, two-stage build.

Layout:
- Top-level: `project.proj`, `project.xml`, `index.ds`, `doc.bat`. `<dg:source>` lists `src/cpp/`.
- `prepare/cpp/`: `Doxyfile`, `project.proj` (with `ClearDoc`/`Doxygen`/`Prepare` targets), `prepareproject.xml`, `prepare.bat`, `src/null.ds`.

The user must:
1. Replace `REPLACE_WITH_*` placeholders.
2. Set `INPUT` in `Doxyfile` to point at the C++ source directories.
3. From `prepare/cpp/`, run `prepare.bat`. Output lands in `prepare/cpp/dst/`.
4. Copy `prepare/cpp/dst/*.ds` into `doc/src/cpp/` (or change `project.xml` to reference `prepare/cpp/dst/` directly).
5. From `doc/`, run `doc.bat`.

## `doxygen-java-only/`

Same as `doxygen-cpp-only/` but with a Java-tuned `Doxyfile` (`OPTIMIZE_OUTPUT_JAVA=YES`, `JAVADOC_AUTOBRIEF=YES`, `*.java` patterns) and a `prepareproject.xml` that sets `language=java`, `divisor=.`, and excludes `java:.*` (the JDK).

The user must:
1. Replace `REPLACE_WITH_*` placeholders.
2. Set `INPUT` in `Doxyfile` to point at the Java source directories.
3. From `prepare/java/`, run `prepare.bat`.
4. Copy `prepare/java/dst/*.ds` into `doc/src/java/`.
5. From `doc/`, run `doc.bat`.

## `doxygen-multilang/`

C++ and Java in one docs site. Top-level `project.xml` lists both `src/cpp/` and `src/java/`; `index.ds` has a top-level group with two language subgroups.

`prepare/cpp/` and `prepare/java/` are independent — refresh each separately, then build the combined docs from `doc/`. To add or remove a language later, drop the directory and adjust `project.xml` and `index.ds`.

## Conventions

All scaffolds share these conventions (matching Gehtsoft de-facto standards):

- The MSBuild file is `project.proj`; the docgen config is `project.xml`.
- `doc.bat` runs `MakeDoc`. `prepare.bat` (when present) runs the auto-gen pipeline.
- Output goes to `doc/dst/` (gitignored).
- Auto-generated `src/raw/*.ds` files are committed (CI builds without source).
- `<dg:common>` always sets `simplified-text-syntax=yes`.
- `<dg:source>` order: hand-written first, auto-generated last.

For the `.ds` syntax, see `references/ds-format.md`. For the build files in detail, see `references/project-files.md`. For the auto-generation pipelines, see `references/source-extraction.md`.
