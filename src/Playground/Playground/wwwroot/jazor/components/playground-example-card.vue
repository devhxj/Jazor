<template>
<VCard class="playground-example-card" :variant="__jazorVueSfcBinding0">
  <VCardText>
    <div class="playground-example-card__topline">
      <VChip size="small" color="primary" :text="props.example.Category" />
      
            
      <span class="playground-example-card__runtime">
        {{ props.example.Runtime }}
      </span>
    </div>
    

        
    <div class="playground-example-card__title-row">
      <a class="playground-example-card__title" :href="props.detailHref">
        {{ props.example.Title }}
      </a>
      
            
      <div>
        <template v-if="__jazorVueSfcBinding1">
          <VChip size="x-small" color="warning" text="Featured" />
        </template>
        <template v-if="__jazorVueSfcBinding2">
          <VChip size="x-small" color="secondary" text="Saved" />
        </template>
      </div>
    </div>
    

        
    <p class="playground-example-card__summary">
      {{ props.example.Summary }}
    </p>
    

        
    <div class="playground-example-card__meta">
      <span>
        {{ props.example.Difficulty }}
      </span>
      
            
      <span>
        {{ __jazorVueSfcBinding3 }}
      </span>
    </div>
    

        
    <div class="playground-example-card__tags">
      <template v-for="tag in __jazorVueSfcBinding4">
        <VChip size="x-small" :variant="&quot;text&quot;" :text="tag" />
      </template>
    </div>
  </VCardText>
  

    
  <VDivider />
  

    
  <VCardActions>
    <VBtn :variant="__jazorVueSfcBinding5" text="Open example" :href="props.detailHref" />
  </VCardActions>
</VCard>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { VBtn as VBtn, VCard as VCard, VCardActions as VCardActions, VCardText as VCardText, VChip as VChip, VDivider as VDivider } from "vuetify/components";

const props = defineProps<{ detailHref?: any; example?: any }>();
const emit = defineEmits<{ }>();
const __jazorVueSfcBinding0 = computed(() => "outlined");
const __jazorVueSfcBinding1 = computed(() => props.example.Featured);
const __jazorVueSfcBinding2 = computed(() => props.example.IsFavorite);
const __jazorVueSfcBinding3 = computed(() => (props.example.EstimatedMinutes + " min"));
const __jazorVueSfcBinding4 = computed(() => props.example.Tags);
const __jazorVueSfcBinding5 = computed(() => "text");
</script>
