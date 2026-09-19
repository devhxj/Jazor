import { defineComponent, h } from "vue";

function toPixelSize(size) {
    return typeof size === "number" || !isNaN(Number(size)) ? Number(size) : 24;
}

function toMaskData(viewBox, content) {
    const svgContent = content.replace(/currentColor/g, "#000000");
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="${viewBox}">${svgContent}</svg>`;
    return "data:image/svg+xml," + encodeURIComponent(svg);
}

export function createIconStyle(size, color, maskData) {
    const pixelSize = toPixelSize(size) + "px";
    return {
        "--vu-icon-size": pixelSize,
        "--vu-icon-color": color === "currentColor" ? "#333333" : color,
        "--vu-icon-mask": "url(" + maskData + ")"
    };
}

export function createVuIcon(componentName, viewBox, content) {
    const maskData = toMaskData(viewBox, content);

    return defineComponent({
        name: componentName,
        inheritAttrs: false,
        props: {
            size: { type: [Number, String], default: 24 },
            color: { type: String, default: "currentColor" },
            className: { type: String, default: "" },
            spin: { type: Boolean, default: false }
        },
        setup(props, { attrs }) {
            return () => h("div", {
                ...attrs,
                class: [attrs.class, props.className, "vu-icon", props.spin && "vu-icon-spin"],
                style: [attrs.style, createIconStyle(props.size, props.color, maskData)]
            });
        }
    });
}