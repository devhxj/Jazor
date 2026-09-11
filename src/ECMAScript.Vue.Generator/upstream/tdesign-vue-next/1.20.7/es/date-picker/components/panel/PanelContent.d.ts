import { PropType } from 'vue';
import type { TdDatePickerProps, TdDateRangePickerProps } from '../../type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    mode: PropType<TdDatePickerProps["mode"]>;
    format: PropType<TdDatePickerProps["format"]>;
    enableTimePicker: PropType<TdDatePickerProps["enableTimePicker"]>;
    timePickerProps: {
        type: PropType<TdDatePickerProps["timePickerProps"]>;
        default: () => {};
    };
    year: NumberConstructor;
    month: NumberConstructor;
    range: PropType<TdDatePickerProps["range"]>;
    tableData: ArrayConstructor;
    time: StringConstructor;
    multiple: BooleanConstructor;
    firstDayOfWeek: NumberConstructor;
    partial: StringConstructor;
    popupVisible: BooleanConstructor;
    onYearChange: FunctionConstructor;
    onMonthChange: FunctionConstructor;
    onJumperClick: FunctionConstructor;
    onCellMouseEnter: FunctionConstructor;
    onCellClick: FunctionConstructor;
    onCellMouseLeave: FunctionConstructor;
    onTimePickerChange: FunctionConstructor;
    value: (ArrayConstructor | NumberConstructor | DateConstructor | StringConstructor)[];
    internalYear: PropType<Array<number>>;
    disableTime: FunctionConstructor;
    defaultTime: PropType<TdDatePickerProps["defaultTime"] | TdDateRangePickerProps["defaultTime"]>;
    cell: {
        type: PropType<TdDatePickerProps["cell"] | TdDateRangePickerProps["cell"]>;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    mode: PropType<TdDatePickerProps["mode"]>;
    format: PropType<TdDatePickerProps["format"]>;
    enableTimePicker: PropType<TdDatePickerProps["enableTimePicker"]>;
    timePickerProps: {
        type: PropType<TdDatePickerProps["timePickerProps"]>;
        default: () => {};
    };
    year: NumberConstructor;
    month: NumberConstructor;
    range: PropType<TdDatePickerProps["range"]>;
    tableData: ArrayConstructor;
    time: StringConstructor;
    multiple: BooleanConstructor;
    firstDayOfWeek: NumberConstructor;
    partial: StringConstructor;
    popupVisible: BooleanConstructor;
    onYearChange: FunctionConstructor;
    onMonthChange: FunctionConstructor;
    onJumperClick: FunctionConstructor;
    onCellMouseEnter: FunctionConstructor;
    onCellClick: FunctionConstructor;
    onCellMouseLeave: FunctionConstructor;
    onTimePickerChange: FunctionConstructor;
    value: (ArrayConstructor | NumberConstructor | DateConstructor | StringConstructor)[];
    internalYear: PropType<Array<number>>;
    disableTime: FunctionConstructor;
    defaultTime: PropType<TdDatePickerProps["defaultTime"] | TdDateRangePickerProps["defaultTime"]>;
    cell: {
        type: PropType<TdDatePickerProps["cell"] | TdDateRangePickerProps["cell"]>;
    };
}>> & Readonly<{}>, {
    multiple: boolean;
    popupVisible: boolean;
    timePickerProps: import("../../..").TdTimePickerProps;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
