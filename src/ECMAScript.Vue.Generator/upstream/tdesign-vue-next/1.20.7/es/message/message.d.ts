declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    placement: StringConstructor;
    className: StringConstructor;
    closeBtn: {
        type: import("vue").PropType<import("./type").TdMessageProps["closeBtn"]>;
        default: import("./type").TdMessageProps["closeBtn"];
    };
    content: {
        type: import("vue").PropType<import("./type").TdMessageProps["content"]>;
    };
    duration: {
        type: NumberConstructor;
        default: number;
    };
    icon: {
        type: import("vue").PropType<import("./type").TdMessageProps["icon"]>;
        default: import("./type").TdMessageProps["icon"];
    };
    theme: {
        type: import("vue").PropType<import("./type").TdMessageProps["theme"]>;
        default: import("./type").TdMessageProps["theme"];
        validator(val: import("./type").TdMessageProps["theme"]): boolean;
    };
    onClose: import("vue").PropType<import("./type").TdMessageProps["onClose"]>;
    onCloseBtnClick: import("vue").PropType<import("./type").TdMessageProps["onCloseBtnClick"]>;
    onDurationEnd: import("vue").PropType<import("./type").TdMessageProps["onDurationEnd"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    placement: StringConstructor;
    className: StringConstructor;
    closeBtn: {
        type: import("vue").PropType<import("./type").TdMessageProps["closeBtn"]>;
        default: import("./type").TdMessageProps["closeBtn"];
    };
    content: {
        type: import("vue").PropType<import("./type").TdMessageProps["content"]>;
    };
    duration: {
        type: NumberConstructor;
        default: number;
    };
    icon: {
        type: import("vue").PropType<import("./type").TdMessageProps["icon"]>;
        default: import("./type").TdMessageProps["icon"];
    };
    theme: {
        type: import("vue").PropType<import("./type").TdMessageProps["theme"]>;
        default: import("./type").TdMessageProps["theme"];
        validator(val: import("./type").TdMessageProps["theme"]): boolean;
    };
    onClose: import("vue").PropType<import("./type").TdMessageProps["onClose"]>;
    onCloseBtnClick: import("vue").PropType<import("./type").TdMessageProps["onCloseBtnClick"]>;
    onDurationEnd: import("vue").PropType<import("./type").TdMessageProps["onDurationEnd"]>;
}>> & Readonly<{}>, {
    icon: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    duration: number;
    theme: import("./type").MessageThemeList;
    closeBtn: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
