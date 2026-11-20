<script setup lang="ts">
import { ref, useTemplateRef } from 'vue';
import { Meter } from '@jazor/library-meter-fixture';
import JsxMeter from './JsxMeter';
import StockRows from './Rows.vue';

const value = ref(2);
const meter = useTemplateRef('meter');
const product = ref(0);
function multiply() {
    product.value = meter.value?.Multiply(3) ?? 0;
}
</script>

<template>
    <div id="sfc">
        <Meter ref="meter" v-model:value="value" caption="Add from SFC">
            <template #reading="{ amount }">
                <output class="meter-reading">{{ amount.toFixed(0) }}</output>
            </template>
        </Meter>
        <output class="parent-value">{{ value }}</output>
        <button class="multiply" type="button" @click="multiply">Multiply</button>
        <output class="product">{{ product }}</output>
    </div>
    <JsxMeter />
    <div id="stock"><StockRows /></div>
    <button class="meter-button meter-global" id="outside" type="button">Outside the library</button>
</template>
