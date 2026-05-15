<template>
<div v-bind="__jazorVueMergeAttributes({ &quot;class&quot;: (props.collapsed ? &quot;vben-ep-sidebar vben-ep-sidebar--collapsed&quot; : &quot;vben-ep-sidebar&quot;), &quot;style&quot;: props.cssStyle }, props.additionalAttributes)">
  <template v-if="__jazor$0">
    <div class="vben-ep-sidebar__logo">
      <slot name="logo" />
    </div>
  </template>
  <ElScrollbar class="vben-ep-sidebar__scroll">
    <ElMenu mode="vertical" :collapse="props.collapsed" :defaultActive="props.selectedKey" :defaultOpeneds="props.expandedKeys" class="vben-ep-sidebar__menu">
      <template v-if="__jazor$1">
        <template v-for="item in props.items.AsArray">
          <ElementSidebarMenuNodeComponent :item="item" :selectedKey="props.selectedKey" @update:selectedKey="(__value) =&gt; emit(&quot;update:selectedKey&quot;, __value)" />
        </template>
      </template>
    </ElMenu>
  </ElScrollbar>
</div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { ElMenu as ElMenu, ElScrollbar as ElScrollbar } from "element-plus";
import ElementSidebarMenuNodeComponent from "./element-sidebar-menu-node.vue";

const props = defineProps<{ additionalAttributes?: any; collapsed?: any; cssClass?: any; cssStyle?: any; expandedKeys?: any; items?: any; selectedKey?: any }>();
const emit = defineEmits<{ (event: "update:expandedKeys", payload?: any): void; (event: "update:selectedKey", payload?: any): void }>();
function __jazorVueAssignMergedAttribute(target, key, value) {
  if (typeof key !== "string" || key.length === 0) {
    throw new Error("RazorVue attribute spread encountered a non-string attribute name.");
  }
  target[key] = value;
}

function __jazorVueAssignMergedAttributeEntry(target, entry) {
  if (Array.isArray(entry)) {
    if (entry.length < 2) {
      throw new Error("RazorVue attribute spread encountered an entry tuple without both name and value.");
    }
    __jazorVueAssignMergedAttribute(target, entry[0], entry[1]);
    return;
  }
  if (entry && typeof entry === "object") {
    if ("Key" in entry && "Value" in entry) {
      __jazorVueAssignMergedAttribute(target, entry.Key, entry.Value);
      return;
    }
    if ("key" in entry && "value" in entry) {
      __jazorVueAssignMergedAttribute(target, entry.key, entry.value);
      return;
    }
  }
  throw new Error("RazorVue attribute spread only supports object-like dictionaries, Maps, or key/value entry sequences.");
}

function __jazorVueMergeAttributes(...sources) {
  const result = {};
  for (const source of sources) {
    if (source === null || source === undefined) {
      continue;
    }
    if (source instanceof Map) {
      for (const entry of source) {
        __jazorVueAssignMergedAttributeEntry(result, entry);
      }
      continue;
    }
    if (typeof source !== "string" && typeof source[Symbol.iterator] === "function") {
      for (const entry of source) {
        __jazorVueAssignMergedAttributeEntry(result, entry);
      }
      continue;
    }
    if (typeof source === "object") {
      for (const key of Object.keys(source)) {
        __jazorVueAssignMergedAttribute(result, key, source[key]);
      }
      continue;
    }
    throw new Error("RazorVue attribute spread only supports object-like dictionaries, Maps, or key/value entry sequences.");
  }
  return result;
}

const __jazor$0 = computed(() => !(props.logo === null));
const __jazor$1 = computed(() => (!(props.items === null) && !(props.items.AsArray === null) && props.items.AsArray.length > 0));
</script>
