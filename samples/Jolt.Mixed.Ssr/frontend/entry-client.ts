import { createWebHistory } from 'vue-router';
import { createWorkspace, type WorkspaceState } from './workspace';

const state: WorkspaceState = JSON.parse(document.getElementById('__jazor_ssr_state')!.textContent!);
const { app, router } = createWorkspace(createWebHistory(), state);
await router.isReady();

const container = document.getElementById('app')!;
app.mount(container);
container.dataset.hydrated = 'true';
