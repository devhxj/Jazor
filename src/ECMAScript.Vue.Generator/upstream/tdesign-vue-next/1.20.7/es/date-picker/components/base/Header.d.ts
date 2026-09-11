import { PropType } from 'vue';
import { JumperTrigger } from '../../../pagination';
import type { TdDatePickerProps } from '../../type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    mode: {
        type: PropType<TdDatePickerProps["mode"]>;
        default: string;
    };
    range: PropType<TdDatePickerProps["range"]>;
    year: NumberConstructor;
    month: NumberConstructor;
    internalYear: PropType<Array<number>>;
    partial: StringConstructor;
    onMonthChange: FunctionConstructor;
    onYearChange: FunctionConstructor;
    onJumperClick: PropType<(context: {
        e: MouseEvent;
        trigger: JumperTrigger;
    }) => {}>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    mode: {
        type: PropType<TdDatePickerProps["mode"]>;
        default: string;
    };
    range: PropType<TdDatePickerProps["range"]>;
    year: NumberConstructor;
    month: NumberConstructor;
    internalYear: PropType<Array<number>>;
    partial: StringConstructor;
    onMonthChange: FunctionConstructor;
    onYearChange: FunctionConstructor;
    onJumperClick: PropType<(context: {
        e: MouseEvent;
        trigger: JumperTrigger;
    }) => {}>;
}>> & Readonly<{}>, {
    mode: "date" | "month" | "year" | "quarter" | "week";
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
