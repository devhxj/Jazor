import { TdTimePickerProps } from './type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    allowInput: BooleanConstructor;
    borderless: BooleanConstructor;
    clearable: BooleanConstructor;
    disableTime: {
        type: import("vue").PropType<TdTimePickerProps["disableTime"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    format: {
        type: StringConstructor;
        default: string;
    };
    hideDisabledTime: {
        type: BooleanConstructor;
        default: boolean;
    };
    inputProps: {
        type: import("vue").PropType<TdTimePickerProps["inputProps"]>;
    };
    label: {
        type: import("vue").PropType<TdTimePickerProps["label"]>;
    };
    placeholder: {
        type: StringConstructor;
        default: any;
    };
    popupProps: {
        type: import("vue").PropType<TdTimePickerProps["popupProps"]>;
    };
    prefixIcon: {
        type: import("vue").PropType<TdTimePickerProps["prefixIcon"]>;
    };
    presets: {
        type: import("vue").PropType<TdTimePickerProps["presets"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    selectInputProps: {
        type: import("vue").PropType<TdTimePickerProps["selectInputProps"]>;
    };
    size: {
        type: import("vue").PropType<TdTimePickerProps["size"]>;
        default: TdTimePickerProps["size"];
        validator(val: TdTimePickerProps["size"]): boolean;
    };
    status: {
        type: import("vue").PropType<TdTimePickerProps["status"]>;
        default: TdTimePickerProps["status"];
        validator(val: TdTimePickerProps["status"]): boolean;
    };
    steps: {
        type: import("vue").PropType<TdTimePickerProps["steps"]>;
        default: () => TdTimePickerProps["steps"];
    };
    suffixIcon: {
        type: import("vue").PropType<TdTimePickerProps["suffixIcon"]>;
    };
    tips: {
        type: import("vue").PropType<TdTimePickerProps["tips"]>;
    };
    value: {
        type: import("vue").PropType<TdTimePickerProps["value"]>;
        default: TdTimePickerProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdTimePickerProps["value"]>;
        default: TdTimePickerProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdTimePickerProps["defaultValue"]>;
        default: TdTimePickerProps["defaultValue"];
    };
    valueDisplay: {
        type: import("vue").PropType<TdTimePickerProps["valueDisplay"]>;
    };
    onBlur: import("vue").PropType<TdTimePickerProps["onBlur"]>;
    onChange: import("vue").PropType<TdTimePickerProps["onChange"]>;
    onClear: import("vue").PropType<TdTimePickerProps["onClear"]>;
    onClose: import("vue").PropType<TdTimePickerProps["onClose"]>;
    onConfirm: import("vue").PropType<TdTimePickerProps["onConfirm"]>;
    onFocus: import("vue").PropType<TdTimePickerProps["onFocus"]>;
    onInput: import("vue").PropType<TdTimePickerProps["onInput"]>;
    onOpen: import("vue").PropType<TdTimePickerProps["onOpen"]>;
    onPick: import("vue").PropType<TdTimePickerProps["onPick"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    allowInput: BooleanConstructor;
    borderless: BooleanConstructor;
    clearable: BooleanConstructor;
    disableTime: {
        type: import("vue").PropType<TdTimePickerProps["disableTime"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    format: {
        type: StringConstructor;
        default: string;
    };
    hideDisabledTime: {
        type: BooleanConstructor;
        default: boolean;
    };
    inputProps: {
        type: import("vue").PropType<TdTimePickerProps["inputProps"]>;
    };
    label: {
        type: import("vue").PropType<TdTimePickerProps["label"]>;
    };
    placeholder: {
        type: StringConstructor;
        default: any;
    };
    popupProps: {
        type: import("vue").PropType<TdTimePickerProps["popupProps"]>;
    };
    prefixIcon: {
        type: import("vue").PropType<TdTimePickerProps["prefixIcon"]>;
    };
    presets: {
        type: import("vue").PropType<TdTimePickerProps["presets"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    selectInputProps: {
        type: import("vue").PropType<TdTimePickerProps["selectInputProps"]>;
    };
    size: {
        type: import("vue").PropType<TdTimePickerProps["size"]>;
        default: TdTimePickerProps["size"];
        validator(val: TdTimePickerProps["size"]): boolean;
    };
    status: {
        type: import("vue").PropType<TdTimePickerProps["status"]>;
        default: TdTimePickerProps["status"];
        validator(val: TdTimePickerProps["status"]): boolean;
    };
    steps: {
        type: import("vue").PropType<TdTimePickerProps["steps"]>;
        default: () => TdTimePickerProps["steps"];
    };
    suffixIcon: {
        type: import("vue").PropType<TdTimePickerProps["suffixIcon"]>;
    };
    tips: {
        type: import("vue").PropType<TdTimePickerProps["tips"]>;
    };
    value: {
        type: import("vue").PropType<TdTimePickerProps["value"]>;
        default: TdTimePickerProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdTimePickerProps["value"]>;
        default: TdTimePickerProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdTimePickerProps["defaultValue"]>;
        default: TdTimePickerProps["defaultValue"];
    };
    valueDisplay: {
        type: import("vue").PropType<TdTimePickerProps["valueDisplay"]>;
    };
    onBlur: import("vue").PropType<TdTimePickerProps["onBlur"]>;
    onChange: import("vue").PropType<TdTimePickerProps["onChange"]>;
    onClear: import("vue").PropType<TdTimePickerProps["onClear"]>;
    onClose: import("vue").PropType<TdTimePickerProps["onClose"]>;
    onConfirm: import("vue").PropType<TdTimePickerProps["onConfirm"]>;
    onFocus: import("vue").PropType<TdTimePickerProps["onFocus"]>;
    onInput: import("vue").PropType<TdTimePickerProps["onInput"]>;
    onOpen: import("vue").PropType<TdTimePickerProps["onOpen"]>;
    onPick: import("vue").PropType<TdTimePickerProps["onPick"]>;
}>> & Readonly<{}>, {
    steps: (string | number)[];
    value: string;
    format: string;
    size: "small" | "medium" | "large";
    status: "default" | "error" | "success" | "warning";
    disabled: boolean;
    defaultValue: string;
    placeholder: string;
    modelValue: string;
    readonly: boolean;
    borderless: boolean;
    clearable: boolean;
    allowInput: boolean;
    hideDisabledTime: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
