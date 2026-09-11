declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    bgColor: {
        type: StringConstructor;
        default: string;
    };
    borderless: BooleanConstructor;
    color: {
        type: StringConstructor;
        default: string;
    };
    icon: {
        type: StringConstructor;
        default: string;
    };
    iconSize: {
        type: import("vue").PropType<import("./type").TdQRCodeProps["iconSize"]>;
        default: import("./type").TdQRCodeProps["iconSize"];
    };
    level: {
        type: import("vue").PropType<import("./type").TdQRCodeProps["level"]>;
        default: import("./type").TdQRCodeProps["level"];
        validator(val: import("./type").TdQRCodeProps["level"]): boolean;
    };
    size: {
        type: NumberConstructor;
        default: number;
    };
    status: {
        type: import("vue").PropType<import("./type").TdQRCodeProps["status"]>;
        default: import("./type").TdQRCodeProps["status"];
        validator(val: import("./type").TdQRCodeProps["status"]): boolean;
    };
    statusRender: {
        type: import("vue").PropType<import("./type").TdQRCodeProps["statusRender"]>;
    };
    type: {
        type: import("vue").PropType<import("./type").TdQRCodeProps["type"]>;
        default: import("./type").TdQRCodeProps["type"];
        validator(val: import("./type").TdQRCodeProps["type"]): boolean;
    };
    value: {
        type: StringConstructor;
        default: string;
    };
    onRefresh: import("vue").PropType<import("./type").TdQRCodeProps["onRefresh"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    bgColor: {
        type: StringConstructor;
        default: string;
    };
    borderless: BooleanConstructor;
    color: {
        type: StringConstructor;
        default: string;
    };
    icon: {
        type: StringConstructor;
        default: string;
    };
    iconSize: {
        type: import("vue").PropType<import("./type").TdQRCodeProps["iconSize"]>;
        default: import("./type").TdQRCodeProps["iconSize"];
    };
    level: {
        type: import("vue").PropType<import("./type").TdQRCodeProps["level"]>;
        default: import("./type").TdQRCodeProps["level"];
        validator(val: import("./type").TdQRCodeProps["level"]): boolean;
    };
    size: {
        type: NumberConstructor;
        default: number;
    };
    status: {
        type: import("vue").PropType<import("./type").TdQRCodeProps["status"]>;
        default: import("./type").TdQRCodeProps["status"];
        validator(val: import("./type").TdQRCodeProps["status"]): boolean;
    };
    statusRender: {
        type: import("vue").PropType<import("./type").TdQRCodeProps["statusRender"]>;
    };
    type: {
        type: import("vue").PropType<import("./type").TdQRCodeProps["type"]>;
        default: import("./type").TdQRCodeProps["type"];
        validator(val: import("./type").TdQRCodeProps["type"]): boolean;
    };
    value: {
        type: StringConstructor;
        default: string;
    };
    onRefresh: import("vue").PropType<import("./type").TdQRCodeProps["onRefresh"]>;
}>> & Readonly<{}>, {
    icon: string;
    color: string;
    value: string;
    type: "canvas" | "svg";
    size: number;
    status: import("./type").QRStatus;
    level: "M" | "Q" | "L" | "H";
    borderless: boolean;
    iconSize: number | {
        width: number;
        height: number;
    };
    bgColor: string;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
