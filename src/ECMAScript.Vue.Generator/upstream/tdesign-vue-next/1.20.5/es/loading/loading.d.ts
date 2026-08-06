declare const _default: import("vue").DefineComponent<{
    attach: {
        type: import("vue").PropType<import("./type").TdLoadingProps["attach"]>;
        default: import("./type").TdLoadingProps["attach"];
    };
    content: {
        type: import("vue").PropType<import("./type").TdLoadingProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdLoadingProps["default"]>;
    };
    delay: {
        type: NumberConstructor;
        default: any;
    };
    fullscreen: BooleanConstructor;
    indicator: {
        type: import("vue").PropType<import("./type").TdLoadingProps["indicator"]>;
        default: import("./type").TdLoadingProps["indicator"];
    };
    inheritColor: {
        type: BooleanConstructor;
        default: any;
    };
    loading: {
        type: BooleanConstructor;
        default: boolean;
    };
    preventScrollThrough: {
        type: BooleanConstructor;
        default: any;
    };
    showOverlay: {
        type: BooleanConstructor;
        default: any;
    };
    size: {
        type: StringConstructor;
        default: any;
    };
    text: {
        type: import("vue").PropType<import("./type").TdLoadingProps["text"]>;
    };
    zIndex: {
        type: NumberConstructor;
    };
}, () => import("vue/jsx-runtime").JSX.Element, unknown, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps, Readonly<import("vue").ExtractPropTypes<{
    attach: {
        type: import("vue").PropType<import("./type").TdLoadingProps["attach"]>;
        default: import("./type").TdLoadingProps["attach"];
    };
    content: {
        type: import("vue").PropType<import("./type").TdLoadingProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdLoadingProps["default"]>;
    };
    delay: {
        type: NumberConstructor;
        default: any;
    };
    fullscreen: BooleanConstructor;
    indicator: {
        type: import("vue").PropType<import("./type").TdLoadingProps["indicator"]>;
        default: import("./type").TdLoadingProps["indicator"];
    };
    inheritColor: {
        type: BooleanConstructor;
        default: any;
    };
    loading: {
        type: BooleanConstructor;
        default: boolean;
    };
    preventScrollThrough: {
        type: BooleanConstructor;
        default: any;
    };
    showOverlay: {
        type: BooleanConstructor;
        default: any;
    };
    size: {
        type: StringConstructor;
        default: any;
    };
    text: {
        type: import("vue").PropType<import("./type").TdLoadingProps["text"]>;
    };
    zIndex: {
        type: NumberConstructor;
    };
}>>, {
    loading: boolean;
    size: string;
    attach: import("..").AttachNode;
    delay: number;
    indicator: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    fullscreen: boolean;
    inheritColor: boolean;
    preventScrollThrough: boolean;
    showOverlay: boolean;
}, {}>;
export default _default;
