<template>
<VbenAdminLayoutComponent mode="mixed" :navItems="props.navItems" title="ECMAScript.Vben" subtitle="Element Plus injected shell" :collapsed="props.collapsed" @update:collapsed="(__value) =&gt; emit(&quot;update:collapsed&quot;, __value)" :selectedKey="props.selectedKey" @update:selectedKey="(__value) =&gt; emit(&quot;update:selectedKey&quot;, __value)" :expandedKeys="props.expandedKeys" @update:expandedKeys="(__value) =&gt; emit(&quot;update:expandedKeys&quot;, __value)">
  <template #headerActions>
    <ElButton :text="true" type="primary">
      Publish
    </ElButton>
    <ElButton>
      Refresh
    </ElButton>
  </template>
  <template #userRegion>
    <ElTag type="success" effect="light" :round="true">
      ops@prod
    </ElTag>
  </template>
  <VbenPageContainerComponent title="Operations overview" subtitle="Compile-time injected Vben shell composed with Element Plus" :breadcrumbItems="props.breadcrumbItems" :actions="props.pageActions">
    <template #extra>
      <ElButtonGroup>
        <ElButton type="primary">
          Create release
        </ElButton>
        <ElButton>
          Export report
        </ElButton>
      </ElButtonGroup>
    </template>
    <section class="vben-demo-metrics">
      <ElCard header="Build pipeline" bodyClass="vben-demo-card__body">
        <div class="vben-demo-card__metric">
          <ElText tag="strong">
            99.96%
          </ElText>
          <ElTag type="success" effect="light">
            stable
          </ElTag>
        </div>
        <p class="vben-demo-card__copy">
          Host requirements, SFC bridge generation, SSR smoke, bundle smoke, and browser smoke are all wired through the Deno consumer path.
        </p>
      </ElCard>
      <ElCard header="Container injection" bodyClass="vben-demo-card__body">
        <div class="vben-demo-card__metric">
          <ElText tag="strong">
            4/4
          </ElText>
          <ElTag type="primary" effect="light">
            shell contracts
          </ElTag>
        </div>
        <p class="vben-demo-card__copy">
          The app authors only against Vben contracts. Concrete Element Plus layout components are resolved by assembly-level [VueInject].
        </p>
      </ElCard>
      <ElCard header="Consumer contract" bodyClass="vben-demo-card__body">
        <div class="vben-demo-card__metric">
          <ElText tag="strong">
            Deno only
          </ElText>
          <ElTag type="warning" effect="light">
            official consumer entry
          </ElTag>
        </div>
        <p class="vben-demo-card__copy">
          Browser assets are bundled by deno bundle and mounted through the official razorvue-consumer-entry flow.
        </p>
      </ElCard>
    </section>
  </VbenPageContainerComponent>
</VbenAdminLayoutComponent>
</template>

<script setup lang="ts">
import { ElButton as ElButton, ElButtonGroup as ElButtonGroup, ElCard as ElCard, ElTag as ElTag, ElText as ElText } from "element-plus";
import VbenAdminLayoutComponent from "./element-admin-layout.vue";
import VbenPageContainerComponent from "./element-page-container.vue";

const props = defineProps<{ breadcrumbItems?: any; collapsed?: any; expandedKeys?: any; navItems?: any; pageActions?: any; selectedKey?: any }>();
const emit = defineEmits<{ (event: "update:collapsed", payload?: any): void; (event: "update:expandedKeys", payload?: any): void; (event: "update:selectedKey", payload?: any): void }>();
</script>
