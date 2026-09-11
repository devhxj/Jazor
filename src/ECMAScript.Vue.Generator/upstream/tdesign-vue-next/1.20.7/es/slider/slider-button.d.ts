import { PropType } from 'vue';
import type { TdSliderProps } from './type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    value: {
        type: NumberConstructor[];
        default: number;
    };
    vertical: {
        type: BooleanConstructor;
        default: boolean;
    };
    tooltipProps: {
        type: (ObjectConstructor | BooleanConstructor)[];
        default: boolean;
    };
    label: {
        type: PropType<TdSliderProps["label"]>;
    };
    range: {
        type: BooleanConstructor;
        default: boolean;
    };
    position: {
        type: StringConstructor;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, ("input" | "mouseup")[], "input" | "mouseup", import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    value: {
        type: NumberConstructor[];
        default: number;
    };
    vertical: {
        type: BooleanConstructor;
        default: boolean;
    };
    tooltipProps: {
        type: (ObjectConstructor | BooleanConstructor)[];
        default: boolean;
    };
    label: {
        type: PropType<TdSliderProps["label"]>;
    };
    range: {
        type: BooleanConstructor;
        default: boolean;
    };
    position: {
        type: StringConstructor;
    };
}>> & Readonly<{
    onInput?: (...args: any[]) => any;
    onMouseup?: (...args: any[]) => any;
}>, {
    value: number;
    vertical: boolean;
    range: boolean;
    tooltipProps: boolean | Record<string, any>;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
