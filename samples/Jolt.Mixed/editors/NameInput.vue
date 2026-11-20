<script setup lang="ts">
defineProps<{ limit: number }>();
const text = defineModel<string>('text', { required: true });
</script>

<template>
    <label class="editor-label">
        <span id="editor-caption">Name</span>
        <img id="editor-icon" src="./draft.svg" alt="" width="16" height="16" />
        <input id="draft" v-model.lazy="text" :maxlength="limit" />
    </label>
</template>

<style scoped>
.editor-label {
    color: rgb(20, 30, 190);
}
</style>
