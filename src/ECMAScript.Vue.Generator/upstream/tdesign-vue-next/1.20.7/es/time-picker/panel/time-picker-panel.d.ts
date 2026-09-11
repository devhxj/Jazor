declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    isFooterDisplay: BooleanConstructor;
    handleConfirmClick: FunctionConstructor;
    onChange: FunctionConstructor;
    position: StringConstructor;
    disabled: {
        default: boolean;
        type: BooleanConstructor;
        validator(v: boolean): boolean;
    };
    isFocus: {
        default: boolean;
        type: BooleanConstructor;
        validator(v: boolean): boolean;
    };
    value: {
        type: StringConstructor;
        default: string;
    };
    format: {
        type: StringConstructor;
        default: string;
    };
    steps: {
        default: number[];
        type: import("vue").PropType<Array<string | number>>;
    };
    isShowPanel: {
        default: boolean;
        type: BooleanConstructor;
        validator(v: boolean): boolean;
    };
    activeIndex: {
        type: NumberConstructor;
    };
    presets: {
        type: import("vue").PropType<import("..").TdTimePickerProps["presets"] | import("..").TdTimeRangePickerProps["presets"]>;
    };
    hideDisabledTime: {
        type: BooleanConstructor;
        default: boolean;
    };
    disableTime: {
        type: FunctionConstructor;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    isFooterDisplay: BooleanConstructor;
    handleConfirmClick: FunctionConstructor;
    onChange: FunctionConstructor;
    position: StringConstructor;
    disabled: {
        default: boolean;
        type: BooleanConstructor;
        validator(v: boolean): boolean;
    };
    isFocus: {
        default: boolean;
        type: BooleanConstructor;
        validator(v: boolean): boolean;
    };
    value: {
        type: StringConstructor;
        default: string;
    };
    format: {
        type: StringConstructor;
        default: string;
    };
    steps: {
        default: number[];
        type: import("vue").PropType<Array<string | number>>;
    };
    isShowPanel: {
        default: boolean;
        type: BooleanConstructor;
        validator(v: boolean): boolean;
    };
    activeIndex: {
        type: NumberConstructor;
    };
    presets: {
        type: import("vue").PropType<import("..").TdTimePickerProps["presets"] | import("..").TdTimeRangePickerProps["presets"]>;
    };
    hideDisabledTime: {
        type: BooleanConstructor;
        default: boolean;
    };
    disableTime: {
        type: FunctionConstructor;
    };
}>> & Readonly<{}>, {
    steps: (string | number)[];
    value: string;
    format: string;
    disabled: boolean;
    isFocus: boolean;
    isShowPanel: boolean;
    hideDisabledTime: boolean;
    isFooterDisplay: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
