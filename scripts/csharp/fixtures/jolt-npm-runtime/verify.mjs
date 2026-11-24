import assert from 'node:assert/strict';
import { createSSRApp } from 'vue';
import { renderToString } from 'vue/server-renderer';
import { provideSSRWidth } from '@vueuse/core';
import { offset } from '@floating-ui/dom';
import { JSDOM } from 'jsdom';
import App, { ReadFrenchDistance, ReadSundayDefault, ReadWeekDefaults } from './.jolt/App.mjs';
import { getDefaultOptions, setDefaultOptions } from 'date-fns';
import { defineRule } from 'vee-validate';
import Vee from './.jolt/Vee.mjs';
import { Copy, Position, Track } from './.jolt/Floating.mjs';
import { Exercise } from './.jolt/EventHook.mjs';

assert.equal(await Exercise(), 'A1B1B2C4array');

const app = createSSRApp(App);
provideSSRWidth(800, app);
app.config.errorHandler = error => { throw error; };
const html = await renderToString(app);
const previousDefaults = getDefaultOptions();
try {
    assert.equal(ReadWeekDefaults(), 14);
    assert.equal(ReadSundayDefault(), 0);
    assert.equal(ReadFrenchDistance(), '2 jours|2 jours');
} finally {
    setDefaultOptions({
        weekStartsOn: previousDefaults.weekStartsOn,
        firstWeekContainsDate: previousDefaults.firstWeekContainsDate,
        locale: previousDefaults.locale,
    });
}
assert.match(html, /<output id="online">online<\/output>/);
assert.match(html, /<output id="client">server<\/output>/);
assert.match(html, /<output id="day">16<\/output>/);
assert.match(html, /<output id="added">17<\/output>/);
assert.match(html, /<output id="subtracted">13<\/output>/);
assert.match(html, /<output id="interval-count">2<\/output>/);
assert.match(html, /<output id="interval-last">17<\/output>/);
assert.match(html, /<output id="clamped">17<\/output>/);
assert.match(html, /<output id="basic-date">20260115<\/output>/);
assert.match(html, /<output id="range-days">2<\/output>/);
assert.match(html, /<output id="range-description">2 days<\/output>/);
assert.match(html, /<output id="date-check">date<\/output>/);
assert.match(html, /<output id="earliest">1700000000000<\/output>/);
assert.match(html, /<output id="latest">1710000000000<\/output>/);
assert.match(html, /<output id="mouse-x">0<\/output>/);
assert.match(html, /<output id="media-value">matches<\/output>/);
assert.match(html, /<output id="media-ref">misses<\/output>/);
assert.match(html, /<output id="media-getter">matches<\/output>/);
assert.match(html, /<output id="storage">changed<\/output>/);
console.log('Vue rendered the generated VueUse and date-fns imports against real locked npm packages.');

const form = createSSRApp(Vee);
defineRule('projection_example_required', value => value ? true : 'named-rule-error');
form.config.errorHandler = error => { throw error; };
const fields = await renderToString(form);
assert.match(fields, /<output id="field-value">ada@example.test<\/output>/);
assert.match(fields, /<output id="field-error">invalid-email<\/output>/);
assert.match(fields, /<output id="field-valid">valid<\/output>/);
assert.match(fields, /<output id="field-number">3<\/output>/);
assert.match(fields, /<output id="field-short">named-rule-error<\/output>/);
assert.match(fields, /<output id="field-context">invalid\|record@example.test<\/output>/);
assert.match(fields, /<output id="field-context-scalars">0\|false\|missing<\/output>/);
assert.match(fields, /<output id="form-error">invalid-form<\/output>/);
assert.match(fields, /<output id="form-invalid-value">invalid<\/output>/);
assert.match(fields, /<output id="form-submitted">16<\/output>/);
assert.match(fields, /<output id="form-initial">invalid<\/output>/);
assert.match(fields, /<output id="form-invalid-state">invalid\|invalid-form<\/output>/);
assert.match(fields, /<output id="form-final">ada@example.test<\/output>/);
assert.match(fields, /<output id="context-form-errors">invalid-form\|cleared<\/output>/);
assert.match(fields, /<output id="bulk-errors">one\|two\|three\|cleared\|cleared<\/output>/);
assert.match(fields, /<output id="bulk-touched">touched\|untouched<\/output>/);
assert.match(fields, /<output id="form-control">invalid\|invalid-form\|invalid-form<\/output>/);
assert.match(fields, /<output id="form-array">first\|third\|prepended\|updated\|inserted\|pushed\|replacement\|getter-path<\/output>/);
assert.match(fields, /<output id="form-writes">field\|updated@example.test\|0\|literal\|ada@example.test<\/output>/);
assert.match(fields, /<output id="form-partial">invalid\|missing<\/output>/);
assert.match(fields, /<output id="form-partial-nested">nested@example\.test\|0\|false\|missing\|missing<\/output>/);
assert.match(fields, /<output id="form-defined-field">0\|missing\|4\|touched\|deferred\|invalid-contact-count<\/output>/);
console.log('VeeValidate awaited the validator and called both submission handlers with the schema input and output.');

const { window } = new JSDOM('<div id="reference"></div><div id="floating"><div id="arrow"></div></div>');
const domGlobals = ['window', 'Node', 'Element', 'HTMLElement', 'ShadowRoot'];
try {
    // JSDOM supplies the DOM contracts; fixed layout makes the coordinates deterministic.
    for (const name of domGlobals)
        globalThis[name] = window[name];
    const reference = window.document.getElementById('reference');
    const floating = window.document.getElementById('floating');
    reference.getBoundingClientRect = () => new window.DOMRect(20, 30, 80, 40);
    floating.style.width = '20px';
    Object.defineProperty(floating, 'offsetWidth', { value: 20 });
    const arrow = window.document.getElementById('arrow');
    arrow.style.width = '4px';
    Object.defineProperty(arrow, 'offsetWidth', { value: 4 });
    const source = [offset(4)];
    const sequence = Copy(source);
    assert.equal(sequence[0], source[0]);
    sequence[0] = false;
    assert.equal(source[0].name, 'offset');
    assert.ok(await Position(reference, floating, arrow) >= 0);
    let calls = 0;
    const stop = Track(reference, floating, () => calls++);
    assert.equal(calls, 1);
    window.dispatchEvent(new window.Event('resize'));
    assert.equal(calls, 2);
    stop();
    window.dispatchEvent(new window.Event('resize'));
    assert.equal(calls, 2);
    console.log('Floating UI returned the arrow coordinate, updated on resize and stopped after cleanup.');
} finally {
    for (const name of domGlobals)
        delete globalThis[name];
    window.close();
}
