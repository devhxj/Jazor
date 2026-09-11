import './style';
export * from './types';
export declare const TreeSelect: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        autoWidth?: boolean;
        borderless?: boolean;
        clearable?: boolean;
        collapsedItems?: (h: typeof import("vue").h, props: {
            value: import("..").TreeOptionData[];
            collapsedSelectedItems: import("..").TreeOptionData[];
            count: number;
            onClose: (context: {
                index: number;
                e?: MouseEvent;
            }) => void;
        }) => import("..").TNodeReturnValue;
        data?: import("..").TreeOptionData[];
        disabled?: boolean;
        empty?: string | import("..").TNode;
        filter?: (filterWords: string, option: import("..").TreeOptionData) => boolean;
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
        value?: import("./type").TreeSelectValue;
        defaultValue?: import("./type").TreeSelectValue;
        modelValue?: import("./type").TreeSelectValue;
        valueDisplay?: (h: typeof import("vue").h, props: {
            value: import("..").TreeOptionData[];
            onClose: () => void;
        }) => import("..").TNodeReturnValue;
        valueType?: "value" | "object";
        onBlur?: (context: {
            value: import("./type").TreeSelectValue;
            e: FocusEvent;
        }) => void;
        onChange?: (value: import("./type").TreeSelectValue, context: import("./type").TreeSelectChangeContext<import("..").TreeOptionData>) => void;
        onClear?: (context: {
            e: MouseEvent;
        }) => void;
        onFocus?: (context: {
            value: import("./type").TreeSelectValue;
            e: FocusEvent;
        }) => void;
        onInputChange?: (value: string, context?: import("..").SelectInputValueChangeContext) => void;
        onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
        onRemove?: (options: import("./type").RemoveOptions<import("..").TreeOptionData>) => void;
        onSearch?: (filterWords: string) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        loading: boolean;
        value: import("./type").TreeSelectValue;
        valueType: "object" | "value";
        multiple: boolean;
        max: number;
        size: "small" | "medium" | "large";
        data: import("..").TreeOptionData[];
        disabled: boolean;
        placeholder: string;
        modelValue: import("./type").TreeSelectValue;
        readonly: boolean;
        autoWidth: boolean;
        borderless: boolean;
        clearable: boolean;
        inputValue: string;
        minCollapsedNum: number;
        popupVisible: boolean;
        loadingText: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        filterable: boolean;
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
        collapsedItems?: (h: typeof import("vue").h, props: {
            value: import("..").TreeOptionData[];
            collapsedSelectedItems: import("..").TreeOptionData[];
            count: number;
            onClose: (context: {
                index: number;
                e?: MouseEvent;
            }) => void;
        }) => import("..").TNodeReturnValue;
        data?: import("..").TreeOptionData[];
        disabled?: boolean;
        empty?: string | import("..").TNode;
        filter?: (filterWords: string, option: import("..").TreeOptionData) => boolean;
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
        value?: import("./type").TreeSelectValue;
        defaultValue?: import("./type").TreeSelectValue;
        modelValue?: import("./type").TreeSelectValue;
        valueDisplay?: (h: typeof import("vue").h, props: {
            value: import("..").TreeOptionData[];
            onClose: () => void;
        }) => import("..").TNodeReturnValue;
        valueType?: "value" | "object";
        onBlur?: (context: {
            value: import("./type").TreeSelectValue;
            e: FocusEvent;
        }) => void;
        onChange?: (value: import("./type").TreeSelectValue, context: import("./type").TreeSelectChangeContext<import("..").TreeOptionData>) => void;
        onClear?: (context: {
            e: MouseEvent;
        }) => void;
        onFocus?: (context: {
            value: import("./type").TreeSelectValue;
            e: FocusEvent;
        }) => void;
        onInputChange?: (value: string, context?: import("..").SelectInputValueChangeContext) => void;
        onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
        onRemove?: (options: import("./type").RemoveOptions<import("..").TreeOptionData>) => void;
        onSearch?: (filterWords: string) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        loading: boolean;
        value: import("./type").TreeSelectValue;
        valueType: "object" | "value";
        multiple: boolean;
        max: number;
        size: "small" | "medium" | "large";
        data: import("..").TreeOptionData[];
        disabled: boolean;
        placeholder: string;
        modelValue: import("./type").TreeSelectValue;
        readonly: boolean;
        autoWidth: boolean;
        borderless: boolean;
        clearable: boolean;
        inputValue: string;
        minCollapsedNum: number;
        popupVisible: boolean;
        loadingText: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        filterable: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    autoWidth?: boolean;
    borderless?: boolean;
    clearable?: boolean;
    collapsedItems?: (h: typeof import("vue").h, props: {
        value: import("..").TreeOptionData[];
        collapsedSelectedItems: import("..").TreeOptionData[];
        count: number;
        onClose: (context: {
            index: number;
            e?: MouseEvent;
        }) => void;
    }) => import("..").TNodeReturnValue;
    data?: import("..").TreeOptionData[];
    disabled?: boolean;
    empty?: string | import("..").TNode;
    filter?: (filterWords: string, option: import("..").TreeOptionData) => boolean;
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
    value?: import("./type").TreeSelectValue;
    defaultValue?: import("./type").TreeSelectValue;
    modelValue?: import("./type").TreeSelectValue;
    valueDisplay?: (h: typeof import("vue").h, props: {
        value: import("..").TreeOptionData[];
        onClose: () => void;
    }) => import("..").TNodeReturnValue;
    valueType?: "value" | "object";
    onBlur?: (context: {
        value: import("./type").TreeSelectValue;
        e: FocusEvent;
    }) => void;
    onChange?: (value: import("./type").TreeSelectValue, context: import("./type").TreeSelectChangeContext<import("..").TreeOptionData>) => void;
    onClear?: (context: {
        e: MouseEvent;
    }) => void;
    onFocus?: (context: {
        value: import("./type").TreeSelectValue;
        e: FocusEvent;
    }) => void;
    onInputChange?: (value: string, context?: import("..").SelectInputValueChangeContext) => void;
    onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
    onRemove?: (options: import("./type").RemoveOptions<import("..").TreeOptionData>) => void;
    onSearch?: (filterWords: string) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    loading: boolean;
    value: import("./type").TreeSelectValue;
    valueType: "object" | "value";
    multiple: boolean;
    max: number;
    size: "small" | "medium" | "large";
    data: import("..").TreeOptionData[];
    disabled: boolean;
    placeholder: string;
    modelValue: import("./type").TreeSelectValue;
    readonly: boolean;
    autoWidth: boolean;
    borderless: boolean;
    clearable: boolean;
    inputValue: string;
    minCollapsedNum: number;
    popupVisible: boolean;
    loadingText: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    filterable: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default TreeSelect;
