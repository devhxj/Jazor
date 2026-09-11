import { TdAlertProps } from './type';
import './style';
export * from './type';
export type AlertProps = TdAlertProps;
export declare const Alert: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        close: {
            type: import("vue").PropType<TdAlertProps["close"]>;
            default: TdAlertProps["close"];
        };
        closeBtn: {
            type: import("vue").PropType<TdAlertProps["closeBtn"]>;
            default: TdAlertProps["closeBtn"];
        };
        default: {
            type: import("vue").PropType<TdAlertProps["default"]>;
        };
        icon: {
            type: import("vue").PropType<TdAlertProps["icon"]>;
        };
        maxLine: {
            type: NumberConstructor;
            default: number;
        };
        message: {
            type: import("vue").PropType<TdAlertProps["message"]>;
        };
        operation: {
            type: import("vue").PropType<TdAlertProps["operation"]>;
        };
        theme: {
            type: import("vue").PropType<TdAlertProps["theme"]>;
            default: TdAlertProps["theme"];
            validator(val: TdAlertProps["theme"]): boolean;
        };
        title: {
            type: import("vue").PropType<TdAlertProps["title"]>;
        };
        onClose: import("vue").PropType<TdAlertProps["onClose"]>;
        onClosed: import("vue").PropType<TdAlertProps["onClosed"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        close: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        theme: "error" | "info" | "success" | "warning";
        closeBtn: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        maxLine: number;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        close: {
            type: import("vue").PropType<TdAlertProps["close"]>;
            default: TdAlertProps["close"];
        };
        closeBtn: {
            type: import("vue").PropType<TdAlertProps["closeBtn"]>;
            default: TdAlertProps["closeBtn"];
        };
        default: {
            type: import("vue").PropType<TdAlertProps["default"]>;
        };
        icon: {
            type: import("vue").PropType<TdAlertProps["icon"]>;
        };
        maxLine: {
            type: NumberConstructor;
            default: number;
        };
        message: {
            type: import("vue").PropType<TdAlertProps["message"]>;
        };
        operation: {
            type: import("vue").PropType<TdAlertProps["operation"]>;
        };
        theme: {
            type: import("vue").PropType<TdAlertProps["theme"]>;
            default: TdAlertProps["theme"];
            validator(val: TdAlertProps["theme"]): boolean;
        };
        title: {
            type: import("vue").PropType<TdAlertProps["title"]>;
        };
        onClose: import("vue").PropType<TdAlertProps["onClose"]>;
        onClosed: import("vue").PropType<TdAlertProps["onClosed"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        close: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        theme: "error" | "info" | "success" | "warning";
        closeBtn: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        maxLine: number;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    close: {
        type: import("vue").PropType<TdAlertProps["close"]>;
        default: TdAlertProps["close"];
    };
    closeBtn: {
        type: import("vue").PropType<TdAlertProps["closeBtn"]>;
        default: TdAlertProps["closeBtn"];
    };
    default: {
        type: import("vue").PropType<TdAlertProps["default"]>;
    };
    icon: {
        type: import("vue").PropType<TdAlertProps["icon"]>;
    };
    maxLine: {
        type: NumberConstructor;
        default: number;
    };
    message: {
        type: import("vue").PropType<TdAlertProps["message"]>;
    };
    operation: {
        type: import("vue").PropType<TdAlertProps["operation"]>;
    };
    theme: {
        type: import("vue").PropType<TdAlertProps["theme"]>;
        default: TdAlertProps["theme"];
        validator(val: TdAlertProps["theme"]): boolean;
    };
    title: {
        type: import("vue").PropType<TdAlertProps["title"]>;
    };
    onClose: import("vue").PropType<TdAlertProps["onClose"]>;
    onClosed: import("vue").PropType<TdAlertProps["onClosed"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    close: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    theme: "error" | "info" | "success" | "warning";
    closeBtn: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    maxLine: number;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Alert;
