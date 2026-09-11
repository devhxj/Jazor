import { PropType } from 'vue';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    className: {
        type: StringConstructor;
        default: string;
    };
    value: {
        type: NumberConstructor;
        default: number;
    };
    maxValue: {
        type: NumberConstructor;
        default: number;
    };
    railStyle: {
        type: PropType<any>;
    };
    type: {
        type: PropType<"hue" | "alpha">;
        default: string;
    };
    disabled: BooleanConstructor;
    color: {
        type: PropType<import("@common/js/color-picker").Color>;
    };
    onChange: {
        type: FunctionConstructor;
        default: () => () => void;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    className: {
        type: StringConstructor;
        default: string;
    };
    value: {
        type: NumberConstructor;
        default: number;
    };
    maxValue: {
        type: NumberConstructor;
        default: number;
    };
    railStyle: {
        type: PropType<any>;
    };
    type: {
        type: PropType<"hue" | "alpha">;
        default: string;
    };
    disabled: BooleanConstructor;
    color: {
        type: PropType<import("@common/js/color-picker").Color>;
    };
    onChange: {
        type: FunctionConstructor;
        default: () => () => void;
    };
}>> & Readonly<{}>, {
    value: number;
    type: "alpha" | "hue";
    disabled: boolean;
    className: string;
    onChange: Function;
    maxValue: number;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
