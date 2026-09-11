import { TdMenuProps, TdHeadMenuProps, TdSubmenuProps, TdMenuItemProps } from './type';
import './style';
export * from './type';
export type MenuProps = TdMenuProps;
export type HeadMenuProps = TdHeadMenuProps;
export type SubmenuProps = TdSubmenuProps;
export type MenuItemProps = TdMenuItemProps;
export declare const Menu: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        onCollapsed: FunctionConstructor;
        collapsed: BooleanConstructor;
        expanded: {
            type: import("vue").PropType<TdMenuProps["expanded"]>;
            default: any;
        };
        defaultExpanded: {
            type: import("vue").PropType<TdMenuProps["defaultExpanded"]>;
            default: any[];
        };
        expandMutex: BooleanConstructor;
        expandType: {
            type: import("vue").PropType<TdMenuProps["expandType"]>;
            default: TdMenuProps["expandType"];
            validator(val: TdMenuProps["expandType"]): boolean;
        };
        logo: {
            type: import("vue").PropType<TdMenuProps["logo"]>;
        };
        operations: {
            type: import("vue").PropType<TdMenuProps["operations"]>;
        };
        theme: {
            type: import("vue").PropType<TdMenuProps["theme"]>;
            default: TdMenuProps["theme"];
            validator(val: TdMenuProps["theme"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdMenuProps["value"]>;
            default: TdMenuProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdMenuProps["value"]>;
            default: TdMenuProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdMenuProps["defaultValue"]>;
        };
        width: {
            type: import("vue").PropType<TdMenuProps["width"]>;
            default: string;
        };
        onChange: import("vue").PropType<TdMenuProps["onChange"]>;
        onExpand: import("vue").PropType<TdMenuProps["onExpand"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").MenuValue;
        width: string | number | (string | number)[];
        expanded: import("./type").MenuValue[];
        expandMutex: boolean;
        theme: "dark" | "light";
        modelValue: import("./type").MenuValue;
        defaultExpanded: import("./type").MenuValue[];
        expandType: "popup" | "normal";
        collapsed: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        onCollapsed: FunctionConstructor;
        collapsed: BooleanConstructor;
        expanded: {
            type: import("vue").PropType<TdMenuProps["expanded"]>;
            default: any;
        };
        defaultExpanded: {
            type: import("vue").PropType<TdMenuProps["defaultExpanded"]>;
            default: any[];
        };
        expandMutex: BooleanConstructor;
        expandType: {
            type: import("vue").PropType<TdMenuProps["expandType"]>;
            default: TdMenuProps["expandType"];
            validator(val: TdMenuProps["expandType"]): boolean;
        };
        logo: {
            type: import("vue").PropType<TdMenuProps["logo"]>;
        };
        operations: {
            type: import("vue").PropType<TdMenuProps["operations"]>;
        };
        theme: {
            type: import("vue").PropType<TdMenuProps["theme"]>;
            default: TdMenuProps["theme"];
            validator(val: TdMenuProps["theme"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdMenuProps["value"]>;
            default: TdMenuProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdMenuProps["value"]>;
            default: TdMenuProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdMenuProps["defaultValue"]>;
        };
        width: {
            type: import("vue").PropType<TdMenuProps["width"]>;
            default: string;
        };
        onChange: import("vue").PropType<TdMenuProps["onChange"]>;
        onExpand: import("vue").PropType<TdMenuProps["onExpand"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").MenuValue;
        width: string | number | (string | number)[];
        expanded: import("./type").MenuValue[];
        expandMutex: boolean;
        theme: "dark" | "light";
        modelValue: import("./type").MenuValue;
        defaultExpanded: import("./type").MenuValue[];
        expandType: "popup" | "normal";
        collapsed: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    onCollapsed: FunctionConstructor;
    collapsed: BooleanConstructor;
    expanded: {
        type: import("vue").PropType<TdMenuProps["expanded"]>;
        default: any;
    };
    defaultExpanded: {
        type: import("vue").PropType<TdMenuProps["defaultExpanded"]>;
        default: any[];
    };
    expandMutex: BooleanConstructor;
    expandType: {
        type: import("vue").PropType<TdMenuProps["expandType"]>;
        default: TdMenuProps["expandType"];
        validator(val: TdMenuProps["expandType"]): boolean;
    };
    logo: {
        type: import("vue").PropType<TdMenuProps["logo"]>;
    };
    operations: {
        type: import("vue").PropType<TdMenuProps["operations"]>;
    };
    theme: {
        type: import("vue").PropType<TdMenuProps["theme"]>;
        default: TdMenuProps["theme"];
        validator(val: TdMenuProps["theme"]): boolean;
    };
    value: {
        type: import("vue").PropType<TdMenuProps["value"]>;
        default: TdMenuProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdMenuProps["value"]>;
        default: TdMenuProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdMenuProps["defaultValue"]>;
    };
    width: {
        type: import("vue").PropType<TdMenuProps["width"]>;
        default: string;
    };
    onChange: import("vue").PropType<TdMenuProps["onChange"]>;
    onExpand: import("vue").PropType<TdMenuProps["onExpand"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").MenuValue;
    width: string | number | (string | number)[];
    expanded: import("./type").MenuValue[];
    expandMutex: boolean;
    theme: "dark" | "light";
    modelValue: import("./type").MenuValue;
    defaultExpanded: import("./type").MenuValue[];
    expandType: "popup" | "normal";
    collapsed: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const HeadMenu: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        expanded: {
            type: import("vue").PropType<TdHeadMenuProps["expanded"]>;
            default: any;
        };
        defaultExpanded: {
            type: import("vue").PropType<TdHeadMenuProps["defaultExpanded"]>;
            default: any[];
        };
        expandType: {
            type: import("vue").PropType<TdHeadMenuProps["expandType"]>;
            default: TdHeadMenuProps["expandType"];
            validator(val: TdHeadMenuProps["expandType"]): boolean;
        };
        logo: {
            type: import("vue").PropType<TdHeadMenuProps["logo"]>;
        };
        operations: {
            type: import("vue").PropType<TdHeadMenuProps["operations"]>;
        };
        theme: {
            type: import("vue").PropType<TdHeadMenuProps["theme"]>;
            default: TdHeadMenuProps["theme"];
            validator(val: TdHeadMenuProps["theme"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdHeadMenuProps["value"]>;
            default: TdHeadMenuProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdHeadMenuProps["value"]>;
            default: TdHeadMenuProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdHeadMenuProps["defaultValue"]>;
        };
        onChange: import("vue").PropType<TdHeadMenuProps["onChange"]>;
        onExpand: import("vue").PropType<TdHeadMenuProps["onExpand"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").MenuValue;
        expanded: import("./type").MenuValue[];
        theme: "dark" | "light";
        modelValue: import("./type").MenuValue;
        defaultExpanded: import("./type").MenuValue[];
        expandType: "popup" | "normal";
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        expanded: {
            type: import("vue").PropType<TdHeadMenuProps["expanded"]>;
            default: any;
        };
        defaultExpanded: {
            type: import("vue").PropType<TdHeadMenuProps["defaultExpanded"]>;
            default: any[];
        };
        expandType: {
            type: import("vue").PropType<TdHeadMenuProps["expandType"]>;
            default: TdHeadMenuProps["expandType"];
            validator(val: TdHeadMenuProps["expandType"]): boolean;
        };
        logo: {
            type: import("vue").PropType<TdHeadMenuProps["logo"]>;
        };
        operations: {
            type: import("vue").PropType<TdHeadMenuProps["operations"]>;
        };
        theme: {
            type: import("vue").PropType<TdHeadMenuProps["theme"]>;
            default: TdHeadMenuProps["theme"];
            validator(val: TdHeadMenuProps["theme"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdHeadMenuProps["value"]>;
            default: TdHeadMenuProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdHeadMenuProps["value"]>;
            default: TdHeadMenuProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdHeadMenuProps["defaultValue"]>;
        };
        onChange: import("vue").PropType<TdHeadMenuProps["onChange"]>;
        onExpand: import("vue").PropType<TdHeadMenuProps["onExpand"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").MenuValue;
        expanded: import("./type").MenuValue[];
        theme: "dark" | "light";
        modelValue: import("./type").MenuValue;
        defaultExpanded: import("./type").MenuValue[];
        expandType: "popup" | "normal";
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    expanded: {
        type: import("vue").PropType<TdHeadMenuProps["expanded"]>;
        default: any;
    };
    defaultExpanded: {
        type: import("vue").PropType<TdHeadMenuProps["defaultExpanded"]>;
        default: any[];
    };
    expandType: {
        type: import("vue").PropType<TdHeadMenuProps["expandType"]>;
        default: TdHeadMenuProps["expandType"];
        validator(val: TdHeadMenuProps["expandType"]): boolean;
    };
    logo: {
        type: import("vue").PropType<TdHeadMenuProps["logo"]>;
    };
    operations: {
        type: import("vue").PropType<TdHeadMenuProps["operations"]>;
    };
    theme: {
        type: import("vue").PropType<TdHeadMenuProps["theme"]>;
        default: TdHeadMenuProps["theme"];
        validator(val: TdHeadMenuProps["theme"]): boolean;
    };
    value: {
        type: import("vue").PropType<TdHeadMenuProps["value"]>;
        default: TdHeadMenuProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdHeadMenuProps["value"]>;
        default: TdHeadMenuProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdHeadMenuProps["defaultValue"]>;
    };
    onChange: import("vue").PropType<TdHeadMenuProps["onChange"]>;
    onExpand: import("vue").PropType<TdHeadMenuProps["onExpand"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").MenuValue;
    expanded: import("./type").MenuValue[];
    theme: "dark" | "light";
    modelValue: import("./type").MenuValue;
    defaultExpanded: import("./type").MenuValue[];
    expandType: "popup" | "normal";
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const Submenu: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        content?: string | import("..").TNode;
        default?: string | import("..").TNode;
        disabled?: boolean;
        icon?: import("..").TNode;
        popupProps?: import("..").PopupProps;
        title?: string | import("..").TNode;
        value?: import("./type").MenuValue;
        expandType?: string;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        disabled: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        content?: string | import("..").TNode;
        default?: string | import("..").TNode;
        disabled?: boolean;
        icon?: import("..").TNode;
        popupProps?: import("..").PopupProps;
        title?: string | import("..").TNode;
        value?: import("./type").MenuValue;
        expandType?: string;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        disabled: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    content?: string | import("..").TNode;
    default?: string | import("..").TNode;
    disabled?: boolean;
    icon?: import("..").TNode;
    popupProps?: import("..").PopupProps;
    title?: string | import("..").TNode;
    value?: import("./type").MenuValue;
    expandType?: string;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    disabled: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const MenuItem: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        content?: string | import("..").TNode;
        default?: string | import("..").TNode;
        disabled?: boolean;
        href?: string;
        icon?: import("..").TNode;
        replace?: boolean;
        router?: Record<string, any>;
        routerLink?: boolean;
        target?: "_blank" | "_self" | "_parent" | "_top";
        to?: string | import("./type").MenuRoute;
        value?: import("./type").MenuValue;
        onClick?: (context: {
            e: MouseEvent;
            value: import("./type").MenuValue;
        }) => void;
    }> & Readonly<{
        onClick?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "click"[], import("vue").PublicProps, {
        replace: boolean;
        disabled: boolean;
        href: string;
        target: "_self" | "_blank" | "_parent" | "_top";
        routerLink: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        content?: string | import("..").TNode;
        default?: string | import("..").TNode;
        disabled?: boolean;
        href?: string;
        icon?: import("..").TNode;
        replace?: boolean;
        router?: Record<string, any>;
        routerLink?: boolean;
        target?: "_blank" | "_self" | "_parent" | "_top";
        to?: string | import("./type").MenuRoute;
        value?: import("./type").MenuValue;
        onClick?: (context: {
            e: MouseEvent;
            value: import("./type").MenuValue;
        }) => void;
    }> & Readonly<{
        onClick?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        replace: boolean;
        disabled: boolean;
        href: string;
        target: "_self" | "_blank" | "_parent" | "_top";
        routerLink: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    content?: string | import("..").TNode;
    default?: string | import("..").TNode;
    disabled?: boolean;
    href?: string;
    icon?: import("..").TNode;
    replace?: boolean;
    router?: Record<string, any>;
    routerLink?: boolean;
    target?: "_blank" | "_self" | "_parent" | "_top";
    to?: string | import("./type").MenuRoute;
    value?: import("./type").MenuValue;
    onClick?: (context: {
        e: MouseEvent;
        value: import("./type").MenuValue;
    }) => void;
}> & Readonly<{
    onClick?: (...args: any[]) => any;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "click"[], "click", {
    replace: boolean;
    disabled: boolean;
    href: string;
    target: "_self" | "_blank" | "_parent" | "_top";
    routerLink: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const MenuGroup: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        title: {
            type: import("vue").PropType<import("./type").TdMenuGroupProps["title"]>;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {}, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        title: {
            type: import("vue").PropType<import("./type").TdMenuGroupProps["title"]>;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {}>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    title: {
        type: import("vue").PropType<import("./type").TdMenuGroupProps["title"]>;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
