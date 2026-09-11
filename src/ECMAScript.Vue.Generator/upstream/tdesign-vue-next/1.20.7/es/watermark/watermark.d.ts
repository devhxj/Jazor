declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    alpha: {
        type: NumberConstructor;
        default: number;
    };
    content: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["default"]>;
    };
    height: {
        type: NumberConstructor;
    };
    isRepeat: {
        type: BooleanConstructor;
        default: boolean;
    };
    layout: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["layout"]>;
        default: import("./type").TdWatermarkProps["layout"];
        validator(val: import("./type").TdWatermarkProps["layout"]): boolean;
    };
    lineSpace: {
        type: NumberConstructor;
        default: number;
    };
    movable: BooleanConstructor;
    moveInterval: {
        type: NumberConstructor;
        default: number;
    };
    offset: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["offset"]>;
    };
    removable: {
        type: BooleanConstructor;
        default: boolean;
    };
    rotate: {
        type: NumberConstructor;
        default: number;
    };
    watermarkContent: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["watermarkContent"]>;
    };
    width: {
        type: NumberConstructor;
    };
    x: {
        type: NumberConstructor;
    };
    y: {
        type: NumberConstructor;
    };
    zIndex: {
        type: NumberConstructor;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    alpha: {
        type: NumberConstructor;
        default: number;
    };
    content: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["default"]>;
    };
    height: {
        type: NumberConstructor;
    };
    isRepeat: {
        type: BooleanConstructor;
        default: boolean;
    };
    layout: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["layout"]>;
        default: import("./type").TdWatermarkProps["layout"];
        validator(val: import("./type").TdWatermarkProps["layout"]): boolean;
    };
    lineSpace: {
        type: NumberConstructor;
        default: number;
    };
    movable: BooleanConstructor;
    moveInterval: {
        type: NumberConstructor;
        default: number;
    };
    offset: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["offset"]>;
    };
    removable: {
        type: BooleanConstructor;
        default: boolean;
    };
    rotate: {
        type: NumberConstructor;
        default: number;
    };
    watermarkContent: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["watermarkContent"]>;
    };
    width: {
        type: NumberConstructor;
    };
    x: {
        type: NumberConstructor;
    };
    y: {
        type: NumberConstructor;
    };
    zIndex: {
        type: NumberConstructor;
    };
}>> & Readonly<{}>, {
    layout: "rectangular" | "hexagonal";
    alpha: number;
    rotate: number;
    lineSpace: number;
    removable: boolean;
    isRepeat: boolean;
    movable: boolean;
    moveInterval: number;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
