import type { PopupProps, PopupVisibleChangeContext } from '../popup';
import type { SelectInputValueChangeContext } from '../select-input';
import type { SelectValue, TdOptionProps } from './type';
import { SelectInputValueDisplayOptions } from '../select-input/hooks/useSingle';
declare const _default: import("vue").DefineComponent<{
    autoWidth?: boolean;
    autofocus?: boolean;
    borderless?: boolean;
    clearable?: boolean;
    collapsedItems?: (h: typeof import("vue").h, props: {
        value: import("./type").SelectOption[];
        collapsedSelectedItems: import("./type").SelectOption[];
        count: number;
        onClose: (context: {
            index: number;
            e?: MouseEvent;
        }) => void;
    }) => import("..").TNodeReturnValue;
    creatable?: boolean;
    disabled?: boolean;
    empty?: string | import("..").TNode;
    filter?: (filterWords: string, option: import("./type").SelectOption) => boolean | Promise<boolean>;
    filterable?: boolean;
    inputProps?: import("..").InputProps;
    inputValue?: string;
    defaultInputValue?: string;
    keys?: import("..").KeysType;
    label?: string | import("..").TNode;
    loading?: boolean;
    loadingText?: string | import("..").TNode;
    max?: number;
    minCollapsedNum?: number;
    multiple?: boolean;
    options?: import("./type").SelectOption[];
    panelBottomContent?: string | import("..").TNode;
    panelTopContent?: string | import("..").TNode;
    placeholder?: string;
    popupProps?: PopupProps;
    popupVisible?: boolean;
    defaultPopupVisible?: boolean;
    prefixIcon?: import("..").TNode;
    readonly?: boolean;
    reserveKeyword?: boolean;
    scroll?: import("..").InfinityScroll;
    selectInputProps?: import("..").SelectInputProps;
    showArrow?: boolean;
    size?: import("..").SizeEnum;
    status?: "default" | "success" | "warning" | "error";
    suffix?: string | import("..").TNode;
    suffixIcon?: import("..").TNode;
    tagInputProps?: import("..").TagInputProps;
    tagProps?: import("..").TagProps;
    tips?: string | import("..").TNode;
    value?: SelectValue;
    defaultValue?: SelectValue;
    modelValue?: SelectValue;
    valueDisplay?: string | import("..").TNode<{
        value: SelectValue;
        onClose: (index: number) => void;
        displayValue?: SelectValue;
    } | SelectValue>;
    valueType?: "value" | "object";
    onBlur?: (context: {
        value: SelectValue;
        e: FocusEvent | KeyboardEvent;
    }) => void;
    onChange?: (value: SelectValue, context: {
        option?: import("./type").SelectOption;
        selectedOptions: import("./type").SelectOption[];
        trigger: import("./type").SelectValueChangeTrigger;
        e?: MouseEvent | KeyboardEvent;
    }) => void;
    onClear?: (context: {
        e: MouseEvent;
    }) => void;
    onCreate?: (value: string | number | boolean | bigint) => void;
    onEnter?: (context: {
        inputValue: string;
        e: KeyboardEvent;
        value: SelectValue;
    }) => void;
    onFocus?: (context: {
        value: SelectValue;
        e: FocusEvent | KeyboardEvent;
    }) => void;
    onInputChange?: (value: string, context?: SelectInputValueChangeContext) => void;
    onPopupVisibleChange?: (visible: boolean, context: PopupVisibleChangeContext) => void;
    onRemove?: (options: import("./type").SelectRemoveContext<import("./type").SelectOption>) => void;
    onSearch?: (filterWords: string, context: {
        e: KeyboardEvent;
    }) => void;
    valueDisplayOptions?: SelectInputValueDisplayOptions;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    autoWidth?: boolean;
    autofocus?: boolean;
    borderless?: boolean;
    clearable?: boolean;
    collapsedItems?: (h: typeof import("vue").h, props: {
        value: import("./type").SelectOption[];
        collapsedSelectedItems: import("./type").SelectOption[];
        count: number;
        onClose: (context: {
            index: number;
            e?: MouseEvent;
        }) => void;
    }) => import("..").TNodeReturnValue;
    creatable?: boolean;
    disabled?: boolean;
    empty?: string | import("..").TNode;
    filter?: (filterWords: string, option: import("./type").SelectOption) => boolean | Promise<boolean>;
    filterable?: boolean;
    inputProps?: import("..").InputProps;
    inputValue?: string;
    defaultInputValue?: string;
    keys?: import("..").KeysType;
    label?: string | import("..").TNode;
    loading?: boolean;
    loadingText?: string | import("..").TNode;
    max?: number;
    minCollapsedNum?: number;
    multiple?: boolean;
    options?: import("./type").SelectOption[];
    panelBottomContent?: string | import("..").TNode;
    panelTopContent?: string | import("..").TNode;
    placeholder?: string;
    popupProps?: PopupProps;
    popupVisible?: boolean;
    defaultPopupVisible?: boolean;
    prefixIcon?: import("..").TNode;
    readonly?: boolean;
    reserveKeyword?: boolean;
    scroll?: import("..").InfinityScroll;
    selectInputProps?: import("..").SelectInputProps;
    showArrow?: boolean;
    size?: import("..").SizeEnum;
    status?: "default" | "success" | "warning" | "error";
    suffix?: string | import("..").TNode;
    suffixIcon?: import("..").TNode;
    tagInputProps?: import("..").TagInputProps;
    tagProps?: import("..").TagProps;
    tips?: string | import("..").TNode;
    value?: SelectValue;
    defaultValue?: SelectValue;
    modelValue?: SelectValue;
    valueDisplay?: string | import("..").TNode<{
        value: SelectValue;
        onClose: (index: number) => void;
        displayValue?: SelectValue;
    } | SelectValue>;
    valueType?: "value" | "object";
    onBlur?: (context: {
        value: SelectValue;
        e: FocusEvent | KeyboardEvent;
    }) => void;
    onChange?: (value: SelectValue, context: {
        option?: import("./type").SelectOption;
        selectedOptions: import("./type").SelectOption[];
        trigger: import("./type").SelectValueChangeTrigger;
        e?: MouseEvent | KeyboardEvent;
    }) => void;
    onClear?: (context: {
        e: MouseEvent;
    }) => void;
    onCreate?: (value: string | number | boolean | bigint) => void;
    onEnter?: (context: {
        inputValue: string;
        e: KeyboardEvent;
        value: SelectValue;
    }) => void;
    onFocus?: (context: {
        value: SelectValue;
        e: FocusEvent | KeyboardEvent;
    }) => void;
    onInputChange?: (value: string, context?: SelectInputValueChangeContext) => void;
    onPopupVisibleChange?: (visible: boolean, context: PopupVisibleChangeContext) => void;
    onRemove?: (options: import("./type").SelectRemoveContext<import("./type").SelectOption>) => void;
    onSearch?: (filterWords: string, context: {
        e: KeyboardEvent;
    }) => void;
    valueDisplayOptions?: SelectInputValueDisplayOptions;
}> & Readonly<{}>, {
    loading: boolean;
    value: string | number | bigint | boolean | import("..").PlainObject | TdOptionProps | import("./type").SelectOptionGroup | SelectValue<import("./type").SelectOption>[];
    valueType: "object" | "value";
    multiple: boolean;
    max: number;
    size: import("..").SizeEnum;
    status: "default" | "error" | "success" | "warning";
    disabled: boolean;
    defaultValue: any;
    placeholder: string;
    autofocus: boolean;
    modelValue: string | number | bigint | boolean | import("..").PlainObject | TdOptionProps | import("./type").SelectOptionGroup | SelectValue<import("./type").SelectOption>[];
    readonly: boolean;
    showArrow: boolean;
    autoWidth: boolean;
    borderless: boolean;
    clearable: boolean;
    inputValue: string;
    minCollapsedNum: number;
    popupVisible: boolean;
    defaultPopupVisible: boolean;
    reserveKeyword: boolean;
    creatable: boolean;
    filterable: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
