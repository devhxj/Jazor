import { TdTagInputProps } from './type';
import './style';
export * from './type';
export type TagInputProps = TdTagInputProps;
export declare const TagInput: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        autoWidth?: boolean;
        borderless?: boolean;
        clearable?: boolean;
        collapsedItems?: import("..").TNode<{
            value: import("./type").TagInputValue;
            collapsedSelectedItems: import("./type").TagInputValue;
            count: number;
            onClose: (context: {
                index: number;
                e?: MouseEvent;
            }) => void;
        }>;
        disabled?: boolean;
        dragSort?: boolean;
        excessTagsDisplayType?: "scroll" | "break-line";
        inputProps?: import("..").InputProps;
        inputValue?: string;
        defaultInputValue?: string;
        label?: string | import("..").TNode;
        max?: number;
        minCollapsedNum?: number;
        placeholder?: string;
        prefixIcon?: import("..").TNode;
        readonly?: boolean;
        size?: import("..").SizeEnum;
        status?: "default" | "success" | "warning" | "error";
        suffix?: string | import("..").TNode;
        suffixIcon?: import("..").TNode;
        tag?: string | import("..").TNode<{
            value: string | number;
        }>;
        tagProps?: import("..").TagProps;
        tips?: string | import("..").TNode;
        value?: import("./type").TagInputValue;
        defaultValue?: import("./type").TagInputValue;
        modelValue?: import("./type").TagInputValue;
        valueDisplay?: string | import("..").TNode<{
            value: import("./type").TagInputValue;
            onClose: (index: number, item?: any) => void;
        }>;
        onBlur?: (value: import("./type").TagInputValue, context: {
            inputValue: string;
            e: FocusEvent;
        }) => void;
        onChange?: (value: import("./type").TagInputValue, context: import("./type").TagInputChangeContext) => void;
        onClear?: (context: {
            e: MouseEvent;
        }) => void;
        onClick?: (context: {
            e: MouseEvent;
        }) => void;
        onDragSort?: (context: import("./type").TagInputDragSortContext) => void;
        onEnter?: (value: import("./type").TagInputValue, context: {
            e: KeyboardEvent;
            inputValue: string;
        }) => void;
        onFocus?: (value: import("./type").TagInputValue, context: {
            inputValue: string;
            e: FocusEvent;
        }) => void;
        onInputChange?: (value: string, context?: import("./type").InputValueChangeContext) => void;
        onMouseenter?: (context: {
            e: MouseEvent;
        }) => void;
        onMouseleave?: (context: {
            e: MouseEvent;
        }) => void;
        onPaste?: (context: {
            e: ClipboardEvent;
            pasteValue: string;
        }) => void;
        onRemove?: (context: import("./type").TagInputRemoveContext) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").TagInputValue;
        size: import("..").SizeEnum;
        disabled: boolean;
        defaultValue: import("./type").TagInputValue;
        placeholder: string;
        modelValue: import("./type").TagInputValue;
        readonly: boolean;
        autoWidth: boolean;
        borderless: boolean;
        clearable: boolean;
        inputValue: string;
        excessTagsDisplayType: "scroll" | "break-line";
        defaultInputValue: string;
        dragSort: boolean;
        minCollapsedNum: number;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        autoWidth?: boolean;
        borderless?: boolean;
        clearable?: boolean;
        collapsedItems?: import("..").TNode<{
            value: import("./type").TagInputValue;
            collapsedSelectedItems: import("./type").TagInputValue;
            count: number;
            onClose: (context: {
                index: number;
                e?: MouseEvent;
            }) => void;
        }>;
        disabled?: boolean;
        dragSort?: boolean;
        excessTagsDisplayType?: "scroll" | "break-line";
        inputProps?: import("..").InputProps;
        inputValue?: string;
        defaultInputValue?: string;
        label?: string | import("..").TNode;
        max?: number;
        minCollapsedNum?: number;
        placeholder?: string;
        prefixIcon?: import("..").TNode;
        readonly?: boolean;
        size?: import("..").SizeEnum;
        status?: "default" | "success" | "warning" | "error";
        suffix?: string | import("..").TNode;
        suffixIcon?: import("..").TNode;
        tag?: string | import("..").TNode<{
            value: string | number;
        }>;
        tagProps?: import("..").TagProps;
        tips?: string | import("..").TNode;
        value?: import("./type").TagInputValue;
        defaultValue?: import("./type").TagInputValue;
        modelValue?: import("./type").TagInputValue;
        valueDisplay?: string | import("..").TNode<{
            value: import("./type").TagInputValue;
            onClose: (index: number, item?: any) => void;
        }>;
        onBlur?: (value: import("./type").TagInputValue, context: {
            inputValue: string;
            e: FocusEvent;
        }) => void;
        onChange?: (value: import("./type").TagInputValue, context: import("./type").TagInputChangeContext) => void;
        onClear?: (context: {
            e: MouseEvent;
        }) => void;
        onClick?: (context: {
            e: MouseEvent;
        }) => void;
        onDragSort?: (context: import("./type").TagInputDragSortContext) => void;
        onEnter?: (value: import("./type").TagInputValue, context: {
            e: KeyboardEvent;
            inputValue: string;
        }) => void;
        onFocus?: (value: import("./type").TagInputValue, context: {
            inputValue: string;
            e: FocusEvent;
        }) => void;
        onInputChange?: (value: string, context?: import("./type").InputValueChangeContext) => void;
        onMouseenter?: (context: {
            e: MouseEvent;
        }) => void;
        onMouseleave?: (context: {
            e: MouseEvent;
        }) => void;
        onPaste?: (context: {
            e: ClipboardEvent;
            pasteValue: string;
        }) => void;
        onRemove?: (context: import("./type").TagInputRemoveContext) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").TagInputValue;
        size: import("..").SizeEnum;
        disabled: boolean;
        defaultValue: import("./type").TagInputValue;
        placeholder: string;
        modelValue: import("./type").TagInputValue;
        readonly: boolean;
        autoWidth: boolean;
        borderless: boolean;
        clearable: boolean;
        inputValue: string;
        excessTagsDisplayType: "scroll" | "break-line";
        defaultInputValue: string;
        dragSort: boolean;
        minCollapsedNum: number;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    autoWidth?: boolean;
    borderless?: boolean;
    clearable?: boolean;
    collapsedItems?: import("..").TNode<{
        value: import("./type").TagInputValue;
        collapsedSelectedItems: import("./type").TagInputValue;
        count: number;
        onClose: (context: {
            index: number;
            e?: MouseEvent;
        }) => void;
    }>;
    disabled?: boolean;
    dragSort?: boolean;
    excessTagsDisplayType?: "scroll" | "break-line";
    inputProps?: import("..").InputProps;
    inputValue?: string;
    defaultInputValue?: string;
    label?: string | import("..").TNode;
    max?: number;
    minCollapsedNum?: number;
    placeholder?: string;
    prefixIcon?: import("..").TNode;
    readonly?: boolean;
    size?: import("..").SizeEnum;
    status?: "default" | "success" | "warning" | "error";
    suffix?: string | import("..").TNode;
    suffixIcon?: import("..").TNode;
    tag?: string | import("..").TNode<{
        value: string | number;
    }>;
    tagProps?: import("..").TagProps;
    tips?: string | import("..").TNode;
    value?: import("./type").TagInputValue;
    defaultValue?: import("./type").TagInputValue;
    modelValue?: import("./type").TagInputValue;
    valueDisplay?: string | import("..").TNode<{
        value: import("./type").TagInputValue;
        onClose: (index: number, item?: any) => void;
    }>;
    onBlur?: (value: import("./type").TagInputValue, context: {
        inputValue: string;
        e: FocusEvent;
    }) => void;
    onChange?: (value: import("./type").TagInputValue, context: import("./type").TagInputChangeContext) => void;
    onClear?: (context: {
        e: MouseEvent;
    }) => void;
    onClick?: (context: {
        e: MouseEvent;
    }) => void;
    onDragSort?: (context: import("./type").TagInputDragSortContext) => void;
    onEnter?: (value: import("./type").TagInputValue, context: {
        e: KeyboardEvent;
        inputValue: string;
    }) => void;
    onFocus?: (value: import("./type").TagInputValue, context: {
        inputValue: string;
        e: FocusEvent;
    }) => void;
    onInputChange?: (value: string, context?: import("./type").InputValueChangeContext) => void;
    onMouseenter?: (context: {
        e: MouseEvent;
    }) => void;
    onMouseleave?: (context: {
        e: MouseEvent;
    }) => void;
    onPaste?: (context: {
        e: ClipboardEvent;
        pasteValue: string;
    }) => void;
    onRemove?: (context: import("./type").TagInputRemoveContext) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").TagInputValue;
    size: import("..").SizeEnum;
    disabled: boolean;
    defaultValue: import("./type").TagInputValue;
    placeholder: string;
    modelValue: import("./type").TagInputValue;
    readonly: boolean;
    autoWidth: boolean;
    borderless: boolean;
    clearable: boolean;
    inputValue: string;
    excessTagsDisplayType: "scroll" | "break-line";
    defaultInputValue: string;
    dragSort: boolean;
    minCollapsedNum: number;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default TagInput;
