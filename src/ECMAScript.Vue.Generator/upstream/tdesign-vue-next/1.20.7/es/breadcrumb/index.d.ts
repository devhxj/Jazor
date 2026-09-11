import { TdBreadcrumbProps, TdBreadcrumbItemProps } from './type';
import './style';
export * from './type';
export type BreadcrumbProps = TdBreadcrumbProps;
export type BreadcrumbItemProps = TdBreadcrumbItemProps;
export declare const Breadcrumb: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        ellipsis?: string | import("..").TNode<{
            items: Array<TdBreadcrumbItemProps>;
            separator: TdBreadcrumbProps["separator"];
        }>;
        itemsAfterCollapse?: number;
        itemsBeforeCollapse?: number;
        maxItemWidth?: string;
        maxItems?: number;
        options?: Array<TdBreadcrumbItemProps>;
        separator?: string | import("..").TNode;
        theme?: "light";
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        theme: "light";
        maxItems: number;
        itemsBeforeCollapse: number;
        itemsAfterCollapse: number;
        maxItemWidth: string;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        ellipsis?: string | import("..").TNode<{
            items: Array<TdBreadcrumbItemProps>;
            separator: TdBreadcrumbProps["separator"];
        }>;
        itemsAfterCollapse?: number;
        itemsBeforeCollapse?: number;
        maxItemWidth?: string;
        maxItems?: number;
        options?: Array<TdBreadcrumbItemProps>;
        separator?: string | import("..").TNode;
        theme?: "light";
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        theme: "light";
        maxItems: number;
        itemsBeforeCollapse: number;
        itemsAfterCollapse: number;
        maxItemWidth: string;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    ellipsis?: string | import("..").TNode<{
        items: Array<TdBreadcrumbItemProps>;
        separator: TdBreadcrumbProps["separator"];
    }>;
    itemsAfterCollapse?: number;
    itemsBeforeCollapse?: number;
    maxItemWidth?: string;
    maxItems?: number;
    options?: Array<TdBreadcrumbItemProps>;
    separator?: string | import("..").TNode;
    theme?: "light";
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    theme: "light";
    maxItems: number;
    itemsBeforeCollapse: number;
    itemsAfterCollapse: number;
    maxItemWidth: string;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const BreadcrumbItem: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        isEllipsisItem: BooleanConstructor;
        content: {
            type: import("vue").PropType<TdBreadcrumbItemProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdBreadcrumbItemProps["default"]>;
        };
        disabled: BooleanConstructor;
        href: {
            type: StringConstructor;
            default: string;
        };
        icon: {
            type: import("vue").PropType<TdBreadcrumbItemProps["icon"]>;
        };
        maxWidth: {
            type: StringConstructor;
            default: any;
        };
        replace: BooleanConstructor;
        router: {
            type: import("vue").PropType<TdBreadcrumbItemProps["router"]>;
        };
        target: {
            type: import("vue").PropType<TdBreadcrumbItemProps["target"]>;
            default: TdBreadcrumbItemProps["target"];
            validator(val: TdBreadcrumbItemProps["target"]): boolean;
        };
        to: {
            type: import("vue").PropType<TdBreadcrumbItemProps["to"]>;
        };
        onClick: import("vue").PropType<TdBreadcrumbItemProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        replace: boolean;
        maxWidth: string;
        disabled: boolean;
        href: string;
        target: "_self" | "_blank" | "_parent" | "_top";
        isEllipsisItem: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        isEllipsisItem: BooleanConstructor;
        content: {
            type: import("vue").PropType<TdBreadcrumbItemProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdBreadcrumbItemProps["default"]>;
        };
        disabled: BooleanConstructor;
        href: {
            type: StringConstructor;
            default: string;
        };
        icon: {
            type: import("vue").PropType<TdBreadcrumbItemProps["icon"]>;
        };
        maxWidth: {
            type: StringConstructor;
            default: any;
        };
        replace: BooleanConstructor;
        router: {
            type: import("vue").PropType<TdBreadcrumbItemProps["router"]>;
        };
        target: {
            type: import("vue").PropType<TdBreadcrumbItemProps["target"]>;
            default: TdBreadcrumbItemProps["target"];
            validator(val: TdBreadcrumbItemProps["target"]): boolean;
        };
        to: {
            type: import("vue").PropType<TdBreadcrumbItemProps["to"]>;
        };
        onClick: import("vue").PropType<TdBreadcrumbItemProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        replace: boolean;
        maxWidth: string;
        disabled: boolean;
        href: string;
        target: "_self" | "_blank" | "_parent" | "_top";
        isEllipsisItem: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    isEllipsisItem: BooleanConstructor;
    content: {
        type: import("vue").PropType<TdBreadcrumbItemProps["content"]>;
    };
    default: {
        type: import("vue").PropType<TdBreadcrumbItemProps["default"]>;
    };
    disabled: BooleanConstructor;
    href: {
        type: StringConstructor;
        default: string;
    };
    icon: {
        type: import("vue").PropType<TdBreadcrumbItemProps["icon"]>;
    };
    maxWidth: {
        type: StringConstructor;
        default: any;
    };
    replace: BooleanConstructor;
    router: {
        type: import("vue").PropType<TdBreadcrumbItemProps["router"]>;
    };
    target: {
        type: import("vue").PropType<TdBreadcrumbItemProps["target"]>;
        default: TdBreadcrumbItemProps["target"];
        validator(val: TdBreadcrumbItemProps["target"]): boolean;
    };
    to: {
        type: import("vue").PropType<TdBreadcrumbItemProps["to"]>;
    };
    onClick: import("vue").PropType<TdBreadcrumbItemProps["onClick"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    replace: boolean;
    maxWidth: string;
    disabled: boolean;
    href: string;
    target: "_self" | "_blank" | "_parent" | "_top";
    isEllipsisItem: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Breadcrumb;
