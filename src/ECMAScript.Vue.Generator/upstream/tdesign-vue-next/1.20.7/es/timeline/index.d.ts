import { TdTimelineProps } from './type';
import './style';
export * from './type';
export type TimelineProps = TdTimelineProps;
export declare const Timeline: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        labelAlign: {
            type: import("vue").PropType<TdTimelineProps["labelAlign"]>;
            default: TdTimelineProps["labelAlign"];
            validator(val: TdTimelineProps["labelAlign"]): boolean;
        };
        layout: {
            type: import("vue").PropType<TdTimelineProps["layout"]>;
            default: TdTimelineProps["layout"];
            validator(val: TdTimelineProps["layout"]): boolean;
        };
        mode: {
            type: import("vue").PropType<TdTimelineProps["mode"]>;
            default: TdTimelineProps["mode"];
            validator(val: TdTimelineProps["mode"]): boolean;
        };
        reverse: BooleanConstructor;
        theme: {
            type: import("vue").PropType<TdTimelineProps["theme"]>;
            default: TdTimelineProps["theme"];
            validator(val: TdTimelineProps["theme"]): boolean;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        layout: "vertical" | "horizontal";
        reverse: boolean;
        mode: "same" | "alternate";
        theme: "default" | "dot";
        labelAlign: "left" | "right" | "top" | "bottom" | "alternate";
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        labelAlign: {
            type: import("vue").PropType<TdTimelineProps["labelAlign"]>;
            default: TdTimelineProps["labelAlign"];
            validator(val: TdTimelineProps["labelAlign"]): boolean;
        };
        layout: {
            type: import("vue").PropType<TdTimelineProps["layout"]>;
            default: TdTimelineProps["layout"];
            validator(val: TdTimelineProps["layout"]): boolean;
        };
        mode: {
            type: import("vue").PropType<TdTimelineProps["mode"]>;
            default: TdTimelineProps["mode"];
            validator(val: TdTimelineProps["mode"]): boolean;
        };
        reverse: BooleanConstructor;
        theme: {
            type: import("vue").PropType<TdTimelineProps["theme"]>;
            default: TdTimelineProps["theme"];
            validator(val: TdTimelineProps["theme"]): boolean;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        layout: "vertical" | "horizontal";
        reverse: boolean;
        mode: "same" | "alternate";
        theme: "default" | "dot";
        labelAlign: "left" | "right" | "top" | "bottom" | "alternate";
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    labelAlign: {
        type: import("vue").PropType<TdTimelineProps["labelAlign"]>;
        default: TdTimelineProps["labelAlign"];
        validator(val: TdTimelineProps["labelAlign"]): boolean;
    };
    layout: {
        type: import("vue").PropType<TdTimelineProps["layout"]>;
        default: TdTimelineProps["layout"];
        validator(val: TdTimelineProps["layout"]): boolean;
    };
    mode: {
        type: import("vue").PropType<TdTimelineProps["mode"]>;
        default: TdTimelineProps["mode"];
        validator(val: TdTimelineProps["mode"]): boolean;
    };
    reverse: BooleanConstructor;
    theme: {
        type: import("vue").PropType<TdTimelineProps["theme"]>;
        default: TdTimelineProps["theme"];
        validator(val: TdTimelineProps["theme"]): boolean;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    layout: "vertical" | "horizontal";
    reverse: boolean;
    mode: "same" | "alternate";
    theme: "default" | "dot";
    labelAlign: "left" | "right" | "top" | "bottom" | "alternate";
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const TimelineItem: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        index: {
            type: NumberConstructor;
        };
        content: {
            type: import("vue").PropType<import("./type").TdTimelineItemProps["content"]>;
        };
        dot: {
            type: import("vue").PropType<import("./type").TdTimelineItemProps["dot"]>;
        };
        dotColor: {
            type: import("vue").PropType<import("./type").TdTimelineItemProps["dotColor"]>;
            default: string;
        };
        label: {
            type: import("vue").PropType<import("./type").TdTimelineItemProps["label"]>;
        };
        labelAlign: {
            type: import("vue").PropType<import("./type").TdTimelineItemProps["labelAlign"]>;
            validator(val: import("./type").TdTimelineItemProps["labelAlign"]): boolean;
        };
        loading: BooleanConstructor;
        onClick: import("vue").PropType<import("./type").TdTimelineItemProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        loading: boolean;
        dotColor: string;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        index: {
            type: NumberConstructor;
        };
        content: {
            type: import("vue").PropType<import("./type").TdTimelineItemProps["content"]>;
        };
        dot: {
            type: import("vue").PropType<import("./type").TdTimelineItemProps["dot"]>;
        };
        dotColor: {
            type: import("vue").PropType<import("./type").TdTimelineItemProps["dotColor"]>;
            default: string;
        };
        label: {
            type: import("vue").PropType<import("./type").TdTimelineItemProps["label"]>;
        };
        labelAlign: {
            type: import("vue").PropType<import("./type").TdTimelineItemProps["labelAlign"]>;
            validator(val: import("./type").TdTimelineItemProps["labelAlign"]): boolean;
        };
        loading: BooleanConstructor;
        onClick: import("vue").PropType<import("./type").TdTimelineItemProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        loading: boolean;
        dotColor: string;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    index: {
        type: NumberConstructor;
    };
    content: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["content"]>;
    };
    dot: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["dot"]>;
    };
    dotColor: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["dotColor"]>;
        default: string;
    };
    label: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["label"]>;
    };
    labelAlign: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["labelAlign"]>;
        validator(val: import("./type").TdTimelineItemProps["labelAlign"]): boolean;
    };
    loading: BooleanConstructor;
    onClick: import("vue").PropType<import("./type").TdTimelineItemProps["onClick"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    loading: boolean;
    dotColor: string;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Timeline;
