import { TdSelectInputProps } from './type';
import './style';
export * from './type';
export type SelectInputProps = TdSelectInputProps;
export declare const SelectInput: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        allowInput?: boolean;
        autoWidth?: boolean;
        autofocus?: boolean;
        borderless?: boolean;
        clearable?: boolean;
        collapsedItems?: import("..").TNode<{
            value: import("./type").SelectInputValue;
            collapsedSelectedItems: import("./type").SelectInputValue;
            count: number;
            onClose: (context: {
                index: number;
                e?: MouseEvent;
            }) => void;
        }>;
        disabled?: boolean;
        inputProps?: import("..").InputProps;
        inputValue?: string;
        defaultInputValue?: string;
        keys?: import("./type").SelectInputKeys;
        label?: string | import("..").TNode;
        loading?: boolean;
        minCollapsedNum?: number;
        multiple?: boolean;
        panel?: string | import("..").TNode;
        placeholder?: string;
        popupProps?: import("..").PopupProps;
        popupVisible?: boolean;
        defaultPopupVisible?: boolean;
        prefixIcon?: import("..").TNode;
        readonly?: boolean;
        reserveKeyword?: boolean;
        size?: import("..").SizeEnum;
        status?: "default" | "success" | "warning" | "error";
        suffix?: string | import("..").TNode;
        suffixIcon?: import("..").TNode;
        tag?: string | import("..").TNode<{
            value: string | number;
        }>;
        tagInputProps?: import("..").TagInputProps;
        tagProps?: import("..").TagProps;
        tips?: string | import("..").TNode;
        value?: import("./type").SelectInputValue;
        valueDisplay?: string | import("..").TNode<{
            value: import("..").TagInputValue;
            onClose: (index: number, item?: any) => void;
        }>;
        onBlur?: (value: import("./type").SelectInputValue, context: import("./type").SelectInputBlurContext) => void;
        onClear?: (context: {
            e: MouseEvent;
        }) => void;
        onEnter?: (value: import("./type").SelectInputValue, context: {
            e: KeyboardEvent;
            inputValue: string;
            tagInputValue?: import("..").TagInputValue;
        }) => void;
        onFocus?: (value: import("./type").SelectInputValue, context: import("./type").SelectInputFocusContext) => void;
        onInputChange?: (value: string, context?: import("./type").SelectInputValueChangeContext) => void;
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
        onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
        onTagChange?: (value: import("..").TagInputValue, context: import("./type").SelectInputChangeContext) => void;
        valueDisplayOptions?: import("./hooks").SelectInputValueDisplayOptions;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        loading: boolean;
        value: string | number | boolean | Object | any[] | Date | import("./type").SelectInputValue[];
        multiple: boolean;
        size: import("..").SizeEnum;
        status: "default" | "error" | "success" | "warning";
        disabled: boolean;
        placeholder: string;
        autofocus: boolean;
        readonly: boolean;
        autoWidth: boolean;
        borderless: boolean;
        clearable: boolean;
        inputValue: string;
        minCollapsedNum: number;
        allowInput: boolean;
        popupVisible: boolean;
        defaultPopupVisible: boolean;
        reserveKeyword: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        allowInput?: boolean;
        autoWidth?: boolean;
        autofocus?: boolean;
        borderless?: boolean;
        clearable?: boolean;
        collapsedItems?: import("..").TNode<{
            value: import("./type").SelectInputValue;
            collapsedSelectedItems: import("./type").SelectInputValue;
            count: number;
            onClose: (context: {
                index: number;
                e?: MouseEvent;
            }) => void;
        }>;
        disabled?: boolean;
        inputProps?: import("..").InputProps;
        inputValue?: string;
        defaultInputValue?: string;
        keys?: import("./type").SelectInputKeys;
        label?: string | import("..").TNode;
        loading?: boolean;
        minCollapsedNum?: number;
        multiple?: boolean;
        panel?: string | import("..").TNode;
        placeholder?: string;
        popupProps?: import("..").PopupProps;
        popupVisible?: boolean;
        defaultPopupVisible?: boolean;
        prefixIcon?: import("..").TNode;
        readonly?: boolean;
        reserveKeyword?: boolean;
        size?: import("..").SizeEnum;
        status?: "default" | "success" | "warning" | "error";
        suffix?: string | import("..").TNode;
        suffixIcon?: import("..").TNode;
        tag?: string | import("..").TNode<{
            value: string | number;
        }>;
        tagInputProps?: import("..").TagInputProps;
        tagProps?: import("..").TagProps;
        tips?: string | import("..").TNode;
        value?: import("./type").SelectInputValue;
        valueDisplay?: string | import("..").TNode<{
            value: import("..").TagInputValue;
            onClose: (index: number, item?: any) => void;
        }>;
        onBlur?: (value: import("./type").SelectInputValue, context: import("./type").SelectInputBlurContext) => void;
        onClear?: (context: {
            e: MouseEvent;
        }) => void;
        onEnter?: (value: import("./type").SelectInputValue, context: {
            e: KeyboardEvent;
            inputValue: string;
            tagInputValue?: import("..").TagInputValue;
        }) => void;
        onFocus?: (value: import("./type").SelectInputValue, context: import("./type").SelectInputFocusContext) => void;
        onInputChange?: (value: string, context?: import("./type").SelectInputValueChangeContext) => void;
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
        onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
        onTagChange?: (value: import("..").TagInputValue, context: import("./type").SelectInputChangeContext) => void;
        valueDisplayOptions?: import("./hooks").SelectInputValueDisplayOptions;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        loading: boolean;
        value: string | number | boolean | Object | any[] | Date | import("./type").SelectInputValue[];
        multiple: boolean;
        size: import("..").SizeEnum;
        status: "default" | "error" | "success" | "warning";
        disabled: boolean;
        placeholder: string;
        autofocus: boolean;
        readonly: boolean;
        autoWidth: boolean;
        borderless: boolean;
        clearable: boolean;
        inputValue: string;
        minCollapsedNum: number;
        allowInput: boolean;
        popupVisible: boolean;
        defaultPopupVisible: boolean;
        reserveKeyword: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    allowInput?: boolean;
    autoWidth?: boolean;
    autofocus?: boolean;
    borderless?: boolean;
    clearable?: boolean;
    collapsedItems?: import("..").TNode<{
        value: import("./type").SelectInputValue;
        collapsedSelectedItems: import("./type").SelectInputValue;
        count: number;
        onClose: (context: {
            index: number;
            e?: MouseEvent;
        }) => void;
    }>;
    disabled?: boolean;
    inputProps?: import("..").InputProps;
    inputValue?: string;
    defaultInputValue?: string;
    keys?: import("./type").SelectInputKeys;
    label?: string | import("..").TNode;
    loading?: boolean;
    minCollapsedNum?: number;
    multiple?: boolean;
    panel?: string | import("..").TNode;
    placeholder?: string;
    popupProps?: import("..").PopupProps;
    popupVisible?: boolean;
    defaultPopupVisible?: boolean;
    prefixIcon?: import("..").TNode;
    readonly?: boolean;
    reserveKeyword?: boolean;
    size?: import("..").SizeEnum;
    status?: "default" | "success" | "warning" | "error";
    suffix?: string | import("..").TNode;
    suffixIcon?: import("..").TNode;
    tag?: string | import("..").TNode<{
        value: string | number;
    }>;
    tagInputProps?: import("..").TagInputProps;
    tagProps?: import("..").TagProps;
    tips?: string | import("..").TNode;
    value?: import("./type").SelectInputValue;
    valueDisplay?: string | import("..").TNode<{
        value: import("..").TagInputValue;
        onClose: (index: number, item?: any) => void;
    }>;
    onBlur?: (value: import("./type").SelectInputValue, context: import("./type").SelectInputBlurContext) => void;
    onClear?: (context: {
        e: MouseEvent;
    }) => void;
    onEnter?: (value: import("./type").SelectInputValue, context: {
        e: KeyboardEvent;
        inputValue: string;
        tagInputValue?: import("..").TagInputValue;
    }) => void;
    onFocus?: (value: import("./type").SelectInputValue, context: import("./type").SelectInputFocusContext) => void;
    onInputChange?: (value: string, context?: import("./type").SelectInputValueChangeContext) => void;
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
    onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
    onTagChange?: (value: import("..").TagInputValue, context: import("./type").SelectInputChangeContext) => void;
    valueDisplayOptions?: import("./hooks").SelectInputValueDisplayOptions;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    loading: boolean;
    value: string | number | boolean | Object | any[] | Date | import("./type").SelectInputValue[];
    multiple: boolean;
    size: import("..").SizeEnum;
    status: "default" | "error" | "success" | "warning";
    disabled: boolean;
    placeholder: string;
    autofocus: boolean;
    readonly: boolean;
    autoWidth: boolean;
    borderless: boolean;
    clearable: boolean;
    inputValue: string;
    minCollapsedNum: number;
    allowInput: boolean;
    popupVisible: boolean;
    defaultPopupVisible: boolean;
    reserveKeyword: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default SelectInput;
