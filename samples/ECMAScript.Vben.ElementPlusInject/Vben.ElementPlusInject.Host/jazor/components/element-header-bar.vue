<template>
<div v-bind="__jazorVueMergeAttributes({ &quot;class&quot;: &quot;vben-ep-header&quot;, &quot;style&quot;: props.cssStyle }, props.additionalAttributes)">
  <div class="vben-ep-header__identity">
    <template v-if="__jazor$0">
      <div class="vben-ep-header__logo">
        <slot name="logo" />
      </div>
    </template>
    <div class="vben-ep-header__titles">
      <template v-if="__jazor$1">
        <ElText tag="strong" class="vben-ep-header__title">
          {{ props.title }}
        </ElText>
      </template>
      <template v-if="__jazor$2">
        <ElText tag="span" type="info" class="vben-ep-header__subtitle">
          {{ props.subtitle }}
        </ElText>
      </template>
    </div>
  </div>
  <div class="vben-ep-header__actions">
    <slot name="actions" />
    <slot name="userRegion" />
  </div>
</div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { ElText as ElText } from "element-plus";

const props = defineProps<{ additionalAttributes?: any; cssClass?: any; cssStyle?: any; subtitle?: any; title?: any }>();
const emit = defineEmits<{ }>();
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
const __jazor$1 = computed(() => !(!props.title?.trim()));
const __jazor$2 = computed(() => !(!props.subtitle?.trim()));
</script>
