import { TdDatePickerProps } from './type';
import { PropType } from 'vue';
declare const _default: {
    allowInput: BooleanConstructor;
    borderless: BooleanConstructor;
    cell: {
        type: PropType<TdDatePickerProps["cell"]>;
    };
    clearable: BooleanConstructor;
    defaultTime: {
        type: StringConstructor;
        default: string;
    };
    disableDate: {
        type: PropType<TdDatePickerProps["disableDate"]>;
    };
    disableTime: {
        type: PropType<TdDatePickerProps["disableTime"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: TdDatePickerProps["disabled"];
    };
    enableTimePicker: BooleanConstructor;
    firstDayOfWeek: {
        type: NumberConstructor;
        validator(val: TdDatePickerProps["firstDayOfWeek"]): boolean;
    };
    format: {
        type: StringConstructor;
        default: any;
    };
    inputProps: {
        type: PropType<TdDatePickerProps["inputProps"]>;
    };
    label: {
        type: PropType<TdDatePickerProps["label"]>;
    };
    mode: {
        type: PropType<TdDatePickerProps["mode"]>;
        default: TdDatePickerProps["mode"];
        validator(val: TdDatePickerProps["mode"]): boolean;
    };
    multiple: BooleanConstructor;
    needConfirm: {
        type: BooleanConstructor;
        default: boolean;
    };
    panelActiveDate: {
        type: PropType<TdDatePickerProps["panelActiveDate"]>;
        default: TdDatePickerProps["panelActiveDate"];
    };
    placeholder: {
        type: PropType<TdDatePickerProps["placeholder"]>;
        default: TdDatePickerProps["placeholder"];
    };
    popupProps: {
        type: PropType<TdDatePickerProps["popupProps"]>;
    };
    prefixIcon: {
        type: PropType<TdDatePickerProps["prefixIcon"]>;
    };
    presets: {
        type: PropType<TdDatePickerProps["presets"]>;
    };
    presetsPlacement: {
        type: PropType<TdDatePickerProps["presetsPlacement"]>;
        default: TdDatePickerProps["presetsPlacement"];
        validator(val: TdDatePickerProps["presetsPlacement"]): boolean;
    };
    range: {
        type: PropType<TdDatePickerProps["range"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    selectInputProps: {
        type: PropType<TdDatePickerProps["selectInputProps"]>;
    };
    size: {
        type: PropType<TdDatePickerProps["size"]>;
        default: TdDatePickerProps["size"];
        validator(val: TdDatePickerProps["size"]): boolean;
    };
    status: {
        type: PropType<TdDatePickerProps["status"]>;
        default: TdDatePickerProps["status"];
        validator(val: TdDatePickerProps["status"]): boolean;
    };
    suffixIcon: {
        type: PropType<TdDatePickerProps["suffixIcon"]>;
    };
    timePickerProps: {
        type: PropType<TdDatePickerProps["timePickerProps"]>;
    };
    tips: {
        type: PropType<TdDatePickerProps["tips"]>;
    };
    value: {
        type: PropType<TdDatePickerProps["value"]>;
        default: TdDatePickerProps["value"];
    };
    modelValue: {
        type: PropType<TdDatePickerProps["value"]>;
        default: TdDatePickerProps["value"];
    };
    defaultValue: {
        type: PropType<TdDatePickerProps["defaultValue"]>;
        default: TdDatePickerProps["defaultValue"];
    };
    valueDisplay: {
        type: PropType<TdDatePickerProps["valueDisplay"]>;
    };
    valueType: {
        type: PropType<TdDatePickerProps["valueType"]>;
        default: TdDatePickerProps["valueType"];
    };
    onBlur: PropType<TdDatePickerProps["onBlur"]>;
    onChange: PropType<TdDatePickerProps["onChange"]>;
    onClear: PropType<TdDatePickerProps["onClear"]>;
    onConfirm: PropType<TdDatePickerProps["onConfirm"]>;
    onFocus: PropType<TdDatePickerProps["onFocus"]>;
    onMonthChange: PropType<TdDatePickerProps["onMonthChange"]>;
    onPick: PropType<TdDatePickerProps["onPick"]>;
    onPresetClick: PropType<TdDatePickerProps["onPresetClick"]>;
    onYearChange: PropType<TdDatePickerProps["onYearChange"]>;
};
export default _default;
