import { createPinia } from 'pinia';
import { createSSRApp } from 'vue';
import { createRouter, type RouterHistory } from 'vue-router';
import App from './App.vue';
import Editor from './Editor.vue';
import Overview from './Overview.vue';
import { routes as pageRoutes } from './.jolt/_routes.mjs';
import type { WorkspaceData } from './store';

export interface WorkspaceState {
    props: { failPrefetch: boolean; pinia?: { workspace: { state: WorkspaceData } } };
    providers: { key: string; value: string }[];
}

export function createWorkspace(history: RouterHistory, state: WorkspaceState) {
    const pinia = createPinia();
    if (state.props.pinia) {
        pinia.state.value = state.props.pinia;
    }

    const router = createRouter({
        history,
        routes: [
            { path: '/', component: Overview },
            { path: '/editor', component: Editor },
            ...pageRoutes,
        ],
    });
    const app = createSSRApp(App, { failPrefetch: state.props.failPrefetch });
    for (const { key, value } of state.providers) {
        app.provide<string>(key, value);
    }
    app.use(pinia);
    app.use(router);

    return { app, pinia, router };
}
