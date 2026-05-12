<template>
<div class="playground-page-catalog">
  <VContainer :fluid="true">
    <div class="playground-hero">
      <VRow align="center">
        <VCol :cols="12" :lg="7">
          <div class="playground-hero__eyebrow">
            Library Mode Showcase
          </div>
          
            
          <h1 class="playground-hero__title">
            RazorVue + Vuetify + Pinia + VueRoute on ASP.NET Core
          </h1>
          
            
          <p class="playground-hero__body">
            
                Playground is a production-style catalog app that proves the library-mode route:
                RazorVue authors the UI, ASP.NET Core serves APIs and static assets, and the Deno consumer wires Vue runtime concerns.
            
          </p>
        </VCol>
        
        
        <VCol :cols="12" :lg="5">
          <VRow>
            <VCol :cols="12" :sm="4">
              <PlaygroundStatCardComponent eyebrow="Examples" :value="__jazorVueSfcBinding0" caption="Real workflows, not toy snippets" />
            </VCol>
            
                
            <VCol :cols="12" :sm="4">
              <PlaygroundStatCardComponent eyebrow="Featured" :value="__jazorVueSfcBinding1" caption="Opinionated reference paths" />
            </VCol>
            
                
            <VCol :cols="12" :sm="4">
              <PlaygroundStatCardComponent eyebrow="Saved" :value="__jazorVueSfcBinding2" caption="Pinned by current operator" />
            </VCol>
          </VRow>
        </VCol>
      </VRow>
    </div>
    

    
    <div class="playground-toolbar">
      <VRow align="center">
        <VCol :cols="12" :md="7">
          <VTextField label="Search examples" placeholder="Search category, runtime, workflow, API..." :density="__jazorVueSfcBinding3" :modelValue="props.model.Query" @update:modelValue="(__value) =&gt; emit(&quot;queryChanged&quot;, __value)" />
        </VCol>
        
        
        <VCol :cols="12" :md="5">
          <div class="playground-category-list">
            <VChipGroup :column="true" color="secondary" :modelValue="props.model.ActiveCategory" @update:modelValue="(__value) =&gt; emit(&quot;categorySelected&quot;, __value)">
              <template v-for="category in __jazorVueSfcBinding4">
                <div class="playground-category-list__chip">
                  <VChip class="playground-category-chip" :value="category" :variant="(category === props.model.ActiveCategory ? &quot;flat&quot; : &quot;outlined&quot;)" :text="category" />
                </div>
              </template>
            </VChipGroup>
          </div>
        </VCol>
      </VRow>
    </div>
    

    
    <div class="playground-results-summary">
      <VRow>
        <VCol :cols="12">
          <span>
            {{ __jazorVueSfcBinding5 }}
          </span>
        </VCol>
      </VRow>
    </div>
    <template v-if="__jazorVueSfcBinding6">
      <VRow>
        <VCol :cols="12">
          <PlaygroundEmptyStateComponent :title="props.model.EmptyStateTitle" :body="props.model.EmptyStateBody" />
        </VCol>
      </VRow>
    </template>
    <template v-else>
      <VRow>
        <template v-for="example in __jazorVueSfcBinding7">
          <VCol :cols="12" :md="6" :xl="4">
            <PlaygroundExampleCardComponent :example="example" :detailHref="(&quot;/examples/&quot; + example.Id + props.detailQuerySuffix)" />
          </VCol>
        </template>
      </VRow>
    </template>
  </VContainer>
</div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import PlaygroundEmptyStateComponent from "../components/playground-empty-state.vue";
import PlaygroundExampleCardComponent from "../components/playground-example-card.vue";
import PlaygroundStatCardComponent from "../components/playground-stat-card.vue";
import { VChip as VChip, VChipGroup as VChipGroup, VCol as VCol, VContainer as VContainer, VRow as VRow, VTextField as VTextField } from "vuetify/components";

const props = defineProps<{ detailQuerySuffix?: any; model?: any }>();
const emit = defineEmits<{ (event: "categorySelected", payload?: any): void; (event: "queryChanged", payload?: any): void }>();
const __jazorVueSfcBinding0 = computed(() => props.model.TotalExamples.toString());
const __jazorVueSfcBinding1 = computed(() => props.model.FeaturedCount.toString());
const __jazorVueSfcBinding2 = computed(() => props.model.FavoritesCount.toString());
const __jazorVueSfcBinding3 = computed(() => "comfortable");
const __jazorVueSfcBinding4 = computed(() => props.model.Categories);
const __jazorVueSfcBinding5 = computed(() => (props.model.VisibleCount + " result(s) matched the current view."));
const __jazorVueSfcBinding6 = computed(() => (props.model.Examples.length === 0));
const __jazorVueSfcBinding7 = computed(() => props.model.Examples);
</script>
