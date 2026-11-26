---
name: klyroo-table
description: Klyroo.Table data lists for Blazor and .NET MAUI Blazor Hybrid, a table and card grid with search, filters, sorting, paging, selection, actions, grouping, footer summaries and a headless tree model.
---

# Klyroo.Table

- `Klyroo.Table.Core` holds the UI-free tree model: `DataListTree<T>` and `DataListTreeNode<T>`. It builds on `Klyroo.Query.Core` (`DataListSchema<T>`, `DataListState`, `DataListRequest`, `DataListResult<T>`).
- `Klyroo.Table` holds the Blazor components. Register it with `services.AddKlyrooTable()` (it also registers Klyroo.UI and Klyroo.Query). The Blazor project needs `@using Klyroo.UI`, `@using Klyroo.Table` and `@using Klyroo.Query`, and Tailwind compiled by the app.
- Place `<KlyDataList TItem="T" Schema="..." Items="..." />` (or `Queryable`, or `Provider`: exactly one source). Without child content it renders a default layout. With child content, place the parts you want: `KlyDataListTable` with column children (`KlyDataListColumn` with a typed `Property="@(x => x.Total)"`, `KlyDataListFieldColumn` with a schema `Field="total"`, `KlyDataListTemplateColumn` with a `Template` and a `Key`; the row and value types are inferred, write no type argument), `KlyDataListGrid`, `KlyDataListSearch`, `KlyDataListFilterTags`, `KlyDataListPagination`, `KlyDataListInfiniteLoader`, `KlyDataListSelectionActions`.
- Declare the schema once in a `static readonly` field: `DataListSchema.For<T>(s => s.RowKey(...).Search(...).Field(x => x.Name, f => f.Sortable().Filter(DataListFilter.Text())))`. Selection and cursor paging need a `RowKey`.
- State lives in the query string by default (`q`, `p`, `ps`, `s`, `f.<key>`, `c`, `l`); give each list on a page a `UrlKey`, or set `UrlSync="false"`.
- Group with `DataListGroupBy<T>.By(x => x.Status)` (typed value) or `.ByField("status")`; `DataListSummaries<T>.ResolveAsync` returns `DataListSummaryValue` (a number or a text) per column key.
- `DataListRowSummary<T>`, `DataListGroupBy<T>`, `DataListSummaries<T>`, `DataListAction<T>` and `DataListRowAction<T>` belong to `Klyroo.Table`, not to the core package.

Documentation lives in the repository: `README.md`, `docs/getting-started.md`, `docs/guides.md`, `docs/concepts.md`, `docs/platforms.md`, `docs/faq.md` and the generated `docs/api-reference.md`.
