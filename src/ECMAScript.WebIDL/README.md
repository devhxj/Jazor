# ECMAScript.WebIDL

Archived legacy TypeScript WebIDL emitter.

Status:

- no longer part of `Jazor.slnx`
- no longer the active WebIDL generation pipeline
- kept temporarily for historical reference and migration fallback only

Active replacement:

- inventory collection and binding generation now live in `../ECMAScript.WebIDL.Generator/`
- generated artifacts now go under `src/ECMAScript/webidl`

Rule:

- do not add new generation rules here
- do not treat `app.ts` output as the source of truth
- remove this directory only after the remaining migration/history value is gone
