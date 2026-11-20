import { createApp, h, nextTick, shallowReactive } from 'vue';
import KeyedRows from './component.js';
import './KeyedRows.css';

const check = (condition, message) => {
    if (!condition) throw new Error(message);
};
const state = shallowReactive({
    rows: Array.from({ length: 1000 }, (_, index) => String(index)),
    label: '500',
});
const events = [];
const callbacks = {
    onUpdate() { events.push('update'); state.label = 'edited'; },
    onInsert() { events.push('insert'); state.rows = ['new', ...state.rows]; },
    onReverse() { events.push('reverse'); state.rows = state.rows.toReversed(); },
    onSelect(row) { events.push(row); },
};

async function measure() {
    const mountStart = performance.now();
    createApp({ render: () => h(KeyedRows, { ...state, ...callbacks }) }).mount('#app');
    const mount = performance.now() - mountStart;
    const list = document.querySelector('#rows');
    const rows = [...list.children];
    const byKey = new Map(rows.map(row => [row.dataset.row, row]));
    check(rows.length === 1000 && byKey.size === 1000, 'Initial keyed rows');
    let focus = rows[0].querySelector('input');
    focus.focus();
    focus.value = 'draft';

    async function operation(action, expected) {
        const records = [];
        const observer = new MutationObserver(batch => records.push(...batch));
        observer.observe(list, { subtree: true, childList: true, characterData: true, attributes: true });
        const button = document.querySelector('#' + action);
        const start = performance.now();
        button.click();
        await nextTick();
        const time = performance.now() - start;
        const mutated = new Set();
        const add = node => {
            const row = (node.nodeType === Node.ELEMENT_NODE ? node : node.parentElement)?.closest('li');
            if (row) mutated.add(row.dataset.row);
        };
        records.push(...observer.takeRecords());
        for (const record of records) {
            add(record.target);
            for (const node of record.addedNodes) add(node);
            for (const node of record.removedNodes) add(node);
        }
        observer.disconnect();
        check(mutated.size === expected, action + ' mutated ' + mutated.size + ' rows, expected ' + expected);
        check(list.children.length === state.rows.length, action + ' row count');
        for (const [index, key] of state.rows.entries()) {
            const row = list.children[index];
            check(row.dataset.row === key, action + ' row order');
            if (byKey.has(key)) check(row === byKey.get(key), action + ' row identity ' + key);
            check(row.querySelector('span').textContent === (key === '500' ? state.label : key), action + ' row text');
        }
        check(document.activeElement === focus && focus.value === 'draft', action + ' focus and draft');
        return { time, mutated: mutated.size };
    }

    const update = await operation('update', 1);
    const insert = await operation('insert', 1);
    byKey.set('new', list.firstElementChild);
    // Focus the node Vue keeps in place while it moves the remaining rows.
    focus = list.firstElementChild.querySelector('input');
    focus.focus();
    focus.value = 'draft';
    const reverse = await operation('reverse', 1000);
    byKey.get('500').querySelector('button').click();
    check(events.join(',') === 'update,insert,reverse,500', 'Event count and selection after reorder');
    return { mount: { time: mount, mutated: 1000 }, update, insert, reverse };
}

// Run after navigation has loaded the production modules, so the stopwatch
// captures mount and DOM commits without server or DevTools round trips.
window.measureKeyedRows = measure;
