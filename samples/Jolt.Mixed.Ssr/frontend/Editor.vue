<script setup lang="ts">
import { onUnmounted } from 'vue';
import DraftEditor from './DraftEditor.jazor';
import { useWorkspace } from './store';

const workspace = useWorkspace();
onUnmounted(() => workspace.state.cleanupCount++);
</script>

<template>
    <section>
        <h1 id="route-heading">Editor</h1>
        <DraftEditor />
    </section>
</template>
