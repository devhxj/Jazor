import { editor, MarkerSeverity } from 'monaco-editor/editor/editor.api.js';
import { DetachAndRestoreModel, MountEditor, ObserveContent, ObserveModelDisposal, ObserveView, ReadAttachedModelId, ReadMarker, ReadViewHeight, TypeText, UpdateEditor } from './App.jazor';

const host = document.getElementById('editor');
const transport = new Worker(new URL('./editor-worker.js', import.meta.url), { type: 'module' });
const terminate = transport.terminate.bind(transport);
// Monaco terminates the browser worker after its transport promise settles.
const terminated = new Promise(resolve => {
    transport.terminate = () => {
        terminate();
        resolve();
    };
});
let dispose;
const workerResult = new Promise((resolve, reject) => {
    transport.addEventListener('error', reject, { once: true });
    dispose = MountEditor(host, transport, resolve);
});
const [instance] = editor.getEditors();
const [model] = editor.getModels();
if (!instance || !model || instance.getModel() !== model || instance.getValue() !== 'one line')
    throw new Error('Expected the generated editor to mount the generated model');
const modelId = ReadAttachedModelId();
if (typeof modelId !== 'string' || modelId !== model.id)
    throw new Error('Expected the generated getter to return the attached model and its string ID');
if (!DetachAndRestoreModel() || instance.getModel() !== model)
    throw new Error('Expected null after detaching and the same model after restoring');
const originalMarkers = editor.getModelMarkers({ resource: model.uri });
if (originalMarkers.length !== 2)
    throw new Error('Expected both owners before clearing');
if (originalMarkers.some(marker => marker.message !== 'typed warning' ||
    marker.severity !== MarkerSeverity.Warning || marker.endColumn !== 2))
    throw new Error('Expected the generated calls to write the warning and range');
if (ReadMarker() !== 'keep-me|typed warning|warning')
    throw new Error('Expected the generated call to read the typed marker');
UpdateEditor();
const markers = editor.getModelMarkers({ resource: model.uri });
if (markers.length !== 1 || markers[0].owner !== 'keep-me')
    throw new Error('Expected only the other owner to remain');
await new Promise(requestAnimationFrame);
const background = host.querySelector('.monaco-editor-background');
if (!background || getComputedStyle(background).backgroundColor !== 'rgb(18, 52, 86)')
    throw new Error('Expected the generated theme to change the Monaco background');
if (await workerResult !== 'one line')
    throw new Error('Expected the Monaco worker to read the synced model');
const versions = [];
const unsubscribe = ObserveContent(version => versions.push(version));
instance.setPosition({ lineNumber: 1, column: 1 });
TypeText('typed ');
if (model.getValue() !== 'typed one line' || versions.length !== 1 || versions[0] !== model.getVersionId())
    throw new Error('Expected the generated command to insert text and report one content event');
unsubscribe();
model.setValue('third version');
if (versions.length !== 1)
    throw new Error('Expected the generated disposal to unsubscribe');

const outside = document.body.appendChild(document.createElement('button'));
outside.focus();
const transitions = [];
const widths = [];
const focused = Promise.withResolvers();
const blurred = Promise.withResolvers();
const stopView = ObserveView(() => {
    transitions.push('focus');
    focused.resolve();
}, () => {
    transitions.push('blur');
    blurred.resolve();
}, width => widths.push(width));
instance.focus();
await focused.promise;
// Detaching the focused model makes Monaco report a text blur immediately.
instance.setModel(null);
await blurred.promise;
instance.setModel(model);
instance.layout({ width: 410, height: 205 });
if (transitions.join(',') !== 'focus,blur' || widths.at(-1) !== 410 || ReadViewHeight() !== 205)
    throw new Error('Expected typed focus, blur and layout notifications and a matching layout snapshot: ' + JSON.stringify({ transitions, widths, height: ReadViewHeight() }));
stopView();
const resizeCount = widths.length;
instance.focus();
instance.layout({ width: 400, height: 200 });
await new Promise(requestAnimationFrame);
if (transitions.length !== 2 || widths.length !== resizeCount)
    throw new Error('Expected focus and layout listeners to be released');
let disposals = 0;
ObserveModelDisposal(() => disposals++);
dispose();
if (host.querySelector('.monaco-editor') || editor.getEditors().includes(instance) || !model.isDisposed() || disposals !== 1)
    throw new Error('Expected the generated cleanup to dispose the editor and model');
await terminated;
document.body.dataset.monacoVerified = 'true';
