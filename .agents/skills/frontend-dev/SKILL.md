---
name: frontend-dev
description: "Use this skill when the user wants to create, modify, or fix frontend UI in the ToDo.Web Blazor project. This includes adding pages, components, layouts, styling with Tailwind CSS 4.3+, responsive design, accessibility, animations, theming, or any visual/interactive work. Trigger on any mention of 'page', 'component', 'UI', 'layout', 'Blazor', 'Tailwind', 'responsive', 'mobile', 'dark mode', 'accessibility', 'a11y', 'CSS', 'styling', or requests to change how something looks or behaves in the browser. Do not use for API endpoints (use api-creator), Aspire infrastructure, or backend logic."
---

# Frontend Development Skill

Use this skill to build, modify, and style frontend UI in the `ToDo.Web` Blazor Server project using Tailwind CSS 4.3+ with responsive, accessible, and performant patterns.

## When to use

- Creating new pages or components
- Modifying existing UI (layout, styling, interactions)
- Adding responsive breakpoints or mobile-first design
- Implementing accessibility (ARIA, keyboard nav, focus management)
- Theming, dark mode, or design system changes
- Adding animations or transitions
- Fixing CSS/layout issues
- Updating the Tailwind theme or design tokens

## When NOT to use

- API endpoint work (use `api-creator`)
- Aspire infrastructure (use `aspire`)
- Backend logic or database changes
- Integration tests for API endpoints

## Architecture

### Project structure

```
ToDo.Web/
├── Components/
│   ├── App.razor                  # Root document (HTML shell, fonts, meta)
│   ├── Routes.razor               # Router
│   ├── _Imports.razor             # Global using directives
│   ├── Layout/
│   │   ├── MainLayout.razor       # App shell (sidebar + content area)
│   │   ├── MainLayout.razor.css   # Scoped styles (avoid — prefer Tailwind)
│   │   ├── NavMenu.razor          # Sidebar navigation
│   │   └── NavMenu.razor.css      # Scoped styles (avoid — prefer Tailwind)
│   └── Pages/
│       ├── Home.razor             # Main todo list page
│       └── Error.razor            # Error page
├── Styles/
│   └── app.css                    # Tailwind entry point + theme config
├── wwwroot/
│   └── css/app.built.css          # Built Tailwind output (do not edit)
├── TodoApiClient.cs               # Typed HTTP client for API calls
├── Program.cs                     # App startup
└── package.json                   # Tailwind CLI tooling
```

### Render mode

- **Interactive Server** (`@rendermode="InteractiveServer"`) — all components use SignalR for interactivity
- No WASM or static SSR in this project

### Communication with API

- Use `TodoApiClient` (injected via `@inject TodoApiClient TodoApi`)
- Base address is `https+http://apiservice` (Aspire service discovery)
- Add new methods to `TodoApiClient.cs` when the API grows

## Tailwind CSS 4.3+ Conventions

### Setup

- Tailwind 4.3+ with the new CSS-first configuration (no `tailwind.config.js`)
- Entry point: `Styles/app.css`
- Build: `npm run css:build` (or `npm run css:watch` during development)
- Output: `wwwroot/css/app.built.css`

### Theme (Design Tokens)

All custom colors and fonts are defined in `Styles/app.css` using the `@theme` directive:

```css
@theme {
  --font-sans: "Nunito", "Segoe UI", system-ui, -apple-system, sans-serif;
  --font-heading: "Quicksand", "Segoe UI", system-ui, -apple-system, sans-serif;

  --color-healdocs-bg: #e8edf5;
  --color-healdocs-sidebar: #ffffff;
  --color-healdocs-card-pink: #fce4ec;
  --color-healdocs-card-lavender: #ede7f6;
  --color-healdocs-card-yellow: #fff9e6;
  --color-healdocs-card-green: #e0f2e9;
  --color-healdocs-accent: #e53935;
  --color-healdocs-text: #1e293b;
  --color-healdocs-text-muted: #64748b;
  --color-healdocs-border: #e2e8f0;
  --color-healdocs-active-bg: #eef2ff;
  --color-healdocs-active-text: #4f46e5;
}
```

**Rules:**
- Always use theme tokens (e.g., `text-healdocs-text`, `bg-healdocs-bg`) — never hardcode hex values in markup
- Add new tokens to `@theme` when the design requires new colors/spacing
- Use `--color-*` prefix for colors, `--font-*` for font families, `--spacing-*` for custom spacing

### Tailwind 4.x key differences from v3

- **No `tailwind.config.js`** — all configuration is in CSS via `@theme`, `@source`, `@layer`
- **`@source` directive** — tells Tailwind where to scan for class usage:
  ```css
  @source "../Components/**/*.razor";
  @source "../Components/**/*.cshtml";
  ```
- **`@theme` directive** — replaces `theme.extend` in the old config
- **No `@apply` in component styles** — prefer utility classes directly in markup
- **New color syntax** — use `bg-healdocs-accent/80` for opacity (slash syntax)

### Utility-first rules

1. **Prefer utility classes in markup** over custom CSS classes
2. **Extract components** (Blazor `.razor` files) instead of creating CSS abstractions
3. **Use `@layer base`** only for truly global resets (already set in `app.css`)
4. **Never edit `wwwroot/css/app.built.css`** — it's auto-generated

## Responsive Design

### Mobile-first approach

Always design for mobile first, then add breakpoints for larger screens:

```html
<!-- Mobile: single column, Tablet: 2 cols, Desktop: 3 cols -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
```

### Breakpoints (Tailwind defaults)

| Prefix | Min-width | Target |
|--------|-----------|--------|
| (none) | 0px | Mobile (default) |
| `sm:` | 640px | Large phones / small tablets |
| `md:` | 768px | Tablets |
| `lg:` | 1024px | Laptops |
| `xl:` | 1280px | Desktops |
| `2xl:` | 1536px | Large desktops |

### Responsive patterns used in this project

- **Sidebar**: Hidden on mobile (`hidden md:flex`), full sidebar on `md:` and up
- **Grid layouts**: `grid-cols-1 md:grid-cols-2 lg:grid-cols-3`
- **Flex wrapping**: `flex flex-col sm:flex-row` for form layouts
- **Container widths**: Use `max-w-*` utilities for content containment

### Mobile navigation (when adding)

For mobile, add a hamburger menu that reveals the sidebar as an overlay:
- Use Blazor `@onclick` to toggle visibility
- Animate with `transition-transform` and `translate-x`
- Add backdrop overlay with `bg-black/50`

## Accessibility (a11y)

### Required practices

1. **Semantic HTML** — use `<nav>`, `<main>`, `<header>`, `<aside>`, `<button>`, `<label>` correctly
2. **ARIA labels** — add `aria-label` on icon-only buttons, `aria-describedby` for form hints
3. **Focus management** — visible focus rings (`focus:ring-2 focus:ring-healdocs-active-text/20`)
4. **Keyboard navigation** — all interactive elements must be reachable via Tab and operable via Enter/Space
5. **Color contrast** — maintain WCAG AA (4.5:1 for text, 3:1 for large text/UI)
6. **Alt text** — all `<img>` elements must have meaningful `alt` attributes
7. **Form labels** — every input must have an associated `<label>` (visible or `sr-only`)
8. **Loading states** — use `aria-busy="true"` and `aria-live="polite"` for async content

### Focus ring pattern

```html
<button class="... focus:outline-none focus:ring-2 focus:ring-healdocs-active-text/20 focus:border-healdocs-active-text">
```

### Screen-reader only text

```html
<span class="sr-only">Close menu</span>
```

## Component Patterns

### New page template

```razor
@page "/my-page"

@inject TodoApiClient TodoApi

<PageTitle>My Page</PageTitle>

<div class="space-y-6">
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <h1 class="text-2xl font-bold text-healdocs-text">Page Title</h1>
    </div>

    @* Content *@
</div>

@code {
    // Component logic
}
```

### Reusable component template

```razor
@* Components/Shared/Card.razor *@

<div class="rounded-2xl p-5 shadow-sm border border-healdocs-border bg-white transition-all hover:shadow-md @AdditionalClasses">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string AdditionalClasses { get; set; } = "";
}
```

### Button variants

```razor
@* Primary *@
<button class="px-5 py-2 rounded-xl bg-healdocs-accent text-white text-sm font-semibold shadow-sm hover:bg-red-600 active:bg-red-700 transition-colors">

@* Secondary *@
<button class="px-5 py-2 rounded-xl bg-healdocs-active-text text-white text-sm font-semibold hover:bg-indigo-600 active:bg-indigo-700 transition-colors">

@* Ghost *@
<button class="px-5 py-2 rounded-xl text-sm font-medium text-healdocs-text-muted hover:bg-healdocs-active-bg hover:text-healdocs-active-text transition-colors">
```

### Card color rotation

Use the existing pattern for visual variety:
```csharp
private static readonly string[] cardColors = ["bg-healdocs-card-pink", "bg-healdocs-card-lavender", "bg-healdocs-card-yellow", "bg-healdocs-card-green"];
private string GetCardColor(int id) => cardColors[id % cardColors.Length];
```

## Animations & Transitions

### Preferred patterns

```html
<!-- Smooth color/shadow transitions -->
<div class="transition-all hover:shadow-md">

<!-- Scale on hover (cards, buttons) -->
<div class="transition-transform hover:scale-[1.02]">

<!-- Fade in (loading states) -->
<div class="animate-pulse">

<!-- Spin (loading spinners) -->
<svg class="animate-spin">
```

### Page transitions (when adding)

Use CSS transitions with Blazor's `NavigationManager` — avoid heavy JS libraries.

## Performance Best Practices

1. **Minimize re-renders** — use `@key` on list items to help Blazor diff efficiently
2. **Debounce search** — for search inputs, consider a debounce pattern before calling the API
3. **Lazy load** — use `@if` to conditionally render expensive component trees
4. **Optimize images** — use WebP/AVIF formats, include `width`/`height` attributes, use `loading="lazy"`
5. **Avoid scoped CSS** — prefer Tailwind utilities (scoped CSS generates extra HTTP requests)
6. **Batch state updates** — call `StateHasChanged()` only when necessary (Blazor auto-calls after events)

## Dark Mode (when implementing)

Use Tailwind's `dark:` variant with CSS custom properties:

```css
@theme {
  /* Light mode tokens already defined */
  --color-healdocs-bg: #e8edf5;
  /* Add dark variants */
}

@media (prefers-color-scheme: dark) {
  :root {
    --color-healdocs-bg: #1a1a2e;
    --color-healdocs-text: #e2e8f0;
    /* ... override all tokens */
  }
}
```

## Build & Verify Commands

```bash
# Build Tailwind CSS
cd ToDo.Web && npm run css:build

# Watch mode (during development)
cd ToDo.Web && npm run css:watch

# Build the web project
dotnet build ToDo.Web/ToDo.Web.csproj

# Run the full app (through Aspire)
dotnet run --project ToDo.AppHost

# Run frontend tests
dotnet test ToDo.Tests/ToDo.Tests.csproj --filter "FullyQualifiedName~WebTests"
```

## Checklist for frontend changes

- [ ] Mobile-first: works on 320px width and up
- [ ] Responsive: tested at `sm:`, `md:`, `lg:` breakpoints
- [ ] Accessible: semantic HTML, ARIA labels, focus visible, keyboard navigable
- [ ] Themed: uses design tokens from `@theme` (no hardcoded colors)
- [ ] Performant: no unnecessary re-renders, images optimized
- [ ] Consistent: follows existing component patterns (rounded-2xl cards, same spacing)
- [ ] Tailwind CSS rebuilt: `npm run css:build` after adding new utilities
- [ ] Build passes: `dotnet build ToDo.Web/ToDo.Web.csproj`
