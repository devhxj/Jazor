import { TdAnchorProps, TdAnchorTargetProps, TdAnchorItemProps } from './type';
import './style';
export * from './type';
export type AnchorProps = TdAnchorProps;
export type AnchorTargetProps = TdAnchorTargetProps;
export type AnchorItemProps = TdAnchorItemProps;
export declare const Anchor: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        affixProps?: import("..").AffixProps;
        bounds?: number;
        container?: import("..").ScrollContainer;
        cursor?: import("..").TNode;
        size?: "small" | "medium" | "large";
        targetOffset?: number;
        onChange?: (currentLink: string, prevLink: string) => void;
        onClick?: (link: {
            href: string;
            title: string;
            e: MouseEvent;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        size: "small" | "medium" | "large";
        container: import("..").ScrollContainer;
        bounds: number;
        targetOffset: number;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        affixProps?: import("..").AffixProps;
        bounds?: number;
        container?: import("..").ScrollContainer;
        cursor?: import("..").TNode;
        size?: "small" | "medium" | "large";
        targetOffset?: number;
        onChange?: (currentLink: string, prevLink: string) => void;
        onClick?: (link: {
            href: string;
            title: string;
            e: MouseEvent;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        size: "small" | "medium" | "large";
        container: import("..").ScrollContainer;
        bounds: number;
        targetOffset: number;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    affixProps?: import("..").AffixProps;
    bounds?: number;
    container?: import("..").ScrollContainer;
    cursor?: import("..").TNode;
    size?: "small" | "medium" | "large";
    targetOffset?: number;
    onChange?: (currentLink: string, prevLink: string) => void;
    onClick?: (link: {
        href: string;
        title: string;
        e: MouseEvent;
    }) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    size: "small" | "medium" | "large";
    container: import("..").ScrollContainer;
    bounds: number;
    targetOffset: number;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const AnchorItem: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        href: {
            type: StringConstructor;
            required: boolean;
            validator(v: string): boolean;
        };
        target: {
            type: import("vue").PropType<TdAnchorItemProps["target"]>;
            default: TdAnchorItemProps["target"];
            validator(val: TdAnchorItemProps["target"]): boolean;
        };
        title: {
            type: import("vue").PropType<TdAnchorItemProps["title"]>;
            default: string;
        };
        customScroll: {
            type: BooleanConstructor;
            default: boolean;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        title: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        target: "_self" | "_blank" | "_parent" | "_top";
        customScroll: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        href: {
            type: StringConstructor;
            required: boolean;
            validator(v: string): boolean;
        };
        target: {
            type: import("vue").PropType<TdAnchorItemProps["target"]>;
            default: TdAnchorItemProps["target"];
            validator(val: TdAnchorItemProps["target"]): boolean;
        };
        title: {
            type: import("vue").PropType<TdAnchorItemProps["title"]>;
            default: string;
        };
        customScroll: {
            type: BooleanConstructor;
            default: boolean;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        title: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        target: "_self" | "_blank" | "_parent" | "_top";
        customScroll: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    href: {
        type: StringConstructor;
        required: boolean;
        validator(v: string): boolean;
    };
    target: {
        type: import("vue").PropType<TdAnchorItemProps["target"]>;
        default: TdAnchorItemProps["target"];
        validator(val: TdAnchorItemProps["target"]): boolean;
    };
    title: {
        type: import("vue").PropType<TdAnchorItemProps["title"]>;
        default: string;
    };
    customScroll: {
        type: BooleanConstructor;
        default: boolean;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    title: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    target: "_self" | "_blank" | "_parent" | "_top";
    customScroll: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const AnchorTarget: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        id: {
            type: StringConstructor;
            default: string;
            required: boolean;
        };
        tag: {
            type: StringConstructor;
            default: string;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        tag: string;
        id: string;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        id: {
            type: StringConstructor;
            default: string;
            required: boolean;
        };
        tag: {
            type: StringConstructor;
            default: string;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        tag: string;
        id: string;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    id: {
        type: StringConstructor;
        default: string;
        required: boolean;
    };
    tag: {
        type: StringConstructor;
        default: string;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    tag: string;
    id: string;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Anchor;
