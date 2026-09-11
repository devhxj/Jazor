declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    color: {
        type: StringConstructor;
        default: string;
    };
    content: {
        type: import("vue").PropType<import("./type").TdBadgeProps["content"]>;
    };
    count: {
        type: import("vue").PropType<import("./type").TdBadgeProps["count"]>;
        default: number;
    };
    default: {
        type: import("vue").PropType<import("./type").TdBadgeProps["default"]>;
    };
    dot: BooleanConstructor;
    maxCount: {
        type: NumberConstructor;
        default: number;
    };
    offset: {
        type: import("vue").PropType<import("./type").TdBadgeProps["offset"]>;
    };
    shape: {
        type: import("vue").PropType<import("./type").TdBadgeProps["shape"]>;
        default: import("./type").TdBadgeProps["shape"];
        validator(val: import("./type").TdBadgeProps["shape"]): boolean;
    };
    showZero: BooleanConstructor;
    size: {
        type: import("vue").PropType<import("./type").TdBadgeProps["size"]>;
        default: import("./type").TdBadgeProps["size"];
        validator(val: import("./type").TdBadgeProps["size"]): boolean;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    color: {
        type: StringConstructor;
        default: string;
    };
    content: {
        type: import("vue").PropType<import("./type").TdBadgeProps["content"]>;
    };
    count: {
        type: import("vue").PropType<import("./type").TdBadgeProps["count"]>;
        default: number;
    };
    default: {
        type: import("vue").PropType<import("./type").TdBadgeProps["default"]>;
    };
    dot: BooleanConstructor;
    maxCount: {
        type: NumberConstructor;
        default: number;
    };
    offset: {
        type: import("vue").PropType<import("./type").TdBadgeProps["offset"]>;
    };
    shape: {
        type: import("vue").PropType<import("./type").TdBadgeProps["shape"]>;
        default: import("./type").TdBadgeProps["shape"];
        validator(val: import("./type").TdBadgeProps["shape"]): boolean;
    };
    showZero: BooleanConstructor;
    size: {
        type: import("vue").PropType<import("./type").TdBadgeProps["size"]>;
        default: import("./type").TdBadgeProps["size"];
        validator(val: import("./type").TdBadgeProps["size"]): boolean;
    };
}>> & Readonly<{}>, {
    color: string;
    count: string | number | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    size: "small" | "medium";
    shape: "circle" | "round";
    dot: boolean;
    maxCount: number;
    showZero: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
