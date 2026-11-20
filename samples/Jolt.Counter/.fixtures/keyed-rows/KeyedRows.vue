<script setup>
defineProps({
    rows: Array,
    label: String,
    onUpdate: Function,
    onInsert: Function,
    onReverse: Function,
    onSelect: Function,
});
</script>

<template>
    <section>
        <button id="update" @click="onUpdate()">Update row</button>
        <button id="insert" @click="onInsert()">Insert row</button>
        <button id="reverse" @click="onReverse()">Reverse rows</button>
        <ul id="rows">
            <li v-for="row in rows" :key="row" :data-row="row">
                <input :aria-label="row" />
                <span>{{ row === '500' ? label : row }}</span>
                <button class="select" @click="onSelect(row)">Select</button>
            </li>
        </ul>
    </section>
</template>
