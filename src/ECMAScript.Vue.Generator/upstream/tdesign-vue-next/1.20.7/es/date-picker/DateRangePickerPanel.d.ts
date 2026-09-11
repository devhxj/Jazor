import dayjs from 'dayjs';
import { DateRangePickerPartial, DatePickerYearChangeTrigger, DatePickerMonthChangeTrigger } from './type';
declare const _default: import("vue").DefineComponent<{
    defaultTime?: string[];
    onCellClick?: (context: {
        date: Date[];
        partial: DateRangePickerPartial;
        e: MouseEvent;
    }) => void;
    onChange?: (value: import("./type").DateRangeValue, context: {
        dayjsValue?: dayjs.Dayjs[];
        partial: DateRangePickerPartial;
        e?: MouseEvent;
        trigger?: import("./type").DatePickerTriggerSource;
    }) => void;
    onConfirm?: (context: {
        date: Date[];
        e: MouseEvent;
    }) => void;
    onMonthChange?: (context: {
        month: number;
        date: Date[];
        partial: DateRangePickerPartial;
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
        date: Date[];
        partial: DateRangePickerPartial;
        trigger: import("./type").DatePickerTimeChangeTrigger;
        e?: MouseEvent;
    }) => void;
    onYearChange?: (context: {
        year: number;
        date: Date[];
        partial: DateRangePickerPartial;
        trigger: DatePickerYearChangeTrigger;
        e?: MouseEvent;
    }) => void;
    value?: import("./type").DateRangeValue;
    format?: string;
    mode?: "year" | "quarter" | "month" | "week" | "date";
    enableTimePicker?: boolean;
    firstDayOfWeek?: number;
    disableDate?: import("./type").DisableRangeDate;
    defaultValue?: import("./type").DateRangeValue;
    range?: import("./type").PickerDateRange | import("./type").PickerDateRange[];
    presets?: import("..").TNode | import("./type").PresetRange;
    presetsPlacement?: "left" | "top" | "right" | "bottom";
    timePickerProps?: import("..").TimePickerProps;
    panelPreselection?: boolean;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    defaultTime?: string[];
    onCellClick?: (context: {
        date: Date[];
        partial: DateRangePickerPartial;
        e: MouseEvent;
    }) => void;
    onChange?: (value: import("./type").DateRangeValue, context: {
        dayjsValue?: dayjs.Dayjs[];
        partial: DateRangePickerPartial;
        e?: MouseEvent;
        trigger?: import("./type").DatePickerTriggerSource;
    }) => void;
    onConfirm?: (context: {
        date: Date[];
        e: MouseEvent;
    }) => void;
    onMonthChange?: (context: {
        month: number;
        date: Date[];
        partial: DateRangePickerPartial;
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
        date: Date[];
        partial: DateRangePickerPartial;
        trigger: import("./type").DatePickerTimeChangeTrigger;
        e?: MouseEvent;
    }) => void;
    onYearChange?: (context: {
        year: number;
        date: Date[];
        partial: DateRangePickerPartial;
        trigger: DatePickerYearChangeTrigger;
        e?: MouseEvent;
    }) => void;
    value?: import("./type").DateRangeValue;
    format?: string;
    mode?: "year" | "quarter" | "month" | "week" | "date";
    enableTimePicker?: boolean;
    firstDayOfWeek?: number;
    disableDate?: import("./type").DisableRangeDate;
    defaultValue?: import("./type").DateRangeValue;
    range?: import("./type").PickerDateRange | import("./type").PickerDateRange[];
    presets?: import("..").TNode | import("./type").PresetRange;
    presetsPlacement?: "left" | "top" | "right" | "bottom";
    timePickerProps?: import("..").TimePickerProps;
    panelPreselection?: boolean;
}> & Readonly<{}>, {
    value: import("./type").DateRangeValue;
    format: string;
    defaultTime: string[];
    mode: "date" | "month" | "year" | "quarter" | "week";
    enableTimePicker: boolean;
    disabled: boolean | boolean[];
    defaultValue: import("./type").DateRangeValue;
    modelValue: import("./type").DateRangeValue;
    presetsPlacement: "left" | "right" | "top" | "bottom";
    panelPreselection: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
