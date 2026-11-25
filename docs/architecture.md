# Architecture — Klyroo.Table

Klyroo.Table follows the strict headless two-layer architecture shared across every repository in **Klyroo**:

```text
+-------------------------------------------------------------------+
|  Klyroo.Table                                                       |
|  Blazor Web (SSR / Server / WASM / Auto) & .NET MAUI Hybrid       |
|  SupportedPlatform: browser, ios, android, maccatalyst, windows   |
+-------------------------------------------------------------------+
                                |
                                v
+-------------------------------------------------------------------+
|  Klyroo.Table.Core                                                  |
|  100% Headless C# · Zero UI Dependency · IsAotCompatible=true     |
|  Works in APIs, Workers, CLI, NativeAOT & Native C# MAUI          |
+-------------------------------------------------------------------+
```

## Design Principles

1. **Zero UI Coupling in `Klyroo.Table.Core`** — State machines, validation rules, ASTs, formatters, and parsers live in `Klyroo.Table.Core` so they can be unit tested in milliseconds with plain `xUnit` without spinning up a renderer.
2. **NativeAOT & Trimming Verified** — `Klyroo.Table.Core` sets `<IsAotCompatible>true</IsAotCompatible>` and `<IsTrimmable>true</IsTrimmable>` with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
3. **Every Render Mode & Native Hybrid** — `Klyroo.Table` avoids browser-only assumptions during prerendering and declares `<SupportedPlatform>` for `browser`, `ios`, `android`, `maccatalyst`, and `windows`.
4. **Composable with `Klyroo.Stack`** — Install `Klyroo.Table` standalone or aggregate the entire ecosystem with `dotnet add package Klyroo.Stack` and `services.AddKlyrooStack()`.

## Supported Platforms

- Headless C# / NativeAOT
- Blazor Server / WASM / Auto
- .NET MAUI Blazor Hybrid
- Responsive Mobile Summary Rows
