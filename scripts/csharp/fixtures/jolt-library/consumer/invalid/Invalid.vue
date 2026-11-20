<script setup lang="ts">
import { ref, useTemplateRef } from 'vue';
import { Meter } from '@jazor/library-meter-fixture';

const badModel = ref('wrong');
const meter = useTemplateRef('meter');
meter.value?.Multiply('wrong');
</script>

<template>
    <Meter />
    <Meter :value="1" :caption="42" />
    <Meter v-model:value="badModel" />
    <Meter ref="meter" :value="1">
        <template #reading="{ amount }">{{ amount.toUpperCase() }}</template>
    </Meter>
</template>
