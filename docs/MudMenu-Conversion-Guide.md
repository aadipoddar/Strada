# Task: Convert a page's Syncfusion `SfMenu` to MudBlazor `MudMenu`

You are editing **one Blazor page** in the Strada repo. Each page is two files:
`<PageName>.razor` (markup) and `<PageName>.razor.cs` (logic). Convert the header
menu from Syncfusion `SfMenu` to MudBlazor `MudMenu`, then delete the now-dead
handler. **Do not change anything else on the page.** Keep all labels, shortcut
hints, `Id` → action mappings, and `Disabled` conditions identical.

When done, build `Strada/Strada.Shared/Strada.Shared.csproj` and confirm 0 errors.

---

## Step 1 — Read the menu markup in `.razor`

Find the block that looks like this (inside `<LeftContent>` of `<Header>`):

```razor
<SfMenu TValue="Syncfusion.Blazor.Navigations.MenuItem">
    <MenuAnimationSettings ... />                          <!-- may or may not be present -->
    <MenuEvents TValue="Syncfusion.Blazor.Navigations.MenuItem" ItemSelected="OnMenuSelected" ... />
    <MenuItems>
        <MenuItem Text="File" IconCss="e-icons e-folder">      <!-- TOP-LEVEL group -->
            <MenuItems>
                <MenuItem Text="New (Ctrl + N)" Id="NewTransaction" Disabled="@_isProcessing" />  <!-- LEAF -->
                <MenuItem Separator="true" />                                                     <!-- DIVIDER -->
                <MenuItem Text="Export PDF (Ctrl + P)" Id="ExportPdf" Disabled="@_isProcessing" />
            </MenuItems>
        </MenuItem>
        <MenuItem Text="Transaction" ...>                     <!-- another TOP-LEVEL group -->
            <MenuItems> ... </MenuItems>
        </MenuItem>
    </MenuItems>
</SfMenu>
```

- **Top-level `MenuItem`** (has a nested `<MenuItems>`) = a menu button → becomes one `<MudMenu>`.
- **Leaf `MenuItem`** (has an `Id="..."`) = a clickable row → becomes one `<MudMenuItem>`.
- **`MenuItem Separator="true"`** → becomes `<MudBlazor.MudDivider />`.

## Step 2 — Read the handler in `.razor.cs`

Find the method `OnMenuSelected`. It is a `switch` on `args.Item.Id`:

```csharp
private async Task OnMenuSelected(Syncfusion.Blazor.Navigations.MenuEventArgs<Syncfusion.Blazor.Navigations.MenuItem> args)
{
    switch (args.Item.Id)
    {
        case "NewTransaction": ResetPage(); break;
        case "SaveTransaction": await SaveTransaction(); break;
        case "ExportPdf": await ExportMaster(); break;
        case "ExportExcel": await ExportMaster(true); break;
        ...
    }
}
```

Each `Id` maps to a method call. **That call body is exactly what the `OnClick` will run.**

## Step 3 — Rewrite the markup

Replace the entire `<SfMenu>...</SfMenu>` block with one `<MudBlazor.MudMenu>` per
top-level group. For each leaf, take its `Text`, find the matching `case "<Id>":` in
`OnMenuSelected`, and put that call into `OnClick`.

**`OnClick` rule:**
- Call with **no arguments** → use the method group: `OnClick="MethodName"`
  (works for both `void` and `async Task` methods, e.g. `ResetPage` or `SaveTransaction`).
- Call **with arguments** → use a lambda: `OnClick="() => ExportMaster(true)"`.

**Style (match exactly):**
- Use the fully-qualified `MudBlazor.` prefix on every tag (`MudBlazor.MudMenu`,
  `MudBlazor.MudMenuItem`, `MudBlazor.MudDivider`). This avoids a `MenuItem` type
  clash with Syncfusion.
- Put the label in the `Label="..."` attribute. Keep the shortcut hint text verbatim.
- Escape `&` as `&amp;` (e.g. `Label="Save &amp; PDF (Ctrl + P)"`).
- Add only `Dense="true"` to each `<MudBlazor.MudMenu>` — **no `Variant`, no icons.**
  Drop every `IconCss`.
- **Disabled handling (important):**
  - Almost every leaf in the old menu has the same `Disabled="@_isProcessing"`. Put that
    **once on the `<MudBlazor.MudMenu>` group** as `Disabled="_isProcessing"` (no `@`).
    Disabling the menu button makes all its items unreachable, so do **not** repeat
    `Disabled="@_isProcessing"` on the individual `<MudBlazor.MudMenuItem>`s.
  - Keep `Disabled` on a `<MudBlazor.MudMenuItem>` **only when its condition is more than
    just `_isProcessing`** — copy that fuller expression verbatim, e.g.
    `Disabled="@(_isProcessing || !Id.HasValue || Id.Value <= 0)"`.
- Keep dynamic labels as-is, e.g. `Label="@(_showDeleted ? "Hide Deleted (Ctrl + Delete)" : "Show Deleted (Ctrl + Delete)")"`.

### Worked example (master page)

The markup + handler above become:

```razor
<LeftContent>
    <MudBlazor.MudMenu Label="File" Dense="true" Disabled="_isProcessing">
        <MudBlazor.MudMenuItem Label="New (Ctrl + N)" OnClick="ResetPage" />
        <MudBlazor.MudMenuItem Label="Save (Ctrl + S)" OnClick="SaveTransaction" />
        <MudBlazor.MudMenuItem Label="@(_showDeleted ? "Hide Deleted (Ctrl + Delete)" : "Show Deleted (Ctrl + Delete)")" OnClick="ToggleDeleted" />
        <MudBlazor.MudDivider />
        <MudBlazor.MudMenuItem Label="Export Excel (Ctrl + E)" OnClick="() => ExportMaster(true)" />
        <MudBlazor.MudMenuItem Label="Export PDF (Ctrl + P)" OnClick="ExportMaster" />
    </MudBlazor.MudMenu>

    <MudBlazor.MudMenu Label="Transaction" Dense="true" Disabled="_isProcessing">
        <MudBlazor.MudMenuItem Label="Edit (Insert)" OnClick="EditSelectedItem" />
        <MudBlazor.MudMenuItem Label="Delete / Recover (Del)" OnClick="DeleteRecoverSelectedItem" />
    </MudBlazor.MudMenu>
</LeftContent>
```

Note how `Disabled="@_isProcessing"` left every leaf and now sits once on each group.
A leaf keeps its own `Disabled` only when the old condition was richer, e.g. the export
items on the Trip page:

```razor
<MudBlazor.MudMenuItem Label="Export PDF (Alt + P)" OnClick="ExportPdfInvoice" Disabled="@(_isProcessing || !Id.HasValue || Id.Value <= 0)" />
```

## Step 4 — Delete the dead handler in `.razor.cs`

Delete the **entire** `OnMenuSelected` method (signature through closing `}`).
**Do not touch** `OnGridContextMenuItemClicked` or any other `On...ContextMenu...`
method — those are still used by the grid right-click menu and also switch on
`args.Item.Id`. Only `OnMenuSelected` goes.

## Step 5 — Build & verify

```
dotnet build Strada/Strada.Shared/Strada.Shared.csproj -clp:ErrorsOnly
```

Must report **0 Errors**. If a leaf's `Id` has no matching `case`, leave that item
out and note it. If a `case` calls a method with arguments, use the lambda form.

---

## Checklist

- [ ] One `<MudBlazor.MudMenu Label="..." Dense="true" Disabled="_isProcessing">` per top-level group, in the same order. No `Variant`, no icons.
- [ ] Every leaf → `<MudBlazor.MudMenuItem Label="..." OnClick="..." />` with the action copied from the matching `case` (look it up — the `Id` text often differs from the method name, e.g. `PeriodToday` → `HandleDatesChanged(DateRangeType.Today)`).
- [ ] `Disabled="@_isProcessing"` moved up to the group; a leaf keeps `Disabled` only when its old condition was richer than plain `_isProcessing`.
- [ ] `Separator="true"` → `<MudBlazor.MudDivider />`.
- [ ] All `IconCss` removed; `&` written as `&amp;`.
- [ ] `OnMenuSelected` method deleted from `.razor.cs`; other context-menu handlers untouched.
- [ ] Build is clean (0 errors).
