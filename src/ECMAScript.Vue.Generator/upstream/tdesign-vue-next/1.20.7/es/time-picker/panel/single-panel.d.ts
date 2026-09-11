import dayjs from 'dayjs';
import { EPickerCols } from 'tdesign-vue-next/es/common/js/time-picker/const';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    position: StringConstructor;
    triggerScroll: BooleanConstructor;
    onChange: FunctionConstructor;
    resetTriggerScroll: FunctionConstructor;
    isShowPanel: BooleanConstructor;
    format: {
        type: StringConstructor;
        default: string;
    };
    cols: {
        type: import("vue").PropType<Array<EPickerCols>>;
        default: () => EPickerCols[];
    };
    value: {
        type: StringConstructor;
        default: string;
    };
    internalValue: {
        type: StringConstructor;
    };
    range: {
        type: import("vue").PropType<Array<dayjs.Dayjs>>;
        default: () => Array<dayjs.Dayjs>;
    };
    steps: {
        default: number[];
        type: import("vue").PropType<Array<string | number>>;
    };
    hideDisabledTime: {
        type: BooleanConstructor;
        default: boolean;
    };
    disableTime: {
        type: FunctionConstructor;
    };
    localeMeridiems: {
        type: import("vue").PropType<Array<string>>;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    position: StringConstructor;
    triggerScroll: BooleanConstructor;
    onChange: FunctionConstructor;
    resetTriggerScroll: FunctionConstructor;
    isShowPanel: BooleanConstructor;
    format: {
        type: StringConstructor;
        default: string;
    };
    cols: {
        type: import("vue").PropType<Array<EPickerCols>>;
        default: () => EPickerCols[];
    };
    value: {
        type: StringConstructor;
        default: string;
    };
    internalValue: {
        type: StringConstructor;
    };
    range: {
        type: import("vue").PropType<Array<dayjs.Dayjs>>;
        default: () => Array<dayjs.Dayjs>;
    };
    steps: {
        default: number[];
        type: import("vue").PropType<Array<string | number>>;
    };
    hideDisabledTime: {
        type: BooleanConstructor;
        default: boolean;
    };
    disableTime: {
        type: FunctionConstructor;
    };
    localeMeridiems: {
        type: import("vue").PropType<Array<string>>;
    };
}>> & Readonly<{}>, {
    steps: (string | number)[];
    value: string;
    format: string;
    cols: EPickerCols[];
    range: dayjs.Dayjs[];
    triggerScroll: boolean;
    isShowPanel: boolean;
    hideDisabledTime: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
