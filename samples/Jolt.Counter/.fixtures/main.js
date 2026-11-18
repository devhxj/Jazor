import { createApp } from 'vue';
import App, { Advance, Step } from './.jolt/App.mjs';

window.joltNamedExports = [Step, Advance(2)];
createApp(App).mount('#app');
