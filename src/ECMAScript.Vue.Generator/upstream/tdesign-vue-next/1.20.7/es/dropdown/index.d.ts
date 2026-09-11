import { TdDropdownProps, TdDropdownItemProps } from './type';
import './style';
export * from './type';
export type DropdownProps = TdDropdownProps;
export type DropdownItemProps = TdDropdownItemProps;
export type DropdownMenuProps = TdDropdownProps;
export declare const Dropdown: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        direction?: "left" | "right";
        disabled?: boolean;
        hideAfterItemClick?: boolean;
        maxColumnWidth?: string | number;
        maxHeight?: number;
        minColumnWidth?: string | number;
        options?: Array<import("./type").DropdownOption>;
        panelBottomContent?: string | import("..").TNode;
        panelTopContent?: string | import("..").TNode;
        placement?: "top" | "left" | "right" | "bottom" | "top-left" | "top-right" | "bottom-left" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
        popupProps?: import("..").PopupProps;
        trigger?: "hover" | "click" | "focus" | "context-menu";
        onClick?: (dropdownItem: TdDropdownItemProps["value"], context: {
            e: MouseEvent;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        maxHeight: number;
        direction: "left" | "right";
        disabled: boolean;
        options: import("./type").DropdownOption[];
        placement: "left" | "right" | "top" | "bottom" | "top-left" | "bottom-left" | "top-right" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
        trigger: "click" | "focus" | "hover" | "context-menu";
        maxColumnWidth: string | number;
        minColumnWidth: string | number;
        hideAfterItemClick: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        direction?: "left" | "right";
        disabled?: boolean;
        hideAfterItemClick?: boolean;
        maxColumnWidth?: string | number;
        maxHeight?: number;
        minColumnWidth?: string | number;
        options?: Array<import("./type").DropdownOption>;
        panelBottomContent?: string | import("..").TNode;
        panelTopContent?: string | import("..").TNode;
        placement?: "top" | "left" | "right" | "bottom" | "top-left" | "top-right" | "bottom-left" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
        popupProps?: import("..").PopupProps;
        trigger?: "hover" | "click" | "focus" | "context-menu";
        onClick?: (dropdownItem: TdDropdownItemProps["value"], context: {
            e: MouseEvent;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        maxHeight: number;
        direction: "left" | "right";
        disabled: boolean;
        options: import("./type").DropdownOption[];
        placement: "left" | "right" | "top" | "bottom" | "top-left" | "bottom-left" | "top-right" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
        trigger: "click" | "focus" | "hover" | "context-menu";
        maxColumnWidth: string | number;
        minColumnWidth: string | number;
        hideAfterItemClick: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    direction?: "left" | "right";
    disabled?: boolean;
    hideAfterItemClick?: boolean;
    maxColumnWidth?: string | number;
    maxHeight?: number;
    minColumnWidth?: string | number;
    options?: Array<import("./type").DropdownOption>;
    panelBottomContent?: string | import("..").TNode;
    panelTopContent?: string | import("..").TNode;
    placement?: "top" | "left" | "right" | "bottom" | "top-left" | "top-right" | "bottom-left" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
    popupProps?: import("..").PopupProps;
    trigger?: "hover" | "click" | "focus" | "context-menu";
    onClick?: (dropdownItem: TdDropdownItemProps["value"], context: {
        e: MouseEvent;
    }) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    maxHeight: number;
    direction: "left" | "right";
    disabled: boolean;
    options: import("./type").DropdownOption[];
    placement: "left" | "right" | "top" | "bottom" | "top-left" | "bottom-left" | "top-right" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
    trigger: "click" | "focus" | "hover" | "context-menu";
    maxColumnWidth: string | number;
    minColumnWidth: string | number;
    hideAfterItemClick: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const DropdownItem: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        maxColumnWidth: {
            type: import("vue").PropType<TdDropdownProps["maxColumnWidth"]>;
            default: number;
        };
        minColumnWidth: {
            type: import("vue").PropType<TdDropdownProps["minColumnWidth"]>;
            default: number;
        };
        isSubmenu: BooleanConstructor;
        active: BooleanConstructor;
        content: {
            type: import("vue").PropType<TdDropdownItemProps["content"]>;
            default: string;
        };
        disabled: BooleanConstructor;
        divider: BooleanConstructor;
        prefixIcon: {
            type: import("vue").PropType<TdDropdownItemProps["prefixIcon"]>;
        };
        theme: {
            type: import("vue").PropType<TdDropdownItemProps["theme"]>;
            default: TdDropdownItemProps["theme"];
            validator(val: TdDropdownItemProps["theme"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdDropdownItemProps["value"]>;
        };
        onClick: import("vue").PropType<TdDropdownItemProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        divider: boolean;
        active: boolean;
        disabled: boolean;
        theme: import("./type").DropdownItemTheme;
        content: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        maxColumnWidth: string | number;
        minColumnWidth: string | number;
        isSubmenu: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        maxColumnWidth: {
            type: import("vue").PropType<TdDropdownProps["maxColumnWidth"]>;
            default: number;
        };
        minColumnWidth: {
            type: import("vue").PropType<TdDropdownProps["minColumnWidth"]>;
            default: number;
        };
        isSubmenu: BooleanConstructor;
        active: BooleanConstructor;
        content: {
            type: import("vue").PropType<TdDropdownItemProps["content"]>;
            default: string;
        };
        disabled: BooleanConstructor;
        divider: BooleanConstructor;
        prefixIcon: {
            type: import("vue").PropType<TdDropdownItemProps["prefixIcon"]>;
        };
        theme: {
            type: import("vue").PropType<TdDropdownItemProps["theme"]>;
            default: TdDropdownItemProps["theme"];
            validator(val: TdDropdownItemProps["theme"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdDropdownItemProps["value"]>;
        };
        onClick: import("vue").PropType<TdDropdownItemProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        divider: boolean;
        active: boolean;
        disabled: boolean;
        theme: import("./type").DropdownItemTheme;
        content: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        maxColumnWidth: string | number;
        minColumnWidth: string | number;
        isSubmenu: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    maxColumnWidth: {
        type: import("vue").PropType<TdDropdownProps["maxColumnWidth"]>;
        default: number;
    };
    minColumnWidth: {
        type: import("vue").PropType<TdDropdownProps["minColumnWidth"]>;
        default: number;
    };
    isSubmenu: BooleanConstructor;
    active: BooleanConstructor;
    content: {
        type: import("vue").PropType<TdDropdownItemProps["content"]>;
        default: string;
    };
    disabled: BooleanConstructor;
    divider: BooleanConstructor;
    prefixIcon: {
        type: import("vue").PropType<TdDropdownItemProps["prefixIcon"]>;
    };
    theme: {
        type: import("vue").PropType<TdDropdownItemProps["theme"]>;
        default: TdDropdownItemProps["theme"];
        validator(val: TdDropdownItemProps["theme"]): boolean;
    };
    value: {
        type: import("vue").PropType<TdDropdownItemProps["value"]>;
    };
    onClick: import("vue").PropType<TdDropdownItemProps["onClick"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    divider: boolean;
    active: boolean;
    disabled: boolean;
    theme: import("./type").DropdownItemTheme;
    content: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    maxColumnWidth: string | number;
    minColumnWidth: string | number;
    isSubmenu: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const DropdownMenu: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        direction: {
            type: import("vue").PropType<TdDropdownProps["direction"]>;
            default: TdDropdownProps["direction"];
            validator(val: TdDropdownProps["direction"]): boolean;
        };
        disabled: BooleanConstructor;
        hideAfterItemClick: {
            type: BooleanConstructor;
            default: boolean;
        };
        maxColumnWidth: {
            type: import("vue").PropType<TdDropdownProps["maxColumnWidth"]>;
            default: TdDropdownProps["maxColumnWidth"];
        };
        maxHeight: {
            type: NumberConstructor;
            default: number;
        };
        minColumnWidth: {
            type: import("vue").PropType<TdDropdownProps["minColumnWidth"]>;
            default: TdDropdownProps["minColumnWidth"];
        };
        options: {
            type: import("vue").PropType<TdDropdownProps["options"]>;
            default: () => TdDropdownProps["options"];
        };
        panelBottomContent: {
            type: import("vue").PropType<TdDropdownProps["panelBottomContent"]>;
        };
        panelTopContent: {
            type: import("vue").PropType<TdDropdownProps["panelTopContent"]>;
        };
        placement: {
            type: import("vue").PropType<TdDropdownProps["placement"]>;
            default: TdDropdownProps["placement"];
            validator(val: TdDropdownProps["placement"]): boolean;
        };
        popupProps: {
            type: import("vue").PropType<TdDropdownProps["popupProps"]>;
        };
        trigger: {
            type: import("vue").PropType<TdDropdownProps["trigger"]>;
            default: TdDropdownProps["trigger"];
            validator(val: TdDropdownProps["trigger"]): boolean;
        };
        onClick: import("vue").PropType<TdDropdownProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        maxHeight: number;
        direction: "left" | "right";
        disabled: boolean;
        options: import("./type").DropdownOption[];
        placement: "left" | "right" | "top" | "bottom" | "top-left" | "bottom-left" | "top-right" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
        trigger: "click" | "focus" | "hover" | "context-menu";
        maxColumnWidth: string | number;
        minColumnWidth: string | number;
        hideAfterItemClick: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        direction: {
            type: import("vue").PropType<TdDropdownProps["direction"]>;
            default: TdDropdownProps["direction"];
            validator(val: TdDropdownProps["direction"]): boolean;
        };
        disabled: BooleanConstructor;
        hideAfterItemClick: {
            type: BooleanConstructor;
            default: boolean;
        };
        maxColumnWidth: {
            type: import("vue").PropType<TdDropdownProps["maxColumnWidth"]>;
            default: TdDropdownProps["maxColumnWidth"];
        };
        maxHeight: {
            type: NumberConstructor;
            default: number;
        };
        minColumnWidth: {
            type: import("vue").PropType<TdDropdownProps["minColumnWidth"]>;
            default: TdDropdownProps["minColumnWidth"];
        };
        options: {
            type: import("vue").PropType<TdDropdownProps["options"]>;
            default: () => TdDropdownProps["options"];
        };
        panelBottomContent: {
            type: import("vue").PropType<TdDropdownProps["panelBottomContent"]>;
        };
        panelTopContent: {
            type: import("vue").PropType<TdDropdownProps["panelTopContent"]>;
        };
        placement: {
            type: import("vue").PropType<TdDropdownProps["placement"]>;
            default: TdDropdownProps["placement"];
            validator(val: TdDropdownProps["placement"]): boolean;
        };
        popupProps: {
            type: import("vue").PropType<TdDropdownProps["popupProps"]>;
        };
        trigger: {
            type: import("vue").PropType<TdDropdownProps["trigger"]>;
            default: TdDropdownProps["trigger"];
            validator(val: TdDropdownProps["trigger"]): boolean;
        };
        onClick: import("vue").PropType<TdDropdownProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        maxHeight: number;
        direction: "left" | "right";
        disabled: boolean;
        options: import("./type").DropdownOption[];
        placement: "left" | "right" | "top" | "bottom" | "top-left" | "bottom-left" | "top-right" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
        trigger: "click" | "focus" | "hover" | "context-menu";
        maxColumnWidth: string | number;
        minColumnWidth: string | number;
        hideAfterItemClick: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    direction: {
        type: import("vue").PropType<TdDropdownProps["direction"]>;
        default: TdDropdownProps["direction"];
        validator(val: TdDropdownProps["direction"]): boolean;
    };
    disabled: BooleanConstructor;
    hideAfterItemClick: {
        type: BooleanConstructor;
        default: boolean;
    };
    maxColumnWidth: {
        type: import("vue").PropType<TdDropdownProps["maxColumnWidth"]>;
        default: TdDropdownProps["maxColumnWidth"];
    };
    maxHeight: {
        type: NumberConstructor;
        default: number;
    };
    minColumnWidth: {
        type: import("vue").PropType<TdDropdownProps["minColumnWidth"]>;
        default: TdDropdownProps["minColumnWidth"];
    };
    options: {
        type: import("vue").PropType<TdDropdownProps["options"]>;
        default: () => TdDropdownProps["options"];
    };
    panelBottomContent: {
        type: import("vue").PropType<TdDropdownProps["panelBottomContent"]>;
    };
    panelTopContent: {
        type: import("vue").PropType<TdDropdownProps["panelTopContent"]>;
    };
    placement: {
        type: import("vue").PropType<TdDropdownProps["placement"]>;
        default: TdDropdownProps["placement"];
        validator(val: TdDropdownProps["placement"]): boolean;
    };
    popupProps: {
        type: import("vue").PropType<TdDropdownProps["popupProps"]>;
    };
    trigger: {
        type: import("vue").PropType<TdDropdownProps["trigger"]>;
        default: TdDropdownProps["trigger"];
        validator(val: TdDropdownProps["trigger"]): boolean;
    };
    onClick: import("vue").PropType<TdDropdownProps["onClick"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    maxHeight: number;
    direction: "left" | "right";
    disabled: boolean;
    options: import("./type").DropdownOption[];
    placement: "left" | "right" | "top" | "bottom" | "top-left" | "bottom-left" | "top-right" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
    trigger: "click" | "focus" | "hover" | "context-menu";
    maxColumnWidth: string | number;
    minColumnWidth: string | number;
    hideAfterItemClick: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Dropdown;
