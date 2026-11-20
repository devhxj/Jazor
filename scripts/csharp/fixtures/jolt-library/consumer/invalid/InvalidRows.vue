<script setup lang="ts">
import { Rows } from '@jazor/library-meter-fixture';

const items = [{ product: 'Paper', units: 3 }] as const;
const details = { Id: 1n, Total: 5n, Header: { Label: 'Stock' } };
</script>

<template>
    <Rows :Items="items" :Details="{ ...details, Id: 1 }" />
    <Rows :Items="items" :Details="{ ...details, Header: { label: 'Stock' } }" />
    <Rows :Items="items" :Details="details">
        <template #row="{ item, details }">
            <td>{{ item.product.toFixed(0) }}</td>
            <td>{{ details.Total.toFixed(0) }}</td>
            <td>{{ details.Header.Label.notAStringMember() }}</td>
        </template>
    </Rows>
</template>
