import { defineComponent as _defineComponent } from 'vue';
import { computed } from "vue";
import TodoSummaryCardComponent from "./todo-summary-card.mjs";
import { VAlert as VAlert, VBtn as VBtn, VCard as VCard, VCardText as VCardText, VCardTitle as VCardTitle, VCheckbox as VCheckbox, VChip as VChip, VCol as VCol, VContainer as VContainer, VList as VList, VListItem as VListItem, VRow as VRow, VSwitch as VSwitch, VTextField as VTextField } from "vuetify/components";
const _sfc_main = /*@__PURE__*/ _defineComponent({
    __name: 'todo-app',
    props: {
        completedCount: {},
        draftCategory: {},
        draftPinned: {},
        draftTitle: {},
        openCount: {},
        pinnedCount: {},
        showCompleted: {},
        statusMessage: {},
        tasks: {},
        totalCount: {},
        visibleCount: {}
    },
    emits: ["addRequested", "update:draftCategory", "update:draftPinned", "update:draftTitle", "update:showCompleted"],
    setup(__props, { expose: __expose, emit: __emit }) {
        __expose();
        const props = __props;
        const emit = __emit;
        const __jazorVueSfcBinding0 = computed(() => (props.statusMessage !== null && props.statusMessage !== ""));
        const __jazorVueSfcBinding1 = computed(() => `${props.totalCount} tasks in scope`);
        const __jazorVueSfcBinding2 = computed(() => `${props.completedCount} completed`);
        const __jazorVueSfcBinding3 = computed(() => `${props.openCount} still active`);
        const __jazorVueSfcBinding4 = computed(() => `${props.pinnedCount} pinned for focus`);
        const __jazorVueSfcBinding5 = computed(() => (props.visibleCount === 0));
        const __jazorVueSfcBinding6 = computed(() => props.tasks);
        const __returned__ = { props, emit, __jazorVueSfcBinding0, __jazorVueSfcBinding1, __jazorVueSfcBinding2, __jazorVueSfcBinding3, __jazorVueSfcBinding4, __jazorVueSfcBinding5, __jazorVueSfcBinding6, TodoSummaryCardComponent, get VAlert() { return VAlert; }, get VBtn() { return VBtn; }, get VCard() { return VCard; }, get VCardText() { return VCardText; }, get VCardTitle() { return VCardTitle; }, get VCheckbox() { return VCheckbox; }, get VChip() { return VChip; }, get VCol() { return VCol; }, get VContainer() { return VContainer; }, get VList() { return VList; }, get VListItem() { return VListItem; }, get VRow() { return VRow; }, get VSwitch() { return VSwitch; }, get VTextField() { return VTextField; } };
        Object.defineProperty(__returned__, '__isScriptSetup', { enumerable: false, value: true });
        return __returned__;
    }
});

import { createVNode as _createVNode, withCtx as _withCtx, toDisplayString as _toDisplayString, openBlock as _openBlock, createElementBlock as _createElementBlock, createCommentVNode as _createCommentVNode, createBlock as _createBlock, renderList as _renderList, Fragment as _Fragment } from "vue"

const _hoisted_1 = { key: 0 }

export function render(_ctx, _cache, $props, $setup, $data, $options) {
  return (_openBlock(), _createBlock($setup["VContainer"], { fluid: true }, {
    default: _withCtx(() => [
      _createVNode($setup["VRow"], { justify: "center" }, {
        default: _withCtx(() => [
          _createVNode($setup["VCol"], {
            cols: 12,
            md: 10,
            lg: 8
          }, {
            default: _withCtx(() => [
              _createVNode($setup["VCard"], null, {
                default: _withCtx(() => [
                  _createVNode($setup["VCardTitle"], { text: "RazorVue Todo Workspace" }),
                  _createVNode($setup["VCardText"], null, {
                    default: _withCtx(() => [
                      _createVNode($setup["VRow"], null, {
                        default: _withCtx(() => [
                          _createVNode($setup["VCol"], {
                            cols: 12,
                            md: 8
                          }, {
                            default: _withCtx(() => [
                              _createVNode($setup["VTextField"], {
                                label: "New task title",
                                modelValue: $setup.props.draftTitle,
                                "onUpdate:modelValue": _cache[0] || (_cache[0] = (__value) => $setup.emit("update:draftTitle", __value))
                              }, null, 8 /* PROPS */, ["modelValue"])
                            ]),
                            _: 1 /* STABLE */
                          }),
                          _createVNode($setup["VCol"], {
                            cols: 12,
                            md: 4
                          }, {
                            default: _withCtx(() => [
                              _createVNode($setup["VTextField"], {
                                label: "Category",
                                modelValue: $setup.props.draftCategory,
                                "onUpdate:modelValue": _cache[1] || (_cache[1] = (__value) => $setup.emit("update:draftCategory", __value))
                              }, null, 8 /* PROPS */, ["modelValue"])
                            ]),
                            _: 1 /* STABLE */
                          })
                        ]),
                        _: 1 /* STABLE */
                      }),
                      _createVNode($setup["VRow"], null, {
                        default: _withCtx(() => [
                          _createVNode($setup["VCol"], {
                            cols: 12,
                            md: 4
                          }, {
                            default: _withCtx(() => [
                              _createVNode($setup["VCheckbox"], {
                                label: "Create pinned task",
                                modelValue: $setup.props.draftPinned,
                                "onUpdate:modelValue": _cache[2] || (_cache[2] = (__value) => $setup.emit("update:draftPinned", __value))
                              }, null, 8 /* PROPS */, ["modelValue"])
                            ]),
                            _: 1 /* STABLE */
                          }),
                          _createVNode($setup["VCol"], {
                            cols: 12,
                            md: 4
                          }, {
                            default: _withCtx(() => [
                              _createVNode($setup["VSwitch"], {
                                label: "Show completed",
                                modelValue: $setup.props.showCompleted,
                                "onUpdate:modelValue": _cache[3] || (_cache[3] = (__value) => $setup.emit("update:showCompleted", __value))
                              }, null, 8 /* PROPS */, ["modelValue"])
                            ]),
                            _: 1 /* STABLE */
                          }),
                          _createVNode($setup["VCol"], {
                            cols: 12,
                            md: 4
                          }, {
                            default: _withCtx(() => [
                              _createVNode($setup["VBtn"], {
                                text: "Add task",
                                onClick: _cache[4] || (_cache[4] = () => $setup.emit("addRequested"))
                              })
                            ]),
                            _: 1 /* STABLE */
                          })
                        ]),
                        _: 1 /* STABLE */
                      }),
                      ($setup.__jazorVueSfcBinding0)
                        ? (_openBlock(), _createElementBlock("p", _hoisted_1, _toDisplayString($setup.props.statusMessage), 1 /* TEXT */))
                        : _createCommentVNode("v-if", true)
                    ]),
                    _: 1 /* STABLE */
                  })
                ]),
                _: 1 /* STABLE */
              })
            ]),
            _: 1 /* STABLE */
          })
        ]),
        _: 1 /* STABLE */
      }),
      _createVNode($setup["VRow"], { justify: "center" }, {
        default: _withCtx(() => [
          _createVNode($setup["VCol"], {
            cols: 12,
            md: 10,
            lg: 8
          }, {
            default: _withCtx(() => [
              _createVNode($setup["TodoSummaryCardComponent"], {
                totalCount: $setup.props.totalCount,
                completedCount: $setup.props.completedCount,
                openCount: $setup.props.openCount,
                pinnedCount: $setup.props.pinnedCount,
                totalText: $setup.__jazorVueSfcBinding1,
                completedText: $setup.__jazorVueSfcBinding2,
                openText: $setup.__jazorVueSfcBinding3,
                pinnedText: $setup.__jazorVueSfcBinding4
              }, null, 8 /* PROPS */, ["totalCount", "completedCount", "openCount", "pinnedCount", "totalText", "completedText", "openText", "pinnedText"])
            ]),
            _: 1 /* STABLE */
          })
        ]),
        _: 1 /* STABLE */
      }),
      _createVNode($setup["VRow"], { justify: "center" }, {
        default: _withCtx(() => [
          _createVNode($setup["VCol"], {
            cols: 12,
            md: 10,
            lg: 8
          }, {
            default: _withCtx(() => [
              _createVNode($setup["VCard"], null, {
                default: _withCtx(() => [
                  _createVNode($setup["VCardTitle"], { text: "Tasks" }),
                  _createVNode($setup["VCardText"], null, {
                    default: _withCtx(() => [
                      ($setup.__jazorVueSfcBinding5)
                        ? (_openBlock(), _createBlock($setup["VAlert"], {
                            key: 0,
                            type: "info",
                            variant: "tonal",
                            text: "No tasks match the current filter."
                          }))
                        : (_openBlock(), _createBlock($setup["VList"], {
                            key: 1,
                            density: "comfortable"
                          }, {
                            default: _withCtx(() => [
                              (_openBlock(true), _createElementBlock(_Fragment, null, _renderList($setup.__jazorVueSfcBinding6, (item) => {
                                return (_openBlock(), _createElementBlock(_Fragment, null, [
                                  (($setup.props.showCompleted || !item.IsDone))
                                    ? (_openBlock(), _createBlock($setup["VListItem"], {
                                        key: 0,
                                        title: item.Title,
                                        subtitle: (item.Category + " | " + (item.IsDone ? "Completed" : "Active"))
                                      }, {
                                        default: _withCtx(() => [
                                          (item.IsPinned)
                                            ? (_openBlock(), _createBlock($setup["VChip"], {
                                                key: 0,
                                                text: "Pinned",
                                                color: "primary"
                                              }))
                                            : _createCommentVNode("v-if", true)
                                        ]),
                                        _: 2 /* DYNAMIC */
                                      }, 1032 /* PROPS, DYNAMIC_SLOTS */, ["title", "subtitle"]))
                                    : _createCommentVNode("v-if", true)
                                ], 64 /* STABLE_FRAGMENT */))
                              }), 256 /* UNKEYED_FRAGMENT */))
                            ]),
                            _: 1 /* STABLE */
                          }))
                    ]),
                    _: 1 /* STABLE */
                  })
                ]),
                _: 1 /* STABLE */
              })
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
