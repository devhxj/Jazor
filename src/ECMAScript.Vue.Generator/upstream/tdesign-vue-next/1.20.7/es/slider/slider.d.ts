import type { SliderValue } from './type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    inputNumberProps: {
        type: import("vue").PropType<import("./type").TdSliderProps["inputNumberProps"]>;
        default: import("./type").TdSliderProps["inputNumberProps"];
    };
    label: {
        type: import("vue").PropType<import("./type").TdSliderProps["label"]>;
        default: import("./type").TdSliderProps["label"];
    };
    layout: {
        type: import("vue").PropType<import("./type").TdSliderProps["layout"]>;
        default: import("./type").TdSliderProps["layout"];
        validator(val: import("./type").TdSliderProps["layout"]): boolean;
    };
    marks: {
        type: import("vue").PropType<import("./type").TdSliderProps["marks"]>;
    };
    max: {
        type: NumberConstructor;
        default: number;
    };
    min: {
        type: NumberConstructor;
        default: number;
    };
    range: BooleanConstructor;
    showStep: BooleanConstructor;
    step: {
        type: NumberConstructor;
        default: number;
    };
    tooltipProps: {
        type: import("vue").PropType<import("./type").TdSliderProps["tooltipProps"]>;
    };
    value: {
        type: import("vue").PropType<import("./type").TdSliderProps["value"]>;
        default: import("./type").TdSliderProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<import("./type").TdSliderProps["value"]>;
        default: import("./type").TdSliderProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<import("./type").TdSliderProps["defaultValue"]>;
        default: import("./type").TdSliderProps["defaultValue"];
    };
    onChange: import("vue").PropType<import("./type").TdSliderProps["onChange"]>;
    onChangeEnd: import("vue").PropType<import("./type").TdSliderProps["onChangeEnd"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    inputNumberProps: {
        type: import("vue").PropType<import("./type").TdSliderProps["inputNumberProps"]>;
        default: import("./type").TdSliderProps["inputNumberProps"];
    };
    label: {
        type: import("vue").PropType<import("./type").TdSliderProps["label"]>;
        default: import("./type").TdSliderProps["label"];
    };
    layout: {
        type: import("vue").PropType<import("./type").TdSliderProps["layout"]>;
        default: import("./type").TdSliderProps["layout"];
        validator(val: import("./type").TdSliderProps["layout"]): boolean;
    };
    marks: {
        type: import("vue").PropType<import("./type").TdSliderProps["marks"]>;
    };
    max: {
        type: NumberConstructor;
        default: number;
    };
    min: {
        type: NumberConstructor;
        default: number;
    };
    range: BooleanConstructor;
    showStep: BooleanConstructor;
    step: {
        type: NumberConstructor;
        default: number;
    };
    tooltipProps: {
        type: import("vue").PropType<import("./type").TdSliderProps["tooltipProps"]>;
    };
    value: {
        type: import("vue").PropType<import("./type").TdSliderProps["value"]>;
        default: import("./type").TdSliderProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<import("./type").TdSliderProps["value"]>;
        default: import("./type").TdSliderProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<import("./type").TdSliderProps["defaultValue"]>;
        default: import("./type").TdSliderProps["defaultValue"];
    };
    onChange: import("vue").PropType<import("./type").TdSliderProps["onChange"]>;
    onChangeEnd: import("vue").PropType<import("./type").TdSliderProps["onChangeEnd"]>;
}>> & Readonly<{}>, {
    layout: "vertical" | "horizontal";
    value: SliderValue;
    min: number;
    max: number;
    step: number;
    label: string | boolean | ((h: typeof import("vue").h, props: {
        value: SliderValue;
        position?: "start" | "end";
    }) => import("..").TNodeReturnValue);
    disabled: boolean;
    defaultValue: SliderValue;
    modelValue: SliderValue;
    range: boolean;
    inputNumberProps: boolean | import("..").InputNumberProps;
    showStep: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
