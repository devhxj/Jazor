import type { TdRadioGroupProps } from './type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    allowUncheck: BooleanConstructor;
    direction: {
        type: import("vue").PropType<TdRadioGroupProps["direction"]>;
        default: TdRadioGroupProps["direction"];
        validator(val: TdRadioGroupProps["direction"]): boolean;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    name: {
        type: StringConstructor;
        default: string;
    };
    options: {
        type: import("vue").PropType<TdRadioGroupProps["options"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    size: {
        type: import("vue").PropType<TdRadioGroupProps["size"]>;
        default: TdRadioGroupProps["size"];
        validator(val: TdRadioGroupProps["size"]): boolean;
    };
    theme: {
        type: import("vue").PropType<TdRadioGroupProps["theme"]>;
        default: TdRadioGroupProps["theme"];
        validator(val: TdRadioGroupProps["theme"]): boolean;
    };
    value: {
        type: import("vue").PropType<TdRadioGroupProps["value"]>;
        default: TdRadioGroupProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdRadioGroupProps["value"]>;
        default: TdRadioGroupProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdRadioGroupProps["defaultValue"]>;
        default: TdRadioGroupProps["defaultValue"];
    };
    variant: {
        type: import("vue").PropType<TdRadioGroupProps["variant"]>;
        default: TdRadioGroupProps["variant"];
        validator(val: TdRadioGroupProps["variant"]): boolean;
    };
    onChange: import("vue").PropType<TdRadioGroupProps["onChange"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    allowUncheck: BooleanConstructor;
    direction: {
        type: import("vue").PropType<TdRadioGroupProps["direction"]>;
        default: TdRadioGroupProps["direction"];
        validator(val: TdRadioGroupProps["direction"]): boolean;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    name: {
        type: StringConstructor;
        default: string;
    };
    options: {
        type: import("vue").PropType<TdRadioGroupProps["options"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    size: {
        type: import("vue").PropType<TdRadioGroupProps["size"]>;
        default: TdRadioGroupProps["size"];
        validator(val: TdRadioGroupProps["size"]): boolean;
    };
    theme: {
        type: import("vue").PropType<TdRadioGroupProps["theme"]>;
        default: TdRadioGroupProps["theme"];
        validator(val: TdRadioGroupProps["theme"]): boolean;
    };
    value: {
        type: import("vue").PropType<TdRadioGroupProps["value"]>;
        default: TdRadioGroupProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdRadioGroupProps["value"]>;
        default: TdRadioGroupProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdRadioGroupProps["defaultValue"]>;
        default: TdRadioGroupProps["defaultValue"];
    };
    variant: {
        type: import("vue").PropType<TdRadioGroupProps["variant"]>;
        default: TdRadioGroupProps["variant"];
        validator(val: TdRadioGroupProps["variant"]): boolean;
    };
    onChange: import("vue").PropType<TdRadioGroupProps["onChange"]>;
}>> & Readonly<{}>, {
    value: import("./type").RadioValue;
    size: import("..").SizeEnum;
    direction: "vertical" | "horizontal";
    disabled: boolean;
    name: string;
    defaultValue: import("./type").RadioValue;
    theme: "button" | "radio";
    allowUncheck: boolean;
    modelValue: import("./type").RadioValue;
    readonly: boolean;
    variant: "outline" | "primary-filled" | "default-filled";
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
