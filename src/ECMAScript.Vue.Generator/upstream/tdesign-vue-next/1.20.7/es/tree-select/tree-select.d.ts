import { PopupVisibleChangeContext } from '../popup';
import { TreeSelectValue } from './type';
import { TreeOptionData } from '../common';
declare const _default: import("vue").DefineComponent<{
    autoWidth?: boolean;
    borderless?: boolean;
    clearable?: boolean;
    collapsedItems?: (h: typeof import("vue").h, props: {
        value: TreeOptionData[];
        collapsedSelectedItems: TreeOptionData[];
        count: number;
        onClose: (context: {
            index: number;
            e?: MouseEvent;
        }) => void;
    }) => import("..").TNodeReturnValue;
    data?: TreeOptionData[];
    disabled?: boolean;
    empty?: string | import("..").TNode;
    filter?: (filterWords: string, option: TreeOptionData) => boolean;
    filterable?: boolean;
    inputProps?: import("..").InputProps;
    inputValue?: string;
    defaultInputValue?: string;
    keys?: import("..").TreeKeysType;
    loading?: boolean;
    loadingText?: string | import("..").TNode;
    max?: number;
    minCollapsedNum?: number;
    multiple?: boolean;
    panelBottomContent?: string | import("..").TNode;
    panelTopContent?: string | import("..").TNode;
    placeholder?: string;
    popupProps?: import("..").PopupProps;
    popupVisible?: boolean;
    prefixIcon?: import("..").TNode;
    readonly?: boolean;
    selectInputProps?: import("..").SelectInputProps;
    size?: "small" | "medium" | "large";
    suffix?: string | import("..").TNode;
    suffixIcon?: import("..").TNode;
    tagProps?: import("..").TagProps;
    treeProps?: import("..").TdTreeProps<import("../tree/utils/adapt").TypeTreeOptionData> & {
        treeStore?: import("@common/js/tree/tree-store").TreeStore;
    };
    value?: TreeSelectValue;
    defaultValue?: TreeSelectValue;
    modelValue?: TreeSelectValue;
    valueDisplay?: (h: typeof import("vue").h, props: {
        value: TreeOptionData[];
        onClose: () => void;
    }) => import("..").TNodeReturnValue;
    valueType?: "value" | "object";
    onBlur?: (context: {
        value: TreeSelectValue;
        e: FocusEvent;
    }) => void;
    onChange?: (value: TreeSelectValue, context: import("./type").TreeSelectChangeContext<TreeOptionData>) => void;
    onClear?: (context: {
        e: MouseEvent;
    }) => void;
    onFocus?: (context: {
        value: TreeSelectValue;
        e: FocusEvent;
    }) => void;
    onInputChange?: (value: string, context?: import("..").SelectInputValueChangeContext) => void;
    onPopupVisibleChange?: (visible: boolean, context: PopupVisibleChangeContext) => void;
    onRemove?: (options: import("./type").RemoveOptions<TreeOptionData>) => void;
    onSearch?: (filterWords: string) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    autoWidth?: boolean;
    borderless?: boolean;
    clearable?: boolean;
    collapsedItems?: (h: typeof import("vue").h, props: {
        value: TreeOptionData[];
        collapsedSelectedItems: TreeOptionData[];
        count: number;
        onClose: (context: {
            index: number;
            e?: MouseEvent;
        }) => void;
    }) => import("..").TNodeReturnValue;
    data?: TreeOptionData[];
    disabled?: boolean;
    empty?: string | import("..").TNode;
    filter?: (filterWords: string, option: TreeOptionData) => boolean;
    filterable?: boolean;
    inputProps?: import("..").InputProps;
    inputValue?: string;
    defaultInputValue?: string;
    keys?: import("..").TreeKeysType;
    loading?: boolean;
    loadingText?: string | import("..").TNode;
    max?: number;
    minCollapsedNum?: number;
    multiple?: boolean;
    panelBottomContent?: string | import("..").TNode;
    panelTopContent?: string | import("..").TNode;
    placeholder?: string;
    popupProps?: import("..").PopupProps;
    popupVisible?: boolean;
    prefixIcon?: import("..").TNode;
    readonly?: boolean;
    selectInputProps?: import("..").SelectInputProps;
    size?: "small" | "medium" | "large";
    suffix?: string | import("..").TNode;
    suffixIcon?: import("..").TNode;
    tagProps?: import("..").TagProps;
    treeProps?: import("..").TdTreeProps<import("../tree/utils/adapt").TypeTreeOptionData> & {
        treeStore?: import("@common/js/tree/tree-store").TreeStore;
    };
    value?: TreeSelectValue;
    defaultValue?: TreeSelectValue;
    modelValue?: TreeSelectValue;
    valueDisplay?: (h: typeof import("vue").h, props: {
        value: TreeOptionData[];
        onClose: () => void;
    }) => import("..").TNodeReturnValue;
    valueType?: "value" | "object";
    onBlur?: (context: {
        value: TreeSelectValue;
        e: FocusEvent;
    }) => void;
    onChange?: (value: TreeSelectValue, context: import("./type").TreeSelectChangeContext<TreeOptionData>) => void;
    onClear?: (context: {
        e: MouseEvent;
    }) => void;
    onFocus?: (context: {
        value: TreeSelectValue;
        e: FocusEvent;
    }) => void;
    onInputChange?: (value: string, context?: import("..").SelectInputValueChangeContext) => void;
    onPopupVisibleChange?: (visible: boolean, context: PopupVisibleChangeContext) => void;
    onRemove?: (options: import("./type").RemoveOptions<TreeOptionData>) => void;
    onSearch?: (filterWords: string) => void;
}> & Readonly<{}>, {
    empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    loading: boolean;
    value: TreeSelectValue;
    valueType: "object" | "value";
    multiple: boolean;
    max: number;
    size: "small" | "medium" | "large";
    data: TreeOptionData[];
    disabled: boolean;
    placeholder: string;
    modelValue: TreeSelectValue;
    readonly: boolean;
    autoWidth: boolean;
    borderless: boolean;
    clearable: boolean;
    inputValue: string;
    minCollapsedNum: number;
    popupVisible: boolean;
    loadingText: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    filterable: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
