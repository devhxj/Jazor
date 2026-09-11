import { PropType } from 'vue';
import { TdColorPickerProps } from '../../type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    color: {
        type: StringConstructor;
        default: string;
    };
    disabled: {
        type: BooleanConstructor;
        default: boolean;
    };
    borderless: {
        type: BooleanConstructor;
        default: boolean;
    };
    clearable: {
        type: BooleanConstructor;
        default: boolean;
    };
    inputProps: {
        type: PropType<TdColorPickerProps["inputProps"]>;
        default: () => {
            autoWidth: boolean;
        };
    };
    onTriggerChange: {
        type: FunctionConstructor;
        default: () => () => void;
    };
    onTriggerClear: {
        type: FunctionConstructor;
        default: () => () => void;
    };
    size: {
        type: PropType<TdColorPickerProps["size"]>;
        default: string;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    color: {
        type: StringConstructor;
        default: string;
    };
    disabled: {
        type: BooleanConstructor;
        default: boolean;
    };
    borderless: {
        type: BooleanConstructor;
        default: boolean;
    };
    clearable: {
        type: BooleanConstructor;
        default: boolean;
    };
    inputProps: {
        type: PropType<TdColorPickerProps["inputProps"]>;
        default: () => {
            autoWidth: boolean;
        };
    };
    onTriggerChange: {
        type: FunctionConstructor;
        default: () => () => void;
    };
    onTriggerClear: {
        type: FunctionConstructor;
        default: () => () => void;
    };
    size: {
        type: PropType<TdColorPickerProps["size"]>;
        default: string;
    };
}>> & Readonly<{}>, {
    color: string;
    size: import("../../..").SizeEnum;
    disabled: boolean;
    borderless: boolean;
    clearable: boolean;
    inputProps: import("../../..").InputProps;
    onTriggerChange: Function;
    onTriggerClear: Function;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
