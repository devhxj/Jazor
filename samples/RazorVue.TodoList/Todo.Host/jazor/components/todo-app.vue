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
                  <VTextField label="New task title" :modelValue="props.draftTitle" @update:modelValue="__jazor$0" />
                </VCol>
                
                                
                <VCol :cols="12" :md="4">
                  <VTextField label="Category" :modelValue="props.draftCategory" @update:modelValue="__jazor$1" />
                </VCol>
              </VRow>
              

                            
              <VRow>
                <VCol :cols="12" :md="4">
                  <VCheckbox label="Create pinned task" :modelValue="props.draftPinned" @update:modelValue="__jazor$2" />
                </VCol>
                
                                
                <VCol :cols="12" :md="4">
                  <VSwitch label="Show completed" :modelValue="props.showCompleted" @update:modelValue="__jazor$3" />
                </VCol>
                
                                
                <VCol :cols="12" :md="4">
                  <VBtn text="Add task" @click="__jazor$4" />
                </VCol>
              </VRow>
              <template v-if="(props.statusMessage !== null &amp;&amp; props.statusMessage !== &quot;&quot;)">
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
          <TodoSummaryCardComponent :totalCount="props.totalCount" :completedCount="props.completedCount" :openCount="props.openCount" :pinnedCount="props.pinnedCount" :totalText="`${props.totalCount} tasks in scope`" :completedText="`${props.completedCount} completed`" :openText="`${props.openCount} still active`" :pinnedText="`${props.pinnedCount} pinned for focus`" />
        </VCol>
      </VRow>
      

            
      <VRow justify="center">
        <VCol :cols="12" :md="10" :lg="8">
          <VCard>
            <VCardTitle>
              Tasks
            </VCardTitle>
            
                        
            <VCardText>
              <template v-if="(props.visibleCount === 0)">
                <VAlert type="info" variant="tonal" text="No tasks match the current filter." />
              </template>
              <template v-else>
                <VList density="comfortable">
                  <template v-for="item in props.tasks">
                    <template v-if="(props.showCompleted || !item.isDone)">
                      <VListItem :title="item.title" :subtitle="(item.category + &quot; | &quot; + (item.isDone ? &quot;Completed&quot; : &quot;Active&quot;))">
                        <template v-if="item.isPinned">
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

const __jazorRawProps = defineProps<{ completedCount?: any; draftCategory?: any; draftPinned?: any; draftTitle?: any; openCount?: any; pinnedCount?: any; showCompleted?: any; statusMessage?: any; tasks?: any; totalCount?: any; visibleCount?: any }>();
const __jazorPropDefaultCache = Object.create(null);
const props = new Proxy(__jazorRawProps, {
  get(target, key, receiver) {
    if (typeof key === "string") {
      if (key === "tasks") {
        const value = Reflect.get(target, key, receiver);
        if (value !== undefined) return value;
        if (Object.prototype.hasOwnProperty.call(__jazorPropDefaultCache, key)) return __jazorPropDefaultCache[key];
        const defaultValue = [];
        __jazorPropDefaultCache[key] = defaultValue;
        return defaultValue;
      }
    }
    return Reflect.get(target, key, receiver);
  }
});
const emit = defineEmits<{ (event: "addRequested", payload?: any): void; (event: "update:draftCategory", payload?: any): void; (event: "update:draftPinned", payload?: any): void; (event: "update:draftTitle", payload?: any): void; (event: "update:showCompleted", payload?: any): void }>();
const __jazor$0 = computed(() => (__value) => emit("update:draftTitle", __value));
const __jazor$1 = computed(() => (__value) => emit("update:draftCategory", __value));
const __jazor$2 = computed(() => (__value) => emit("update:draftPinned", __value));
const __jazor$3 = computed(() => (__value) => emit("update:showCompleted", __value));
const __jazor$4 = computed(() => () => emit("addRequested"));
</script>
