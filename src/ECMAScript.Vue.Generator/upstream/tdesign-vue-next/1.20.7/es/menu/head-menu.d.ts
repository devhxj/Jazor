import { MenuValue } from './type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    expanded: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["expanded"]>;
        default: any;
    };
    defaultExpanded: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["defaultExpanded"]>;
        default: any[];
    };
    expandType: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["expandType"]>;
        default: import("./type").TdHeadMenuProps["expandType"];
        validator(val: import("./type").TdHeadMenuProps["expandType"]): boolean;
    };
    logo: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["logo"]>;
    };
    operations: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["operations"]>;
    };
    theme: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["theme"]>;
        default: import("./type").TdHeadMenuProps["theme"];
        validator(val: import("./type").TdHeadMenuProps["theme"]): boolean;
    };
    value: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["value"]>;
        default: import("./type").TdHeadMenuProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["value"]>;
        default: import("./type").TdHeadMenuProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["defaultValue"]>;
    };
    onChange: import("vue").PropType<import("./type").TdHeadMenuProps["onChange"]>;
    onExpand: import("vue").PropType<import("./type").TdHeadMenuProps["onExpand"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    expanded: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["expanded"]>;
        default: any;
    };
    defaultExpanded: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["defaultExpanded"]>;
        default: any[];
    };
    expandType: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["expandType"]>;
        default: import("./type").TdHeadMenuProps["expandType"];
        validator(val: import("./type").TdHeadMenuProps["expandType"]): boolean;
    };
    logo: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["logo"]>;
    };
    operations: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["operations"]>;
    };
    theme: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["theme"]>;
        default: import("./type").TdHeadMenuProps["theme"];
        validator(val: import("./type").TdHeadMenuProps["theme"]): boolean;
    };
    value: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["value"]>;
        default: import("./type").TdHeadMenuProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["value"]>;
        default: import("./type").TdHeadMenuProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<import("./type").TdHeadMenuProps["defaultValue"]>;
    };
    onChange: import("vue").PropType<import("./type").TdHeadMenuProps["onChange"]>;
    onExpand: import("vue").PropType<import("./type").TdHeadMenuProps["onExpand"]>;
}>> & Readonly<{}>, {
    value: MenuValue;
    expanded: MenuValue[];
    theme: "dark" | "light";
    modelValue: MenuValue;
    defaultExpanded: MenuValue[];
    expandType: "popup" | "normal";
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
