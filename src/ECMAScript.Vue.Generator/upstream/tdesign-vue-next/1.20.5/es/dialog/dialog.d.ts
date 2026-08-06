import { TdDialogProps } from './type';
declare const _default: import("vue").DefineComponent<{
    attach: {
        type: import("vue").PropType<TdDialogProps["attach"]>;
    };
    body: {
        type: import("vue").PropType<TdDialogProps["body"]>;
    };
    cancelBtn: {
        type: import("vue").PropType<TdDialogProps["cancelBtn"]>;
    };
    closeBtn: {
        type: import("vue").PropType<TdDialogProps["closeBtn"]>;
        default: TdDialogProps["closeBtn"];
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
        type: import("vue").PropType<TdDialogProps["confirmBtn"]>;
    };
    confirmLoading: {
        type: BooleanConstructor;
        default: any;
    };
    confirmOnEnter: BooleanConstructor;
    default: {
        type: import("vue").PropType<TdDialogProps["default"]>;
    };
    destroyOnClose: BooleanConstructor;
    dialogClassName: {
        type: StringConstructor;
        default: string;
    };
    dialogStyle: {
        type: import("vue").PropType<TdDialogProps["dialogStyle"]>;
    };
    draggable: BooleanConstructor;
    footer: {
        type: import("vue").PropType<TdDialogProps["footer"]>;
        default: TdDialogProps["footer"];
    };
    header: {
        type: import("vue").PropType<TdDialogProps["header"]>;
        default: TdDialogProps["header"];
    };
    lazy: BooleanConstructor;
    mode: {
        type: import("vue").PropType<TdDialogProps["mode"]>;
        default: TdDialogProps["mode"];
        validator(val: TdDialogProps["mode"]): boolean;
    };
    placement: {
        type: import("vue").PropType<TdDialogProps["placement"]>;
        validator(val: TdDialogProps["placement"]): boolean;
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
    theme: {
        type: import("vue").PropType<TdDialogProps["theme"]>;
        default: TdDialogProps["theme"];
        validator(val: TdDialogProps["theme"]): boolean;
    };
    top: {
        type: import("vue").PropType<TdDialogProps["top"]>;
    };
    visible: BooleanConstructor;
    width: {
        type: import("vue").PropType<TdDialogProps["width"]>;
    };
    zIndex: {
        type: NumberConstructor;
    };
    onBeforeClose: import("vue").PropType<TdDialogProps["onBeforeClose"]>;
    onBeforeOpen: import("vue").PropType<TdDialogProps["onBeforeOpen"]>;
    onCancel: import("vue").PropType<TdDialogProps["onCancel"]>;
    onClose: import("vue").PropType<TdDialogProps["onClose"]>;
    onCloseBtnClick: import("vue").PropType<TdDialogProps["onCloseBtnClick"]>;
    onClosed: import("vue").PropType<TdDialogProps["onClosed"]>;
    onConfirm: import("vue").PropType<TdDialogProps["onConfirm"]>;
    onEscKeydown: import("vue").PropType<TdDialogProps["onEscKeydown"]>;
    onOpened: import("vue").PropType<TdDialogProps["onOpened"]>;
    onOverlayClick: import("vue").PropType<TdDialogProps["onOverlayClick"]>;
}, () => import("vue/jsx-runtime").JSX.Element, unknown, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "update:visible"[], "update:visible", import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps, Readonly<import("vue").ExtractPropTypes<{
    attach: {
        type: import("vue").PropType<TdDialogProps["attach"]>;
    };
    body: {
        type: import("vue").PropType<TdDialogProps["body"]>;
    };
    cancelBtn: {
        type: import("vue").PropType<TdDialogProps["cancelBtn"]>;
    };
    closeBtn: {
        type: import("vue").PropType<TdDialogProps["closeBtn"]>;
        default: TdDialogProps["closeBtn"];
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
        type: import("vue").PropType<TdDialogProps["confirmBtn"]>;
    };
    confirmLoading: {
        type: BooleanConstructor;
        default: any;
    };
    confirmOnEnter: BooleanConstructor;
    default: {
        type: import("vue").PropType<TdDialogProps["default"]>;
    };
    destroyOnClose: BooleanConstructor;
    dialogClassName: {
        type: StringConstructor;
        default: string;
    };
    dialogStyle: {
        type: import("vue").PropType<TdDialogProps["dialogStyle"]>;
    };
    draggable: BooleanConstructor;
    footer: {
        type: import("vue").PropType<TdDialogProps["footer"]>;
        default: TdDialogProps["footer"];
    };
    header: {
        type: import("vue").PropType<TdDialogProps["header"]>;
        default: TdDialogProps["header"];
    };
    lazy: BooleanConstructor;
    mode: {
        type: import("vue").PropType<TdDialogProps["mode"]>;
        default: TdDialogProps["mode"];
        validator(val: TdDialogProps["mode"]): boolean;
    };
    placement: {
        type: import("vue").PropType<TdDialogProps["placement"]>;
        validator(val: TdDialogProps["placement"]): boolean;
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
    theme: {
        type: import("vue").PropType<TdDialogProps["theme"]>;
        default: TdDialogProps["theme"];
        validator(val: TdDialogProps["theme"]): boolean;
    };
    top: {
        type: import("vue").PropType<TdDialogProps["top"]>;
    };
    visible: BooleanConstructor;
    width: {
        type: import("vue").PropType<TdDialogProps["width"]>;
    };
    zIndex: {
        type: NumberConstructor;
    };
    onBeforeClose: import("vue").PropType<TdDialogProps["onBeforeClose"]>;
    onBeforeOpen: import("vue").PropType<TdDialogProps["onBeforeOpen"]>;
    onCancel: import("vue").PropType<TdDialogProps["onCancel"]>;
    onClose: import("vue").PropType<TdDialogProps["onClose"]>;
    onCloseBtnClick: import("vue").PropType<TdDialogProps["onCloseBtnClick"]>;
    onClosed: import("vue").PropType<TdDialogProps["onClosed"]>;
    onConfirm: import("vue").PropType<TdDialogProps["onConfirm"]>;
    onEscKeydown: import("vue").PropType<TdDialogProps["onEscKeydown"]>;
    onOpened: import("vue").PropType<TdDialogProps["onOpened"]>;
    onOverlayClick: import("vue").PropType<TdDialogProps["onOverlayClick"]>;
}>> & {
    "onUpdate:visible"?: (...args: any[]) => any;
}, {
    footer: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    mode: "normal" | "modal" | "modeless" | "full-screen";
    header: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    draggable: boolean;
    visible: boolean;
    lazy: boolean;
    theme: "default" | "info" | "success" | "warning" | "danger";
    destroyOnClose: boolean;
    preventScrollThrough: boolean;
    showOverlay: boolean;
    closeBtn: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    confirmLoading: boolean;
    closeOnEscKeydown: boolean;
    closeOnOverlayClick: boolean;
    confirmOnEnter: boolean;
    dialogClassName: string;
    showInAttachedElement: boolean;
}, {}>;
export default _default;
