declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    close: {
        type: import("vue").PropType<import("./type").TdAlertProps["close"]>;
        default: import("./type").TdAlertProps["close"];
    };
    closeBtn: {
        type: import("vue").PropType<import("./type").TdAlertProps["closeBtn"]>;
        default: import("./type").TdAlertProps["closeBtn"];
    };
    default: {
        type: import("vue").PropType<import("./type").TdAlertProps["default"]>;
    };
    icon: {
        type: import("vue").PropType<import("./type").TdAlertProps["icon"]>;
    };
    maxLine: {
        type: NumberConstructor;
        default: number;
    };
    message: {
        type: import("vue").PropType<import("./type").TdAlertProps["message"]>;
    };
    operation: {
        type: import("vue").PropType<import("./type").TdAlertProps["operation"]>;
    };
    theme: {
        type: import("vue").PropType<import("./type").TdAlertProps["theme"]>;
        default: import("./type").TdAlertProps["theme"];
        validator(val: import("./type").TdAlertProps["theme"]): boolean;
    };
    title: {
        type: import("vue").PropType<import("./type").TdAlertProps["title"]>;
    };
    onClose: import("vue").PropType<import("./type").TdAlertProps["onClose"]>;
    onClosed: import("vue").PropType<import("./type").TdAlertProps["onClosed"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    close: {
        type: import("vue").PropType<import("./type").TdAlertProps["close"]>;
        default: import("./type").TdAlertProps["close"];
    };
    closeBtn: {
        type: import("vue").PropType<import("./type").TdAlertProps["closeBtn"]>;
        default: import("./type").TdAlertProps["closeBtn"];
    };
    default: {
        type: import("vue").PropType<import("./type").TdAlertProps["default"]>;
    };
    icon: {
        type: import("vue").PropType<import("./type").TdAlertProps["icon"]>;
    };
    maxLine: {
        type: NumberConstructor;
        default: number;
    };
    message: {
        type: import("vue").PropType<import("./type").TdAlertProps["message"]>;
    };
    operation: {
        type: import("vue").PropType<import("./type").TdAlertProps["operation"]>;
    };
    theme: {
        type: import("vue").PropType<import("./type").TdAlertProps["theme"]>;
        default: import("./type").TdAlertProps["theme"];
        validator(val: import("./type").TdAlertProps["theme"]): boolean;
    };
    title: {
        type: import("vue").PropType<import("./type").TdAlertProps["title"]>;
    };
    onClose: import("vue").PropType<import("./type").TdAlertProps["onClose"]>;
    onClosed: import("vue").PropType<import("./type").TdAlertProps["onClosed"]>;
}>> & Readonly<{}>, {
    close: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    theme: "error" | "info" | "success" | "warning";
    closeBtn: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    maxLine: number;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
