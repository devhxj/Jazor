<template>
<VCard class="playground-empty-state" :variant="__jazorVueSfcBinding0">
  <VCardText>
    <div class="playground-empty-state__title">
      {{ props.title }}
    </div>
    
        
    <div class="playground-empty-state__body">
      {{ props.body }}
    </div>
  </VCardText>
</VCard>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { VCard as VCard, VCardText as VCardText } from "vuetify/components";

const props = defineProps<{ body?: any; title?: any }>();
const emit = defineEmits<{ }>();
const __jazorVueSfcBinding0 = computed(() => "outlined");
</script>
