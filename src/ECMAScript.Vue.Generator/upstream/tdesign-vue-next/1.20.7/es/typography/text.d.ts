import type { TdTextProps } from './type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    code: BooleanConstructor;
    content: {
        type: import("vue").PropType<TdTextProps["content"]>;
    };
    copyable: {
        type: import("vue").PropType<TdTextProps["copyable"]>;
        default: TdTextProps["copyable"];
    };
    default: {
        type: import("vue").PropType<TdTextProps["default"]>;
    };
    delete: BooleanConstructor;
    disabled: BooleanConstructor;
    ellipsis: {
        type: import("vue").PropType<TdTextProps["ellipsis"]>;
        default: TdTextProps["ellipsis"];
    };
    italic: BooleanConstructor;
    keyboard: BooleanConstructor;
    mark: {
        type: import("vue").PropType<TdTextProps["mark"]>;
        default: TdTextProps["mark"];
    };
    strong: BooleanConstructor;
    theme: {
        type: import("vue").PropType<TdTextProps["theme"]>;
        validator(val: TdTextProps["theme"]): boolean;
    };
    underline: BooleanConstructor;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    code: BooleanConstructor;
    content: {
        type: import("vue").PropType<TdTextProps["content"]>;
    };
    copyable: {
        type: import("vue").PropType<TdTextProps["copyable"]>;
        default: TdTextProps["copyable"];
    };
    default: {
        type: import("vue").PropType<TdTextProps["default"]>;
    };
    delete: BooleanConstructor;
    disabled: BooleanConstructor;
    ellipsis: {
        type: import("vue").PropType<TdTextProps["ellipsis"]>;
        default: TdTextProps["ellipsis"];
    };
    italic: BooleanConstructor;
    keyboard: BooleanConstructor;
    mark: {
        type: import("vue").PropType<TdTextProps["mark"]>;
        default: TdTextProps["mark"];
    };
    strong: BooleanConstructor;
    theme: {
        type: import("vue").PropType<TdTextProps["theme"]>;
        validator(val: TdTextProps["theme"]): boolean;
    };
    underline: BooleanConstructor;
}>> & Readonly<{}>, {
    code: boolean;
    mark: string | boolean;
    strong: boolean;
    disabled: boolean;
    delete: boolean;
    italic: boolean;
    underline: boolean;
    ellipsis: boolean | import("./type").TypographyEllipsis;
    copyable: boolean | import("./type").TypographyCopyable;
    keyboard: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
