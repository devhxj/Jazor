declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    index: NumberConstructor;
    content: {
        type: import("vue").PropType<import("./type").TdStepItemProps["content"]>;
        default: import("./type").TdStepItemProps["content"];
    };
    default: {
        type: import("vue").PropType<import("./type").TdStepItemProps["default"]>;
    };
    extra: {
        type: import("vue").PropType<import("./type").TdStepItemProps["extra"]>;
    };
    icon: {
        type: import("vue").PropType<import("./type").TdStepItemProps["icon"]>;
        default: import("./type").TdStepItemProps["icon"];
    };
    status: {
        type: import("vue").PropType<import("./type").TdStepItemProps["status"]>;
        default: import("./type").TdStepItemProps["status"];
        validator(val: import("./type").TdStepItemProps["status"]): boolean;
    };
    title: {
        type: import("vue").PropType<import("./type").TdStepItemProps["title"]>;
        default: import("./type").TdStepItemProps["title"];
    };
    value: {
        type: import("vue").PropType<import("./type").TdStepItemProps["value"]>;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    index: NumberConstructor;
    content: {
        type: import("vue").PropType<import("./type").TdStepItemProps["content"]>;
        default: import("./type").TdStepItemProps["content"];
    };
    default: {
        type: import("vue").PropType<import("./type").TdStepItemProps["default"]>;
    };
    extra: {
        type: import("vue").PropType<import("./type").TdStepItemProps["extra"]>;
    };
    icon: {
        type: import("vue").PropType<import("./type").TdStepItemProps["icon"]>;
        default: import("./type").TdStepItemProps["icon"];
    };
    status: {
        type: import("vue").PropType<import("./type").TdStepItemProps["status"]>;
        default: import("./type").TdStepItemProps["status"];
        validator(val: import("./type").TdStepItemProps["status"]): boolean;
    };
    title: {
        type: import("vue").PropType<import("./type").TdStepItemProps["title"]>;
        default: import("./type").TdStepItemProps["title"];
    };
    value: {
        type: import("vue").PropType<import("./type").TdStepItemProps["value"]>;
    };
}>> & Readonly<{}>, {
    icon: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    status: import("./type").StepStatus;
    title: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    content: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
