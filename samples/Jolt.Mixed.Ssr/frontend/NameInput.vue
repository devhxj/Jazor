<script setup lang="ts">
defineProps<{ limit: number }>();
const text = defineModel<string>('text', { required: true });
</script>

<template>
    <label>Draft <input id="draft" v-model="text" :maxlength="limit"></label>
</template>
