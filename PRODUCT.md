# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- **Primary**: .NET 10 / Blazor and `.NET MAUI Blazor Hybrid` developers building data-heavy enterprise tables, backoffice lists, tree grids, and responsive card directories.

## Product Purpose

`klyroo-table` pairs a headless NativeAOT tree hierarchy engine (`Klyroo.Table.Core`) with `KlyDataList` (`Klyroo.Table`), a high-density enterprise data table and card grid component supporting visual grouping, footer aggregations, sorting/column header menus, loading progress indicators, virtualization, and responsive mobile layouts.

Success means teams can ship dense, accessible, URL-synchronized enterprise tables in minutes — with zero-friction precompiled CSS (`klyroo-table.css`) or native Tailwind v4 — without maintaining bespoke table state, grouping, or tree flattening code.

## Positioning

Unlike lightweight grids limited to flat `IQueryable` rows or monolithic commercial suites tied to proprietary stylesheets, `Klyroo.Table` combines a zero-UI NativeAOT hierarchy engine (`DataListTree<TItem>`), `Klyroo.Query` URL/facet/EF Core integration, precompiled zero-friction CSS, and Static SSR + Interactive + MAUI Hybrid support under MIT.

## Operating Context

- **Packages**: `Klyroo.Table.Core` (headless NativeAOT tree & aggregation engine) and `Klyroo.Table` (`KlyDataList`, `KlyDataListTable`, `KlyDataListColumn`, `klyroo-table.css`).
- **Integration**: Consumes `Klyroo.Query.Core` (`DataListSchema<T>`, `DataListRequest`, `DataListStateQuery`) and `Klyroo.UI` primitives.
- **Showcase**: Used live in the `Accounts` tab of `Klyroo.Stack.Site` `/showcase` with grouping, search, summaries, and undo/redo.

## Capabilities and Constraints

- `Klyroo.Table.Core` is 100% headless with `<IsAotCompatible>true</IsAotCompatible>` and `<IsTrimmable>true</IsTrimmable>`.
- `Klyroo.Table` ships standalone precompiled CSS (`klyroo-table.css`) so projects without Node/Tailwind render cleanly out of the box.
- Supports table and card layouts, visual grouping (`GroupBy`), footer summaries (`DataListSummaryKind`), column header menus, 2px loading progress line, virtualization, and responsive mobile rows.
- 100% MIT open-source license; strict `Klyroo.Table*` / `KlyDataList*` / `--klyroo-*` naming.

## Brand Commitments

- **Name**: `Klyroo.Table` (`klyroo-table`), part of the **Klyroo** ecosystem.
- **Voice**: Technical, pragmatic, and precision-oriented for enterprise data UIs.

## Evidence on Hand

- Passing unit and bUnit tests (`tests/Klyroo.Table.Tests`) plus NativeAOT publish verification in CI.
- Live interactive demonstration in `Klyroo.Stack.Site` `/showcase` (`Accounts` view) and dedicated product docs at `/table`.
- Verified zero-friction standalone CSS bundle (`klyroo-table.css`).
- **Explicit absences (do not fabricate)**: No NuGet download stats, customer logos, or third-party testimonials exist yet.

## Product Principles

1. **Headless data structures, thin UI**: Tree flattening, depth rails, and summaries belong in `Klyroo.Table.Core`, not scattered across Razor markup.
2. **Dense readability without clutter**: Tabular numerals, clean alignment, grouping, and non-blocking loading feedback keep operational tables scannable.
3. **Zero-friction styling**: Works immediately via precompiled `klyroo-table.css` or natively inside a Tailwind CSS v4 pipeline.
4. **Responsive across viewports**: Desktop density gracefully adapts to mobile summary layouts without horizontal breakage.

## Accessibility & Inclusion

- Durable standard: **WCAG 2.1 AA** across `KlyDataList` table and card views: proper `<table>`/`<thead>`/`<th scope>` semantics, keyboard-accessible header menus and sorting controls, `aria-sort` state, and screen-reader-friendly loading/empty states.
