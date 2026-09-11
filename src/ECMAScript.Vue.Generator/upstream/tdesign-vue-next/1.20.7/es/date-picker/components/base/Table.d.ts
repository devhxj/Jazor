import { PropType } from 'vue';
import type { TdDatePickerProps } from '../../type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    mode: {
        type: PropType<TdDatePickerProps["mode"]>;
        default: string;
    };
    value: (ArrayConstructor | NumberConstructor | DateConstructor | StringConstructor)[];
    format: StringConstructor;
    firstDayOfWeek: NumberConstructor;
    multiple: BooleanConstructor;
    data: ArrayConstructor;
    time: StringConstructor;
    onCellClick: FunctionConstructor;
    onCellMouseEnter: FunctionConstructor;
    onCellMouseLeave: FunctionConstructor;
    cell: {
        type: PropType<TdDatePickerProps["cell"]>;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    mode: {
        type: PropType<TdDatePickerProps["mode"]>;
        default: string;
    };
    value: (ArrayConstructor | NumberConstructor | DateConstructor | StringConstructor)[];
    format: StringConstructor;
    firstDayOfWeek: NumberConstructor;
    multiple: BooleanConstructor;
    data: ArrayConstructor;
    time: StringConstructor;
    onCellClick: FunctionConstructor;
    onCellMouseEnter: FunctionConstructor;
    onCellMouseLeave: FunctionConstructor;
    cell: {
        type: PropType<TdDatePickerProps["cell"]>;
    };
}>> & Readonly<{}>, {
    mode: "date" | "month" | "year" | "quarter" | "week";
    multiple: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
