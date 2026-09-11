import type { TextareaValue } from './type';
declare const _default: import("vue").DefineComponent<{
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
    value?: TextareaValue;
    defaultValue?: TextareaValue;
    modelValue?: TextareaValue;
    onBlur?: (value: TextareaValue, context: {
        e: FocusEvent;
    }) => void;
    onChange?: (value: TextareaValue, context?: {
        e?: InputEvent;
    }) => void;
    onFocus?: (value: TextareaValue, context: {
        e: FocusEvent;
    }) => void;
    onKeydown?: (value: TextareaValue, context: {
        e: KeyboardEvent;
    }) => void;
    onKeypress?: (value: TextareaValue, context: {
        e: KeyboardEvent;
    }) => void;
    onKeyup?: (value: TextareaValue, context: {
        e: KeyboardEvent;
    }) => void;
    onValidate?: (context: {
        error?: "exceed-maximum" | "below-minimum";
    }) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
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
    value?: TextareaValue;
    defaultValue?: TextareaValue;
    modelValue?: TextareaValue;
    onBlur?: (value: TextareaValue, context: {
        e: FocusEvent;
    }) => void;
    onChange?: (value: TextareaValue, context?: {
        e?: InputEvent;
    }) => void;
    onFocus?: (value: TextareaValue, context: {
        e: FocusEvent;
    }) => void;
    onKeydown?: (value: TextareaValue, context: {
        e: KeyboardEvent;
    }) => void;
    onKeypress?: (value: TextareaValue, context: {
        e: KeyboardEvent;
    }) => void;
    onKeyup?: (value: TextareaValue, context: {
        e: KeyboardEvent;
    }) => void;
    onValidate?: (context: {
        error?: "exceed-maximum" | "below-minimum";
    }) => void;
}> & Readonly<{}>, {
    value: TextareaValue;
    status: "default" | "error" | "success" | "warning";
    disabled: boolean;
    name: string;
    placeholder: string;
    autofocus: boolean;
    modelValue: TextareaValue;
    readonly: boolean;
    allowInputOverMax: boolean;
    autosize: boolean | {
        minRows?: number;
        maxRows?: number;
    };
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
