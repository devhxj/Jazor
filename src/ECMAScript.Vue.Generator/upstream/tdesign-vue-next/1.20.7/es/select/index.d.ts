import { TdSelectProps, TdOptionProps, TdOptionGroupProps, SelectOption } from './type';
import './style';
export * from './type';
export type SelectProps<T = SelectOption> = TdSelectProps<T>;
export type OptionProps = TdOptionProps;
export type OptionGroupProps = TdOptionGroupProps;
export declare const Select: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        autoWidth?: boolean;
        autofocus?: boolean;
        borderless?: boolean;
        clearable?: boolean;
        collapsedItems?: (h: typeof import("vue").h, props: {
            value: SelectOption[];
            collapsedSelectedItems: SelectOption[];
            count: number;
            onClose: (context: {
                index: number;
                e?: MouseEvent;
            }) => void;
        }) => import("..").TNodeReturnValue;
        creatable?: boolean;
        disabled?: boolean;
        empty?: string | import("..").TNode;
        filter?: (filterWords: string, option: SelectOption) => boolean | Promise<boolean>;
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
        options?: SelectOption[];
        panelBottomContent?: string | import("..").TNode;
        panelTopContent?: string | import("..").TNode;
        placeholder?: string;
        popupProps?: import("..").PopupProps;
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
        value?: import("./type").SelectValue;
        defaultValue?: import("./type").SelectValue;
        modelValue?: import("./type").SelectValue;
        valueDisplay?: string | import("..").TNode<{
            value: import("./type").SelectValue;
            onClose: (index: number) => void;
            displayValue?: import("./type").SelectValue;
        } | import("./type").SelectValue>;
        valueType?: "value" | "object";
        onBlur?: (context: {
            value: import("./type").SelectValue;
            e: FocusEvent | KeyboardEvent;
        }) => void;
        onChange?: (value: import("./type").SelectValue, context: {
            option?: SelectOption;
            selectedOptions: SelectOption[];
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
            value: import("./type").SelectValue;
        }) => void;
        onFocus?: (context: {
            value: import("./type").SelectValue;
            e: FocusEvent | KeyboardEvent;
        }) => void;
        onInputChange?: (value: string, context?: import("..").SelectInputValueChangeContext) => void;
        onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
        onRemove?: (options: import("./type").SelectRemoveContext<SelectOption>) => void;
        onSearch?: (filterWords: string, context: {
            e: KeyboardEvent;
        }) => void;
        valueDisplayOptions?: import("../select-input/hooks").SelectInputValueDisplayOptions;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        loading: boolean;
        value: string | number | bigint | boolean | import("..").PlainObject | TdOptionProps | import("./type").SelectOptionGroup | import("./type").SelectValue<SelectOption>[];
        valueType: "object" | "value";
        multiple: boolean;
        max: number;
        size: import("..").SizeEnum;
        status: "default" | "error" | "success" | "warning";
        disabled: boolean;
        defaultValue: any;
        placeholder: string;
        autofocus: boolean;
        modelValue: string | number | bigint | boolean | import("..").PlainObject | TdOptionProps | import("./type").SelectOptionGroup | import("./type").SelectValue<SelectOption>[];
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
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        autoWidth?: boolean;
        autofocus?: boolean;
        borderless?: boolean;
        clearable?: boolean;
        collapsedItems?: (h: typeof import("vue").h, props: {
            value: SelectOption[];
            collapsedSelectedItems: SelectOption[];
            count: number;
            onClose: (context: {
                index: number;
                e?: MouseEvent;
            }) => void;
        }) => import("..").TNodeReturnValue;
        creatable?: boolean;
        disabled?: boolean;
        empty?: string | import("..").TNode;
        filter?: (filterWords: string, option: SelectOption) => boolean | Promise<boolean>;
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
        options?: SelectOption[];
        panelBottomContent?: string | import("..").TNode;
        panelTopContent?: string | import("..").TNode;
        placeholder?: string;
        popupProps?: import("..").PopupProps;
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
        value?: import("./type").SelectValue;
        defaultValue?: import("./type").SelectValue;
        modelValue?: import("./type").SelectValue;
        valueDisplay?: string | import("..").TNode<{
            value: import("./type").SelectValue;
            onClose: (index: number) => void;
            displayValue?: import("./type").SelectValue;
        } | import("./type").SelectValue>;
        valueType?: "value" | "object";
        onBlur?: (context: {
            value: import("./type").SelectValue;
            e: FocusEvent | KeyboardEvent;
        }) => void;
        onChange?: (value: import("./type").SelectValue, context: {
            option?: SelectOption;
            selectedOptions: SelectOption[];
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
            value: import("./type").SelectValue;
        }) => void;
        onFocus?: (context: {
            value: import("./type").SelectValue;
            e: FocusEvent | KeyboardEvent;
        }) => void;
        onInputChange?: (value: string, context?: import("..").SelectInputValueChangeContext) => void;
        onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
        onRemove?: (options: import("./type").SelectRemoveContext<SelectOption>) => void;
        onSearch?: (filterWords: string, context: {
            e: KeyboardEvent;
        }) => void;
        valueDisplayOptions?: import("../select-input/hooks").SelectInputValueDisplayOptions;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        loading: boolean;
        value: string | number | bigint | boolean | import("..").PlainObject | TdOptionProps | import("./type").SelectOptionGroup | import("./type").SelectValue<SelectOption>[];
        valueType: "object" | "value";
        multiple: boolean;
        max: number;
        size: import("..").SizeEnum;
        status: "default" | "error" | "success" | "warning";
        disabled: boolean;
        defaultValue: any;
        placeholder: string;
        autofocus: boolean;
        modelValue: string | number | bigint | boolean | import("..").PlainObject | TdOptionProps | import("./type").SelectOptionGroup | import("./type").SelectValue<SelectOption>[];
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
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    autoWidth?: boolean;
    autofocus?: boolean;
    borderless?: boolean;
    clearable?: boolean;
    collapsedItems?: (h: typeof import("vue").h, props: {
        value: SelectOption[];
        collapsedSelectedItems: SelectOption[];
        count: number;
        onClose: (context: {
            index: number;
            e?: MouseEvent;
        }) => void;
    }) => import("..").TNodeReturnValue;
    creatable?: boolean;
    disabled?: boolean;
    empty?: string | import("..").TNode;
    filter?: (filterWords: string, option: SelectOption) => boolean | Promise<boolean>;
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
    options?: SelectOption[];
    panelBottomContent?: string | import("..").TNode;
    panelTopContent?: string | import("..").TNode;
    placeholder?: string;
    popupProps?: import("..").PopupProps;
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
    value?: import("./type").SelectValue;
    defaultValue?: import("./type").SelectValue;
    modelValue?: import("./type").SelectValue;
    valueDisplay?: string | import("..").TNode<{
        value: import("./type").SelectValue;
        onClose: (index: number) => void;
        displayValue?: import("./type").SelectValue;
    } | import("./type").SelectValue>;
    valueType?: "value" | "object";
    onBlur?: (context: {
        value: import("./type").SelectValue;
        e: FocusEvent | KeyboardEvent;
    }) => void;
    onChange?: (value: import("./type").SelectValue, context: {
        option?: SelectOption;
        selectedOptions: SelectOption[];
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
        value: import("./type").SelectValue;
    }) => void;
    onFocus?: (context: {
        value: import("./type").SelectValue;
        e: FocusEvent | KeyboardEvent;
    }) => void;
    onInputChange?: (value: string, context?: import("..").SelectInputValueChangeContext) => void;
    onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
    onRemove?: (options: import("./type").SelectRemoveContext<SelectOption>) => void;
    onSearch?: (filterWords: string, context: {
        e: KeyboardEvent;
    }) => void;
    valueDisplayOptions?: import("../select-input/hooks").SelectInputValueDisplayOptions;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    loading: boolean;
    value: string | number | bigint | boolean | import("..").PlainObject | TdOptionProps | import("./type").SelectOptionGroup | import("./type").SelectValue<SelectOption>[];
    valueType: "object" | "value";
    multiple: boolean;
    max: number;
    size: import("..").SizeEnum;
    status: "default" | "error" | "success" | "warning";
    disabled: boolean;
    defaultValue: any;
    placeholder: string;
    autofocus: boolean;
    modelValue: string | number | bigint | boolean | import("..").PlainObject | TdOptionProps | import("./type").SelectOptionGroup | import("./type").SelectValue<SelectOption>[];
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
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const Option: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        createAble: BooleanConstructor;
        multiple: BooleanConstructor;
        index: NumberConstructor;
        rowIndex: NumberConstructor;
        trs: MapConstructor;
        scrollType: StringConstructor;
        isVirtual: BooleanConstructor;
        bufferSize: NumberConstructor;
        checkAll: BooleanConstructor;
        onRowMounted: FunctionConstructor;
        content: {
            type: import("vue").PropType<TdOptionProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdOptionProps["default"]>;
        };
        disabled: BooleanConstructor;
        label: {
            type: StringConstructor;
            default: string;
        };
        title: {
            type: StringConstructor;
            default: string;
        };
        value: {
            type: import("vue").PropType<TdOptionProps["value"]>;
        };
    }>> & Readonly<{
        "onRow-mounted"?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "row-mounted"[], import("vue").PublicProps, {
        multiple: boolean;
        label: string;
        title: string;
        disabled: boolean;
        checkAll: boolean;
        createAble: boolean;
        isVirtual: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        createAble: BooleanConstructor;
        multiple: BooleanConstructor;
        index: NumberConstructor;
        rowIndex: NumberConstructor;
        trs: MapConstructor;
        scrollType: StringConstructor;
        isVirtual: BooleanConstructor;
        bufferSize: NumberConstructor;
        checkAll: BooleanConstructor;
        onRowMounted: FunctionConstructor;
        content: {
            type: import("vue").PropType<TdOptionProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdOptionProps["default"]>;
        };
        disabled: BooleanConstructor;
        label: {
            type: StringConstructor;
            default: string;
        };
        title: {
            type: StringConstructor;
            default: string;
        };
        value: {
            type: import("vue").PropType<TdOptionProps["value"]>;
        };
    }>> & Readonly<{
        "onRow-mounted"?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        multiple: boolean;
        label: string;
        title: string;
        disabled: boolean;
        checkAll: boolean;
        createAble: boolean;
        isVirtual: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    createAble: BooleanConstructor;
    multiple: BooleanConstructor;
    index: NumberConstructor;
    rowIndex: NumberConstructor;
    trs: MapConstructor;
    scrollType: StringConstructor;
    isVirtual: BooleanConstructor;
    bufferSize: NumberConstructor;
    checkAll: BooleanConstructor;
    onRowMounted: FunctionConstructor;
    content: {
        type: import("vue").PropType<TdOptionProps["content"]>;
    };
    default: {
        type: import("vue").PropType<TdOptionProps["default"]>;
    };
    disabled: BooleanConstructor;
    label: {
        type: StringConstructor;
        default: string;
    };
    title: {
        type: StringConstructor;
        default: string;
    };
    value: {
        type: import("vue").PropType<TdOptionProps["value"]>;
    };
}>> & Readonly<{
    "onRow-mounted"?: (...args: any[]) => any;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "row-mounted"[], "row-mounted", {
    multiple: boolean;
    label: string;
    title: string;
    disabled: boolean;
    checkAll: boolean;
    createAble: boolean;
    isVirtual: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const OptionGroup: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        divider: {
            type: BooleanConstructor;
            default: boolean;
        };
        label: {
            type: StringConstructor;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        divider: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        divider: {
            type: BooleanConstructor;
            default: boolean;
        };
        label: {
            type: StringConstructor;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        divider: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    divider: {
        type: BooleanConstructor;
        default: boolean;
    };
    label: {
        type: StringConstructor;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    divider: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Select;
