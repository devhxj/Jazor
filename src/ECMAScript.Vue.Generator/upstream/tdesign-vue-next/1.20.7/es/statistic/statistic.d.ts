declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    animation: {
        type: import("vue").PropType<import("./type").TdStatisticProps["animation"]>;
    };
    animationStart: BooleanConstructor;
    color: {
        type: StringConstructor;
        default: string;
    };
    decimalPlaces: {
        type: NumberConstructor;
    };
    extra: {
        type: import("vue").PropType<import("./type").TdStatisticProps["extra"]>;
    };
    format: {
        type: import("vue").PropType<import("./type").TdStatisticProps["format"]>;
    };
    loading: BooleanConstructor;
    prefix: {
        type: import("vue").PropType<import("./type").TdStatisticProps["prefix"]>;
    };
    separator: {
        type: StringConstructor;
        default: string;
    };
    suffix: {
        type: import("vue").PropType<import("./type").TdStatisticProps["suffix"]>;
    };
    title: {
        type: import("vue").PropType<import("./type").TdStatisticProps["title"]>;
    };
    trend: {
        type: import("vue").PropType<import("./type").TdStatisticProps["trend"]>;
        validator(val: import("./type").TdStatisticProps["trend"]): boolean;
    };
    trendPlacement: {
        type: import("vue").PropType<import("./type").TdStatisticProps["trendPlacement"]>;
        default: import("./type").TdStatisticProps["trendPlacement"];
        validator(val: import("./type").TdStatisticProps["trendPlacement"]): boolean;
    };
    unit: {
        type: import("vue").PropType<import("./type").TdStatisticProps["unit"]>;
    };
    value: {
        type: NumberConstructor;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    animation: {
        type: import("vue").PropType<import("./type").TdStatisticProps["animation"]>;
    };
    animationStart: BooleanConstructor;
    color: {
        type: StringConstructor;
        default: string;
    };
    decimalPlaces: {
        type: NumberConstructor;
    };
    extra: {
        type: import("vue").PropType<import("./type").TdStatisticProps["extra"]>;
    };
    format: {
        type: import("vue").PropType<import("./type").TdStatisticProps["format"]>;
    };
    loading: BooleanConstructor;
    prefix: {
        type: import("vue").PropType<import("./type").TdStatisticProps["prefix"]>;
    };
    separator: {
        type: StringConstructor;
        default: string;
    };
    suffix: {
        type: import("vue").PropType<import("./type").TdStatisticProps["suffix"]>;
    };
    title: {
        type: import("vue").PropType<import("./type").TdStatisticProps["title"]>;
    };
    trend: {
        type: import("vue").PropType<import("./type").TdStatisticProps["trend"]>;
        validator(val: import("./type").TdStatisticProps["trend"]): boolean;
    };
    trendPlacement: {
        type: import("vue").PropType<import("./type").TdStatisticProps["trendPlacement"]>;
        default: import("./type").TdStatisticProps["trendPlacement"];
        validator(val: import("./type").TdStatisticProps["trendPlacement"]): boolean;
    };
    unit: {
        type: import("vue").PropType<import("./type").TdStatisticProps["unit"]>;
    };
    value: {
        type: NumberConstructor;
    };
}>> & Readonly<{}>, {
    loading: boolean;
    color: string;
    separator: string;
    trendPlacement: "left" | "right";
    animationStart: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
