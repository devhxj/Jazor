import { TdRangeInputProps, TdRangeInputPopupProps } from './type';
import './style';
export * from './type';
export type RangeInputProps = TdRangeInputProps;
export type RangeInputPopupProps = TdRangeInputPopupProps;
export declare const RangeInput: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        activeIndex: {
            type: NumberConstructor;
        };
        borderless: BooleanConstructor;
        clearable: BooleanConstructor;
        disabled: {
            type: import("vue").PropType<TdRangeInputProps["disabled"]>;
            default: any;
        };
        format: {
            type: import("vue").PropType<TdRangeInputProps["format"]>;
        };
        inputProps: {
            type: import("vue").PropType<TdRangeInputProps["inputProps"]>;
        };
        label: {
            type: import("vue").PropType<TdRangeInputProps["label"]>;
        };
        placeholder: {
            type: import("vue").PropType<TdRangeInputProps["placeholder"]>;
        };
        prefixIcon: {
            type: import("vue").PropType<TdRangeInputProps["prefixIcon"]>;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        separator: {
            type: import("vue").PropType<TdRangeInputProps["separator"]>;
            default: TdRangeInputProps["separator"];
        };
        showClearIconOnEmpty: BooleanConstructor;
        size: {
            type: import("vue").PropType<TdRangeInputProps["size"]>;
            default: TdRangeInputProps["size"];
            validator(val: TdRangeInputProps["size"]): boolean;
        };
        status: {
            type: import("vue").PropType<TdRangeInputProps["status"]>;
            default: TdRangeInputProps["status"];
            validator(val: TdRangeInputProps["status"]): boolean;
        };
        suffix: {
            type: import("vue").PropType<TdRangeInputProps["suffix"]>;
        };
        suffixIcon: {
            type: import("vue").PropType<TdRangeInputProps["suffixIcon"]>;
        };
        tips: {
            type: import("vue").PropType<TdRangeInputProps["tips"]>;
        };
        value: {
            type: import("vue").PropType<TdRangeInputProps["value"]>;
            default: TdRangeInputProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdRangeInputProps["value"]>;
            default: TdRangeInputProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdRangeInputProps["defaultValue"]>;
            default: () => TdRangeInputProps["defaultValue"];
        };
        onBlur: import("vue").PropType<TdRangeInputProps["onBlur"]>;
        onChange: import("vue").PropType<TdRangeInputProps["onChange"]>;
        onClear: import("vue").PropType<TdRangeInputProps["onClear"]>;
        onClick: import("vue").PropType<TdRangeInputProps["onClick"]>;
        onEnter: import("vue").PropType<TdRangeInputProps["onEnter"]>;
        onFocus: import("vue").PropType<TdRangeInputProps["onFocus"]>;
        onMouseenter: import("vue").PropType<TdRangeInputProps["onMouseenter"]>;
        onMouseleave: import("vue").PropType<TdRangeInputProps["onMouseleave"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").RangeInputValue;
        size: "small" | "medium" | "large";
        status: "default" | "error" | "success" | "warning";
        disabled: boolean | boolean[];
        defaultValue: import("./type").RangeInputValue;
        modelValue: import("./type").RangeInputValue;
        readonly: boolean;
        separator: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        borderless: boolean;
        clearable: boolean;
        showClearIconOnEmpty: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        activeIndex: {
            type: NumberConstructor;
        };
        borderless: BooleanConstructor;
        clearable: BooleanConstructor;
        disabled: {
            type: import("vue").PropType<TdRangeInputProps["disabled"]>;
            default: any;
        };
        format: {
            type: import("vue").PropType<TdRangeInputProps["format"]>;
        };
        inputProps: {
            type: import("vue").PropType<TdRangeInputProps["inputProps"]>;
        };
        label: {
            type: import("vue").PropType<TdRangeInputProps["label"]>;
        };
        placeholder: {
            type: import("vue").PropType<TdRangeInputProps["placeholder"]>;
        };
        prefixIcon: {
            type: import("vue").PropType<TdRangeInputProps["prefixIcon"]>;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        separator: {
            type: import("vue").PropType<TdRangeInputProps["separator"]>;
            default: TdRangeInputProps["separator"];
        };
        showClearIconOnEmpty: BooleanConstructor;
        size: {
            type: import("vue").PropType<TdRangeInputProps["size"]>;
            default: TdRangeInputProps["size"];
            validator(val: TdRangeInputProps["size"]): boolean;
        };
        status: {
            type: import("vue").PropType<TdRangeInputProps["status"]>;
            default: TdRangeInputProps["status"];
            validator(val: TdRangeInputProps["status"]): boolean;
        };
        suffix: {
            type: import("vue").PropType<TdRangeInputProps["suffix"]>;
        };
        suffixIcon: {
            type: import("vue").PropType<TdRangeInputProps["suffixIcon"]>;
        };
        tips: {
            type: import("vue").PropType<TdRangeInputProps["tips"]>;
        };
        value: {
            type: import("vue").PropType<TdRangeInputProps["value"]>;
            default: TdRangeInputProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdRangeInputProps["value"]>;
            default: TdRangeInputProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdRangeInputProps["defaultValue"]>;
            default: () => TdRangeInputProps["defaultValue"];
        };
        onBlur: import("vue").PropType<TdRangeInputProps["onBlur"]>;
        onChange: import("vue").PropType<TdRangeInputProps["onChange"]>;
        onClear: import("vue").PropType<TdRangeInputProps["onClear"]>;
        onClick: import("vue").PropType<TdRangeInputProps["onClick"]>;
        onEnter: import("vue").PropType<TdRangeInputProps["onEnter"]>;
        onFocus: import("vue").PropType<TdRangeInputProps["onFocus"]>;
        onMouseenter: import("vue").PropType<TdRangeInputProps["onMouseenter"]>;
        onMouseleave: import("vue").PropType<TdRangeInputProps["onMouseleave"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").RangeInputValue;
        size: "small" | "medium" | "large";
        status: "default" | "error" | "success" | "warning";
        disabled: boolean | boolean[];
        defaultValue: import("./type").RangeInputValue;
        modelValue: import("./type").RangeInputValue;
        readonly: boolean;
        separator: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        borderless: boolean;
        clearable: boolean;
        showClearIconOnEmpty: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    activeIndex: {
        type: NumberConstructor;
    };
    borderless: BooleanConstructor;
    clearable: BooleanConstructor;
    disabled: {
        type: import("vue").PropType<TdRangeInputProps["disabled"]>;
        default: any;
    };
    format: {
        type: import("vue").PropType<TdRangeInputProps["format"]>;
    };
    inputProps: {
        type: import("vue").PropType<TdRangeInputProps["inputProps"]>;
    };
    label: {
        type: import("vue").PropType<TdRangeInputProps["label"]>;
    };
    placeholder: {
        type: import("vue").PropType<TdRangeInputProps["placeholder"]>;
    };
    prefixIcon: {
        type: import("vue").PropType<TdRangeInputProps["prefixIcon"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    separator: {
        type: import("vue").PropType<TdRangeInputProps["separator"]>;
        default: TdRangeInputProps["separator"];
    };
    showClearIconOnEmpty: BooleanConstructor;
    size: {
        type: import("vue").PropType<TdRangeInputProps["size"]>;
        default: TdRangeInputProps["size"];
        validator(val: TdRangeInputProps["size"]): boolean;
    };
    status: {
        type: import("vue").PropType<TdRangeInputProps["status"]>;
        default: TdRangeInputProps["status"];
        validator(val: TdRangeInputProps["status"]): boolean;
    };
    suffix: {
        type: import("vue").PropType<TdRangeInputProps["suffix"]>;
    };
    suffixIcon: {
        type: import("vue").PropType<TdRangeInputProps["suffixIcon"]>;
    };
    tips: {
        type: import("vue").PropType<TdRangeInputProps["tips"]>;
    };
    value: {
        type: import("vue").PropType<TdRangeInputProps["value"]>;
        default: TdRangeInputProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdRangeInputProps["value"]>;
        default: TdRangeInputProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdRangeInputProps["defaultValue"]>;
        default: () => TdRangeInputProps["defaultValue"];
    };
    onBlur: import("vue").PropType<TdRangeInputProps["onBlur"]>;
    onChange: import("vue").PropType<TdRangeInputProps["onChange"]>;
    onClear: import("vue").PropType<TdRangeInputProps["onClear"]>;
    onClick: import("vue").PropType<TdRangeInputProps["onClick"]>;
    onEnter: import("vue").PropType<TdRangeInputProps["onEnter"]>;
    onFocus: import("vue").PropType<TdRangeInputProps["onFocus"]>;
    onMouseenter: import("vue").PropType<TdRangeInputProps["onMouseenter"]>;
    onMouseleave: import("vue").PropType<TdRangeInputProps["onMouseleave"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").RangeInputValue;
    size: "small" | "medium" | "large";
    status: "default" | "error" | "success" | "warning";
    disabled: boolean | boolean[];
    defaultValue: import("./type").RangeInputValue;
    modelValue: import("./type").RangeInputValue;
    readonly: boolean;
    separator: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    borderless: boolean;
    clearable: boolean;
    showClearIconOnEmpty: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const RangeInputPopup: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        autoWidth?: boolean;
        disabled?: boolean | Array<boolean>;
        inputValue?: import("./type").RangeInputValue;
        defaultInputValue?: import("./type").RangeInputValue;
        label?: string | import("..").TNode;
        panel?: string | import("..").TNode;
        popupProps?: import("..").PopupProps;
        popupVisible?: boolean;
        rangeInputProps?: RangeInputProps;
        readonly?: boolean;
        status?: "default" | "success" | "warning" | "error";
        tips?: string | import("..").TNode;
        onInputChange?: (value: import("./type").RangeInputValue, context?: import("./type").RangeInputValueChangeContext) => void;
        onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        status: "default" | "error" | "success" | "warning";
        disabled: boolean | boolean[];
        readonly: boolean;
        autoWidth: boolean;
        inputValue: import("./type").RangeInputValue;
        popupVisible: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        autoWidth?: boolean;
        disabled?: boolean | Array<boolean>;
        inputValue?: import("./type").RangeInputValue;
        defaultInputValue?: import("./type").RangeInputValue;
        label?: string | import("..").TNode;
        panel?: string | import("..").TNode;
        popupProps?: import("..").PopupProps;
        popupVisible?: boolean;
        rangeInputProps?: RangeInputProps;
        readonly?: boolean;
        status?: "default" | "success" | "warning" | "error";
        tips?: string | import("..").TNode;
        onInputChange?: (value: import("./type").RangeInputValue, context?: import("./type").RangeInputValueChangeContext) => void;
        onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        status: "default" | "error" | "success" | "warning";
        disabled: boolean | boolean[];
        readonly: boolean;
        autoWidth: boolean;
        inputValue: import("./type").RangeInputValue;
        popupVisible: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    autoWidth?: boolean;
    disabled?: boolean | Array<boolean>;
    inputValue?: import("./type").RangeInputValue;
    defaultInputValue?: import("./type").RangeInputValue;
    label?: string | import("..").TNode;
    panel?: string | import("..").TNode;
    popupProps?: import("..").PopupProps;
    popupVisible?: boolean;
    rangeInputProps?: RangeInputProps;
    readonly?: boolean;
    status?: "default" | "success" | "warning" | "error";
    tips?: string | import("..").TNode;
    onInputChange?: (value: import("./type").RangeInputValue, context?: import("./type").RangeInputValueChangeContext) => void;
    onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    status: "default" | "error" | "success" | "warning";
    disabled: boolean | boolean[];
    readonly: boolean;
    autoWidth: boolean;
    inputValue: import("./type").RangeInputValue;
    popupVisible: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default RangeInput;
