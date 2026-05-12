<template>
<VCard class="playground-stat-card" :variant="__jazorVueSfcBinding0">
  <VCardText>
    <div class="playground-stat-card__eyebrow">
      {{ props.eyebrow }}
    </div>
    
        
    <div class="playground-stat-card__value">
      {{ props.value }}
    </div>
    
        
    <div class="playground-stat-card__caption">
      {{ props.caption }}
    </div>
  </VCardText>
</VCard>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { VCard as VCard, VCardText as VCardText } from "vuetify/components";

const props = defineProps<{ caption?: any; eyebrow?: any; value?: any }>();
const emit = defineEmits<{ }>();
const __jazorVueSfcBinding0 = computed(() => "tonal");
</script>
