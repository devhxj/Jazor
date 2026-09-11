declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    container: {
        type: import("vue").PropType<import("./type").TdAffixProps["container"]>;
        default: () => import("./type").TdAffixProps["container"];
    };
    content: {
        type: import("vue").PropType<import("./type").TdAffixProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdAffixProps["default"]>;
    };
    offsetBottom: {
        type: NumberConstructor;
        default: number;
    };
    offsetTop: {
        type: NumberConstructor;
        default: number;
    };
    zIndex: {
        type: NumberConstructor;
    };
    onFixedChange: import("vue").PropType<import("./type").TdAffixProps["onFixedChange"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "fixedChange"[], "fixedChange", import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    container: {
        type: import("vue").PropType<import("./type").TdAffixProps["container"]>;
        default: () => import("./type").TdAffixProps["container"];
    };
    content: {
        type: import("vue").PropType<import("./type").TdAffixProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdAffixProps["default"]>;
    };
    offsetBottom: {
        type: NumberConstructor;
        default: number;
    };
    offsetTop: {
        type: NumberConstructor;
        default: number;
    };
    zIndex: {
        type: NumberConstructor;
    };
    onFixedChange: import("vue").PropType<import("./type").TdAffixProps["onFixedChange"]>;
}>> & Readonly<{
    onFixedChange?: (...args: any[]) => any;
}>, {
    offsetTop: number;
    container: import("..").ScrollContainer;
    offsetBottom: number;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
