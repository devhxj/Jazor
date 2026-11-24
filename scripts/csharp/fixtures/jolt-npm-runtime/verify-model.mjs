import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';

const { window } = new JSDOM('<main></main>');
try {
    for (const name of ['window', 'document', 'Element', 'SVGElement'])
        globalThis[name] = window[name];
    const { createApp, defineComponent, h, markRaw, nextTick, ref, shallowReactive, shallowRef, toRaw } = await import('vue');
    const { default: Model } = await import('./.jolt/Model.mjs');
    const amount = ref(7);
    const computedAmount = ref();
    const passiveAmount = ref();
    const payload = ref({ info: { label: 'initial' } });
    const count = ref(7);
    const enabled = ref(true);
    const state = shallowRef({ enabled });
    const originalRefs = { rows: [{ counts: [count], states: [state] }] };
    const refs = ref(originalRefs);
    const refsUpdates = [];
    const deepCount = ref(3);
    const deepNestedCount = ref(5);
    const view = shallowRef({ count: deepCount, details: { nestedCount: deepNestedCount } });
    const viewUpdates = [];
    const updates = [];
    const computedUpdates = [];
    const passiveUpdates = [];
    const payloadUpdates = [];
    const explicitEmits = [];
    let propsEvaluations = 0;
    const Child = defineComponent({
        props: { amount: { type: Number, required: true }, computedAmount: Number, passiveAmount: Number, payload: Object, refs: Object, view: Object },
        emits: ['update:amount', 'update:accepted', 'update:passiveAmount', 'update:payload', 'update:refs', 'update:view'],
        setup(props, { emit }) {
            const getProps = () => {
                propsEvaluations++;
                return props;
            };
            return () => h(Model, {
                GetProps: getProps,
                ReportEmit: (name, length, value) => {
                    explicitEmits.push([name, length, value]);
                    emit(name, value);
                },
                'onUpdate:accepted': value => emit('update:accepted', value),
                'onUpdate:payload': value => emit('update:payload', value),
                'onUpdate:refs': value => emit('update:refs', value),
                'onUpdate:view': value => emit('update:view', value),
            });
        },
    });
    const app = createApp({
        setup() {
            return () => h(Child, {
                amount: amount.value,
                computedAmount: computedAmount.value,
                passiveAmount: passiveAmount.value,
                payload: payload.value,
                refs: refs.value,
                view: view.value,
                'onUpdate:amount': value => {
                    updates.push(value);
                    amount.value = value;
                },
                'onUpdate:accepted': value => {
                    computedUpdates.push(value);
                    computedAmount.value = value;
                },
                'onUpdate:passiveAmount': value => {
                    passiveUpdates.push(value);
                    passiveAmount.value = value;
                },
                'onUpdate:payload': value => {
                    payloadUpdates.push(value);
                    payload.value = value;
                },
                'onUpdate:refs': value => {
                    refsUpdates.push(value);
                    refs.value = value;
                },
                'onUpdate:view': value => {
                    viewUpdates.push(value);
                    view.value = value;
                },
            });
        },
    });
    const mount = window.document.querySelector('main');
    app.mount(mount);
    const content = id => mount.querySelector('#' + id).textContent;
    const click = id => mount.querySelector('#' + id).click();
    assert.equal(content('model'), '7');
    assert.equal(content('computed'), '0');
    assert.equal(content('passive'), '0');
    assert.equal(content('record'), 'initial');
    assert.equal(propsEvaluations, 6);
    click('reset');
    assert.deepEqual(updates, [0]);
    assert.deepEqual(explicitEmits, [['update:amount', 1, 0]]);
    await nextTick();
    assert.equal(content('model'), '0');

    click('reject');
    await nextTick();
    assert.deepEqual(computedUpdates, []);
    assert.equal(content('computed'), '0');
    click('accept');
    assert.deepEqual(computedUpdates, [4]);
    await nextTick();
    assert.equal(content('computed'), '5');

    passiveAmount.value = 2;
    await nextTick();
    assert.deepEqual(passiveUpdates, []);
    assert.equal(content('passive'), '2');
    click('edit-passive');
    assert.deepEqual(passiveUpdates, []);
    await nextTick();
    assert.deepEqual(passiveUpdates, [0]);
    assert.equal(content('passive'), '0');
    amount.value = 6;
    passiveAmount.value = 3;
    await nextTick();
    assert.equal(content('model'), '6');
    assert.equal(content('passive'), '3');
    assert.deepEqual(explicitEmits, [['update:amount', 1, 0], ['update:passiveAmount', 1, 0]]);
    payload.value = { info: { label: 'parent' } };
    await nextTick();
    assert.equal(content('record'), 'parent');
    assert.deepEqual(payloadUpdates, []);
    click('edit-record');
    assert.deepEqual(payloadUpdates, []);
    await nextTick();
    assert.equal(content('record'), 'child');
    assert.deepEqual(payloadUpdates, [{ info: { label: 'child' } }]);
    assert.equal(content('refs'), '7');
    assert.equal(content('shallow-refs'), 'true');
    assert.equal(refsUpdates.length, 0);
    click('edit-refs');
    await nextTick();
    assert.equal(content('refs'), '0');
    assert.equal(content('shallow-refs'), 'false');
    assert.equal(count.value, 0);
    assert.equal(enabled.value, false);
    assert.equal(refsUpdates.length, 1);
    assert.equal(toRaw(refsUpdates[0]), originalRefs);
    assert.equal(toRaw(refsUpdates[0].rows[0].counts[0]), count);
    assert.equal(refsUpdates[0].rows[0].counts[1], undefined);
    assert.equal(toRaw(refsUpdates[0].rows[0].states[0]), state);
    assert.equal(toRaw(refsUpdates[0].rows[0].states[0].value.enabled), enabled);

    click('replace-ref');
    await nextTick();
    const replacement = toRaw(refs.value.rows[0].counts[0]);
    assert.equal(replacement.value, 9);
    assert.equal(content('refs'), '9');
    assert.equal(count.value, 0);
    assert.equal(refsUpdates.length, 2);
    assert.equal(toRaw(refsUpdates[1].rows[0].counts[0]), replacement);

    const parentCount = ref(6);
    const parentEnabled = ref(true);
    const parentState = shallowRef({ enabled: parentEnabled });
    const parentRefs = { rows: [{ counts: [parentCount], states: [parentState] }] };
    refs.value = parentRefs;
    await nextTick();
    assert.equal(content('refs'), '6');
    assert.equal(content('shallow-refs'), 'true');
    assert.equal(refsUpdates.length, 2);
    click('edit-refs');
    await nextTick();
    assert.equal(content('refs'), '0');
    assert.equal(content('shallow-refs'), 'false');
    assert.equal(refsUpdates.length, 3);
    assert.equal(toRaw(refsUpdates[2]), parentRefs);
    assert.equal(toRaw(refsUpdates[2].rows[0].counts[0]), parentCount);
    assert.equal(toRaw(refsUpdates[2].rows[0].states[0]), parentState);
    assert.equal(parentCount.value, 0);
    assert.equal(parentEnabled.value, false);
    assert.equal(content('view-count'), '3');
    assert.equal(content('nested-view-count'), '5');
    const deepView = toRaw(view.value);
    click('edit-view');
    await nextTick();
    assert.equal(content('view-count'), '0');
    assert.equal(content('nested-view-count'), '0');
    assert.equal(deepCount.value, 0);
    assert.equal(deepNestedCount.value, 0);
    assert.equal(toRaw(view.value).count, deepCount);
    assert.equal(toRaw(view.value).details.nestedCount, deepNestedCount);
    assert.equal(viewUpdates.length, 1);
    assert.equal(toRaw(viewUpdates[0]), deepView);

    for (const wrap of [shallowReactive, markRaw]) {
        const count = ref(4);
        const nestedCount = ref(6);
        const supplied = wrap({ count, details: { nestedCount } });
        view.value = supplied;
        await nextTick();
        assert.equal(content('view-count'), '4');
        assert.equal(content('nested-view-count'), '6');
        click('edit-view');
        await nextTick();
        assert.equal(content('view-count'), '0');
        assert.equal(content('nested-view-count'), '0');
        assert.equal(count.value, 0);
        assert.equal(nestedCount.value, 0);
        assert.equal(toRaw(view.value), toRaw(supplied));
        assert.equal(toRaw(view.value).count, count);
        assert.equal(toRaw(view.value).details.nestedCount, nestedCount);
    }
    assert.equal(propsEvaluations, 6);
    app.unmount();
    console.log('VueUse useVModel applied defaults, guarded computed updates, invoked a typed clone, and synchronized a passive model.');
} finally {
    window.close();
}
