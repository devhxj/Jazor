<template>
<div class="playground-page-detail">
  <VContainer :fluid="true">
    <VRow>
      <VCol :cols="12" :xl="8">
        <div class="playground-detail__back">
          <a :href="props.model.BackHref">
            {{ props.model.BackText }}
          </a>
        </div>
        

            
        <div class="playground-detail__hero">
          <div class="playground-detail__chips">
            <VChip color="primary" :text="props.model.Example.Category" />
            
                    
            <VChip :variant="__jazorVueSfcBinding0" :text="props.model.Example.Runtime" />
            
                    
            <VChip :variant="__jazorVueSfcBinding1" :text="props.model.Example.Difficulty" />
          </div>
          
                
          <h1 class="playground-detail__title">
            {{ props.model.Example.Title }}
          </h1>
          
                
          <p class="playground-detail__summary">
            {{ props.model.Example.Summary }}
          </p>
          
                
          <div class="playground-detail__actions">
            <VBtn color="secondary" :variant="__jazorVueSfcBinding2" :text="props.model.FavoriteText" @click="() =&gt; emit(&quot;toggleFavorite&quot;)" />
          </div>
        </div>
        

            
        <VRow>
          <VCol :cols="12" :md="6">
            <VCard :variant="__jazorVueSfcBinding3">
              <VCardTitle>
                Why It Matters
              </VCardTitle>
              
                        
              <VCardText>
                {{ props.model.Example.WhyItMatters }}
              </VCardText>
            </VCard>
          </VCol>
          
                
          <VCol :cols="12" :md="6">
            <VCard :variant="__jazorVueSfcBinding4">
              <VCardTitle>
                Highlights
              </VCardTitle>
              
                        
              <VCardText>
                <VList :density="__jazorVueSfcBinding5">
                  <template v-for="item in __jazorVueSfcBinding6">
                    <VListItem :title="item" />
                  </template>
                </VList>
              </VCardText>
            </VCard>
          </VCol>
        </VRow>
        

            
        <VRow>
          <VCol :cols="12" :md="7">
            <VCard :variant="__jazorVueSfcBinding7">
              <VCardTitle>
                Build Steps
              </VCardTitle>
              
                        
              <VCardText>
                <VList :density="__jazorVueSfcBinding8">
                  <template v-for="step in __jazorVueSfcBinding9">
                    <VListItem :title="step" />
                  </template>
                </VList>
              </VCardText>
            </VCard>
          </VCol>
          
                
          <VCol :cols="12" :md="5">
            <VCard :variant="__jazorVueSfcBinding10">
              <VCardTitle>
                Files Involved
              </VCardTitle>
              
                        
              <VCardText>
                <VList :density="__jazorVueSfcBinding11">
                  <template v-for="file in __jazorVueSfcBinding12">
                    <VListItem :title="file" />
                  </template>
                </VList>
              </VCardText>
            </VCard>
          </VCol>
        </VRow>
      </VCol>
      

        
      <VCol :cols="12" :xl="4">
        <VCard class="playground-detail__sidebar" :variant="__jazorVueSfcBinding13">
          <VCardTitle>
            Reference Snapshot
          </VCardTitle>
          
                
          <VCardText>
            <div class="playground-detail__sidebar-row">
              <span>
                Estimated
              </span>
              
                        
              <strong>
                {{ __jazorVueSfcBinding14 }}
              </strong>
            </div>
            
                    
            <div class="playground-detail__sidebar-row">
              <span>
                Updated
              </span>
              
                        
              <strong>
                {{ props.model.Example.UpdatedAtUtc }}
              </strong>
            </div>
            
                    
            <div class="playground-detail__sidebar-row">
              <span>
                Featured
              </span>
              
                        
              <strong>
                {{ __jazorVueSfcBinding15 }}
              </strong>
            </div>
            
                    
            <div class="playground-detail__tag-list">
              <template v-for="tag in __jazorVueSfcBinding16">
                <VChip size="small" :variant="&quot;text&quot;" :text="tag" />
              </template>
            </div>
          </VCardText>
        </VCard>
      </VCol>
    </VRow>
  </VContainer>
</div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { VBtn as VBtn, VCard as VCard, VCardText as VCardText, VCardTitle as VCardTitle, VChip as VChip, VCol as VCol, VContainer as VContainer, VList as VList, VListItem as VListItem, VRow as VRow } from "vuetify/components";

const props = defineProps<{ model?: any }>();
const emit = defineEmits<{ (event: "toggleFavorite", payload?: any): void }>();
const __jazorVueSfcBinding0 = computed(() => "outlined");
const __jazorVueSfcBinding1 = computed(() => "outlined");
const __jazorVueSfcBinding2 = computed(() => "flat");
const __jazorVueSfcBinding3 = computed(() => "tonal");
const __jazorVueSfcBinding4 = computed(() => "tonal");
const __jazorVueSfcBinding5 = computed(() => "compact");
const __jazorVueSfcBinding6 = computed(() => props.model.Example.Highlights);
const __jazorVueSfcBinding7 = computed(() => "outlined");
const __jazorVueSfcBinding8 = computed(() => "compact");
const __jazorVueSfcBinding9 = computed(() => props.model.Example.Steps);
const __jazorVueSfcBinding10 = computed(() => "outlined");
const __jazorVueSfcBinding11 = computed(() => "compact");
const __jazorVueSfcBinding12 = computed(() => props.model.Example.Files);
const __jazorVueSfcBinding13 = computed(() => "outlined");
const __jazorVueSfcBinding14 = computed(() => (props.model.Example.EstimatedMinutes + " min"));
const __jazorVueSfcBinding15 = computed(() => (props.model.Example.Featured ? "Yes" : "No"));
const __jazorVueSfcBinding16 = computed(() => props.model.Example.Tags);
</script>
