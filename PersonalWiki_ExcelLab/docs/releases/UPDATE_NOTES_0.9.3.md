# Update Notes — 0.9.3 dev

## Added
- `db-batch-source-reconciliation <database.db> <report.json>`: read-only aggregate comparison of import batch imported-row counters against `entry_sources` records.
- Reports batches with/without source records, counter mismatches, source rows exceeding the imported counter, unbatched source records, and source records pointing to missing batches.

## Safety
- SQLite opened in read-only mode; no database or source files are changed.
- Output contains aggregate counts only; no batch IDs, source paths, or record contents.
- Report uses `FileMode.CreateNew` and will not overwrite an existing file.
- SQLite Schema Version remains 1. No dependency changes.

## Validation
- Static wiring/version checks performed. Build and runtime tests not run; reserve for the planned consolidated acceptance pass.
