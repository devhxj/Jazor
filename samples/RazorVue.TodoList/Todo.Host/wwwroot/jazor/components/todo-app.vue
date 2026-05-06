<template>
<VContainer fluid="true">
  <VRow justify="center">
    <VCol cols="12" md="10" lg="8">
      <VCard>
        <VCardTitle text="RazorVue Todo Workspace" />
        
                
        <VCardText>
          <VRow>
            <VCol cols="12" md="8">
              <VTextField label="New task title" :modelValue="props.draftTitle" @update:modelValue="__jazorVueSfcBinding0" />
            </VCol>
            
                        
            <VCol cols="12" md="4">
              <VTextField label="Category" :modelValue="props.draftCategory" @update:modelValue="__jazorVueSfcBinding1" />
            </VCol>
          </VRow>
          

                    
          <VRow>
            <VCol cols="12" md="4">
              <VCheckbox label="Create pinned task" :modelValue="props.draftPinned" @update:modelValue="__jazorVueSfcBinding2" />
            </VCol>
            
                        
            <VCol cols="12" md="4">
              <VSwitch label="Show completed" :modelValue="props.showCompleted" @update:modelValue="__jazorVueSfcBinding3" />
            </VCol>
            
                        
            <VCol cols="12" md="4">
              <VBtn text="Add task" @click="__jazorVueSfcBinding4" />
            </VCol>
          </VRow>
          <template v-if="__jazorVueSfcBinding5">
            <p>
              {{ props.statusMessage }}
            </p>
          </template>
        </VCardText>
      </VCard>
    </VCol>
  </VRow>
  

    
  <VRow justify="center">
    <VCol cols="12" md="10" lg="8">
      <TodoSummaryCardComponent :totalCount="props.totalCount" :completedCount="props.completedCount" :openCount="props.openCount" :pinnedCount="props.pinnedCount" :totalText="__jazorVueSfcBinding6" :completedText="__jazorVueSfcBinding7" :openText="__jazorVueSfcBinding8" :pinnedText="__jazorVueSfcBinding9" />
    </VCol>
  </VRow>
  

    
  <VRow justify="center">
    <VCol cols="12" md="10" lg="8">
      <VCard>
        <VCardTitle text="Tasks" />
        
                
        <VCardText>
          <template v-if="__jazorVueSfcBinding10">
            <VAlert type="info" variant="tonal" text="No tasks match the current filter." />
          </template>
          <template v-else>
            <VList density="comfortable">
              <template v-for="item in __jazorVueSfcBinding11">
                <template v-if="(props.showCompleted || !item.IsDone)">
                  <VListItem :title="item.Title" :subtitle="((item.Category + &quot; | &quot;) + (item.IsDone ? &quot;Completed&quot; : &quot;Active&quot;))">
                    <template v-if="item.IsPinned">
                      <VChip text="Pinned" color="primary" />
                    </template>
                  </VListItem>
                </template>
              </template>
            </VList>
          </template>
        </VCardText>
      </VCard>
    </VCol>
  </VRow>
</VContainer>
</template>

<script setup lang="ts">
import { computed } from "vue";
import TodoSummaryCardComponent from "./todo-summary-card.vue";
import { VAlert as VAlert, VBtn as VBtn, VCard as VCard, VCardText as VCardText, VCardTitle as VCardTitle, VCheckbox as VCheckbox, VChip as VChip, VCol as VCol, VContainer as VContainer, VList as VList, VListItem as VListItem, VRow as VRow, VSwitch as VSwitch, VTextField as VTextField } from "vuetify/components";

const props = defineProps<{ completedCount?: any; draftCategory?: any; draftPinned?: any; draftTitle?: any; openCount?: any; pinnedCount?: any; showCompleted?: any; statusMessage?: any; tasks?: any; totalCount?: any; visibleCount?: any }>();
const emit = defineEmits<{ (event: "addRequested", payload?: any): void; (event: "update:draftCategory", payload?: any): void; (event: "update:draftPinned", payload?: any): void; (event: "update:draftTitle", payload?: any): void; (event: "update:showCompleted", payload?: any): void }>();
const __jazorVueSfcBinding0 = computed(() => (__value) => emit("update:draftTitle", __value));
const __jazorVueSfcBinding1 = computed(() => (__value) => emit("update:draftCategory", __value));
const __jazorVueSfcBinding2 = computed(() => (__value) => emit("update:draftPinned", __value));
const __jazorVueSfcBinding3 = computed(() => (__value) => emit("update:showCompleted", __value));
const __jazorVueSfcBinding4 = computed(() => () => emit("addRequested"));
const __jazorVueSfcBinding5 = computed(() => ((props.statusMessage !== null) && (props.statusMessage !== "")));
const __jazorVueSfcBinding6 = computed(() => `${props.totalCount} tasks in scope`);
const __jazorVueSfcBinding7 = computed(() => `${props.completedCount} completed`);
const __jazorVueSfcBinding8 = computed(() => `${props.openCount} still active`);
const __jazorVueSfcBinding9 = computed(() => `${props.pinnedCount} pinned for focus`);
const __jazorVueSfcBinding10 = computed(() => (props.visibleCount === 0));
const __jazorVueSfcBinding11 = computed(() => props.tasks);
</script>
