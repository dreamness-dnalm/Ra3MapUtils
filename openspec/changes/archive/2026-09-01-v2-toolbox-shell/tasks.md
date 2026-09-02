## 1. Catalog foundation

- [x] 1.1 Add ToolEntry / category / availability models and a static ToolboxCatalog (Core or UI per design)
- [x] 1.2 Add launch abstraction that opens a registered tool or reports unavailable
- [x] 1.3 Register FastHash and image encoding entries with categories

## 2. Toolbox page and shell

- [x] 2.1 Add Fluent ToolBoxPage with category sections bound to the catalog
- [x] 2.2 Wire launch command from page/ViewModel to the launch abstraction
- [x] 2.3 Add 工具箱 navigation item and DI registration (page + ViewModel + catalog/launcher)

## 3. Migrate two simple tools

- [x] 3.1 Port FastHash calculator window/ViewModel into `src/UI` and connect catalog launch
- [x] 3.2 Port image encoding tool window/ViewModel into `src/UI` and connect catalog launch
- [x] 3.3 Ensure icons/resources needed by those two tools are available in the UI project

## 4. Verification

- [x] 4.1 Build `src\UI\UI.csproj` successfully
- [x] 4.2 Manually open 工具箱, confirm grouped FastHash + image encoding entries, and launch both tools
