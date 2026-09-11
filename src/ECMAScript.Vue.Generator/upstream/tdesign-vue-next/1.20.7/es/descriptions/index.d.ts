import { TdDescriptionsProps, TdDescriptionsItemProps } from './type';
import './style';
export * from './type';
export type DescriptionsProps = TdDescriptionsProps;
export type DescriptionsItemProps = TdDescriptionsItemProps;
export declare const Descriptions: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        bordered?: boolean;
        colon?: boolean;
        column?: number;
        contentStyle?: import("..").Styles;
        itemLayout?: "horizontal" | "vertical";
        items?: Array<TdDescriptionsItemProps>;
        labelStyle?: import("..").Styles;
        layout?: "horizontal" | "vertical";
        size?: import("..").SizeEnum;
        tableLayout?: "fixed" | "auto";
        title?: string | import("..").TNode;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        layout: "vertical" | "horizontal";
        size: import("..").SizeEnum;
        tableLayout: "fixed" | "auto";
        column: number;
        colon: boolean;
        itemLayout: "vertical" | "horizontal";
        bordered: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        bordered?: boolean;
        colon?: boolean;
        column?: number;
        contentStyle?: import("..").Styles;
        itemLayout?: "horizontal" | "vertical";
        items?: Array<TdDescriptionsItemProps>;
        labelStyle?: import("..").Styles;
        layout?: "horizontal" | "vertical";
        size?: import("..").SizeEnum;
        tableLayout?: "fixed" | "auto";
        title?: string | import("..").TNode;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        layout: "vertical" | "horizontal";
        size: import("..").SizeEnum;
        tableLayout: "fixed" | "auto";
        column: number;
        colon: boolean;
        itemLayout: "vertical" | "horizontal";
        bordered: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    bordered?: boolean;
    colon?: boolean;
    column?: number;
    contentStyle?: import("..").Styles;
    itemLayout?: "horizontal" | "vertical";
    items?: Array<TdDescriptionsItemProps>;
    labelStyle?: import("..").Styles;
    layout?: "horizontal" | "vertical";
    size?: import("..").SizeEnum;
    tableLayout?: "fixed" | "auto";
    title?: string | import("..").TNode;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    layout: "vertical" | "horizontal";
    size: import("..").SizeEnum;
    tableLayout: "fixed" | "auto";
    column: number;
    colon: boolean;
    itemLayout: "vertical" | "horizontal";
    bordered: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const DescriptionsItem: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        content: {
            type: import("vue").PropType<TdDescriptionsItemProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdDescriptionsItemProps["default"]>;
        };
        label: {
            type: import("vue").PropType<TdDescriptionsItemProps["label"]>;
        };
        span: {
            type: NumberConstructor;
            default: number;
        };
    }>> & Readonly<{}>, {}, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        span: number;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        content: {
            type: import("vue").PropType<TdDescriptionsItemProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdDescriptionsItemProps["default"]>;
        };
        label: {
            type: import("vue").PropType<TdDescriptionsItemProps["label"]>;
        };
        span: {
            type: NumberConstructor;
            default: number;
        };
    }>> & Readonly<{}>, {}, {}, {}, {}, {
        span: number;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    content: {
        type: import("vue").PropType<TdDescriptionsItemProps["content"]>;
    };
    default: {
        type: import("vue").PropType<TdDescriptionsItemProps["default"]>;
    };
    label: {
        type: import("vue").PropType<TdDescriptionsItemProps["label"]>;
    };
    span: {
        type: NumberConstructor;
        default: number;
    };
}>> & Readonly<{}>, {}, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    span: number;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Descriptions;
