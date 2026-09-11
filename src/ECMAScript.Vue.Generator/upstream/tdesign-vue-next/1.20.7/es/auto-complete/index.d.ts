import { AutoCompleteOption, TdAutoCompleteProps } from './type';
import './style';
export * from './type';
export type AutoCompleteProps<T extends AutoCompleteOption = AutoCompleteOption> = TdAutoCompleteProps<T>;
export declare const AutoComplete: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        autofocus?: boolean;
        borderless?: boolean;
        clearable?: boolean;
        default?: string | import("..").TNode;
        disabled?: boolean;
        empty?: string | import("..").TNode;
        filter?: (filterWords: string, option: AutoCompleteOption) => boolean | Promise<boolean>;
        filterable?: boolean;
        highlightKeyword?: boolean;
        inputProps?: import("..").InputProps;
        options?: AutoCompleteOption[];
        panelBottomContent?: string | import("..").TNode;
        panelTopContent?: string | import("..").TNode;
        placeholder?: string;
        popupProps?: import("..").PopupProps;
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
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
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
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        autofocus?: boolean;
        borderless?: boolean;
        clearable?: boolean;
        default?: string | import("..").TNode;
        disabled?: boolean;
        empty?: string | import("..").TNode;
        filter?: (filterWords: string, option: AutoCompleteOption) => boolean | Promise<boolean>;
        filterable?: boolean;
        highlightKeyword?: boolean;
        inputProps?: import("..").InputProps;
        options?: AutoCompleteOption[];
        panelBottomContent?: string | import("..").TNode;
        panelTopContent?: string | import("..").TNode;
        placeholder?: string;
        popupProps?: import("..").PopupProps;
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
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
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
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    autofocus?: boolean;
    borderless?: boolean;
    clearable?: boolean;
    default?: string | import("..").TNode;
    disabled?: boolean;
    empty?: string | import("..").TNode;
    filter?: (filterWords: string, option: AutoCompleteOption) => boolean | Promise<boolean>;
    filterable?: boolean;
    highlightKeyword?: boolean;
    inputProps?: import("..").InputProps;
    options?: AutoCompleteOption[];
    panelBottomContent?: string | import("..").TNode;
    panelTopContent?: string | import("..").TNode;
    placeholder?: string;
    popupProps?: import("..").PopupProps;
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
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
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
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const HighlightOption: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        content: import("vue").PropType<import("./components/highlight-option").HighlightOptionProps["content"]>;
        keyword: import("vue").PropType<import("./components/highlight-option").HighlightOptionProps["keyword"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {}, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        content: import("vue").PropType<import("./components/highlight-option").HighlightOptionProps["content"]>;
        keyword: import("vue").PropType<import("./components/highlight-option").HighlightOptionProps["keyword"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {}>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    content: import("vue").PropType<import("./components/highlight-option").HighlightOptionProps["content"]>;
    keyword: import("vue").PropType<import("./components/highlight-option").HighlightOptionProps["keyword"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default AutoComplete;
