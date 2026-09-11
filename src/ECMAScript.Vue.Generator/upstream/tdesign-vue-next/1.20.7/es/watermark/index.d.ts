export * from './type';
export declare const Watermark: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        alpha: {
            type: NumberConstructor;
            default: number;
        };
        content: {
            type: import("vue").PropType<import("./type").TdWatermarkProps["content"]>;
        };
        default: {
            type: import("vue").PropType<import("./type").TdWatermarkProps["default"]>;
        };
        height: {
            type: NumberConstructor;
        };
        isRepeat: {
            type: BooleanConstructor;
            default: boolean;
        };
        layout: {
            type: import("vue").PropType<import("./type").TdWatermarkProps["layout"]>;
            default: import("./type").TdWatermarkProps["layout"];
            validator(val: import("./type").TdWatermarkProps["layout"]): boolean;
        };
        lineSpace: {
            type: NumberConstructor;
            default: number;
        };
        movable: BooleanConstructor;
        moveInterval: {
            type: NumberConstructor;
            default: number;
        };
        offset: {
            type: import("vue").PropType<import("./type").TdWatermarkProps["offset"]>;
        };
        removable: {
            type: BooleanConstructor;
            default: boolean;
        };
        rotate: {
            type: NumberConstructor;
            default: number;
        };
        watermarkContent: {
            type: import("vue").PropType<import("./type").TdWatermarkProps["watermarkContent"]>;
        };
        width: {
            type: NumberConstructor;
        };
        x: {
            type: NumberConstructor;
        };
        y: {
            type: NumberConstructor;
        };
        zIndex: {
            type: NumberConstructor;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        layout: "rectangular" | "hexagonal";
        alpha: number;
        rotate: number;
        lineSpace: number;
        removable: boolean;
        isRepeat: boolean;
        movable: boolean;
        moveInterval: number;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        alpha: {
            type: NumberConstructor;
            default: number;
        };
        content: {
            type: import("vue").PropType<import("./type").TdWatermarkProps["content"]>;
        };
        default: {
            type: import("vue").PropType<import("./type").TdWatermarkProps["default"]>;
        };
        height: {
            type: NumberConstructor;
        };
        isRepeat: {
            type: BooleanConstructor;
            default: boolean;
        };
        layout: {
            type: import("vue").PropType<import("./type").TdWatermarkProps["layout"]>;
            default: import("./type").TdWatermarkProps["layout"];
            validator(val: import("./type").TdWatermarkProps["layout"]): boolean;
        };
        lineSpace: {
            type: NumberConstructor;
            default: number;
        };
        movable: BooleanConstructor;
        moveInterval: {
            type: NumberConstructor;
            default: number;
        };
        offset: {
            type: import("vue").PropType<import("./type").TdWatermarkProps["offset"]>;
        };
        removable: {
            type: BooleanConstructor;
            default: boolean;
        };
        rotate: {
            type: NumberConstructor;
            default: number;
        };
        watermarkContent: {
            type: import("vue").PropType<import("./type").TdWatermarkProps["watermarkContent"]>;
        };
        width: {
            type: NumberConstructor;
        };
        x: {
            type: NumberConstructor;
        };
        y: {
            type: NumberConstructor;
        };
        zIndex: {
            type: NumberConstructor;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        layout: "rectangular" | "hexagonal";
        alpha: number;
        rotate: number;
        lineSpace: number;
        removable: boolean;
        isRepeat: boolean;
        movable: boolean;
        moveInterval: number;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    alpha: {
        type: NumberConstructor;
        default: number;
    };
    content: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["default"]>;
    };
    height: {
        type: NumberConstructor;
    };
    isRepeat: {
        type: BooleanConstructor;
        default: boolean;
    };
    layout: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["layout"]>;
        default: import("./type").TdWatermarkProps["layout"];
        validator(val: import("./type").TdWatermarkProps["layout"]): boolean;
    };
    lineSpace: {
        type: NumberConstructor;
        default: number;
    };
    movable: BooleanConstructor;
    moveInterval: {
        type: NumberConstructor;
        default: number;
    };
    offset: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["offset"]>;
    };
    removable: {
        type: BooleanConstructor;
        default: boolean;
    };
    rotate: {
        type: NumberConstructor;
        default: number;
    };
    watermarkContent: {
        type: import("vue").PropType<import("./type").TdWatermarkProps["watermarkContent"]>;
    };
    width: {
        type: NumberConstructor;
    };
    x: {
        type: NumberConstructor;
    };
    y: {
        type: NumberConstructor;
    };
    zIndex: {
        type: NumberConstructor;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    layout: "rectangular" | "hexagonal";
    alpha: number;
    rotate: number;
    lineSpace: number;
    removable: boolean;
    isRepeat: boolean;
    movable: boolean;
    moveInterval: number;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Watermark;
