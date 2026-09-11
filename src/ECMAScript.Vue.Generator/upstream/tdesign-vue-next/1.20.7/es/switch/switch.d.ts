import { TNodeReturnValue } from '../common';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    beforeChange: {
        type: import("vue").PropType<import("./type").TdSwitchProps["beforeChange"]>;
    };
    customValue: {
        type: import("vue").PropType<import("./type").TdSwitchProps["customValue"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    label: {
        type: import("vue").PropType<import("./type").TdSwitchProps["label"]>;
        default: () => import("./type").TdSwitchProps["label"];
    };
    loading: BooleanConstructor;
    size: {
        type: import("vue").PropType<import("./type").TdSwitchProps["size"]>;
        default: import("./type").TdSwitchProps["size"];
        validator(val: import("./type").TdSwitchProps["size"]): boolean;
    };
    value: {
        type: import("vue").PropType<import("./type").TdSwitchProps["value"]>;
        default: import("./type").TdSwitchProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<import("./type").TdSwitchProps["value"]>;
        default: import("./type").TdSwitchProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<import("./type").TdSwitchProps["defaultValue"]>;
    };
    onChange: import("vue").PropType<import("./type").TdSwitchProps["onChange"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    beforeChange: {
        type: import("vue").PropType<import("./type").TdSwitchProps["beforeChange"]>;
    };
    customValue: {
        type: import("vue").PropType<import("./type").TdSwitchProps["customValue"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    label: {
        type: import("vue").PropType<import("./type").TdSwitchProps["label"]>;
        default: () => import("./type").TdSwitchProps["label"];
    };
    loading: BooleanConstructor;
    size: {
        type: import("vue").PropType<import("./type").TdSwitchProps["size"]>;
        default: import("./type").TdSwitchProps["size"];
        validator(val: import("./type").TdSwitchProps["size"]): boolean;
    };
    value: {
        type: import("vue").PropType<import("./type").TdSwitchProps["value"]>;
        default: import("./type").TdSwitchProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<import("./type").TdSwitchProps["value"]>;
        default: import("./type").TdSwitchProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<import("./type").TdSwitchProps["defaultValue"]>;
    };
    onChange: import("vue").PropType<import("./type").TdSwitchProps["onChange"]>;
}>> & Readonly<{}>, {
    loading: boolean;
    value: import("./type").SwitchValue;
    size: "small" | "medium" | "large";
    label: (string | ((h: typeof import("vue").h) => TNodeReturnValue))[] | ((h: typeof import("vue").h, props: {
        value: import("./type").SwitchValue;
    }) => TNodeReturnValue);
    disabled: boolean;
    modelValue: import("./type").SwitchValue;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
