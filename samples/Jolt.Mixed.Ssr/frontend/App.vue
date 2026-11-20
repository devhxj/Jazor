<script setup lang="ts">
import { onMounted, onServerPrefetch } from 'vue';
import { RouterLink, RouterView } from 'vue-router';
import RequestBadge from './RequestBadge.jazor';
import { useWorkspace } from './store';

const props = defineProps<{ failPrefetch: boolean }>();
const workspace = useWorkspace();

onServerPrefetch(() => workspace.load(props.failPrefetch));
onMounted(() => {
    if (workspace.state.prefetchCount === 0) {
        return workspace.load(false);
    }
});
</script>

<template>
    <main>
        <nav aria-label="Workspace">
            <RouterLink id="home-link" to="/">Overview</RouterLink>
            <RouterLink id="editor-link" to="/editor">Editor</RouterLink>
            <RouterLink id="draft-link" to="/drafts/42">Draft details</RouterLink>
        </nav>
        <RequestBadge />
        <button id="counter" aria-label="Increment draft count" @click="workspace.state.count++">{{ workspace.state.count }}</button>
        <p>Prefetches <output id="prefetch-count">{{ workspace.state.prefetchCount }}</output></p>
        <p>Editor disposals <output id="cleanup-count">{{ workspace.state.cleanupCount }}</output></p>
        <RouterView v-slot="{ Component }">
            <Transition name="route" mode="out-in">
                <component :is="Component" />
            </Transition>
        </RouterView>
    </main>
</template>

<style>
body { color: #282525; font: 1rem/1.5 system-ui, sans-serif; margin: 2rem auto; max-width: 36rem; padding-inline: 1rem; }
nav { display: flex; gap: 1rem; }
nav a { color: #76262a; }
.route-enter-active, .route-leave-active { transition: opacity 100ms ease; }
.route-enter-from, .route-leave-to { opacity: 0; }
</style>
