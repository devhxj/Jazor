<template>
<template v-if="__jazor$0">
  <ElSubMenu :index="props.item.Key">
    <template #title>
      {{ props.item.Title }}
    </template>
    <template v-for="child in props.item.Children.AsArray">
      <ElementSidebarMenuNodeComponent :item="child" :selectedKey="props.selectedKey" @update:selectedKey="(__value) =&gt; emit(&quot;update:selectedKey&quot;, __value)" />
    </template>
  </ElSubMenu>
</template>
<template v-else>
  <ElMenuItem :index="props.item.Key" :disabled="__jazor$1" @click="__jazor$2">
    {{ props.item.Title }}
  </ElMenuItem>
</template>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { ElMenuItem as ElMenuItem, ElSubMenu as ElSubMenu } from "element-plus";
import ElementSidebarMenuNodeComponent from "./element-sidebar-menu-node.vue";

const props = defineProps<{ item?: any; selectedKey?: any }>();
const emit = defineEmits<{ (event: "update:selectedKey", payload?: any): void }>();
const __jazor$0 = computed(() => (!(props.item.Children === null) && !(props.item.Children.AsArray === null) && props.item.Children.AsArray.length > 0));
const __jazor$1 = computed(() => props.item.Disabled ?? false);
const __jazor$2 = computed(() => () => emit("update:selectedKey", props.item.Key));
</script>
