import { TdInputNumberProps } from './type';
import './style';
export * from './type';
export type InputNumberProps = TdInputNumberProps;
export declare const InputNumber: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        align?: "left" | "center" | "right";
        allowInputOverLimit?: boolean;
        autoWidth?: boolean;
        autofocus?: boolean;
        decimalPlaces?: import("./type").InputNumberDecimalPlaces;
        disabled?: boolean;
        format?: (value: import("./type").InputNumberValue, context?: {
            fixedNumber?: import("./type").InputNumberValue;
        }) => import("./type").InputNumberValue;
        inputProps?: import("..").InputProps;
        label?: string | import("..").TNode;
        largeNumber?: boolean;
        max?: import("./type").InputNumberValue;
        min?: import("./type").InputNumberValue;
        placeholder?: string;
        readonly?: boolean;
        size?: "small" | "medium" | "large";
        status?: "default" | "success" | "warning" | "error";
        step?: import("./type").InputNumberValue;
        suffix?: string | import("..").TNode;
        theme?: "column" | "row" | "normal";
        tips?: string | import("..").TNode;
        value?: import("./type").InputNumberValue;
        defaultValue?: import("./type").InputNumberValue;
        modelValue?: import("./type").InputNumberValue;
        onBlur?: (value: import("./type").InputNumberValue, context: {
            e: FocusEvent;
        }) => void;
        onChange?: (value: import("./type").InputNumberValue, context: import("./type").ChangeContext) => void;
        onEnter?: (value: import("./type").InputNumberValue, context: {
            e: KeyboardEvent;
        }) => void;
        onFocus?: (value: import("./type").InputNumberValue, context: {
            e: FocusEvent;
        }) => void;
        onKeydown?: (value: import("./type").InputNumberValue, context: {
            e: KeyboardEvent;
        }) => void;
        onKeypress?: (value: import("./type").InputNumberValue, context: {
            e: KeyboardEvent;
        }) => void;
        onKeyup?: (value: import("./type").InputNumberValue, context: {
            e: KeyboardEvent;
        }) => void;
        onValidate?: (context: {
            error?: "exceed-maximum" | "below-minimum";
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").InputNumberValue;
        min: import("./type").InputNumberValue;
        max: import("./type").InputNumberValue;
        size: "small" | "medium" | "large";
        status: "default" | "error" | "success" | "warning";
        largeNumber: boolean;
        step: import("./type").InputNumberValue;
        decimalPlaces: import("./type").InputNumberDecimalPlaces;
        disabled: boolean;
        placeholder: string;
        autofocus: boolean;
        theme: "normal" | "row" | "column";
        modelValue: import("./type").InputNumberValue;
        readonly: boolean;
        autoWidth: boolean;
        allowInputOverLimit: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        align?: "left" | "center" | "right";
        allowInputOverLimit?: boolean;
        autoWidth?: boolean;
        autofocus?: boolean;
        decimalPlaces?: import("./type").InputNumberDecimalPlaces;
        disabled?: boolean;
        format?: (value: import("./type").InputNumberValue, context?: {
            fixedNumber?: import("./type").InputNumberValue;
        }) => import("./type").InputNumberValue;
        inputProps?: import("..").InputProps;
        label?: string | import("..").TNode;
        largeNumber?: boolean;
        max?: import("./type").InputNumberValue;
        min?: import("./type").InputNumberValue;
        placeholder?: string;
        readonly?: boolean;
        size?: "small" | "medium" | "large";
        status?: "default" | "success" | "warning" | "error";
        step?: import("./type").InputNumberValue;
        suffix?: string | import("..").TNode;
        theme?: "column" | "row" | "normal";
        tips?: string | import("..").TNode;
        value?: import("./type").InputNumberValue;
        defaultValue?: import("./type").InputNumberValue;
        modelValue?: import("./type").InputNumberValue;
        onBlur?: (value: import("./type").InputNumberValue, context: {
            e: FocusEvent;
        }) => void;
        onChange?: (value: import("./type").InputNumberValue, context: import("./type").ChangeContext) => void;
        onEnter?: (value: import("./type").InputNumberValue, context: {
            e: KeyboardEvent;
        }) => void;
        onFocus?: (value: import("./type").InputNumberValue, context: {
            e: FocusEvent;
        }) => void;
        onKeydown?: (value: import("./type").InputNumberValue, context: {
            e: KeyboardEvent;
        }) => void;
        onKeypress?: (value: import("./type").InputNumberValue, context: {
            e: KeyboardEvent;
        }) => void;
        onKeyup?: (value: import("./type").InputNumberValue, context: {
            e: KeyboardEvent;
        }) => void;
        onValidate?: (context: {
            error?: "exceed-maximum" | "below-minimum";
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").InputNumberValue;
        min: import("./type").InputNumberValue;
        max: import("./type").InputNumberValue;
        size: "small" | "medium" | "large";
        status: "default" | "error" | "success" | "warning";
        largeNumber: boolean;
        step: import("./type").InputNumberValue;
        decimalPlaces: import("./type").InputNumberDecimalPlaces;
        disabled: boolean;
        placeholder: string;
        autofocus: boolean;
        theme: "normal" | "row" | "column";
        modelValue: import("./type").InputNumberValue;
        readonly: boolean;
        autoWidth: boolean;
        allowInputOverLimit: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    align?: "left" | "center" | "right";
    allowInputOverLimit?: boolean;
    autoWidth?: boolean;
    autofocus?: boolean;
    decimalPlaces?: import("./type").InputNumberDecimalPlaces;
    disabled?: boolean;
    format?: (value: import("./type").InputNumberValue, context?: {
        fixedNumber?: import("./type").InputNumberValue;
    }) => import("./type").InputNumberValue;
    inputProps?: import("..").InputProps;
    label?: string | import("..").TNode;
    largeNumber?: boolean;
    max?: import("./type").InputNumberValue;
    min?: import("./type").InputNumberValue;
    placeholder?: string;
    readonly?: boolean;
    size?: "small" | "medium" | "large";
    status?: "default" | "success" | "warning" | "error";
    step?: import("./type").InputNumberValue;
    suffix?: string | import("..").TNode;
    theme?: "column" | "row" | "normal";
    tips?: string | import("..").TNode;
    value?: import("./type").InputNumberValue;
    defaultValue?: import("./type").InputNumberValue;
    modelValue?: import("./type").InputNumberValue;
    onBlur?: (value: import("./type").InputNumberValue, context: {
        e: FocusEvent;
    }) => void;
    onChange?: (value: import("./type").InputNumberValue, context: import("./type").ChangeContext) => void;
    onEnter?: (value: import("./type").InputNumberValue, context: {
        e: KeyboardEvent;
    }) => void;
    onFocus?: (value: import("./type").InputNumberValue, context: {
        e: FocusEvent;
    }) => void;
    onKeydown?: (value: import("./type").InputNumberValue, context: {
        e: KeyboardEvent;
    }) => void;
    onKeypress?: (value: import("./type").InputNumberValue, context: {
        e: KeyboardEvent;
    }) => void;
    onKeyup?: (value: import("./type").InputNumberValue, context: {
        e: KeyboardEvent;
    }) => void;
    onValidate?: (context: {
        error?: "exceed-maximum" | "below-minimum";
    }) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").InputNumberValue;
    min: import("./type").InputNumberValue;
    max: import("./type").InputNumberValue;
    size: "small" | "medium" | "large";
    status: "default" | "error" | "success" | "warning";
    largeNumber: boolean;
    step: import("./type").InputNumberValue;
    decimalPlaces: import("./type").InputNumberDecimalPlaces;
    disabled: boolean;
    placeholder: string;
    autofocus: boolean;
    theme: "normal" | "row" | "column";
    modelValue: import("./type").InputNumberValue;
    readonly: boolean;
    autoWidth: boolean;
    allowInputOverLimit: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default InputNumber;
