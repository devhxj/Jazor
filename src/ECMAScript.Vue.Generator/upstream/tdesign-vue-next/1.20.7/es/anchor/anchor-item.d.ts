declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    href: {
        type: StringConstructor;
        required: boolean;
        validator(v: string): boolean;
    };
    target: {
        type: import("vue").PropType<import("./type").TdAnchorItemProps["target"]>;
        default: import("./type").TdAnchorItemProps["target"];
        validator(val: import("./type").TdAnchorItemProps["target"]): boolean;
    };
    title: {
        type: import("vue").PropType<import("./type").TdAnchorItemProps["title"]>;
        default: string;
    };
    customScroll: {
        type: BooleanConstructor;
        default: boolean;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    href: {
        type: StringConstructor;
        required: boolean;
        validator(v: string): boolean;
    };
    target: {
        type: import("vue").PropType<import("./type").TdAnchorItemProps["target"]>;
        default: import("./type").TdAnchorItemProps["target"];
        validator(val: import("./type").TdAnchorItemProps["target"]): boolean;
    };
    title: {
        type: import("vue").PropType<import("./type").TdAnchorItemProps["title"]>;
        default: string;
    };
    customScroll: {
        type: BooleanConstructor;
        default: boolean;
    };
}>> & Readonly<{}>, {
    title: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    target: "_self" | "_blank" | "_parent" | "_top";
    customScroll: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
