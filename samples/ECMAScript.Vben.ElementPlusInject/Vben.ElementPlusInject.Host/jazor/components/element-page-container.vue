<template>
<section v-bind="__jazorVueMergeAttributes({ &quot;class&quot;: &quot;vben-ep-page&quot;, &quot;style&quot;: props.cssStyle }, props.additionalAttributes)">
  <ElCard bodyClass="vben-ep-page-card__body">
    <template #header>
      <div class="vben-ep-page__header">
        <div class="vben-ep-page__title-group">
          <template v-if="__jazor$0">
            <ElBreadcrumb>
              <template v-for="item in props.breadcrumbItems">
                <ElBreadcrumbItem>
                  {{ item.Title }}
                </ElBreadcrumbItem>
              </template>
            </ElBreadcrumb>
          </template>
          <template v-if="__jazor$1">
            <h1 class="vben-ep-page__title">
              {{ props.title }}
            </h1>
          </template>
          <template v-if="__jazor$2">
            <p class="vben-ep-page__subtitle">
              {{ props.subtitle }}
            </p>
          </template>
        </div>
        <template v-if="__jazor$3">
          <div class="vben-ep-page__toolbar">
            <template v-if="__jazor$4">
              <ElButtonGroup>
                <template v-for="action in props.actions">
                  <ElButton :type="resolveButtonType(action.Kind)" :disabled="action.Disabled ?? false">
                    {{ action.Text }}
                  </ElButton>
                </template>
              </ElButtonGroup>
            </template>
            <template v-if="__jazor$5">
              <div class="vben-ep-page__extra">
                <slot name="extra" />
              </div>
            </template>
          </div>
        </template>
      </div>
    </template>
    <div class="vben-ep-page__body">
      <slot />
    </div>
  </ElCard>
</section>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { ElBreadcrumb as ElBreadcrumb, ElBreadcrumbItem as ElBreadcrumbItem, ElButton as ElButton, ElButtonGroup as ElButtonGroup, ElCard as ElCard } from "element-plus";

const props = defineProps<{ actions?: any; additionalAttributes?: any; breadcrumbItems?: any; cssClass?: any; cssStyle?: any; subtitle?: any; title?: any }>();
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

const __jazor$0 = computed(() => Array.isArray(props.breadcrumbItems) && (props.breadcrumbItems != null && ("length" in props.breadcrumbItems && props.breadcrumbItems.length > 0)));
const __jazor$1 = computed(() => !(!props.title?.trim()));
const __jazor$2 = computed(() => !(!props.subtitle?.trim()));
const __jazor$3 = computed(() => (Array.isArray(props.actions) && (props.actions != null && ("length" in props.actions && props.actions.length > 0)) || !(props.extra === null)));
const __jazor$4 = computed(() => Array.isArray(props.actions) && (props.actions != null && ("length" in props.actions && props.actions.length > 0)));
const __jazor$5 = computed(() => !(props.extra === null));
</script>
