<script setup lang="ts">
import { ref } from 'vue';
import MixedEditor from './editors/MixedEditor.jazor';

const name = ref('First draft');
</script>

<template>
    <MixedEditor :Limit="24" v-model:text="name" />
    <output id="saved">{{ name }}</output>
    <p class="editor-label" id="outside">Outside the editor</p>
</template>
