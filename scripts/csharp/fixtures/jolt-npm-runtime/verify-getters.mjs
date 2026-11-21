import assert from 'node:assert/strict';
import { effectScope } from 'vue';
import { Evaluate, Inspect } from './.jolt/Getters.mjs';

const scope = effectScope();
try {
    assert.equal(scope.run(Evaluate), '6/12/high');
    assert.equal(scope.run(Inspect), 'initial/first/first/True/True/True/jolt getter fixture/fixture.svg/True/True');
} finally {
    scope.stop();
}
console.log('VueUse recomputed the typed getter and its renamed export after a ref write.');
