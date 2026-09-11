import { TdDateRangePickerProps } from '../date-picker/type';
import { PropType } from 'vue';
declare const _default: {
    allowInput: BooleanConstructor;
    borderless: BooleanConstructor;
    cancelRangeSelectLimit: BooleanConstructor;
    cell: {
        type: PropType<TdDateRangePickerProps["cell"]>;
    };
    clearable: BooleanConstructor;
    defaultTime: {
        type: PropType<TdDateRangePickerProps["defaultTime"]>;
        default: () => TdDateRangePickerProps["defaultTime"];
    };
    disableDate: {
        type: PropType<TdDateRangePickerProps["disableDate"]>;
    };
    disableTime: {
        type: PropType<TdDateRangePickerProps["disableTime"]>;
    };
    disabled: {
        type: PropType<TdDateRangePickerProps["disabled"]>;
        default: any;
    };
    enableTimePicker: BooleanConstructor;
    firstDayOfWeek: {
        type: NumberConstructor;
        validator(val: TdDateRangePickerProps["firstDayOfWeek"]): boolean;
    };
    format: {
        type: StringConstructor;
        default: string;
    };
    label: {
        type: PropType<TdDateRangePickerProps["label"]>;
    };
    mode: {
        type: PropType<TdDateRangePickerProps["mode"]>;
        default: TdDateRangePickerProps["mode"];
        validator(val: TdDateRangePickerProps["mode"]): boolean;
    };
    needConfirm: {
        type: BooleanConstructor;
        default: boolean;
    };
    panelActiveDate: {
        type: PropType<TdDateRangePickerProps["panelActiveDate"]>;
        default: TdDateRangePickerProps["panelActiveDate"];
    };
    panelPreselection: {
        type: BooleanConstructor;
        default: boolean;
    };
    placeholder: {
        type: PropType<TdDateRangePickerProps["placeholder"]>;
    };
    popupProps: {
        type: PropType<TdDateRangePickerProps["popupProps"]>;
    };
    prefixIcon: {
        type: PropType<TdDateRangePickerProps["prefixIcon"]>;
    };
    presets: {
        type: PropType<TdDateRangePickerProps["presets"]>;
    };
    presetsPlacement: {
        type: PropType<TdDateRangePickerProps["presetsPlacement"]>;
        default: TdDateRangePickerProps["presetsPlacement"];
        validator(val: TdDateRangePickerProps["presetsPlacement"]): boolean;
    };
    range: {
        type: PropType<TdDateRangePickerProps["range"]>;
    };
    rangeInputProps: {
        type: PropType<TdDateRangePickerProps["rangeInputProps"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    separator: {
        type: StringConstructor;
        default: string;
    };
    size: {
        type: PropType<TdDateRangePickerProps["size"]>;
        default: TdDateRangePickerProps["size"];
        validator(val: TdDateRangePickerProps["size"]): boolean;
    };
    status: {
        type: PropType<TdDateRangePickerProps["status"]>;
        default: TdDateRangePickerProps["status"];
        validator(val: TdDateRangePickerProps["status"]): boolean;
    };
    suffixIcon: {
        type: PropType<TdDateRangePickerProps["suffixIcon"]>;
    };
    timePickerProps: {
        type: PropType<TdDateRangePickerProps["timePickerProps"]>;
    };
    tips: {
        type: PropType<TdDateRangePickerProps["tips"]>;
    };
    value: {
        type: PropType<TdDateRangePickerProps["value"]>;
        default: TdDateRangePickerProps["value"];
    };
    modelValue: {
        type: PropType<TdDateRangePickerProps["value"]>;
        default: TdDateRangePickerProps["value"];
    };
    defaultValue: {
        type: PropType<TdDateRangePickerProps["defaultValue"]>;
        default: () => TdDateRangePickerProps["defaultValue"];
    };
    valueType: {
        type: PropType<TdDateRangePickerProps["valueType"]>;
        validator(val: TdDateRangePickerProps["valueType"]): boolean;
    };
    onBlur: PropType<TdDateRangePickerProps["onBlur"]>;
    onChange: PropType<TdDateRangePickerProps["onChange"]>;
    onConfirm: PropType<TdDateRangePickerProps["onConfirm"]>;
    onFocus: PropType<TdDateRangePickerProps["onFocus"]>;
    onInput: PropType<TdDateRangePickerProps["onInput"]>;
    onMonthChange: PropType<TdDateRangePickerProps["onMonthChange"]>;
    onPick: PropType<TdDateRangePickerProps["onPick"]>;
    onPresetClick: PropType<TdDateRangePickerProps["onPresetClick"]>;
    onYearChange: PropType<TdDateRangePickerProps["onYearChange"]>;
};
export default _default;
