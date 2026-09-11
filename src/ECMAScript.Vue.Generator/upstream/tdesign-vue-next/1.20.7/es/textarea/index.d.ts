import { TdTextareaProps } from './type';
import './style';
export * from './type';
export type TextareaProps = TdTextareaProps;
export declare const Textarea: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        allowInputOverMax?: boolean;
        autofocus?: boolean;
        autosize?: boolean | {
            minRows?: number;
            maxRows?: number;
        };
        disabled?: boolean;
        maxcharacter?: number;
        maxlength?: string | number;
        name?: string;
        placeholder?: string;
        readonly?: boolean;
        status?: "default" | "success" | "warning" | "error";
        tips?: string | import("..").TNode;
        value?: import("./type").TextareaValue;
        defaultValue?: import("./type").TextareaValue;
        modelValue?: import("./type").TextareaValue;
        onBlur?: (value: import("./type").TextareaValue, context: {
            e: FocusEvent;
        }) => void;
        onChange?: (value: import("./type").TextareaValue, context?: {
            e?: InputEvent;
        }) => void;
        onFocus?: (value: import("./type").TextareaValue, context: {
            e: FocusEvent;
        }) => void;
        onKeydown?: (value: import("./type").TextareaValue, context: {
            e: KeyboardEvent;
        }) => void;
        onKeypress?: (value: import("./type").TextareaValue, context: {
            e: KeyboardEvent;
        }) => void;
        onKeyup?: (value: import("./type").TextareaValue, context: {
            e: KeyboardEvent;
        }) => void;
        onValidate?: (context: {
            error?: "exceed-maximum" | "below-minimum";
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").TextareaValue;
        status: "default" | "error" | "success" | "warning";
        disabled: boolean;
        name: string;
        placeholder: string;
        autofocus: boolean;
        modelValue: import("./type").TextareaValue;
        readonly: boolean;
        allowInputOverMax: boolean;
        autosize: boolean | {
            minRows?: number;
            maxRows?: number;
        };
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        allowInputOverMax?: boolean;
        autofocus?: boolean;
        autosize?: boolean | {
            minRows?: number;
            maxRows?: number;
        };
        disabled?: boolean;
        maxcharacter?: number;
        maxlength?: string | number;
        name?: string;
        placeholder?: string;
        readonly?: boolean;
        status?: "default" | "success" | "warning" | "error";
        tips?: string | import("..").TNode;
        value?: import("./type").TextareaValue;
        defaultValue?: import("./type").TextareaValue;
        modelValue?: import("./type").TextareaValue;
        onBlur?: (value: import("./type").TextareaValue, context: {
            e: FocusEvent;
        }) => void;
        onChange?: (value: import("./type").TextareaValue, context?: {
            e?: InputEvent;
        }) => void;
        onFocus?: (value: import("./type").TextareaValue, context: {
            e: FocusEvent;
        }) => void;
        onKeydown?: (value: import("./type").TextareaValue, context: {
            e: KeyboardEvent;
        }) => void;
        onKeypress?: (value: import("./type").TextareaValue, context: {
            e: KeyboardEvent;
        }) => void;
        onKeyup?: (value: import("./type").TextareaValue, context: {
            e: KeyboardEvent;
        }) => void;
        onValidate?: (context: {
            error?: "exceed-maximum" | "below-minimum";
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").TextareaValue;
        status: "default" | "error" | "success" | "warning";
        disabled: boolean;
        name: string;
        placeholder: string;
        autofocus: boolean;
        modelValue: import("./type").TextareaValue;
        readonly: boolean;
        allowInputOverMax: boolean;
        autosize: boolean | {
            minRows?: number;
            maxRows?: number;
        };
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    allowInputOverMax?: boolean;
    autofocus?: boolean;
    autosize?: boolean | {
        minRows?: number;
        maxRows?: number;
    };
    disabled?: boolean;
    maxcharacter?: number;
    maxlength?: string | number;
    name?: string;
    placeholder?: string;
    readonly?: boolean;
    status?: "default" | "success" | "warning" | "error";
    tips?: string | import("..").TNode;
    value?: import("./type").TextareaValue;
    defaultValue?: import("./type").TextareaValue;
    modelValue?: import("./type").TextareaValue;
    onBlur?: (value: import("./type").TextareaValue, context: {
        e: FocusEvent;
    }) => void;
    onChange?: (value: import("./type").TextareaValue, context?: {
        e?: InputEvent;
    }) => void;
    onFocus?: (value: import("./type").TextareaValue, context: {
        e: FocusEvent;
    }) => void;
    onKeydown?: (value: import("./type").TextareaValue, context: {
        e: KeyboardEvent;
    }) => void;
    onKeypress?: (value: import("./type").TextareaValue, context: {
        e: KeyboardEvent;
    }) => void;
    onKeyup?: (value: import("./type").TextareaValue, context: {
        e: KeyboardEvent;
    }) => void;
    onValidate?: (context: {
        error?: "exceed-maximum" | "below-minimum";
    }) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").TextareaValue;
    status: "default" | "error" | "success" | "warning";
    disabled: boolean;
    name: string;
    placeholder: string;
    autofocus: boolean;
    modelValue: import("./type").TextareaValue;
    readonly: boolean;
    allowInputOverMax: boolean;
    autosize: boolean | {
        minRows?: number;
        maxRows?: number;
    };
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Textarea;
