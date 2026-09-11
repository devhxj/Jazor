declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    closable: BooleanConstructor;
    color: {
        type: StringConstructor;
        default: string;
    };
    content: {
        type: import("vue").PropType<import("./type").TdTagProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdTagProps["default"]>;
    };
    disabled: BooleanConstructor;
    icon: {
        type: import("vue").PropType<import("./type").TdTagProps["icon"]>;
        default: any;
    };
    maxWidth: {
        type: import("vue").PropType<import("./type").TdTagProps["maxWidth"]>;
    };
    shape: {
        type: import("vue").PropType<import("./type").TdTagProps["shape"]>;
        default: import("./type").TdTagProps["shape"];
        validator(val: import("./type").TdTagProps["shape"]): boolean;
    };
    size: {
        type: import("vue").PropType<import("./type").TdTagProps["size"]>;
        default: import("./type").TdTagProps["size"];
        validator(val: import("./type").TdTagProps["size"]): boolean;
    };
    theme: {
        type: import("vue").PropType<import("./type").TdTagProps["theme"]>;
        default: import("./type").TdTagProps["theme"];
        validator(val: import("./type").TdTagProps["theme"]): boolean;
    };
    title: {
        type: import("vue").PropType<import("./type").TdTagProps["title"]>;
    };
    variant: {
        type: import("vue").PropType<import("./type").TdTagProps["variant"]>;
        default: import("./type").TdTagProps["variant"];
        validator(val: import("./type").TdTagProps["variant"]): boolean;
    };
    onClick: import("vue").PropType<import("./type").TdTagProps["onClick"]>;
    onClose: import("vue").PropType<import("./type").TdTagProps["onClose"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    closable: BooleanConstructor;
    color: {
        type: StringConstructor;
        default: string;
    };
    content: {
        type: import("vue").PropType<import("./type").TdTagProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdTagProps["default"]>;
    };
    disabled: BooleanConstructor;
    icon: {
        type: import("vue").PropType<import("./type").TdTagProps["icon"]>;
        default: any;
    };
    maxWidth: {
        type: import("vue").PropType<import("./type").TdTagProps["maxWidth"]>;
    };
    shape: {
        type: import("vue").PropType<import("./type").TdTagProps["shape"]>;
        default: import("./type").TdTagProps["shape"];
        validator(val: import("./type").TdTagProps["shape"]): boolean;
    };
    size: {
        type: import("vue").PropType<import("./type").TdTagProps["size"]>;
        default: import("./type").TdTagProps["size"];
        validator(val: import("./type").TdTagProps["size"]): boolean;
    };
    theme: {
        type: import("vue").PropType<import("./type").TdTagProps["theme"]>;
        default: import("./type").TdTagProps["theme"];
        validator(val: import("./type").TdTagProps["theme"]): boolean;
    };
    title: {
        type: import("vue").PropType<import("./type").TdTagProps["title"]>;
    };
    variant: {
        type: import("vue").PropType<import("./type").TdTagProps["variant"]>;
        default: import("./type").TdTagProps["variant"];
        validator(val: import("./type").TdTagProps["variant"]): boolean;
    };
    onClick: import("vue").PropType<import("./type").TdTagProps["onClick"]>;
    onClose: import("vue").PropType<import("./type").TdTagProps["onClose"]>;
}>> & Readonly<{}>, {
    icon: (h: typeof import("vue").h) => import("..").TNodeReturnValue;
    color: string;
    size: import("..").SizeEnum;
    disabled: boolean;
    theme: "default" | "primary" | "success" | "warning" | "danger";
    variant: "outline" | "dark" | "light" | "light-outline";
    shape: "mark" | "round" | "square";
    closable: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
