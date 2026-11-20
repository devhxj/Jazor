import { defineStore } from 'pinia';
import { reactive } from 'vue';

export interface WorkspaceData {
    count: number;
    prefetchCount: number;
    cleanupCount: number;
}

export const useWorkspace = defineStore('workspace', () => {
    // The promise belongs to this store instance. Only resolved data enters the snapshot.
    let pending: Promise<void> | undefined;
    const state = reactive<WorkspaceData>({ count: 0, prefetchCount: 0, cleanupCount: 0 });

    function load(fail: boolean) {
        return pending ??= Promise.resolve().then(() => {
            if (fail) {
                throw new Error('Workspace prefetch failed.');
            }

            state.prefetchCount++;
            state.count = 1;
        });
    }

    return { state, load };
});
