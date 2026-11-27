import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';

const { window } = new JSDOM('<main></main>');
try {
    for (const name of ['window', 'document', 'Element', 'SVGElement'])
        globalThis[name] = window[name];
    const { createApp, nextTick } = await import('vue');
    const { defineRule } = await import('vee-validate');
    const { default: Submit } = await import('./.jazor/Submit.mjs');
    const { default: Vee } = await import('./.jazor/Vee.mjs');
    const container = document.querySelector('main');
    const result = () => container.querySelector('output').textContent;
    const provided = createApp(Submit, { Provide: true });
    provided.mount(container);
    for (let i = 0; i < 100 && result() !== 'submitted'; i++)
        await new Promise(resolve => setTimeout(resolve, 10));
    assert.equal(result(), 'submitted');
    const scope = () => container.querySelector('#form-scope').textContent;
    for (let i = 0; i < 100 && scope() === 'waiting'; i++)
        await new Promise(resolve => setTimeout(resolve, 10));
    assert.equal(scope(), 'submitted|updated');
    provided.unmount();

    const missing = createApp(Submit, { Provide: false });
    missing.mount(container);
    await nextTick();
    assert.equal(result(), 'missing');
    assert.equal(scope(), 'missing');
    missing.unmount();
    defineRule('projection_example_required', value => value ? true : 'named-rule-error');
    const scalarFields = createApp(Vee);
    scalarFields.mount(container);
    assert.equal(container.querySelector('#field-context-scalars').textContent, '0|false|missing');
    scalarFields.unmount();
    console.log('VeeValidate submitted typed values, shared form state with a nested component, and returned undefined without a provider.');
} finally {
    window.close();
}
