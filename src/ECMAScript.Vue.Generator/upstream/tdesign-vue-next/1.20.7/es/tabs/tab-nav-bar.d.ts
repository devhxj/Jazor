import { PropType, VNode } from 'vue';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    navs: {
        type: PropType<VNode[]>;
    };
    placement: {
        type: PropType<import("./type").TdTabsProps["placement"]>;
        default: import("./type").TdTabsProps["placement"];
        validator(val: import("./type").TdTabsProps["placement"]): boolean;
    };
    value: {
        type: PropType<import("./type").TdTabsProps["value"]>;
        default: import("./type").TdTabsProps["value"];
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    navs: {
        type: PropType<VNode[]>;
    };
    placement: {
        type: PropType<import("./type").TdTabsProps["placement"]>;
        default: import("./type").TdTabsProps["placement"];
        validator(val: import("./type").TdTabsProps["placement"]): boolean;
    };
    value: {
        type: PropType<import("./type").TdTabsProps["value"]>;
        default: import("./type").TdTabsProps["value"];
    };
}>> & Readonly<{}>, {
    value: import("./type").TabValue;
    placement: "left" | "right" | "top" | "bottom";
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
