import { Create } from './cell-library.mjs';
import { Direct, Read, Roundtrip, Select, Weight, PlainLabel, ConfiguredLabel, NumericLabel } from './cell-consumer.mjs';
import { Create as CreateAliased, Roundtrip as AliasedRoundtrip, Count, Weight as LocalWeight, AttachedLabel, Prefix } from './aliased-cell-consumer.mjs';
import { Create as CreateShared, Pass as PassShared, Label as LabelShared, OmitMiddleSlot, ForwardMethodGroup } from './shared-cell-consumer.mjs';
import { Create as CreateSharedA } from './shared-cell-producer-a.mjs';
import { Pass as PassSharedB } from './shared-cell-producer-b.mjs';

const cell = Read('from-consumer');
if (cell.value !== 'from-consumer' || cell.label !== 'original')
    throw new Error('The ordinary C# consumer did not return the declared npm record.');
const libraryCell = Create('from-library');
const direct = Direct('direct');
if (direct.value !== 'direct' || Roundtrip(direct) !== direct || Roundtrip(cell) !== cell || Roundtrip(libraryCell) !== libraryCell)
    throw new Error('The npm callback failed to preserve JavaScript record identity.');
if (Select(cell) !== 'from-consumeroriginal' || Weight('second') !== 4 || Weight('missing') !== null || Weight('toString') !== null)
    throw new Error('The callback or the npm dictionary returned an incorrect result.');
console.log('Ordinary C# → DLL → ESM preserved named npm record identity, a separately projected generic callback, and dictionary indexing.');
const aliased = CreateAliased('aliased');
if (aliased.value !== 'aliased' || aliased.label !== 'original' || AliasedRoundtrip(aliased) !== aliased ||
    Count() !== 2 || LocalWeight('first') !== 2 || LocalWeight('missing') !== null)
    throw new Error('Alias-only references did not preserve the producer calls and local npm projection.');
console.log('Alias-only DLL references and the local npm projection ran through the same package.');
if (PlainLabel() !== 'plain' || ConfiguredLabel() !== 'loud:configured' || NumericLabel() !== 'plain:7' ||
    AttachedLabel() !== 'loud:attached' || Prefix() !== 'loud')
    throw new Error('The npm callable, union options or attached member returned an incorrect result.');
console.log('Callable npm exports preserved typed options, attached methods and read-only properties.');
const shared = CreateSharedA('shared');
if (PassSharedB(shared) !== shared || PassShared(shared) !== shared ||
    CreateShared('from-consumer').value !== 'from-consumer' || LabelShared('shared') !== 'original')
    throw new Error('The producers did not preserve the shared contract or its JavaScript object identity.');
console.log('Two producers used the same CellContracts-owned projection through their public APIs.');
if (OmitMiddleSlot() !== '3:first|undefined|third' || ForwardMethodGroup() !== '3:first|second|third')
    throw new Error('The cross-DLL npm call or method group did not preserve JavaScript argument positions.');
