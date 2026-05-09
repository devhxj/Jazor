import { defineComponent as _defineComponent } from 'vue';
import { computed } from "vue";
import { VCard as VCard, VCardText as VCardText, VCardTitle as VCardTitle, VList as VList, VListItem as VListItem } from "vuetify/components";
const _sfc_main = /*@__PURE__*/ _defineComponent({
    __name: 'todo-summary-card',
    props: {
        completedCount: {},
        completedText: {},
        openCount: {},
        openText: {},
        pinnedCount: {},
        pinnedText: {},
        totalCount: {},
        totalText: {}
    },
    setup(__props, { expose: __expose, emit: __emit }) {
        __expose();
        const props = __props;
        const emit = __emit;
        const __jazorVueSfcBinding0 = computed(() => (props.pinnedCount > 0));
        const __returned__ = { props, emit, __jazorVueSfcBinding0, get VCard() { return VCard; }, get VCardText() { return VCardText; }, get VCardTitle() { return VCardTitle; }, get VList() { return VList; }, get VListItem() { return VListItem; } };
        Object.defineProperty(__returned__, '__isScriptSetup', { enumerable: false, value: true });
        return __returned__;
    }
});

import { createVNode as _createVNode, openBlock as _openBlock, createBlock as _createBlock, createCommentVNode as _createCommentVNode, withCtx as _withCtx } from "vue"

export function render(_ctx, _cache, $props, $setup, $data, $options) {
  return (_openBlock(), _createBlock($setup["VCard"], null, {
    default: _withCtx(() => [
      _createVNode($setup["VCardTitle"], { text: "Overview" }),
      _createVNode($setup["VCardText"], null, {
        default: _withCtx(() => [
          _createVNode($setup["VList"], { density: "compact" }, {
            default: _withCtx(() => [
              _createVNode($setup["VListItem"], {
                title: "All tasks",
                subtitle: $setup.props.totalText
              }, null, 8 /* PROPS */, ["subtitle"]),
              _createVNode($setup["VListItem"], {
                title: "Completed",
                subtitle: $setup.props.completedText
              }, null, 8 /* PROPS */, ["subtitle"]),
              _createVNode($setup["VListItem"], {
                title: "Open",
                subtitle: $setup.props.openText
              }, null, 8 /* PROPS */, ["subtitle"]),
              ($setup.__jazorVueSfcBinding0)
                ? (_openBlock(), _createBlock($setup["VListItem"], {
                    key: 0,
                    title: "Pinned",
                    subtitle: $setup.props.pinnedText
                  }, null, 8 /* PROPS */, ["subtitle"]))
                : _createCommentVNode("v-if", true)
            ]),
            _: 1 /* STABLE */
          })
        ]),
        _: 1 /* STABLE */
      })
    ]),
    _: 1 /* STABLE */
  }))
}

_sfc_main.render = render;

export default _sfc_main;
