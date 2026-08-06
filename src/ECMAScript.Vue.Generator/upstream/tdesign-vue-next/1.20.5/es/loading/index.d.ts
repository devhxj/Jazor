import { TdLoadingProps } from './type';
import './style';
export * from './type';
export * from './plugin';
export type LoadingProps = TdLoadingProps;
export { default as LoadingPlugin } from './plugin';
export { default as LoadingDirective } from './directive';
export declare const Loading: {
    new (...args: any[]): import("vue").CreateComponentPublicInstance<Readonly<import("vue").ExtractPropTypes<{
        attach: {
            type: import("vue").PropType<TdLoadingProps["attach"]>;
            default: TdLoadingProps["attach"];
        };
        content: {
            type: import("vue").PropType<TdLoadingProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdLoadingProps["default"]>;
        };
        delay: {
            type: NumberConstructor;
            default: any;
        };
        fullscreen: BooleanConstructor;
        indicator: {
            type: import("vue").PropType<TdLoadingProps["indicator"]>;
            default: TdLoadingProps["indicator"];
        };
        inheritColor: {
            type: BooleanConstructor;
            default: any;
        };
        loading: {
            type: BooleanConstructor;
            default: boolean;
        };
        preventScrollThrough: {
            type: BooleanConstructor;
            default: any;
        };
        showOverlay: {
            type: BooleanConstructor;
            default: any;
        };
        size: {
            type: StringConstructor;
            default: any;
        };
        text: {
            type: import("vue").PropType<TdLoadingProps["text"]>;
        };
        zIndex: {
            type: NumberConstructor;
        };
    }>>, () => import("vue/jsx-runtime").JSX.Element, unknown, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & Readonly<import("vue").ExtractPropTypes<{
        attach: {
            type: import("vue").PropType<TdLoadingProps["attach"]>;
            default: TdLoadingProps["attach"];
        };
        content: {
            type: import("vue").PropType<TdLoadingProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdLoadingProps["default"]>;
        };
        delay: {
            type: NumberConstructor;
            default: any;
        };
        fullscreen: BooleanConstructor;
        indicator: {
            type: import("vue").PropType<TdLoadingProps["indicator"]>;
            default: TdLoadingProps["indicator"];
        };
        inheritColor: {
            type: BooleanConstructor;
            default: any;
        };
        loading: {
            type: BooleanConstructor;
            default: boolean;
        };
        preventScrollThrough: {
            type: BooleanConstructor;
            default: any;
        };
        showOverlay: {
            type: BooleanConstructor;
            default: any;
        };
        size: {
            type: StringConstructor;
            default: any;
        };
        text: {
            type: import("vue").PropType<TdLoadingProps["text"]>;
        };
        zIndex: {
            type: NumberConstructor;
        };
    }>>, {
        loading: boolean;
        size: string;
        attach: import("..").AttachNode;
        delay: number;
        indicator: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        fullscreen: boolean;
        inheritColor: boolean;
        preventScrollThrough: boolean;
        showOverlay: boolean;
    }, true, {}, {}, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        attach: {
            type: import("vue").PropType<TdLoadingProps["attach"]>;
            default: TdLoadingProps["attach"];
        };
        content: {
            type: import("vue").PropType<TdLoadingProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdLoadingProps["default"]>;
        };
        delay: {
            type: NumberConstructor;
            default: any;
        };
        fullscreen: BooleanConstructor;
        indicator: {
            type: import("vue").PropType<TdLoadingProps["indicator"]>;
            default: TdLoadingProps["indicator"];
        };
        inheritColor: {
            type: BooleanConstructor;
            default: any;
        };
        loading: {
            type: BooleanConstructor;
            default: boolean;
        };
        preventScrollThrough: {
            type: BooleanConstructor;
            default: any;
        };
        showOverlay: {
            type: BooleanConstructor;
            default: any;
        };
        size: {
            type: StringConstructor;
            default: any;
        };
        text: {
            type: import("vue").PropType<TdLoadingProps["text"]>;
        };
        zIndex: {
            type: NumberConstructor;
        };
    }>>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        loading: boolean;
        size: string;
        attach: import("..").AttachNode;
        delay: number;
        indicator: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        fullscreen: boolean;
        inheritColor: boolean;
        preventScrollThrough: boolean;
        showOverlay: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    attach: {
        type: import("vue").PropType<TdLoadingProps["attach"]>;
        default: TdLoadingProps["attach"];
    };
    content: {
        type: import("vue").PropType<TdLoadingProps["content"]>;
    };
    default: {
        type: import("vue").PropType<TdLoadingProps["default"]>;
    };
    delay: {
        type: NumberConstructor;
        default: any;
    };
    fullscreen: BooleanConstructor;
    indicator: {
        type: import("vue").PropType<TdLoadingProps["indicator"]>;
        default: TdLoadingProps["indicator"];
    };
    inheritColor: {
        type: BooleanConstructor;
        default: any;
    };
    loading: {
        type: BooleanConstructor;
        default: boolean;
    };
    preventScrollThrough: {
        type: BooleanConstructor;
        default: any;
    };
    showOverlay: {
        type: BooleanConstructor;
        default: any;
    };
    size: {
        type: StringConstructor;
        default: any;
    };
    text: {
        type: import("vue").PropType<TdLoadingProps["text"]>;
    };
    zIndex: {
        type: NumberConstructor;
    };
}>>, () => import("vue/jsx-runtime").JSX.Element, unknown, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    loading: boolean;
    size: string;
    attach: import("..").AttachNode;
    delay: number;
    indicator: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    fullscreen: boolean;
    inheritColor: boolean;
    preventScrollThrough: boolean;
    showOverlay: boolean;
}, {}, string, {}> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Loading;
