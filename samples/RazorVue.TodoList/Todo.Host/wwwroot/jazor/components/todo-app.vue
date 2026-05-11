<template>
<VApp>
  <VMain>
    <VContainer :fluid="true">
      <VRow justify="center">
        <VCol :cols="12" :md="10" :lg="8">
          <VCard>
            <VCardTitle>
              RazorVue Todo Workspace
            </VCardTitle>
            
                        
            <VCardText>
              <VRow>
                <VCol :cols="12" :md="8">
                  <VTextField label="New task title" :modelValue="props.draftTitle" @update:modelValue="(__value) =&gt; emit(&quot;update:draftTitle&quot;, __value)" />
                </VCol>
                
                                
                <VCol :cols="12" :md="4">
                  <VTextField label="Category" :modelValue="props.draftCategory" @update:modelValue="(__value) =&gt; emit(&quot;update:draftCategory&quot;, __value)" />
                </VCol>
              </VRow>
              

                            
              <VRow>
                <VCol :cols="12" :md="4">
                  <VCheckbox label="Create pinned task" :modelValue="props.draftPinned" @update:modelValue="(__value) =&gt; emit(&quot;update:draftPinned&quot;, __value)" />
                </VCol>
                
                                
                <VCol :cols="12" :md="4">
                  <VSwitch label="Show completed" :modelValue="props.showCompleted" @update:modelValue="(__value) =&gt; emit(&quot;update:showCompleted&quot;, __value)" />
                </VCol>
                
                                
                <VCol :cols="12" :md="4">
                  <VBtn text="Add task" @click="() =&gt; emit(&quot;addRequested&quot;)" />
                </VCol>
              </VRow>
              <template v-if="__jazorVueSfcBinding0">
                <p>
                  {{ props.statusMessage }}
                </p>
              </template>
            </VCardText>
          </VCard>
        </VCol>
      </VRow>
      

            
      <VRow justify="center">
        <VCol :cols="12" :md="10" :lg="8">
          <TodoSummaryCardComponent :totalCount="props.totalCount" :completedCount="props.completedCount" :openCount="props.openCount" :pinnedCount="props.pinnedCount" :totalText="__jazorVueSfcBinding1" :completedText="__jazorVueSfcBinding2" :openText="__jazorVueSfcBinding3" :pinnedText="__jazorVueSfcBinding4" />
        </VCol>
      </VRow>
      

            
      <VRow justify="center">
        <VCol :cols="12" :md="10" :lg="8">
          <VCard>
            <VCardTitle>
              Tasks
            </VCardTitle>
            
                        
            <VCardText>
              <template v-if="__jazorVueSfcBinding5">
                <VAlert :type="__jazorVueSfcBinding6" :variant="__jazorVueSfcBinding7" text="No tasks match the current filter." />
              </template>
              <template v-else>
                <VList :density="__jazorVueSfcBinding8">
                  <template v-for="item in __jazorVueSfcBinding9">
                    <template v-if="(props.showCompleted || !item.IsDone)">
                      <VListItem :title="item.Title" :subtitle="(item.Category + &quot; | &quot; + (item.IsDone ? &quot;Completed&quot; : &quot;Active&quot;))">
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
  </VMain>
</VApp>
</template>

<script setup lang="ts">
import { computed } from "vue";
import TodoSummaryCardComponent from "./todo-summary-card.vue";
import { VAlert as VAlert, VApp as VApp, VBtn as VBtn, VCard as VCard, VCardText as VCardText, VCardTitle as VCardTitle, VCheckbox as VCheckbox, VChip as VChip, VCol as VCol, VContainer as VContainer, VList as VList, VListItem as VListItem, VMain as VMain, VRow as VRow, VSwitch as VSwitch, VTextField as VTextField } from "vuetify/components";

const props = defineProps<{ completedCount?: any; draftCategory?: any; draftPinned?: any; draftTitle?: any; openCount?: any; pinnedCount?: any; showCompleted?: any; statusMessage?: any; tasks?: any; totalCount?: any; visibleCount?: any }>();
const emit = defineEmits<{ (event: "addRequested", payload?: any): void; (event: "update:draftCategory", payload?: any): void; (event: "update:draftPinned", payload?: any): void; (event: "update:draftTitle", payload?: any): void; (event: "update:showCompleted", payload?: any): void }>();
const __jazorVueSfcBinding0 = computed(() => (props.statusMessage !== null && props.statusMessage !== ""));
const __jazorVueSfcBinding1 = computed(() => `${props.totalCount} tasks in scope`);
const __jazorVueSfcBinding2 = computed(() => `${props.completedCount} completed`);
const __jazorVueSfcBinding3 = computed(() => `${props.openCount} still active`);
const __jazorVueSfcBinding4 = computed(() => `${props.pinnedCount} pinned for focus`);
const __jazorVueSfcBinding5 = computed(() => (props.visibleCount === 0));
const __jazorVueSfcBinding6 = computed(() => "info");
const __jazorVueSfcBinding7 = computed(() => "tonal");
const __jazorVueSfcBinding8 = computed(() => "comfortable");
const __jazorVueSfcBinding9 = computed(() => props.tasks);
</script>
