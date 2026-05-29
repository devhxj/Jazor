<template>
<VCard>
  <VCardTitle>
    Overview
  </VCardTitle>
  
    
  <VCardText>
    <VList density="compact">
      <VListItem title="All tasks" :subtitle="props.totalText" />
      
            
      <VListItem title="Completed" :subtitle="props.completedText" />
      
            
      <VListItem title="Open" :subtitle="props.openText" />
      <template v-if="(props.pinnedCount &gt; 0)">
        <VListItem title="Pinned" :subtitle="props.pinnedText" />
      </template>
    </VList>
  </VCardText>
</VCard>
</template>

<script setup lang="ts">
import { VCard as VCard, VCardText as VCardText, VCardTitle as VCardTitle, VList as VList, VListItem as VListItem } from "vuetify/components";

const __jazorRawProps = defineProps<{ completedCount?: any; completedText?: any; openCount?: any; openText?: any; pinnedCount?: any; pinnedText?: any; totalCount?: any; totalText?: any }>();
const __jazorPropDefaultCache = Object.create(null);
const props = new Proxy(__jazorRawProps, {
  get(target, key, receiver) {
    if (typeof key === "string") {
      if (key === "totalText") {
        const value = Reflect.get(target, key, receiver);
        if (value !== undefined) return value;
        if (Object.prototype.hasOwnProperty.call(__jazorPropDefaultCache, key)) return __jazorPropDefaultCache[key];
        const defaultValue = "";
        __jazorPropDefaultCache[key] = defaultValue;
        return defaultValue;
      }
      if (key === "completedText") {
        const value = Reflect.get(target, key, receiver);
        if (value !== undefined) return value;
        if (Object.prototype.hasOwnProperty.call(__jazorPropDefaultCache, key)) return __jazorPropDefaultCache[key];
        const defaultValue = "";
        __jazorPropDefaultCache[key] = defaultValue;
        return defaultValue;
      }
      if (key === "openText") {
        const value = Reflect.get(target, key, receiver);
        if (value !== undefined) return value;
        if (Object.prototype.hasOwnProperty.call(__jazorPropDefaultCache, key)) return __jazorPropDefaultCache[key];
        const defaultValue = "";
        __jazorPropDefaultCache[key] = defaultValue;
        return defaultValue;
      }
      if (key === "pinnedText") {
        const value = Reflect.get(target, key, receiver);
        if (value !== undefined) return value;
        if (Object.prototype.hasOwnProperty.call(__jazorPropDefaultCache, key)) return __jazorPropDefaultCache[key];
        const defaultValue = "";
        __jazorPropDefaultCache[key] = defaultValue;
        return defaultValue;
      }
    }
    return Reflect.get(target, key, receiver);
  }
});
const emit = defineEmits<{ }>();
</script>
