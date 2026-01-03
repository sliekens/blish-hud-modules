# Chat Links Module

## Module Data Mechanism

The `ChatLinks` module keeps its static data (items, achievements, etc.) in a local SQLite database stored in the module data directory.

While the module can generate this database if it doesn't exist, it is too slow to do so. To improve first-time startup performance, the module downloads pre-seeded SQLite database files hosted on `bhm.blishhud.com` ([information](https://blishhud.com/docs/modules/ssrd/additional-services#static-hosting)).

This mechanism ensures the module always starts with a valid, integrity-checked snapshot of the static data served from the CDN.

## Updating Seed Data (For Contributors)

When updating the DbContext, it can be necessary to update the static seed databases served by the CDN.

1. Update the DbContext
   - Make necessary changes to the `ChatLinks.EF` project (`src\ChatLinks.EF`) to reflect new or modified entities, properties, or relationships.
   - Ensure that migrations are created and applied locally to verify the schema changes.
   - Increment `ChatLinksContext.SchemaVersion` as appropriate.
2. Regenerate Seed Databases
   - Use the `ChatLinks.Seeder` project (`src\ChatLinks.Seeder`) to regenerate the SQLite seed databases for items, achievements, and other entities.
3. Update CDN Data
   - Copy the data from `artifacts` to the `bhud-static/sliekens.chat_links` branch
     - e.g. copy database files to `v{schema_version}/`
   - Add the new schema version to the `update-seed-index.ps1` script
     - TODO: figure out how to make this automatic
   - Run the `update-seed-index.ps1` script to compresses the database files, generate checksums and update `seed-index.json`
   - Commit and push the changes to the `bhud-static/sliekens.chat_links` branch.

Always verify that the module starts cleanly and that in-game queries relying on the static data (e.g., item/achievement lookups) behave as expected after seed updates.
