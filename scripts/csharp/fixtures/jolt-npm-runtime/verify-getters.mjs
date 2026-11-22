import assert from 'node:assert/strict';
import { effectScope } from 'vue';
import { createSSRApp } from 'vue';
import { renderToString } from 'vue/server-renderer';
import Getters, { Evaluate, Inspect, ThrottleRefs } from './.jolt/Getters.mjs';
import Constant from './.jolt/Constant.mjs';

const scope = effectScope();
try {
    assert.equal(scope.run(Evaluate), '6/12/high');
    assert.equal(scope.run(ThrottleRefs), '2/second');
    assert.match(await renderToString(createSSRApp(Getters)), /<output id="invoke-typed">typed\/7<\/output>/);
    assert.match(await renderToString(createSSRApp(Constant)), /<output id="constant-typed">typed\/7<\/output>/);
    assert.equal(scope.run(Inspect), 'initial/first/first/True/True/True/jolt getter fixture/fixture.svg/True/True');
} finally {
    scope.stop();
}
console.log('VueUse recomputed the typed getter and its renamed export after a ref write.');
