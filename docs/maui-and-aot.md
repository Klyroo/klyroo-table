# .NET MAUI & NativeAOT Guide — Lux.Table

## .NET MAUI Blazor Hybrid (iOS, Android, macOS Catalyst, Windows)

`Lux.Table` declares explicit `<SupportedPlatform>` entries for `browser`, `ios`, `android`, `maccatalyst`, and `windows`, so it compiles cleanly in multi-targeted `.NET MAUI` projects with zero platform-compatibility analyzer warnings.

### 1. Register in `MauiProgram.cs`

```csharp
var builder = MauiApp.CreateBuilder();
builder.UseMauiApp<App>();
builder.Services.AddMauiBlazorWebView();

// Register Lux.Table
builder.Services.AddLuxTable();
```

### 2. Reference Stylesheets in `wwwroot/index.html`

```html
<link rel="stylesheet" href="_content/Lux.UI/lux.preflight.css" />
<link rel="stylesheet" href="_content/Lux.UI/lux.css" />
```

## Native C# .NET MAUI (Without BlazorWebView)

If your MAUI application uses XAML or C# Markup instead of `BlazorWebView`, install only **`Lux.Table.Core`**:

```bash
dotnet add package Lux.Table.Core
```

Because `Lux.Table.Core` has zero ASP.NET Core or Razor dependencies, you can drive your MAUI ViewModels directly from its headless controllers, schemas, and parsers.

## NativeAOT Publishing

`Lux.Table.Core` is verified with `<IsAotCompatible>true</IsAotCompatible>` and `<IsTrimmable>true</IsTrimmable>`:

```bash
dotnet publish -c Release -p:PublishAot=true
```
