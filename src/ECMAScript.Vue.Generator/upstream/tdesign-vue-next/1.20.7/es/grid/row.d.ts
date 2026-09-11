declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    align: {
        type: import("vue").PropType<import("./type").TdRowProps["align"]>;
        default: import("./type").TdRowProps["align"];
        validator(val: import("./type").TdRowProps["align"]): boolean;
    };
    gutter: {
        type: import("vue").PropType<import("./type").TdRowProps["gutter"]>;
        default: import("./type").TdRowProps["gutter"];
    };
    justify: {
        type: import("vue").PropType<import("./type").TdRowProps["justify"]>;
        default: import("./type").TdRowProps["justify"];
        validator(val: import("./type").TdRowProps["justify"]): boolean;
    };
    tag: {
        type: StringConstructor;
        default: string;
    };
}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
    [key: string]: any;
}>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    align: {
        type: import("vue").PropType<import("./type").TdRowProps["align"]>;
        default: import("./type").TdRowProps["align"];
        validator(val: import("./type").TdRowProps["align"]): boolean;
    };
    gutter: {
        type: import("vue").PropType<import("./type").TdRowProps["gutter"]>;
        default: import("./type").TdRowProps["gutter"];
    };
    justify: {
        type: import("vue").PropType<import("./type").TdRowProps["justify"]>;
        default: import("./type").TdRowProps["justify"];
        validator(val: import("./type").TdRowProps["justify"]): boolean;
    };
    tag: {
        type: StringConstructor;
        default: string;
    };
}>> & Readonly<{}>, {
    tag: string;
    justify: "center" | "start" | "end" | "space-around" | "space-between";
    align: "center" | "top" | "middle" | "bottom" | "start" | "end" | "stretch" | "baseline";
    gutter: number | import("./type").GutterObject | (number | import("./type").GutterObject)[];
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
