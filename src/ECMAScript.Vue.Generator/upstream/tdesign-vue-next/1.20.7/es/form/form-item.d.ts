import { AllValidateResult, Data } from './type';
export type FormItemValidateResult<T extends Data = Data> = {
    [key in keyof T]: boolean | AllValidateResult[];
};
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    for: {
        type: StringConstructor;
        default: string;
    };
    help: {
        type: import("vue").PropType<import("./type").TdFormItemProps["help"]>;
    };
    label: {
        type: import("vue").PropType<import("./type").TdFormItemProps["label"]>;
        default: import("./type").TdFormItemProps["label"];
    };
    labelAlign: {
        type: import("vue").PropType<import("./type").TdFormItemProps["labelAlign"]>;
        validator(val: import("./type").TdFormItemProps["labelAlign"]): boolean;
    };
    labelWidth: {
        type: import("vue").PropType<import("./type").TdFormItemProps["labelWidth"]>;
    };
    name: {
        type: StringConstructor;
        default: string;
    };
    requiredMark: {
        type: BooleanConstructor;
        default: any;
    };
    rules: {
        type: import("vue").PropType<import("./type").TdFormItemProps["rules"]>;
    };
    showErrorMessage: {
        type: BooleanConstructor;
        default: any;
    };
    status: {
        type: import("vue").PropType<import("./type").TdFormItemProps["status"]>;
        default: import("./type").TdFormItemProps["status"];
    };
    statusIcon: {
        type: import("vue").PropType<import("./type").TdFormItemProps["statusIcon"]>;
        default: import("./type").TdFormItemProps["statusIcon"];
    };
    successBorder: BooleanConstructor;
    tips: {
        type: import("vue").PropType<import("./type").TdFormItemProps["tips"]>;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    for: {
        type: StringConstructor;
        default: string;
    };
    help: {
        type: import("vue").PropType<import("./type").TdFormItemProps["help"]>;
    };
    label: {
        type: import("vue").PropType<import("./type").TdFormItemProps["label"]>;
        default: import("./type").TdFormItemProps["label"];
    };
    labelAlign: {
        type: import("vue").PropType<import("./type").TdFormItemProps["labelAlign"]>;
        validator(val: import("./type").TdFormItemProps["labelAlign"]): boolean;
    };
    labelWidth: {
        type: import("vue").PropType<import("./type").TdFormItemProps["labelWidth"]>;
    };
    name: {
        type: StringConstructor;
        default: string;
    };
    requiredMark: {
        type: BooleanConstructor;
        default: any;
    };
    rules: {
        type: import("vue").PropType<import("./type").TdFormItemProps["rules"]>;
    };
    showErrorMessage: {
        type: BooleanConstructor;
        default: any;
    };
    status: {
        type: import("vue").PropType<import("./type").TdFormItemProps["status"]>;
        default: import("./type").TdFormItemProps["status"];
    };
    statusIcon: {
        type: import("vue").PropType<import("./type").TdFormItemProps["statusIcon"]>;
        default: import("./type").TdFormItemProps["statusIcon"];
    };
    successBorder: BooleanConstructor;
    tips: {
        type: import("vue").PropType<import("./type").TdFormItemProps["tips"]>;
    };
}>> & Readonly<{}>, {
    status: "error" | "success" | "warning";
    label: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    name: string;
    showErrorMessage: boolean;
    statusIcon: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    for: string;
    requiredMark: boolean;
    successBorder: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
