# Unity Safe Delete with Reference Checking

Production-grade Editor extension implementing a reusable **Asset Reference
Database**, with "Delete With Reference Check" as one consumer of it.

## Install

Copy the `ReferenceDeletion/` folder anywhere under your project's `Assets/`
(or `Packages/`) directory. Everything is editor-only, enforced by
`ReferenceDeletion.Editor.asmdef` (`includePlatforms: ["Editor"]`), so it
never ships in player builds regardless of which physical folder it lives in.

## Use

- Right-click any asset (or select multiple) → **Assets > Delete With Reference Check**.
- No references → a simple confirmation dialog.
- References found → a window listing every referencing asset (icon, type,
  path) with search, sortable columns, multi-select, Ping/Select/Open, and
  **Delete Anyway** / **Cancel**.
- **Tools > Reference Deletion > Rebuild Index** forces a full rescan (rarely
  needed — the index updates incrementally on every import/delete/move).

## How it stays fast at 100k+ assets

```
Delete Request → Asset GUID → Reverse Reference Index → Immediate Result
```

The delete tool **never scans the project**. A background-built,
disk-persisted reverse index (`Referenced Asset → Assets Using It`) is
queried in O(1) average case. The index is built once (first use after a
fresh clone/checkout) and afterwards only ever touched incrementally via
`AssetPostprocessor.OnPostprocessAllAssets`.

Cache lives under `Library/ReferenceDeletion/` — never `Assets/`, so it's
never version controlled and never conflicts between teammates.

## Architecture

```
ReferenceDeletion/
    Core/                   Indexes, the database facade, scan orchestration
        AssetReferenceDatabase   IReferenceDatabase impl — the one shared service
        AssetIndexer             IAssetIndexer impl — only class allowed to full-scan
        ForwardReferenceIndex    Asset -> what it references
        ReverseReferenceIndex    Asset -> what references it (queried on delete)
        AssetReferenceScanner    Composite dispatcher over IReferenceScanner strategies
        CacheSerializer          CacheData DTO <-> live index objects

    Scanners/               IReferenceScanner strategy implementations
        YamlScanner              Prefabs/scenes/materials/etc — manual span parsing, no regex
        DependencyScanner        AssetDatabase.GetDependencies() fallback for binary assets
        SerializedObjectScanner  Custom ScriptableObjects, via SerializedProperty walk
        SceneScanner             Optional deep mode (opens scene additively)

    Services/               Wiring between Unity events and the database
        AssetChangeListener      AssetPostprocessor hook (thin, no logic)
        IndexUpdateService       Applies incremental changes, persists cache
        CacheService              Facade for manual rebuild/clear actions

    Editor/                 UI and entry points
        DeleteWithReferenceCommand   Context menu entry point
        DeleteConfirmationWindow     "No references, delete?" dialog
        ReferenceResultWindow        Reference list window
        ReferenceDatabaseMenu        Manual maintenance menu items

    Models/                 Plain data types (AssetMetadata, ReferenceResult, CacheData, ...)
    Persistence/            CacheStorage — raw binary read/write under Library/
    Interfaces/             IReferenceDatabase, IAssetIndexer, ICacheStorage, ILogger
    Utils/                  GuidParser, AssetUtility, ProgressScope, Logger
    Tests/Editor/           NUnit unit tests (GuidParser, indexes)
```

Every class has one job; everything depends on interfaces and receives its
collaborators via constructor injection, except `AssetReferenceDatabase`,
which is deliberately the single shared/global service
(`AssetReferenceDatabase.Instance`).

## Extending

Every future tool — Find References, Safe Move/Rename, dependency graph,
unused-asset finder, circular-dependency detection, build/Addressables
analysis — calls the same query surface and needs **no additional indexing**:

```csharp
ReferenceResult result = AssetReferenceDatabase.Instance.FindReferences(guid);
IReadOnlyCollection<string> outgoing = AssetReferenceDatabase.Instance.GetForwardReferences(guid);
```

To support a new asset type or serialization format, implement
`IReferenceScanner` and register it in
`AssetReferenceDatabaseFactory.CreateDefault()` (in
`Core/AssetReferenceDatabase.cs`) — ordered before the generic
`DependencyScanner` fallback if it needs to take precedence.

## Notes / things to verify in your project

- `SceneScanner` is off by default (`Enabled = false`) since opening every
  scene additively is expensive; flip it on in
  `AssetReferenceDatabaseFactory.CreateDefault()` only if you need maximum
  accuracy on scene-embedded references and can accept slower full builds.
- The YAML scanner assumes text-serialized assets (Project Settings → Editor
  → Asset Serialization → **Force Text**). Binary-serialized projects will
  fall through to `DependencyScanner`, which is less granular for
  in-file-only references.
- `.asmdef` files assume the Unity Test Framework package is installed for
  `Tests/Editor` to compile; delete that folder if you don't use it.
