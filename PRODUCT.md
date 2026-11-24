# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- **Primary**: .NET 10 / Blazor and `.NET MAUI Blazor Hybrid` developers building data-heavy enterprise tables, backoffice lists, tree grids, and responsive card directories.

## Product Purpose

`lux-table` pairs a headless NativeAOT tree hierarchy engine (`Lux.Table.Core`) with `LxDataList` (`Lux.Table`), a high-density enterprise data table and card grid component supporting visual grouping, footer aggregations, sorting/column header menus, loading progress indicators, virtualization, and responsive mobile layouts.

Success means teams can ship dense, accessible, URL-synchronized enterprise tables in minutes — with zero-friction precompiled CSS (`lux-table.css`) or native Tailwind v4 — without maintaining bespoke table state, grouping, or tree flattening code.

## Positioning

Unlike lightweight grids limited to flat `IQueryable` rows or monolithic commercial suites tied to proprietary stylesheets, `Lux.Table` combines a zero-UI NativeAOT hierarchy engine (`DataListTree<TItem>`), `Lux.Query` URL/facet/EF Core integration, precompiled zero-friction CSS, and Static SSR + Interactive + MAUI Hybrid support under MIT.

## Operating Context

- **Packages**: `Lux.Table.Core` (headless NativeAOT tree & aggregation engine) and `Lux.Table` (`LxDataList`, `LxDataListTable`, `LxDataListColumn`, `lux-table.css`).
- **Integration**: Consumes `Lux.Query.Core` (`DataListSchema<T>`, `DataListRequest`, `DataListStateQuery`) and `Lux.UI` primitives.
- **Showcase**: Used live in the `Accounts` tab of `Lux.Stack.Site` `/showcase` with grouping, search, summaries, and undo/redo.

## Capabilities and Constraints

- `Lux.Table.Core` is 100% headless with `<IsAotCompatible>true</IsAotCompatible>` and `<IsTrimmable>true</IsTrimmable>`.
- `Lux.Table` ships standalone precompiled CSS (`lux-table.css`) so projects without Node/Tailwind render cleanly out of the box.
- Supports table and card layouts, visual grouping (`GroupBy`), footer summaries (`DataListSummaryKind`), column header menus, 2px loading progress line, virtualization, and responsive mobile rows.
- 100% MIT open-source license; strict `Lux.Table*` / `LxDataList*` / `--lux-*` naming.

## Brand Commitments

- **Name**: `Lux.Table` (`lux-table`), part of the **LuxStack** ecosystem.
- **Voice**: Technical, pragmatic, and precision-oriented for enterprise data UIs.

## Evidence on Hand

- Passing unit and bUnit tests (`tests/Lux.Table.Tests`) plus NativeAOT publish verification in CI.
- Live interactive demonstration in `Lux.Stack.Site` `/showcase` (`Accounts` view) and dedicated product docs at `/table`.
- Verified zero-friction standalone CSS bundle (`lux-table.css`).
- **Explicit absences (do not fabricate)**: No NuGet download stats, customer logos, or third-party testimonials exist yet.

## Product Principles

1. **Headless data structures, thin UI**: Tree flattening, depth rails, and summaries belong in `Lux.Table.Core`, not scattered across Razor markup.
2. **Dense readability without clutter**: Tabular numerals, clean alignment, grouping, and non-blocking loading feedback keep operational tables scannable.
3. **Zero-friction styling**: Works immediately via precompiled `lux-table.css` or natively inside a Tailwind CSS v4 pipeline.
4. **Responsive across viewports**: Desktop density gracefully adapts to mobile summary layouts without horizontal breakage.

## Accessibility & Inclusion

- Durable standard: **WCAG 2.1 AA** across `LxDataList` table and card views: proper `<table>`/`<thead>`/`<th scope>` semantics, keyboard-accessible header menus and sorting controls, `aria-sort` state, and screen-reader-friendly loading/empty states.
