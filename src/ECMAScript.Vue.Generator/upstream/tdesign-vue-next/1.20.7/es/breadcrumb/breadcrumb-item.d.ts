declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    isEllipsisItem: BooleanConstructor;
    content: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["default"]>;
    };
    disabled: BooleanConstructor;
    href: {
        type: StringConstructor;
        default: string;
    };
    icon: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["icon"]>;
    };
    maxWidth: {
        type: StringConstructor;
        default: any;
    };
    replace: BooleanConstructor;
    router: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["router"]>;
    };
    target: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["target"]>;
        default: import("./type").TdBreadcrumbItemProps["target"];
        validator(val: import("./type").TdBreadcrumbItemProps["target"]): boolean;
    };
    to: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["to"]>;
    };
    onClick: import("vue").PropType<import("./type").TdBreadcrumbItemProps["onClick"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    isEllipsisItem: BooleanConstructor;
    content: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["default"]>;
    };
    disabled: BooleanConstructor;
    href: {
        type: StringConstructor;
        default: string;
    };
    icon: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["icon"]>;
    };
    maxWidth: {
        type: StringConstructor;
        default: any;
    };
    replace: BooleanConstructor;
    router: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["router"]>;
    };
    target: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["target"]>;
        default: import("./type").TdBreadcrumbItemProps["target"];
        validator(val: import("./type").TdBreadcrumbItemProps["target"]): boolean;
    };
    to: {
        type: import("vue").PropType<import("./type").TdBreadcrumbItemProps["to"]>;
    };
    onClick: import("vue").PropType<import("./type").TdBreadcrumbItemProps["onClick"]>;
}>> & Readonly<{}>, {
    replace: boolean;
    maxWidth: string;
    disabled: boolean;
    href: string;
    target: "_self" | "_blank" | "_parent" | "_top";
    isEllipsisItem: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
