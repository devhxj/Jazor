import { TdDrawerProps } from './type';
import './style';
export * from './type';
export type DrawerProps = TdDrawerProps;
export declare const Drawer: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        attach: {
            type: import("vue").PropType<TdDrawerProps["attach"]>;
        };
        body: {
            type: import("vue").PropType<TdDrawerProps["body"]>;
        };
        cancelBtn: {
            type: import("vue").PropType<TdDrawerProps["cancelBtn"]>;
        };
        closeBtn: {
            type: import("vue").PropType<TdDrawerProps["closeBtn"]>;
        };
        closeOnEscKeydown: {
            type: BooleanConstructor;
            default: any;
        };
        closeOnOverlayClick: {
            type: BooleanConstructor;
            default: any;
        };
        confirmBtn: {
            type: import("vue").PropType<TdDrawerProps["confirmBtn"]>;
        };
        default: {
            type: import("vue").PropType<TdDrawerProps["default"]>;
        };
        destroyOnClose: BooleanConstructor;
        drawerClassName: {
            type: StringConstructor;
            default: string;
        };
        footer: {
            type: import("vue").PropType<TdDrawerProps["footer"]>;
            default: TdDrawerProps["footer"];
        };
        header: {
            type: import("vue").PropType<TdDrawerProps["header"]>;
            default: TdDrawerProps["header"];
        };
        lazy: BooleanConstructor;
        mode: {
            type: import("vue").PropType<TdDrawerProps["mode"]>;
            default: TdDrawerProps["mode"];
            validator(val: TdDrawerProps["mode"]): boolean;
        };
        placement: {
            type: import("vue").PropType<TdDrawerProps["placement"]>;
            default: TdDrawerProps["placement"];
            validator(val: TdDrawerProps["placement"]): boolean;
        };
        preventScrollThrough: {
            type: BooleanConstructor;
            default: boolean;
        };
        showInAttachedElement: BooleanConstructor;
        showOverlay: {
            type: BooleanConstructor;
            default: boolean;
        };
        size: {
            type: StringConstructor;
            default: any;
        };
        sizeDraggable: {
            type: import("vue").PropType<TdDrawerProps["sizeDraggable"]>;
            default: TdDrawerProps["sizeDraggable"];
        };
        visible: BooleanConstructor;
        zIndex: {
            type: NumberConstructor;
        };
        onBeforeClose: import("vue").PropType<TdDrawerProps["onBeforeClose"]>;
        onBeforeOpen: import("vue").PropType<TdDrawerProps["onBeforeOpen"]>;
        onCancel: import("vue").PropType<TdDrawerProps["onCancel"]>;
        onClose: import("vue").PropType<TdDrawerProps["onClose"]>;
        onCloseBtnClick: import("vue").PropType<TdDrawerProps["onCloseBtnClick"]>;
        onConfirm: import("vue").PropType<TdDrawerProps["onConfirm"]>;
        onEscKeydown: import("vue").PropType<TdDrawerProps["onEscKeydown"]>;
        onOverlayClick: import("vue").PropType<TdDrawerProps["onOverlayClick"]>;
        onSizeDragEnd: import("vue").PropType<TdDrawerProps["onSizeDragEnd"]>;
    }>> & Readonly<{
        "onUpdate:visible"?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "update:visible"[], import("vue").PublicProps, {
        footer: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        mode: "overlay" | "push";
        size: string;
        header: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        visible: boolean;
        lazy: boolean;
        placement: "left" | "right" | "top" | "bottom";
        destroyOnClose: boolean;
        preventScrollThrough: boolean;
        showOverlay: boolean;
        closeOnEscKeydown: boolean;
        closeOnOverlayClick: boolean;
        showInAttachedElement: boolean;
        drawerClassName: string;
        sizeDraggable: boolean | import("./type").SizeDragLimit;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        attach: {
            type: import("vue").PropType<TdDrawerProps["attach"]>;
        };
        body: {
            type: import("vue").PropType<TdDrawerProps["body"]>;
        };
        cancelBtn: {
            type: import("vue").PropType<TdDrawerProps["cancelBtn"]>;
        };
        closeBtn: {
            type: import("vue").PropType<TdDrawerProps["closeBtn"]>;
        };
        closeOnEscKeydown: {
            type: BooleanConstructor;
            default: any;
        };
        closeOnOverlayClick: {
            type: BooleanConstructor;
            default: any;
        };
        confirmBtn: {
            type: import("vue").PropType<TdDrawerProps["confirmBtn"]>;
        };
        default: {
            type: import("vue").PropType<TdDrawerProps["default"]>;
        };
        destroyOnClose: BooleanConstructor;
        drawerClassName: {
            type: StringConstructor;
            default: string;
        };
        footer: {
            type: import("vue").PropType<TdDrawerProps["footer"]>;
            default: TdDrawerProps["footer"];
        };
        header: {
            type: import("vue").PropType<TdDrawerProps["header"]>;
            default: TdDrawerProps["header"];
        };
        lazy: BooleanConstructor;
        mode: {
            type: import("vue").PropType<TdDrawerProps["mode"]>;
            default: TdDrawerProps["mode"];
            validator(val: TdDrawerProps["mode"]): boolean;
        };
        placement: {
            type: import("vue").PropType<TdDrawerProps["placement"]>;
            default: TdDrawerProps["placement"];
            validator(val: TdDrawerProps["placement"]): boolean;
        };
        preventScrollThrough: {
            type: BooleanConstructor;
            default: boolean;
        };
        showInAttachedElement: BooleanConstructor;
        showOverlay: {
            type: BooleanConstructor;
            default: boolean;
        };
        size: {
            type: StringConstructor;
            default: any;
        };
        sizeDraggable: {
            type: import("vue").PropType<TdDrawerProps["sizeDraggable"]>;
            default: TdDrawerProps["sizeDraggable"];
        };
        visible: BooleanConstructor;
        zIndex: {
            type: NumberConstructor;
        };
        onBeforeClose: import("vue").PropType<TdDrawerProps["onBeforeClose"]>;
        onBeforeOpen: import("vue").PropType<TdDrawerProps["onBeforeOpen"]>;
        onCancel: import("vue").PropType<TdDrawerProps["onCancel"]>;
        onClose: import("vue").PropType<TdDrawerProps["onClose"]>;
        onCloseBtnClick: import("vue").PropType<TdDrawerProps["onCloseBtnClick"]>;
        onConfirm: import("vue").PropType<TdDrawerProps["onConfirm"]>;
        onEscKeydown: import("vue").PropType<TdDrawerProps["onEscKeydown"]>;
        onOverlayClick: import("vue").PropType<TdDrawerProps["onOverlayClick"]>;
        onSizeDragEnd: import("vue").PropType<TdDrawerProps["onSizeDragEnd"]>;
    }>> & Readonly<{
        "onUpdate:visible"?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        footer: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        mode: "overlay" | "push";
        size: string;
        header: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        visible: boolean;
        lazy: boolean;
        placement: "left" | "right" | "top" | "bottom";
        destroyOnClose: boolean;
        preventScrollThrough: boolean;
        showOverlay: boolean;
        closeOnEscKeydown: boolean;
        closeOnOverlayClick: boolean;
        showInAttachedElement: boolean;
        drawerClassName: string;
        sizeDraggable: boolean | import("./type").SizeDragLimit;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    attach: {
        type: import("vue").PropType<TdDrawerProps["attach"]>;
    };
    body: {
        type: import("vue").PropType<TdDrawerProps["body"]>;
    };
    cancelBtn: {
        type: import("vue").PropType<TdDrawerProps["cancelBtn"]>;
    };
    closeBtn: {
        type: import("vue").PropType<TdDrawerProps["closeBtn"]>;
    };
    closeOnEscKeydown: {
        type: BooleanConstructor;
        default: any;
    };
    closeOnOverlayClick: {
        type: BooleanConstructor;
        default: any;
    };
    confirmBtn: {
        type: import("vue").PropType<TdDrawerProps["confirmBtn"]>;
    };
    default: {
        type: import("vue").PropType<TdDrawerProps["default"]>;
    };
    destroyOnClose: BooleanConstructor;
    drawerClassName: {
        type: StringConstructor;
        default: string;
    };
    footer: {
        type: import("vue").PropType<TdDrawerProps["footer"]>;
        default: TdDrawerProps["footer"];
    };
    header: {
        type: import("vue").PropType<TdDrawerProps["header"]>;
        default: TdDrawerProps["header"];
    };
    lazy: BooleanConstructor;
    mode: {
        type: import("vue").PropType<TdDrawerProps["mode"]>;
        default: TdDrawerProps["mode"];
        validator(val: TdDrawerProps["mode"]): boolean;
    };
    placement: {
        type: import("vue").PropType<TdDrawerProps["placement"]>;
        default: TdDrawerProps["placement"];
        validator(val: TdDrawerProps["placement"]): boolean;
    };
    preventScrollThrough: {
        type: BooleanConstructor;
        default: boolean;
    };
    showInAttachedElement: BooleanConstructor;
    showOverlay: {
        type: BooleanConstructor;
        default: boolean;
    };
    size: {
        type: StringConstructor;
        default: any;
    };
    sizeDraggable: {
        type: import("vue").PropType<TdDrawerProps["sizeDraggable"]>;
        default: TdDrawerProps["sizeDraggable"];
    };
    visible: BooleanConstructor;
    zIndex: {
        type: NumberConstructor;
    };
    onBeforeClose: import("vue").PropType<TdDrawerProps["onBeforeClose"]>;
    onBeforeOpen: import("vue").PropType<TdDrawerProps["onBeforeOpen"]>;
    onCancel: import("vue").PropType<TdDrawerProps["onCancel"]>;
    onClose: import("vue").PropType<TdDrawerProps["onClose"]>;
    onCloseBtnClick: import("vue").PropType<TdDrawerProps["onCloseBtnClick"]>;
    onConfirm: import("vue").PropType<TdDrawerProps["onConfirm"]>;
    onEscKeydown: import("vue").PropType<TdDrawerProps["onEscKeydown"]>;
    onOverlayClick: import("vue").PropType<TdDrawerProps["onOverlayClick"]>;
    onSizeDragEnd: import("vue").PropType<TdDrawerProps["onSizeDragEnd"]>;
}>> & Readonly<{
    "onUpdate:visible"?: (...args: any[]) => any;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "update:visible"[], "update:visible", {
    footer: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    mode: "overlay" | "push";
    size: string;
    header: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    visible: boolean;
    lazy: boolean;
    placement: "left" | "right" | "top" | "bottom";
    destroyOnClose: boolean;
    preventScrollThrough: boolean;
    showOverlay: boolean;
    closeOnEscKeydown: boolean;
    closeOnOverlayClick: boolean;
    showInAttachedElement: boolean;
    drawerClassName: string;
    sizeDraggable: boolean | import("./type").SizeDragLimit;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export { default as DrawerPlugin } from './plugin';
export type { DrawerPluginType } from './plugin';
export default Drawer;
