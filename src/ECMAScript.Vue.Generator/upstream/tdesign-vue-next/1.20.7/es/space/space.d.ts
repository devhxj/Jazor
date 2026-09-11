declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    forceFlexGapPolyfill: BooleanConstructor;
    align: {
        type: import("vue").PropType<import("./type").TdSpaceProps["align"]>;
        validator(val: import("./type").TdSpaceProps["align"]): boolean;
    };
    breakLine: BooleanConstructor;
    direction: {
        type: import("vue").PropType<import("./type").TdSpaceProps["direction"]>;
        default: import("./type").TdSpaceProps["direction"];
        validator(val: import("./type").TdSpaceProps["direction"]): boolean;
    };
    separator: {
        type: import("vue").PropType<import("./type").TdSpaceProps["separator"]>;
    };
    size: {
        type: import("vue").PropType<import("./type").TdSpaceProps["size"]>;
        default: import("./type").TdSpaceProps["size"];
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    forceFlexGapPolyfill: BooleanConstructor;
    align: {
        type: import("vue").PropType<import("./type").TdSpaceProps["align"]>;
        validator(val: import("./type").TdSpaceProps["align"]): boolean;
    };
    breakLine: BooleanConstructor;
    direction: {
        type: import("vue").PropType<import("./type").TdSpaceProps["direction"]>;
        default: import("./type").TdSpaceProps["direction"];
        validator(val: import("./type").TdSpaceProps["direction"]): boolean;
    };
    separator: {
        type: import("vue").PropType<import("./type").TdSpaceProps["separator"]>;
    };
    size: {
        type: import("vue").PropType<import("./type").TdSpaceProps["size"]>;
        default: import("./type").TdSpaceProps["size"];
    };
}>> & Readonly<{}>, {
    size: import("./type").SpaceSize | import("./type").SpaceSize[];
    direction: "vertical" | "horizontal";
    forceFlexGapPolyfill: boolean;
    breakLine: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
