<template>
<VCard>
  <VCardTitle text="Overview" />
  
    
  <VCardText>
    <VList density="compact">
      <VListItem title="All tasks" :subtitle="props.totalText" />
      
            
      <VListItem title="Completed" :subtitle="props.completedText" />
      
            
      <VListItem title="Open" :subtitle="props.openText" />
      <template v-if="__jazorVueSfcBinding0">
        <VListItem title="Pinned" :subtitle="props.pinnedText" />
      </template>
    </VList>
  </VCardText>
</VCard>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { VCard as VCard, VCardText as VCardText, VCardTitle as VCardTitle, VList as VList, VListItem as VListItem } from "vuetify/components";

const props = defineProps<{ completedCount?: any; completedText?: any; openCount?: any; openText?: any; pinnedCount?: any; pinnedText?: any; totalCount?: any; totalText?: any }>();
const emit = defineEmits<{ }>();
const __jazorVueSfcBinding0 = computed(() => (props.pinnedCount > 0));
</script>
