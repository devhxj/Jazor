<template>
<ElContainer direction="vertical" :style="props.cssStyle" :additionalAttributes="props.additionalAttributes">
  <ElHeader height="72px" class="vben-ep-shell__header">
    <template v-if="__jazor$0">
      <slot name="header" />
    </template>
    <template v-else>
      <VbenHeaderBarComponent :title="props.title" :subtitle="props.subtitle">
        <template #logo>
          <slot name="logo" />
        </template>
        <template #actions>
          <slot name="headerActions" />
        </template>
        <template #userRegion>
          <slot name="userRegion" />
        </template>
      </VbenHeaderBarComponent>
    </template>
  </ElHeader>
  <ElContainer class="vben-ep-shell__body">
    <ElAside width="280px" class="vben-ep-shell__aside">
      <template v-if="__jazor$1">
        <slot name="sidebar" />
      </template>
      <template v-else>
        <VbenSidebarMenuComponent :items="props.navItems" :collapsed="props.collapsed" :selectedKey="props.selectedKey" :expandedKeys="props.expandedKeys" @update:selectedKey="(__value) =&gt; emit(&quot;update:selectedKey&quot;, __value)" @update:expandedKeys="(__value) =&gt; emit(&quot;update:expandedKeys&quot;, __value)">
          <template #logo>
            <slot name="logo" />
          </template>
        </VbenSidebarMenuComponent>
      </template>
    </ElAside>
    <ElMain class="vben-ep-shell__main">
      <slot />
    </ElMain>
  </ElContainer>
</ElContainer>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { ElAside as ElAside, ElContainer as ElContainer, ElHeader as ElHeader, ElMain as ElMain } from "element-plus";
import VbenHeaderBarComponent from "./element-header-bar.vue";
import VbenSidebarMenuComponent from "./element-sidebar-menu.vue";

const props = defineProps<{ additionalAttributes?: any; collapsed?: any; cssClass?: any; cssStyle?: any; expandedKeys?: any; mode?: any; navItems?: any; selectedKey?: any; subtitle?: any; title?: any }>();
const emit = defineEmits<{ (event: "update:collapsed", payload?: any): void; (event: "update:expandedKeys", payload?: any): void; (event: "update:selectedKey", payload?: any): void }>();
const __jazor$0 = computed(() => !(props.header === null));
const __jazor$1 = computed(() => !(props.sidebar === null));
</script>
