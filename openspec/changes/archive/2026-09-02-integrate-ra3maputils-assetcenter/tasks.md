## 1. Asset Identity and Secret Boundary

- [x] 1.1 Create checked-in AssetCenter metadata, directories, and ignore rules for generated output and private material
- [x] 1.2 Generate the dedicated Ra3MapUtils signing key outside the repository and place only its public document in the repository
- [x] 1.3 Register `cn.dreamness.ra3maputils`, create beta/stable channels, and record scoped credential/operator handoff without committing secrets

## 2. Provider-Neutral CI/CD Tooling

- [x] 2.1 Add shared PowerShell validation and tool-resolution helpers
- [x] 2.2 Implement explicit-version .NET 10 win-x64 publish, AssetCenter pack, and independent verification
- [x] 2.3 Implement authenticated draft creation, resumable upload, and immutable release finalization
- [x] 2.4 Implement local validation and submission of an already signed beta/stable channel publication
- [x] 2.5 Add CI environment/template documentation without depending on GitHub tags, Releases, or Actions

## 3. SDK and Client Integration Documentation

- [x] 3.1 Document Ra3MapUtils embedded SDK configuration, lifecycle hooks, update UI boundary, and telemetry consent
- [x] 3.2 Document Bootstrapper installation layout, health confirmation, user-data separation, rollback, and beta-to-stable rollout

## 4. Verification

- [x] 4.1 Run OpenSpec strict validation and PowerShell static/parser checks for all new scripts
- [x] 4.2 Build the v2 UI project or document only pre-existing build blockers, and verify the repository additions contain no private key or credential
