## 1. Dependencies and assets

- [x] 1.1 Reference Dreamness map/script DLLs and ScriptExecutor from `Ra3MapUtils/lib` into `src/UI` (and Core as needed)
- [x] 1.2 Enable WinForms on the UI project; bring script dialog helpers with namespaces official Main.cs expect
- [x] 1.3 Copy official `data/nano_programs/**` to UI output; ensure Main.cs is not compiled as project source

## 2. Core nano catalog and v2 database

- [x] 2.1 Add v2 AppData path helper and SQLite database file `Ra3MapUtils.v2.db` (not `Ra3MapUtils.db`)
- [x] 2.2 Implement nano-program meta table + DAO (enable, wb-visible, order)
- [x] 2.3 Implement discovery for Official + User packages and merge with meta
- [x] 2.4 Implement execute orchestration (load Main.cs, pass argument dictionary, return structured result); reject disabled programs

## 3. Single instance and HTTP hosting

- [x] 3.1 Add Mutex `Ra3MapUtils` at startup; exit when not first instance
- [x] 3.2 Host ASP.NET Core API on `http://127.0.0.1:30033` from the UI process
- [x] 3.3 Implement `/api/nanoprogram/list`, `/wb_visible_list`, and `/run/{id}` compatible with legacy clients

## 4. Nano programs UI

- [x] 4.1 Add 微程序 navigation item and Settings-like nano programs page
- [x] 4.2 Wire list/search/enable/visibility/reorder/run/open-folder/refresh to Core services
- [x] 4.3 Add run-result presentation; keep MapFilePath behavior legacy-compatible (no map-page coupling)
- [x] 4.4 Register DI for nano services, page, and ViewModels

## 5. Verification

- [x] 5.1 Build `src\UI\UI.csproj` successfully
- [x] 5.2 Verify single-instance (second launch exits) and port 30033 list/run endpoints
- [x] 5.3 Manually run at least one dialog-based and one map-API official nano program from the UI
