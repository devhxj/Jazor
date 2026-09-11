declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    content: {
        type: import("vue").PropType<import("./type").TdTitleProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdTitleProps["default"]>;
    };
    ellipsis: {
        type: import("vue").PropType<import("./type").TdTitleProps["ellipsis"]>;
        default: import("./type").TdTitleProps["ellipsis"];
    };
    level: {
        type: import("vue").PropType<import("./type").TdTitleProps["level"]>;
        default: import("./type").TdTitleProps["level"];
        validator(val: import("./type").TdTitleProps["level"]): boolean;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    content: {
        type: import("vue").PropType<import("./type").TdTitleProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdTitleProps["default"]>;
    };
    ellipsis: {
        type: import("vue").PropType<import("./type").TdTitleProps["ellipsis"]>;
        default: import("./type").TdTitleProps["ellipsis"];
    };
    level: {
        type: import("vue").PropType<import("./type").TdTitleProps["level"]>;
        default: import("./type").TdTitleProps["level"];
        validator(val: import("./type").TdTitleProps["level"]): boolean;
    };
}>> & Readonly<{}>, {
    level: "h1" | "h2" | "h3" | "h4" | "h5" | "h6";
    ellipsis: boolean | import("./type").TypographyEllipsis;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
