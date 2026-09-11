import { TdListProps, TdListItemProps, TdListItemMetaProps } from './type';
import './style';
export * from './type';
export type ListProps = TdListProps;
export type ListItemProps = TdListItemProps;
export type ListItemMetaProps = TdListItemMetaProps;
export declare const List: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        asyncLoading?: string | import("..").TNode;
        footer?: string | import("..").TNode;
        header?: string | import("..").TNode;
        layout?: "horizontal" | "vertical";
        scroll?: import("..").TScroll;
        size?: "small" | "medium" | "large";
        split?: boolean;
        stripe?: boolean;
        onLoadMore?: (options: {
            e: MouseEvent;
        }) => void;
        onScroll?: (options: {
            e: Event | WheelEvent;
            scrollTop: number;
            scrollBottom: number;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        layout: "vertical" | "horizontal";
        split: boolean;
        size: "small" | "medium" | "large";
        stripe: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        asyncLoading?: string | import("..").TNode;
        footer?: string | import("..").TNode;
        header?: string | import("..").TNode;
        layout?: "horizontal" | "vertical";
        scroll?: import("..").TScroll;
        size?: "small" | "medium" | "large";
        split?: boolean;
        stripe?: boolean;
        onLoadMore?: (options: {
            e: MouseEvent;
        }) => void;
        onScroll?: (options: {
            e: Event | WheelEvent;
            scrollTop: number;
            scrollBottom: number;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        layout: "vertical" | "horizontal";
        split: boolean;
        size: "small" | "medium" | "large";
        stripe: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    asyncLoading?: string | import("..").TNode;
    footer?: string | import("..").TNode;
    header?: string | import("..").TNode;
    layout?: "horizontal" | "vertical";
    scroll?: import("..").TScroll;
    size?: "small" | "medium" | "large";
    split?: boolean;
    stripe?: boolean;
    onLoadMore?: (options: {
        e: MouseEvent;
    }) => void;
    onScroll?: (options: {
        e: Event | WheelEvent;
        scrollTop: number;
        scrollBottom: number;
    }) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    layout: "vertical" | "horizontal";
    split: boolean;
    size: "small" | "medium" | "large";
    stripe: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const ListItem: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        action: {
            type: import("vue").PropType<TdListItemProps["action"]>;
        };
        content: {
            type: import("vue").PropType<TdListItemProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdListItemProps["default"]>;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {}, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        action: {
            type: import("vue").PropType<TdListItemProps["action"]>;
        };
        content: {
            type: import("vue").PropType<TdListItemProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdListItemProps["default"]>;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {}>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    action: {
        type: import("vue").PropType<TdListItemProps["action"]>;
    };
    content: {
        type: import("vue").PropType<TdListItemProps["content"]>;
    };
    default: {
        type: import("vue").PropType<TdListItemProps["default"]>;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const ListItemMeta: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        avatar: {
            type: import("vue").PropType<TdListItemMetaProps["avatar"]>;
        };
        description: {
            type: import("vue").PropType<TdListItemMetaProps["description"]>;
        };
        image: {
            type: import("vue").PropType<TdListItemMetaProps["image"]>;
        };
        title: {
            type: import("vue").PropType<TdListItemMetaProps["title"]>;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {}, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        avatar: {
            type: import("vue").PropType<TdListItemMetaProps["avatar"]>;
        };
        description: {
            type: import("vue").PropType<TdListItemMetaProps["description"]>;
        };
        image: {
            type: import("vue").PropType<TdListItemMetaProps["image"]>;
        };
        title: {
            type: import("vue").PropType<TdListItemMetaProps["title"]>;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {}>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    avatar: {
        type: import("vue").PropType<TdListItemMetaProps["avatar"]>;
    };
    description: {
        type: import("vue").PropType<TdListItemMetaProps["description"]>;
    };
    image: {
        type: import("vue").PropType<TdListItemMetaProps["image"]>;
    };
    title: {
        type: import("vue").PropType<TdListItemMetaProps["title"]>;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
