declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    content: {
        type: import("vue").PropType<import("./type").TdLinkProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdLinkProps["default"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    download: {
        type: import("vue").PropType<import("./type").TdLinkProps["download"]>;
    };
    hover: {
        type: import("vue").PropType<import("./type").TdLinkProps["hover"]>;
        default: import("./type").TdLinkProps["hover"];
        validator(val: import("./type").TdLinkProps["hover"]): boolean;
    };
    href: {
        type: StringConstructor;
        default: string;
    };
    prefixIcon: {
        type: import("vue").PropType<import("./type").TdLinkProps["prefixIcon"]>;
    };
    size: {
        type: import("vue").PropType<import("./type").TdLinkProps["size"]>;
        default: import("./type").TdLinkProps["size"];
        validator(val: import("./type").TdLinkProps["size"]): boolean;
    };
    suffixIcon: {
        type: import("vue").PropType<import("./type").TdLinkProps["suffixIcon"]>;
    };
    target: {
        type: StringConstructor;
        default: string;
    };
    theme: {
        type: import("vue").PropType<import("./type").TdLinkProps["theme"]>;
        default: import("./type").TdLinkProps["theme"];
        validator(val: import("./type").TdLinkProps["theme"]): boolean;
    };
    underline: BooleanConstructor;
    onClick: import("vue").PropType<import("./type").TdLinkProps["onClick"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "click"[], "click", import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    content: {
        type: import("vue").PropType<import("./type").TdLinkProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdLinkProps["default"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    download: {
        type: import("vue").PropType<import("./type").TdLinkProps["download"]>;
    };
    hover: {
        type: import("vue").PropType<import("./type").TdLinkProps["hover"]>;
        default: import("./type").TdLinkProps["hover"];
        validator(val: import("./type").TdLinkProps["hover"]): boolean;
    };
    href: {
        type: StringConstructor;
        default: string;
    };
    prefixIcon: {
        type: import("vue").PropType<import("./type").TdLinkProps["prefixIcon"]>;
    };
    size: {
        type: import("vue").PropType<import("./type").TdLinkProps["size"]>;
        default: import("./type").TdLinkProps["size"];
        validator(val: import("./type").TdLinkProps["size"]): boolean;
    };
    suffixIcon: {
        type: import("vue").PropType<import("./type").TdLinkProps["suffixIcon"]>;
    };
    target: {
        type: StringConstructor;
        default: string;
    };
    theme: {
        type: import("vue").PropType<import("./type").TdLinkProps["theme"]>;
        default: import("./type").TdLinkProps["theme"];
        validator(val: import("./type").TdLinkProps["theme"]): boolean;
    };
    underline: BooleanConstructor;
    onClick: import("vue").PropType<import("./type").TdLinkProps["onClick"]>;
}>> & Readonly<{
    onClick?: (...args: any[]) => any;
}>, {
    size: import("..").SizeEnum;
    disabled: boolean;
    theme: "default" | "primary" | "success" | "warning" | "danger";
    href: string;
    target: string;
    hover: "color" | "underline";
    underline: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
