export declare const IconFont: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    name: {
        type: StringConstructor;
        default: string;
        required: boolean;
    };
    size: {
        type: import("vue").PropType<SizeEnum | string>;
        default: any;
    };
    tag: {
        type: StringConstructor;
        default: string;
    };
    url: {
        type: import("vue").PropType<import("../utils/types").TdIconfontProps["url"]>;
        default: any;
    };
    loadDefaultIcons: {
        type: BooleanConstructor;
        default: boolean;
    };
    onClick: import("vue").PropType<import("../utils/types").TdIconfontProps["onClick"]>;
}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
    [key: string]: any;
}>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    name: {
        type: StringConstructor;
        default: string;
        required: boolean;
    };
    size: {
        type: import("vue").PropType<SizeEnum | string>;
        default: any;
    };
    tag: {
        type: StringConstructor;
        default: string;
    };
    url: {
        type: import("vue").PropType<import("../utils/types").TdIconfontProps["url"]>;
        default: any;
    };
    loadDefaultIcons: {
        type: BooleanConstructor;
        default: boolean;
    };
    onClick: import("vue").PropType<import("../utils/types").TdIconfontProps["onClick"]>;
}>> & Readonly<{}>, {
    tag: string;
    size: string;
    name: string;
    url: string | string[];
    loadDefaultIcons: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default IconFont;
