declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    enableMultipleGradient: {
        type: BooleanConstructor;
        default: boolean;
    };
    disabled: BooleanConstructor;
    color: {
        type: import("vue").PropType<import("@common/js/color-picker").Color>;
    };
    onChange: {
        type: FunctionConstructor;
        default: () => () => void;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    enableMultipleGradient: {
        type: BooleanConstructor;
        default: boolean;
    };
    disabled: BooleanConstructor;
    color: {
        type: import("vue").PropType<import("@common/js/color-picker").Color>;
    };
    onChange: {
        type: FunctionConstructor;
        default: () => () => void;
    };
}>> & Readonly<{}>, {
    disabled: boolean;
    onChange: Function;
    enableMultipleGradient: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
