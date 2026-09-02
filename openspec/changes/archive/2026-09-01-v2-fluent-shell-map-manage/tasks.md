## 1. Solution and projects

- [x] 1.1 Create `src/Core/Core.csproj` (`net10.0` class library) and add it to `Ra3MapUtils.sln`
- [x] 1.2 Update `src/UI/UI.csproj`: set `AssemblyName` to `Ra3MapUtils`, reference `Core`, add CommunityToolkit.Mvvm and Microsoft.Extensions.DependencyInjection; suppress `WPF0001` if needed
- [x] 1.3 Verify `dotnet build src\UI\UI.csproj` produces `Ra3MapUtils.exe`

## 2. Core map catalog

- [x] 2.1 Add map entry model and `IMapCatalogService` (list maps for a root path / default root)
- [x] 2.2 Implement maps-root resolution matching legacy Documents RA3 maps folder semantics
- [x] 2.3 Implement map validity + enumeration (`{folderName}/{folderName}.map`), excluding non-maps; handle missing/unreadable root without throwing to UI crash
- [x] 2.4 Smoke-check catalog against a known maps folder (manual or small console/debug call)

## 3. Fluent app shell

- [x] 3.1 Enable native Fluent theme on `Application` (`ThemeMode=System` and/or Fluent resource dictionary); remove reliance on third-party UI chrome packages
- [x] 3.2 Replace default MainWindow with Settings-like layout: left nav + right content host
- [x] 3.3 Implement navigation ViewModel/items with at least 「地图」; default selection shows map management page on startup
- [x] 3.4 Wire DI in `App` startup (compose Core services + ViewModels + MainWindow); stop relying on bare `StartupUri` if it blocks DI

## 4. Map management page (read-only)

- [x] 4.1 Add MapManage page + ViewModel (CommunityToolkit.Mvvm) bound to `IMapCatalogService`
- [x] 4.2 Show live map list on page load; add refresh command that reloads from disk
- [x] 4.3 Surface empty/error state when maps root is missing or unreadable
- [x] 4.4 Confirm UI exposes no delete/rename/copy/compress actions in this version

## 5. Verification

- [x] 5.1 Build `src\UI\UI.csproj` and confirm output file name is `Ra3MapUtils.exe`
- [x] 5.2 Run the app: Fluent shell visible, left nav selects 地图, right side lists real maps and refresh works
- [x] 5.3 Confirm legacy root projects still build unchanged (no accidental deletion or package.ps1 cutover)
