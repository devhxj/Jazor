import { TdSpaceProps } from './type';
import './style';
export * from './type';
export type SpaceProps = TdSpaceProps;
export declare const Space: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        forceFlexGapPolyfill: BooleanConstructor;
        align: {
            type: import("vue").PropType<TdSpaceProps["align"]>;
            validator(val: TdSpaceProps["align"]): boolean;
        };
        breakLine: BooleanConstructor;
        direction: {
            type: import("vue").PropType<TdSpaceProps["direction"]>;
            default: TdSpaceProps["direction"];
            validator(val: TdSpaceProps["direction"]): boolean;
        };
        separator: {
            type: import("vue").PropType<TdSpaceProps["separator"]>;
        };
        size: {
            type: import("vue").PropType<TdSpaceProps["size"]>;
            default: TdSpaceProps["size"];
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        size: import("./type").SpaceSize | import("./type").SpaceSize[];
        direction: "vertical" | "horizontal";
        forceFlexGapPolyfill: boolean;
        breakLine: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        forceFlexGapPolyfill: BooleanConstructor;
        align: {
            type: import("vue").PropType<TdSpaceProps["align"]>;
            validator(val: TdSpaceProps["align"]): boolean;
        };
        breakLine: BooleanConstructor;
        direction: {
            type: import("vue").PropType<TdSpaceProps["direction"]>;
            default: TdSpaceProps["direction"];
            validator(val: TdSpaceProps["direction"]): boolean;
        };
        separator: {
            type: import("vue").PropType<TdSpaceProps["separator"]>;
        };
        size: {
            type: import("vue").PropType<TdSpaceProps["size"]>;
            default: TdSpaceProps["size"];
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        size: import("./type").SpaceSize | import("./type").SpaceSize[];
        direction: "vertical" | "horizontal";
        forceFlexGapPolyfill: boolean;
        breakLine: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    forceFlexGapPolyfill: BooleanConstructor;
    align: {
        type: import("vue").PropType<TdSpaceProps["align"]>;
        validator(val: TdSpaceProps["align"]): boolean;
    };
    breakLine: BooleanConstructor;
    direction: {
        type: import("vue").PropType<TdSpaceProps["direction"]>;
        default: TdSpaceProps["direction"];
        validator(val: TdSpaceProps["direction"]): boolean;
    };
    separator: {
        type: import("vue").PropType<TdSpaceProps["separator"]>;
    };
    size: {
        type: import("vue").PropType<TdSpaceProps["size"]>;
        default: TdSpaceProps["size"];
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    size: import("./type").SpaceSize | import("./type").SpaceSize[];
    direction: "vertical" | "horizontal";
    forceFlexGapPolyfill: boolean;
    breakLine: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Space;
