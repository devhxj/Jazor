import { InputProps } from '../input';
import { PopupProps } from '../popup';
declare const _default: import("vue").DefineComponent<{
    autofocus?: boolean;
    borderless?: boolean;
    clearable?: boolean;
    default?: string | import("..").TNode;
    disabled?: boolean;
    empty?: string | import("..").TNode;
    filter?: (filterWords: string, option: import("./type").AutoCompleteOption) => boolean | Promise<boolean>;
    filterable?: boolean;
    highlightKeyword?: boolean;
    inputProps?: InputProps;
    options?: import("./type").AutoCompleteOption[];
    panelBottomContent?: string | import("..").TNode;
    panelTopContent?: string | import("..").TNode;
    placeholder?: string;
    popupProps?: PopupProps;
    readonly?: boolean;
    size?: import("..").SizeEnum;
    status?: "default" | "success" | "warning" | "error";
    textareaProps?: import("..").TextareaProps;
    tips?: string | import("..").TNode;
    triggerElement?: string | import("..").TNode;
    value?: string;
    defaultValue?: string;
    modelValue?: string;
    onBlur?: (context: {
        e: FocusEvent;
        value: string;
    }) => void;
    onChange?: (value: string, context?: {
        e?: InputEvent | MouseEvent | CompositionEvent | KeyboardEvent;
    }) => void;
    onClear?: (context: {
        e: MouseEvent;
    }) => void;
    onCompositionend?: (context: {
        e: CompositionEvent;
        value: string;
    }) => void;
    onCompositionstart?: (context: {
        e: CompositionEvent;
        value: string;
    }) => void;
    onEnter?: (context: {
        e: KeyboardEvent;
        value: string;
    }) => void;
    onFocus?: (context: {
        e: FocusEvent;
        value: string;
    }) => void;
    onSelect?: (value: string, context: {
        e: MouseEvent | KeyboardEvent;
    }) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    autofocus?: boolean;
    borderless?: boolean;
    clearable?: boolean;
    default?: string | import("..").TNode;
    disabled?: boolean;
    empty?: string | import("..").TNode;
    filter?: (filterWords: string, option: import("./type").AutoCompleteOption) => boolean | Promise<boolean>;
    filterable?: boolean;
    highlightKeyword?: boolean;
    inputProps?: InputProps;
    options?: import("./type").AutoCompleteOption[];
    panelBottomContent?: string | import("..").TNode;
    panelTopContent?: string | import("..").TNode;
    placeholder?: string;
    popupProps?: PopupProps;
    readonly?: boolean;
    size?: import("..").SizeEnum;
    status?: "default" | "success" | "warning" | "error";
    textareaProps?: import("..").TextareaProps;
    tips?: string | import("..").TNode;
    triggerElement?: string | import("..").TNode;
    value?: string;
    defaultValue?: string;
    modelValue?: string;
    onBlur?: (context: {
        e: FocusEvent;
        value: string;
    }) => void;
    onChange?: (value: string, context?: {
        e?: InputEvent | MouseEvent | CompositionEvent | KeyboardEvent;
    }) => void;
    onClear?: (context: {
        e: MouseEvent;
    }) => void;
    onCompositionend?: (context: {
        e: CompositionEvent;
        value: string;
    }) => void;
    onCompositionstart?: (context: {
        e: CompositionEvent;
        value: string;
    }) => void;
    onEnter?: (context: {
        e: KeyboardEvent;
        value: string;
    }) => void;
    onFocus?: (context: {
        e: FocusEvent;
        value: string;
    }) => void;
    onSelect?: (value: string, context: {
        e: MouseEvent | KeyboardEvent;
    }) => void;
}> & Readonly<{}>, {
    value: string;
    size: import("..").SizeEnum;
    status: "default" | "error" | "success" | "warning";
    disabled: boolean;
    defaultValue: string;
    placeholder: string;
    autofocus: boolean;
    modelValue: string;
    readonly: boolean;
    borderless: boolean;
    clearable: boolean;
    filterable: boolean;
    highlightKeyword: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
