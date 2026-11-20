import { createApp, h, ref } from 'vue';
import App, { Advance, Step } from './.jolt/App.mjs';
import TableEditor from './.jolt/TableEditor.mjs';
import NpmCounter from './.jolt/NpmCounter.mjs';

window.joltNamedExports = [Step, Advance(2)];

const Root = {
    setup() {
        const canonical = ref(false);
        const legacyValue = ref('Legacy');
        const canonicalValue = ref('Canonical');
        const legacyWrites = ref(0);
        const canonicalWrites = ref(0);
        const showNpmCounter = ref(true);
        const npmCounters = { first: [], second: [] };
        window.joltNpmCounters = npmCounters;
        const counterProps = (name, initial) => {
            let observation;
            return {
                Initial: initial,
                OnSetup: counter => {
                    observation = { counter, changes: [] };
                    npmCounters[name].push(observation);
                },
                OnChange: value => observation.changes.push(value),
            };
        };
        const firstCounter = counterProps('first', 3);
        const secondCounter = counterProps('second', 10);
        const setLegacyValue = value => {
            legacyValue.value = value;
            legacyWrites.value++;
        };
        const setCanonicalValue = value => {
            canonicalValue.value = value;
            canonicalWrites.value++;
        };
        return () => [
            h(App),
            h('button', { id: 'npm-counter-toggle', onClick: () => showNpmCounter.value = !showNpmCounter.value }, 'Toggle first npm counter'),
            h('section', { id: 'npm-counter-first' }, showNpmCounter.value
                ? [h(NpmCounter, firstCounter)]
                : []),
            h('section', { id: 'npm-counter-second' }, [
                h(NpmCounter, secondCounter),
            ]),
            h('section', { id: 'vue-model-consumer' }, [
                h('button', { id: 'vue-model-toggle', onClick: () => canonical.value = !canonical.value }, 'Switch model prop'),
                h(TableEditor, canonical.value
                    ? { value: canonicalValue.value, 'onUpdate:value': setCanonicalValue }
                    : { Value: legacyValue.value, ValueChanged: setLegacyValue }),
                h('output', { id: 'vue-model-writes' }, legacyWrites.value + '|' + canonicalWrites.value),
            ]),
        ];
    },
};

createApp(Root).mount('#app');
