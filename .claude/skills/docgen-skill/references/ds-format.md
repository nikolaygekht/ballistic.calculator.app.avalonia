# `.ds` format reference

The `.ds` (documentation source) format is a custom plain-text DSL with `@`-prefixed block tags and, inside the body text, a choice of two inline-formatting syntaxes: **markdown-style** markup (the newer, compact one) and **BBCode** (the original). Both are active whenever `simplified-text-syntax=yes` is set — the standing convention for these projects — and can be mixed freely. See "Inline and block formatting: markdown or BBCode" below for the markdown reference and when to prefer each. It compiles to an XML model and then through XSLT to HTML, CHM, Markdown, etc.

This file is the working reference for the `.ds` block tags and inline formatting you'll use day to day. The format is stable across docgen versions; the tags and markup documented here are the full set needed for authoring and editing docs.

## Document structure

Every `.ds` file is a sequence of top-level blocks. Only three element types are valid at the top level:

- `@group` — a folder/namespace. Holds other groups, articles, and classes in the TOC.
- `@article` — a standalone narrative document.
- `@class` — an API reference page (a class, struct, interface, set of tags, etc.).

Each block opens with the tag, ends with `@end`. Inside the block, attributes appear as `@name=value` lines (one per line), and free text becomes the body. Nested blocks are indented for readability (indentation is not significant).

```
@article
    @key=getting-started
    @title=Getting Started
    @ingroup=main
    @brief=A short tour of the library.

    Free-form body text goes here. BBCode like [b]bold[/b] works.
    Cross-references use [link=other-key]link text[/link].
@end
```

`@key` is required and must be globally unique across articles, groups, and classes — it's the join point for cross-references and the merge key (see SKILL.md "merge mechanism").

`@ingroup` is required for everything except the root group; it points at the parent group's `@key`.

`@brief` is the one-sentence summary shown in indexes and tooltips. Keep it short.

## Top-level blocks

### `@group` — folder / namespace

```
@group
    @key=MyLibrary.Charting
    @title=Namespace MyLibrary.Charting
    @ingroup=main
    @brief=Classes to render charts and overlays.

    Long-form description of what this namespace is for, who should use it,
    common entry points, and pointers into the rest of the API:

    To define a chart programmatically use [clink=MyLibrary.Charting.Data]MyLibrary.Charting.Data[/clink].
@end
```

Useful attributes:

| Attribute       | Purpose                                                                                |
|-----------------|----------------------------------------------------------------------------------------|
| `@key`          | Unique identifier. For namespace groups, use the fully-qualified namespace.            |
| `@title`        | Display name in TOC and headers.                                                       |
| `@ingroup`      | Parent group's key. Use `main` (or whatever the root is) for top-level groups.         |
| `@brief`        | One-line summary.                                                                      |
| `@order`        | `custom` to disable sorting/grouping inside this group (preserve declared order).      |
| `@sortarticles` | `yes`/`no` to control article sorting within the group.                                |
| `@sortgroups`   | `yes`/`no` for nested groups.                                                          |
| `@sortclasses`  | `yes`/`no` for classes.                                                                |
| `@transform`    | `yes` (default) to run BBCode through the formatter; `no` for raw text.                |
| `@if`           | Name of a `<dg:define>` flag — group is only included when the flag is set.            |

### `@article` — narrative document

```
@article
    @key=tutorials.first-shot
    @title=Your First Shot
    @ingroup=tutorials
    @brief=Compute the first ballistic trajectory.

    Step-by-step body. Use [link=...] to reference other articles and
    [clink=Foo.Bar]Bar[/clink] for API references.
@end
```

Useful attributes:

| Attribute          | Purpose                                                                            |
|--------------------|------------------------------------------------------------------------------------|
| `@key`             | Unique identifier.                                                                 |
| `@title`           | Display name.                                                                      |
| `@ingroup`         | Parent group.                                                                      |
| `@brief`           | One-line summary.                                                                  |
| `@aliasId`         | Alias for HTML Help topic list.                                                    |
| `@excludeFromList` | `yes` to omit from the article list within the parent group.                       |
| `@transform`       | Same as group.                                                                     |
| `@if`              | Same as group.                                                                     |

### `@class` — API reference

```
@class
    @name=ChartRenderer
    @key=MyLibrary.Charting.Draw.ChartRenderer
    @ingroup=MyLibrary.Charting.Draw
    @brief=Renders a chart and overlays.
    @type=class
    @parent=object

    @member
        @type=method
        @name=Draw
        @key=Draw
        @brief=Render the chart to the canvas.
        ...
    @end
@end
```

Common attributes (the ones you'll use in practice):

| Attribute            | Purpose                                                                         |
|----------------------|---------------------------------------------------------------------------------|
| `@name`              | Display name.                                                                   |
| `@key`               | Unique identifier (typically fully-qualified type name).                        |
| `@ingroup`           | Parent group's key (typically the namespace).                                   |
| `@brief`             | One-line summary.                                                               |
| `@type`              | `class`, `interface`, `struct`, `enum`, `tags` (custom), template-defined.      |
| `@parent`            | Plain text — base class name for display only, not a key reference.             |
| `@sig`               | Platform-specific signature (e.g., `T:Namespace.ClassName` for .NET).           |
| `@import`            | Key of another class — its members are imported into this one (for inheritance).|
| `@membersToContent`  | `true` to expose individual members as TOC entries.                             |
| `@sort`              | `yes` (default) sorts members alphabetically; `no` keeps declared order.        |
| `@writeSignatures`   | `yes`/`no`/`def` — show full signatures in member lists (for overload-heavy classes). |
| `@declname`          | Name to use in declarations if it differs from `@name`.                         |

For the dual-source pattern (hand-written + auto-generated) the trick is: hand-write a `@class` block with the class's `@key` and document only the members you care about. The auto-generated raw file's class with the same `@key` will silently merge in the rest of its members.

## Member blocks (inside `@class`)

```
@member
    @type=method
    @name=Calculate
    @key=Calculate.AB12CDEF
    @sig=M:Foo.Bar.Calculate(System.Double)
    @visibility=public
    @scope=instance
    @brief=Compute the trajectory.

    @declaration
        @language=cs
        @return=TrajectoryResult
        @params=double angle
    @end

    @param
        @name=angle
        The launch angle in radians.
    @end

    @return
        A populated TrajectoryResult.
    @end

    @exception
        @name=ArgumentOutOfRangeException
        Thrown when angle is outside [-π/2, π/2].
    @end
@end
```

Member attributes:

| Attribute          | Purpose                                                                              |
|--------------------|--------------------------------------------------------------------------------------|
| `@type`            | `field`, `property`, `method`, `constructor`, `event`, `enum-value`, etc.            |
| `@name`            | Display name.                                                                        |
| `@key`             | Unique within the parent class. Auto-generated keys often have a CRC suffix.         |
| `@sig`             | Full signature (e.g., `M:` for methods, `P:` for properties in .NET).                |
| `@visibility`      | `public`, `protected`, `internal`, `private`.                                        |
| `@scope`           | `instance` or `static`.                                                              |
| `@divisor`         | Separator between class name and member name (default `.`).                          |
| `@excludeFromList` | `yes` to keep the member documented but hidden from the class's member list.         |

## Inner blocks

These appear inside `@class`, `@member`, `@article`, and (where it makes sense) `@group`.

### `@param` — parameter / type parameter

```
@param
    @name=count
    @gray=yes
    The number of trials to run. Use 1000 for production.
@end
```

`@gray=yes` highlights the parameter on a tinted background. Used sparingly to flag deprecated or special parameters.

### `@return` — return value

```
@return
    The trajectory result, never null.
@end
```

### `@exception` — thrown exception

```
@exception
    @name=System.ArgumentNullException
    Thrown when target is null.
@end
```

### `@declaration` — language-specific signature (legacy)

`@declaration` is the older way to show signatures. The canonical reference notes it as deprecated and recommends `@example` blocks for new documentation. It's still widely seen in auto-generated content because the `cs2ds` and `doxygen2ds` templates emit it.

```
@declaration
    @language=cs
    @return=void
    @params=int count, string name
@end
```

Attributes: `@language`, `@name`, `@prefix`, `@suffix`, `@name-suffix`, `@params`, `@return`, `@custom` (a fully custom one-line declaration).

### `@example` — runnable / illustrative code

The modern way to show code. Supports tabs (multiple language variants of the same example), syntax highlighting, and Mermaid diagrams.

Simple form:

```
@example
    @title=Computing trajectory
    @highlight=cs

    var calc = new TrajectoryCalculator(ammo);
    var result = calc.Calculate(angle: 0.05);
@end
```

Multi-language tabs:

```
@example
    @title=Hello world
    @tabs=yes

    @tab
        @title=C#
        @highlight=cs

        Console.WriteLine("Hello, world!");
    @end

    @tab
        @title=C++
        @highlight=cpp

        std::cout << "Hello, world!" << std::endl;
    @end
@end
```

When `@tabs=yes`, the body must be only `@tab` blocks; freeform text outside tabs is invalid.

Mermaid diagrams (docgen 0.1.30+):

```
@example
    @highlight=diagram
    @show=yes

    graph LR
      A[Source] --> B[Extractor]
      B --> C[XSLT]
      C --> D[.ds files]
@end
```

`@show=yes` is required for diagrams because Mermaid doesn't render correctly inside collapsed sections.

Other attributes: `@gray=yes` (tinted background), `@show` (`yes`/`always` to expand by default).

**Indentation is trimmed per line.** docgen strips the leading whitespace of *every* line in an `@example` body — not just the common indent. Flat examples (statements all at one level) are unaffected, but any example with *relative* indentation (a method body, a loop, an `if`) collapses flush-left and becomes unreadable. To preserve indentation, mark the lines as literal one of two ways:

1. **`!` line-prefix** — begin each code line with `!` (after the block's reading-indent). Everything after the `!` is preserved verbatim, including its own leading spaces. Works with `@highlight`. A blank line inside the block needs a bare `!` too.

   ```
   @example
       @title=Generic guard
       @highlight=cs
      !static void M<T>(T x)
      !{
      !    if (x != null)
      !        Use(x);
      !}
   @end
   ```

2. **Markdown fenced block** — wrap the code in a triple-backtick fence inside the `@example`.

The `!` only controls whitespace: `<`/`>` inside `!` lines are still HTML-escaped correctly (raw is fine), and `&` still needs `&amp;` (see Escaping below). Always check the *rendered* block — collapsed indentation is invisible in the `.ds` source.

### `@list` / `@list-item` — bulleted or numbered list

```
@list
    @list-item
        First item.
    @end
    @list-item
        Second item.
    @end
@end
```

For numbered lists set `@type=num` on `@list`. (Markdown alternative: `*`/`-` bullets, `0` for numbered items — see "Inline and block formatting" below. Prefer it for new prose.)

Lists can nest:

```
@list
    @list-item
        Outer item.
        @list
            @list-item
                Inner item.
            @end
        @end
    @end
@end
```

### `@table` / `@row` / `@col` — tables

```
@table
    @width=100%
    @row
        @header=yes
        @col
            @width=30%
            Name
        @end
        @col
            @width=70%
            Description
        @end
    @end
    @row
        @col
            x
        @end
        @col
            The x coordinate.
        @end
    @end
@end
```

`@header=yes` on a row marks it as a header row.

**`@width` values are emitted verbatim into the HTML `width=` attribute, so a bare number is pixels, not percent.** `@width=100` renders as `<table width="100">` — i.e. 100 pixels (content-width), almost never what you want. Always include `%` for proportional sizing: `@width=100%` for a full-width table. Set column proportions with `@width=NN%` on the `@col` cells of the **header row** only; the rest of each column follows.

(Markdown alternative: a `|a|b|` table with `!` headers, ` >` for full width, and `NN%,` column widths — see "Inline and block formatting" below. Prefer it for new content; it's far more compact and avoids the pixels-vs-percent trap.)

### `@headline` — section heading

```
@headline
    @level=2
    Configuration
@end
```

Levels 1–4. Use sparingly inside articles for sub-sections. (Markdown alternative: `#`…`####` — see "Inline and block formatting" below.)

### `@see` — see-also reference

```
@see
    @key=related-topic
    @title=Related topic
@end
```

Renders as a "See also" pointer. The `@title` is optional; if omitted, the target's title is used.

**Caveat (HTML template):** a run of `@see` blocks renders as a 2-column *table* whose second (description) column is empty, which looks broken — a wide blank column beside each link. For cross-article navigation, prefer ending the article with a "See also" heading and a list of links instead:

```
@headline
    @level=2
    See also
@end
@list
    @list-item
        [link=other-key]Other Article Title[/link]
    @end
@end
```

That renders as a clean `<h2>See also</h2>` followed by a `<ul>` of links.

### `@note` — callout box

```
@note
    @type=warning
    Don't call this from a finalizer.
@end
```

The HTML template supports `note`, `warning`, and `quote`. Other templates may support more types — check the template specifically. (Available in docgen 0.1.17+.)

### `@import` — pull in members from another class

On a `@class` block, `@import=OtherClass.Key` causes the build to copy members from the referenced class into this one (used for showing inherited members on a derived class). The parser emits `@import key X is not found` if the target doesn't exist.

## Inline and block formatting: markdown or BBCode

docgen supports **two** formatting syntaxes for body content, both active whenever `simplified-text-syntax=yes` is set in `<dg:common>` — the standing convention for these projects (see SKILL.md "Conventions"). They can be mixed in the same file.

- **Markdown-style** — the newer, compact syntax: `**bold**`, `` `code` ``, `# heading`, `|a|b|` tables.
- **BBCode** — the original syntax: `[b]bold[/b]`, `[c]code[/c]`, `[clink=...]`, colors, etc.

**Which to use:**

- **(a) Match the style already established in the project / document.** If a project's `.ds` files are written in BBCode, keep writing BBCode there; if they use markdown, use markdown. Consistency within a doc set matters more than the choice — don't mix the two styles within one document for no reason.
- **(b) For brand-new documentation, prefer markdown.** It is more compact and natural to read and write. Reach for BBCode only for what markdown can't express (see "BBCode-only features" below).

This choice is about *inline and lightweight-block* formatting (emphasis, code, links, lists, tables, headings, code blocks) inside the body of `@article` / `@class` / `@member` / `@group` and the inner text blocks. The structural skeleton is still authored with `@`-tags (`@article`, `@class`, `@member`, `@param`, `@return`, `@example`, …) — markdown or BBCode goes in the *text* of those blocks.

### Markdown reference

Inline:

| Markdown    | Meaning        | BBCode equivalent |
|-------------|----------------|-------------------|
| `**text**`  | Bold           | `[b]text[/b]`     |
| `//text//`  | Italic         | `[i]text[/i]`     |
| `__text__`  | Underline      | `[u]text[/u]`     |
| `~~text~~`  | Strike-through | `[s]text[/s]`     |
| `^^text^^`  | Superscript    | `[sup]text[/sup]` |
| `` `text` ``| Inline code    | `[c]text[/c]`     |

Headings (levels 1–4; equivalent to `@headline` with `@level`):

```
# Heading 1
## Heading 2
### Heading 3
#### Heading 4
```

Links:

| Markdown                       | Meaning                                                              |
|--------------------------------|---------------------------------------------------------------------|
| `<https://example.com>`        | External URL (bare).                                                |
| `[text](https://example.com)`  | External URL with link text.                                        |
| `[text]key`                    | Internal cross-reference to a `@key` (article/group/class) — the markdown form of `[link=key]text[/link]`. Note: no parentheses around the key. |

Lists — a line whose first non-whitespace character is `*` or `-` (plus a space) is a bullet; start a numbered list with `0`. Indent further to nest. Equivalent to `@list`/`@list-item`:

```
* First bullet
* Second bullet

0 First numbered item
0 Second numbered item
```

(This is the same rule behind the "line-leading `- `/`* ` becomes a bullet" gotcha above — in markdown mode that behavior is *intended*; it only bites when prose accidentally wraps a dash to the start of a line.)

Fenced code blocks — triple backticks plus a language id; prefix the language with `+` to render expanded or `-` to render collapsed. Equivalent to `@example` with `@highlight`:

````
```csharp
if (a > b)
    Console.WriteLine("A is bigger");
```
````

A markdown fence preserves the code's own indentation, so it is the natural alternative to the `!`-prefix workaround for indented `@example` bodies (see the `@example` note above).

Tables — each row is a line starting with `|`, cells separated by `|`:

```
|!,Name|Description|
|x|The x coordinate.|
|y|The y coordinate.|
```

- `!` immediately after the opening `|` marks the row as a header row: `|!,...|`.
- A trailing ` >` makes the table full width: `|a|b| >`.
- A `NN%,` prefix inside a cell sets that column's width: `|!50%,Name|50%,Description| >`.

This is the compact alternative to `@table`/`@row`/`@col`. Widths take `%` here just as on `@col`; a full-width markdown table (`>`) sidesteps the `@width` pixels-vs-percent pitfall entirely.

### BBCode-only features

Some constructs have no markdown form — use BBCode for these even in markdown-first documents:

- **Code-style API links:** `[clink=key]Name[/clink]` (a link rendered monospace). The markdown internal link `[Name]key` links but is not code-styled, so `[clink]` stays the preferred form for API references in prose.
- **Colors:** `[red]...[/red]`, `[color=#rrggbb]...[/color]`, etc.
- **Subscript:** `[sub]...[/sub]` (markdown has `^^...^^` for superscript but no subscript form).
- **Images:** `[img=path]`.
- **New-window external link:** `[eurl=https://...]text[/eurl]`.
- **Explicit line break / nil:** `[br]`, `[nil]`.

The full BBCode reference follows.

## BBCode inline markup

BBCode is processed inside body text and most attribute bodies. The behavior depends on the output template having `default-transform=yes`.

### Text styling

| Tag | Meaning                                            |
|-----|----------------------------------------------------|
| `[b]...[/b]`   | Bold                                    |
| `[i]...[/i]`   | Italic                                  |
| `[u]...[/u]`   | Underline                               |
| `[s]...[/s]`   | Strike-through                          |
| `[c]...[/c]`   | Inline code (monospace)                 |
| `[sup]...[/sup]` | Superscript                           |
| `[sub]...[/sub]` | Subscript                             |
| `[size=+1]...[/size]` | Relative font size (`+n` / `-n`) |

### Color

`[red]...[/red]`, `[green]...[/green]`, `[blue]...[/blue]`, `[gray]...[/gray]`, plus the general `[color=name-or-rgb]...[/color]`.

### Links and references

| Tag                            | Use                                                              |
|--------------------------------|------------------------------------------------------------------|
| `[link=key]text[/link]`        | Cross-reference to a `@key` (article, group, or class).          |
| `[clink=key]text[/clink]`      | Same but rendered as code (equivalent to `[link=key][c]text[/c][/link]`). |
| `[url=https://...]text[/url]`  | External link, opens in same window.                             |
| `[eurl=https://...]text[/eurl]`| External link, opens in new window.                              |
| `[img=path]`                   | Inline image (no closing tag).                                   |

Use `[clink=...]` for API references in narrative text — readers expect type names rendered in code style.

### Special tags

- `[br]` — explicit line break (no closing tag).
- `[nil]` — renders nothing. Used to break a literal `[code]`-like sequence so it appears as text instead of being parsed as BBCode.

### Escaping

There is no escape character. To show literal `[code]` text without it being parsed:

```
myArray[[nil]i] + 1
```

The `[nil]` tag splits what would otherwise be a valid BBCode opener so the parser leaves it as text.

**Angle brackets vs. ampersand — they are not symmetric.** `.ds` source is plain text, *not* XML, so you write `<` and `>` raw — a raw `<` is correctly escaped to `&lt;` on output, and generics like `Expression<Func<int,bool>>` render fine as literal text. But a raw `&` is mishandled:

- A single raw `&` (e.g. `[c]a & b[/c]`) emits a bare, unescaped `&` in the HTML — invalid markup.
- A double raw `&&` (e.g. `[c]a && b[/c]`, or `&&` inside an `@example`) loses one of the two ampersands — `x > 0 && x < 100` renders as `x > 0 & x < 100`. That is genuine content corruption, not just an escaping nit.

So write `&` as the entity **`&amp;`** in `.ds` source — in both body text and `@example` code. docgen passes `&amp;` through verbatim and it renders as a literal `&`.

| C# you want to show | Write in `.ds`            | Renders as |
|---------------------|---------------------------|------------|
| `a & b`             | `[c]a &amp; b[/c]`        | `a & b`    |
| `a && b`            | `[c]a &amp;&amp; b[/c]`   | `a && b`   |
| `x => x > 0 && y`   | `x => x > 0 &amp;&amp; y` | `x => x > 0 && y` |

The cross-language rule for hand-written `.ds`: **raw `<` and `>`, but `&amp;` for `&`.** (Contrast with C# `///` comments, which *are* XML and need `&lt;` / `&gt;` / `&amp;` for all three — see `source-extraction.md`.) This corruption is invisible in the `.ds` source; verify against the rendered HTML.

### Line-leading `- ` and `* ` become list bullets

With `simplified-text-syntax=yes` (always set), any line whose first non-whitespace character is `-` or `*` followed by a space is treated as a **markdown-style list bullet**. This bites ordinary prose: if wrapping a sentence for a column limit pushes a mid-sentence dash to the *start* of a line, that line silently renders as a stray bullet.

```
@list-item
    [clink=...VeryLongTypeName]VeryLongTypeName[/clink]
    - the runtime the emitted expressions depend on; load it once.
@end
```

Here the `- the runtime...` line renders as a *nested sub-bullet* under the item instead of as the item's text.

**Never let a body line begin with `- ` or `* ` unless you intend a bullet.** Keep the dash attached to the end of the previous line (`... depend on -` then `load it once`), or reword. When you genuinely want a list, use a real `@list` / `@list-item` block. The same rule applies to the text `cs2ds` emits from C# `///` comments, because it preserves your source line breaks — see `source-extraction.md`.

## Conditional content (`@if`)

Most blocks accept `@if=flagName`. The block is only included in the build if a `<dg:define name="flagName" value="yes"/>` exists in the project's `<dg:common>` or in the matching `<dg:output>`. Useful for emitting different content per output format (e.g., HTML-only vs CHM-only sections).

## Style conventions

Across all Gehtsoft projects:

- `@brief` is one sentence, no period at the end is fine, no markup. It's used in lists and tooltips, where compactness matters.
- Use `[clink=...]` for API references in body text; `[link=...]` for narrative cross-references. The visual difference (code vs prose styling) is meaningful to readers.
- For code fragments inline, use `[c]identifier[/c]`. For multi-line code, use `@example` (or `[code]...[/code]` if you only need a plain block).
- Don't repeat what auto-generation provides. A hand-written `@class` block earns its place by adding orientation, examples, or organization that the source code can't.
- Capitalize `@title` like a heading; use sentence case for `@brief`.
- Indent nested blocks (no semantic effect but reading is much easier).

## Common patterns

### Namespace overview

```
@group
    @key=Foo.Bar
    @title=Namespace Foo.Bar
    @ingroup=main
    @brief=Utilities for working with frobs.

    The classes in this namespace cover three concerns:

    * [clink=Foo.Bar.Frobnicator]Frobnicator[/clink] — the main entry point.
    * [clink=Foo.Bar.FrobOptions]FrobOptions[/clink] — configuration.
    * [clink=Foo.Bar.FrobResult]FrobResult[/clink] — output container.

    For typical usage see [link=tutorials.frobnication]Frobnication tutorial[/link].
@end
```

### Class with selective member documentation (relies on merge)

```
@class
    @name=Frobnicator
    @key=Foo.Bar.Frobnicator
    @ingroup=Foo.Bar
    @brief=Performs frobnication.

    Use this class to frobnicate. Methods not described here are
    documented from XML doc comments.

    @member
        @type=method
        @name=Frobnicate
        @key=Frobnicate.A1B2C3D4

        @example
            @title=Basic frobnication
            @highlight=cs

            var f = new Frobnicator();
            var result = f.Frobnicate(input, FrobOptions.Default);
        @end
    @end
@end
```

The auto-generated raw file with key `Foo.Bar.Frobnicator` will contribute its other members; this hand-written file's `@brief` and the `Frobnicate` member documentation win where they overlap.

### Article with a step list and example

```
@article
    @key=tutorials.frobnication
    @title=Frobnication tutorial
    @ingroup=tutorials
    @brief=End-to-end walkthrough of frobnication.

    @list
        @type=num
        @list-item
            Install the package.
        @end
        @list-item
            Construct a [clink=Foo.Bar.FrobOptions]FrobOptions[/clink].
        @end
        @list-item
            Call [clink=Foo.Bar.Frobnicator]Frobnicator[/clink].Frobnicate.
        @end
    @end

    @example
        @title=Complete example
        @highlight=cs

        var opts = new FrobOptions { Mode = FrobMode.Aggressive };
        var f = new Frobnicator();
        var result = f.Frobnicate("input", opts);
        Console.WriteLine(result.Output);
    @end
@end
```
