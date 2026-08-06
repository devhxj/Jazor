export declare const Icon: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    name: {
        type: StringConstructor;
        default: string;
    };
    size: {
        type: import("vue").PropType<SizeEnum | string>;
        default: any;
    };
    url: {
        type: import("vue").PropType<import("../utils/types").TdIconSVGProps["url"]>;
        default: any;
    };
    loadDefaultIcons: {
        type: BooleanConstructor;
        default: boolean;
    };
    onClick: import("vue").PropType<import("../utils/types").TdIconSVGProps["onClick"]>;
}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
    [key: string]: any;
}>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    name: {
        type: StringConstructor;
        default: string;
    };
    size: {
        type: import("vue").PropType<SizeEnum | string>;
        default: any;
    };
    url: {
        type: import("vue").PropType<import("../utils/types").TdIconSVGProps["url"]>;
        default: any;
    };
    loadDefaultIcons: {
        type: BooleanConstructor;
        default: boolean;
    };
    onClick: import("vue").PropType<import("../utils/types").TdIconSVGProps["onClick"]>;
}>> & Readonly<{}>, {
    size: string;
    name: string;
    url: string | string[];
    loadDefaultIcons: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default Icon;
