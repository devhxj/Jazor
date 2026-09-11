import dayjs from 'dayjs';
import type { DateValue, DatePickerYearChangeTrigger, DatePickerMonthChangeTrigger } from './type';
declare const _default: import("vue").DefineComponent<{
    defaultTime?: string;
    onCellClick?: (context: {
        date: Date;
        e: MouseEvent;
    }) => void;
    onChange?: (value: DateValue, context: {
        dayjsValue?: dayjs.Dayjs;
        e?: MouseEvent;
        trigger?: import("./type").DatePickerTriggerSource;
    }) => void;
    onConfirm?: (context: {
        date: Date;
        e: MouseEvent;
    }) => void;
    onMonthChange?: (context: {
        month: number;
        date: Date;
        e?: MouseEvent;
        trigger: DatePickerMonthChangeTrigger;
    }) => void;
    onPanelClick?: (context: {
        e: MouseEvent;
    }) => void;
    onPresetClick?: (context: {
        preset: import("./type").PresetDate;
        e: MouseEvent;
    }) => void;
    onTimeChange?: (context: {
        time: string;
        date: Date;
        trigger: import("./type").DatePickerTimeChangeTrigger;
        e?: MouseEvent;
    }) => void;
    onYearChange?: (context: {
        year: number;
        date: Date;
        trigger: DatePickerYearChangeTrigger;
        e?: MouseEvent;
    }) => void;
    value?: DateValue | import("./type").DateMultipleValue;
    format?: string;
    mode?: "year" | "quarter" | "month" | "week" | "date";
    enableTimePicker?: boolean;
    firstDayOfWeek?: number;
    disableDate?: import("./type").DisableDate;
    defaultValue?: DateValue | import("./type").DateMultipleValue;
    disableTime?: (time: Date) => Partial<{
        hour: Array<number>;
        minute: Array<number>;
        second: Array<number>;
        millisecond: Array<number>;
    }>;
    presets?: import("..").TNode | import("./type").PresetDate;
    presetsPlacement?: "left" | "top" | "right" | "bottom";
    timePickerProps?: import("..").TimePickerProps;
    needConfirm?: boolean;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    defaultTime?: string;
    onCellClick?: (context: {
        date: Date;
        e: MouseEvent;
    }) => void;
    onChange?: (value: DateValue, context: {
        dayjsValue?: dayjs.Dayjs;
        e?: MouseEvent;
        trigger?: import("./type").DatePickerTriggerSource;
    }) => void;
    onConfirm?: (context: {
        date: Date;
        e: MouseEvent;
    }) => void;
    onMonthChange?: (context: {
        month: number;
        date: Date;
        e?: MouseEvent;
        trigger: DatePickerMonthChangeTrigger;
    }) => void;
    onPanelClick?: (context: {
        e: MouseEvent;
    }) => void;
    onPresetClick?: (context: {
        preset: import("./type").PresetDate;
        e: MouseEvent;
    }) => void;
    onTimeChange?: (context: {
        time: string;
        date: Date;
        trigger: import("./type").DatePickerTimeChangeTrigger;
        e?: MouseEvent;
    }) => void;
    onYearChange?: (context: {
        year: number;
        date: Date;
        trigger: DatePickerYearChangeTrigger;
        e?: MouseEvent;
    }) => void;
    value?: DateValue | import("./type").DateMultipleValue;
    format?: string;
    mode?: "year" | "quarter" | "month" | "week" | "date";
    enableTimePicker?: boolean;
    firstDayOfWeek?: number;
    disableDate?: import("./type").DisableDate;
    defaultValue?: DateValue | import("./type").DateMultipleValue;
    disableTime?: (time: Date) => Partial<{
        hour: Array<number>;
        minute: Array<number>;
        second: Array<number>;
        millisecond: Array<number>;
    }>;
    presets?: import("..").TNode | import("./type").PresetDate;
    presetsPlacement?: "left" | "top" | "right" | "bottom";
    timePickerProps?: import("..").TimePickerProps;
    needConfirm?: boolean;
}> & Readonly<{}>, {
    value: DateValue | import("./type").DateMultipleValue;
    format: string;
    defaultTime: string;
    mode: "date" | "month" | "year" | "quarter" | "week";
    enableTimePicker: boolean;
    disabled: boolean;
    defaultValue: DateValue | import("./type").DateMultipleValue;
    modelValue: DateValue | import("./type").DateMultipleValue;
    presetsPlacement: "left" | "right" | "top" | "bottom";
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
