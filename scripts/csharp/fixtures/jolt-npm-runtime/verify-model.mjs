import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';

const { window } = new JSDOM('<main></main>');
try {
    for (const name of ['window', 'document', 'Element', 'SVGElement'])
        globalThis[name] = window[name];
    const { createApp, defineComponent, h, nextTick, ref } = await import('vue');
    const { default: Model } = await import('./.jolt/Model.mjs');
    const amount = ref(7);
    const computedAmount = ref();
    const passiveAmount = ref();
    const payload = ref({ info: { label: 'initial' } });
    const updates = [];
    const computedUpdates = [];
    const passiveUpdates = [];
    const payloadUpdates = [];
    let propsEvaluations = 0;
    const Child = defineComponent({
        props: { amount: { type: Number, required: true }, computedAmount: Number, passiveAmount: Number, payload: Object },
        emits: ['update:amount', 'update:accepted', 'update:passiveAmount', 'update:payload'],
        setup(props, { emit }) {
            const getProps = () => {
                propsEvaluations++;
                return props;
            };
            return () => h(Model, {
                GetProps: getProps,
                'onUpdate:amount': value => emit('update:amount', value),
                'onUpdate:accepted': value => emit('update:accepted', value),
                'onUpdate:passiveAmount': value => emit('update:passiveAmount', value),
                'onUpdate:payload': value => emit('update:payload', value),
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
    assert.equal(propsEvaluations, 4);
    click('reset');
    assert.deepEqual(updates, [0]);
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

    click('edit-passive');
    assert.deepEqual(passiveUpdates, []);
    await nextTick();
    assert.deepEqual(passiveUpdates, [8]);
    assert.equal(content('passive'), '8');
    passiveAmount.value = 2;
    await nextTick();
    assert.deepEqual(passiveUpdates, [8]);
    assert.equal(content('passive'), '2');
    payload.value = { info: { label: 'parent' } };
    await nextTick();
    assert.equal(content('record'), 'parent');
    assert.deepEqual(payloadUpdates, []);
    click('edit-record');
    assert.deepEqual(payloadUpdates, []);
    await nextTick();
    assert.equal(content('record'), 'child');
    assert.deepEqual(payloadUpdates, [{ info: { label: 'child' } }]);
    assert.equal(propsEvaluations, 4);
    app.unmount();
    console.log('VueUse useVModel applied defaults, guarded computed updates, invoked a typed clone, and synchronized a passive model.');
} finally {
    window.close();
}
