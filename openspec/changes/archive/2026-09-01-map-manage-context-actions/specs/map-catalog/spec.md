## REMOVED Requirements

### Requirement: First version is read-only for map files
**Reason**: Map management now includes context-menu file operations (including delete) under `map-file-actions`.
**Migration**: Mutating map directory operations are specified by `map-file-actions`; listing/preview requirements in this capability remain in force.
