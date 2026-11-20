<script setup>
import { computed, ref } from 'vue';
import { CollapsibleContent, CollapsibleRoot, CollapsibleTrigger } from 'reka-ui';

const labelValue = ref('Initial');
const filterValue = ref('');
const archivedValue = ref(false);
const openValue = ref(false);
const labelWrites = ref(0);
const filterWrites = ref(0);
const archivedWrites = ref(0);
const openWrites = ref(0);
const sorts = ref(0);
const bubbles = ref(0);
const sortDefault = ref('');
const stopSortPropagation = ref(true);
const rows = ref(['alpha', 'beta']);

function countedModel(state, writes) {
    return computed({
        get: () => state.value,
        set(value) {
            state.value = value;
            writes.value++;
        },
    });
}

const label = countedModel(labelValue, labelWrites);
const filter = countedModel(filterValue, filterWrites);
const archived = countedModel(archivedValue, archivedWrites);
const open = countedModel(openValue, openWrites);

function restoreParentValues() {
    labelValue.value = 'Parent';
    filterValue.value = 'Parent filter';
    archivedValue.value = true;
    openValue.value = true;
}

function reverse(event) {
    sortDefault.value = event.defaultPrevented ? 'prevented' : 'allowed';
    if (stopSortPropagation.value)
        event.stopPropagation();
    rows.value = [rows.value[1], rows.value[0]];
    sorts.value++;
}
</script>

<template>
    <section id="editable-list">
        <label>Label <input id="edit-label" v-model.lazy="label" /></label>
        <label>Filter <input id="edit-filter" v-model="filter" /></label>
        <label>Include archived <input id="edit-archived" type="checkbox" v-model="archived" /></label>
        <CollapsibleRoot v-model:open="open">
            <div id="edit-toggle"><CollapsibleTrigger>Details</CollapsibleTrigger></div>
            <div id="edit-details"><CollapsibleContent>Editable rows</CollapsibleContent></div>
        </CollapsibleRoot>
        <output id="edit-label-state">{{ labelValue }}:{{ labelWrites }}</output>
        <output id="edit-filter-state">{{ filterValue }}:{{ filterWrites }}</output>
        <output id="edit-archived-state">{{ archivedValue ? 'checked' : 'unchecked' }}:{{ archivedWrites }}</output>
        <output id="edit-open-state">{{ openValue ? 'open' : 'closed' }}:{{ openWrites }}</output>
        <button id="edit-reset" @click="restoreParentValues">Restore parent values</button>
        <div @click="bubbles++">
            <a id="edit-sort" href="#sorted" @click.prevent="reverse">Reverse rows</a>
        </div>
        <button id="edit-allow-bubble" @click="stopSortPropagation = false">Allow sort to bubble</button>
        <output id="edit-sort-state">{{ sorts }}:{{ bubbles }}:{{ sortDefault }}</output>
        <ul id="edit-rows">
            <li v-for="row in rows" :key="row" :data-row="row"><input :aria-label="row" :value="row" /></li>
        </ul>
    </section>
</template>
