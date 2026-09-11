declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    labelAlign: {
        type: import("vue").PropType<import("./type").TdTimelineProps["labelAlign"]>;
        default: import("./type").TdTimelineProps["labelAlign"];
        validator(val: import("./type").TdTimelineProps["labelAlign"]): boolean;
    };
    layout: {
        type: import("vue").PropType<import("./type").TdTimelineProps["layout"]>;
        default: import("./type").TdTimelineProps["layout"];
        validator(val: import("./type").TdTimelineProps["layout"]): boolean;
    };
    mode: {
        type: import("vue").PropType<import("./type").TdTimelineProps["mode"]>;
        default: import("./type").TdTimelineProps["mode"];
        validator(val: import("./type").TdTimelineProps["mode"]): boolean;
    };
    reverse: BooleanConstructor;
    theme: {
        type: import("vue").PropType<import("./type").TdTimelineProps["theme"]>;
        default: import("./type").TdTimelineProps["theme"];
        validator(val: import("./type").TdTimelineProps["theme"]): boolean;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    labelAlign: {
        type: import("vue").PropType<import("./type").TdTimelineProps["labelAlign"]>;
        default: import("./type").TdTimelineProps["labelAlign"];
        validator(val: import("./type").TdTimelineProps["labelAlign"]): boolean;
    };
    layout: {
        type: import("vue").PropType<import("./type").TdTimelineProps["layout"]>;
        default: import("./type").TdTimelineProps["layout"];
        validator(val: import("./type").TdTimelineProps["layout"]): boolean;
    };
    mode: {
        type: import("vue").PropType<import("./type").TdTimelineProps["mode"]>;
        default: import("./type").TdTimelineProps["mode"];
        validator(val: import("./type").TdTimelineProps["mode"]): boolean;
    };
    reverse: BooleanConstructor;
    theme: {
        type: import("vue").PropType<import("./type").TdTimelineProps["theme"]>;
        default: import("./type").TdTimelineProps["theme"];
        validator(val: import("./type").TdTimelineProps["theme"]): boolean;
    };
}>> & Readonly<{}>, {
    layout: "vertical" | "horizontal";
    reverse: boolean;
    mode: "same" | "alternate";
    theme: "default" | "dot";
    labelAlign: "left" | "right" | "top" | "bottom" | "alternate";
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
