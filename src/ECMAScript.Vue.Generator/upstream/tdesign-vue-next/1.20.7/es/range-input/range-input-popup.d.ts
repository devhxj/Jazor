declare const _default: import("vue").DefineComponent<{
    autoWidth?: boolean;
    disabled?: boolean | Array<boolean>;
    inputValue?: import("./type").RangeInputValue;
    defaultInputValue?: import("./type").RangeInputValue;
    label?: string | import("..").TNode;
    panel?: string | import("..").TNode;
    popupProps?: import("..").PopupProps;
    popupVisible?: boolean;
    rangeInputProps?: import(".").RangeInputProps;
    readonly?: boolean;
    status?: "default" | "success" | "warning" | "error";
    tips?: string | import("..").TNode;
    onInputChange?: (value: import("./type").RangeInputValue, context?: import("./type").RangeInputValueChangeContext) => void;
    onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    autoWidth?: boolean;
    disabled?: boolean | Array<boolean>;
    inputValue?: import("./type").RangeInputValue;
    defaultInputValue?: import("./type").RangeInputValue;
    label?: string | import("..").TNode;
    panel?: string | import("..").TNode;
    popupProps?: import("..").PopupProps;
    popupVisible?: boolean;
    rangeInputProps?: import(".").RangeInputProps;
    readonly?: boolean;
    status?: "default" | "success" | "warning" | "error";
    tips?: string | import("..").TNode;
    onInputChange?: (value: import("./type").RangeInputValue, context?: import("./type").RangeInputValueChangeContext) => void;
    onPopupVisibleChange?: (visible: boolean, context: import("..").PopupVisibleChangeContext) => void;
}> & Readonly<{}>, {
    status: "default" | "error" | "success" | "warning";
    disabled: boolean | boolean[];
    readonly: boolean;
    autoWidth: boolean;
    inputValue: import("./type").RangeInputValue;
    popupVisible: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
