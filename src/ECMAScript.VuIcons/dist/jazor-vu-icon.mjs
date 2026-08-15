import { computed, defineComponent, h } from "vue";
import { iconData } from "./icons-data.js";
import { createIconStyle } from "./jazor-vu-icon-runtime.mjs";

export const VuIcon = defineComponent({
    name: "VuIcon",
    inheritAttrs: false,
    props: {
        name: { type: String, default: "" },
        icon: { type: String, default: "" },
        size: { type: [Number, String], default: 24 },
        color: { type: String, default: "currentColor" },
        spin: { type: Boolean, default: false }
    },
    setup(props, { attrs }) {
        const iconName = computed(() => props.name || props.icon);
        const maskData = computed(() => {
            const data = iconData[iconName.value];
            if (!data) return "";

            const svgContent = data.content.replace(/currentColor/g, "#000000");
            const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="${data.viewBox}">${svgContent}</svg>`;
            return "data:image/svg+xml," + encodeURIComponent(svg);
        });

        return () => h("div", {
            ...attrs,
            class: [attrs.class, "vu-icon", props.spin && "vu-icon-spin"],
            style: [attrs.style, createIconStyle(props.size, props.color, maskData.value)]
        });
    }
});