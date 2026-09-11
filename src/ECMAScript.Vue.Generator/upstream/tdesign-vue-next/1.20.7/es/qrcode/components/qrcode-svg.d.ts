declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    value: {
        type: StringConstructor;
        default: string;
    };
    size: {
        type: NumberConstructor;
        default: number;
    };
    level: {
        type: import("vue").PropType<import("./type").QRCodeSubComponent["level"]>;
        default: string;
    };
    bgColor: {
        type: StringConstructor;
        default: string;
    };
    fgColor: {
        type: StringConstructor;
        default: string;
    };
    style: {
        type: import("vue").PropType<import("./type").QRCodeSubComponent["style"]>;
        default: () => import("./type").QRCodeSubComponent["style"];
    };
    includeMargin: {
        type: BooleanConstructor;
        default: boolean;
    };
    marginSize: {
        type: NumberConstructor;
        default: number;
    };
    imageSettings: {
        type: import("vue").PropType<import("./type").QRCodeSubComponent["imageSettings"]>;
        default: () => {};
    };
    title: {
        type: StringConstructor;
        default: string;
    };
    minVersion: {
        type: NumberConstructor;
        default: number;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    value: {
        type: StringConstructor;
        default: string;
    };
    size: {
        type: NumberConstructor;
        default: number;
    };
    level: {
        type: import("vue").PropType<import("./type").QRCodeSubComponent["level"]>;
        default: string;
    };
    bgColor: {
        type: StringConstructor;
        default: string;
    };
    fgColor: {
        type: StringConstructor;
        default: string;
    };
    style: {
        type: import("vue").PropType<import("./type").QRCodeSubComponent["style"]>;
        default: () => import("./type").QRCodeSubComponent["style"];
    };
    includeMargin: {
        type: BooleanConstructor;
        default: boolean;
    };
    marginSize: {
        type: NumberConstructor;
        default: number;
    };
    imageSettings: {
        type: import("vue").PropType<import("./type").QRCodeSubComponent["imageSettings"]>;
        default: () => {};
    };
    title: {
        type: StringConstructor;
        default: string;
    };
    minVersion: {
        type: NumberConstructor;
        default: number;
    };
}>> & Readonly<{}>, {
    value: string;
    size: number;
    style: import("vue").CSSProperties;
    title: string;
    level: import("@common/js/qrcode/types").ErrorCorrectionLevel;
    imageSettings: import("@common/js/qrcode/types").ImageSettings;
    minVersion: number;
    includeMargin: boolean;
    marginSize: number;
    bgColor: string;
    fgColor: string;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
