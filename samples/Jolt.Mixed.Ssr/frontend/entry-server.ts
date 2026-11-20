import { disposePinia } from 'pinia';
import { renderToString, type SSRContext } from 'vue/server-renderer';
import { createMemoryHistory } from 'vue-router';
import { useWorkspace } from './store';
import { createWorkspace, type WorkspaceState } from './workspace';

export async function render({ url, state }: { url: string; state: WorkspaceState }) {
    const { app, pinia, router } = createWorkspace(createMemoryHistory(), state);
    let failure: unknown;
    // Vue's SSR renderer may swallow a rejected prefetch after error handling.
    app.config.errorHandler = error => { failure ??= error; };
    try {
        await router.push(url);
        await router.isReady();
        const context: SSRContext = {};
        const html = await renderToString(app, context);
        if (failure !== undefined) {
            throw failure;
        }
        state.props.pinia = { workspace: { state: { ...useWorkspace(pinia).state } } };
        return { html, teleports: context.teleports };
    } finally {
        disposePinia(pinia);
    }
}
