import { defineComponent, provide, ref, watch } from "vue";

const missing = Symbol("jazor:cascade:missing");

function key(typeName, name) {
    return `jazor:cascade:${typeName}:${name ?? ""}`;
}

/**
 * Browser adapter for the standard Blazor CascadingValue<T> component.
 * The generated RazorVue module supplies the type token; authors keep the normal
 * <CascadingValue Value="..."> shape and never see this module.
 */
export const CascadingValue = defineComponent({
    name: "JazorCascadingValue",
    props: {
        value: { default: undefined },
        name: { default: undefined },
        isFixed: { default: false },
        __jazorCascadeType: { type: String, required: true },
    },
    setup(props, context) {
        const value = ref(props.value);
        const cascadeKey = key(props.__jazorCascadeType, props.name);
        provide(cascadeKey, value);
        if (!props.isFixed) {
            watch(() => props.value, next => {
                value.value = next;
            });
        }
        return () => {
            const slot = context.slots.default;
            return slot ? slot() : null;
        };
    },
});

export function cascadingKey(typeName, name) {
    return key(typeName, name);
}

export { missing as cascadingMissing };
