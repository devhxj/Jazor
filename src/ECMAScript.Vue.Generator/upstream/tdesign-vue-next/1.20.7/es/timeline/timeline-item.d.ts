declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    index: {
        type: NumberConstructor;
    };
    content: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["content"]>;
    };
    dot: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["dot"]>;
    };
    dotColor: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["dotColor"]>;
        default: string;
    };
    label: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["label"]>;
    };
    labelAlign: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["labelAlign"]>;
        validator(val: import("./type").TdTimelineItemProps["labelAlign"]): boolean;
    };
    loading: BooleanConstructor;
    onClick: import("vue").PropType<import("./type").TdTimelineItemProps["onClick"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    index: {
        type: NumberConstructor;
    };
    content: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["content"]>;
    };
    dot: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["dot"]>;
    };
    dotColor: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["dotColor"]>;
        default: string;
    };
    label: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["label"]>;
    };
    labelAlign: {
        type: import("vue").PropType<import("./type").TdTimelineItemProps["labelAlign"]>;
        validator(val: import("./type").TdTimelineItemProps["labelAlign"]): boolean;
    };
    loading: BooleanConstructor;
    onClick: import("vue").PropType<import("./type").TdTimelineItemProps["onClick"]>;
}>> & Readonly<{}>, {
    loading: boolean;
    dotColor: string;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
