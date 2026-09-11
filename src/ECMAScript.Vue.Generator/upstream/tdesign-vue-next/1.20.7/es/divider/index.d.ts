import { TdDividerProps } from './type';
import './style';
export * from './type';
export type DividerProps = TdDividerProps;
export declare const Divider: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        align: {
            type: import("vue").PropType<TdDividerProps["align"]>;
            default: TdDividerProps["align"];
            validator(val: TdDividerProps["align"]): boolean;
        };
        content: {
            type: import("vue").PropType<TdDividerProps["content"]>;
        };
        dashed: BooleanConstructor;
        default: {
            type: import("vue").PropType<TdDividerProps["default"]>;
        };
        layout: {
            type: import("vue").PropType<TdDividerProps["layout"]>;
            default: TdDividerProps["layout"];
            validator(val: TdDividerProps["layout"]): boolean;
        };
        size: {
            type: import("vue").PropType<TdDividerProps["size"]>;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        layout: "vertical" | "horizontal";
        dashed: boolean;
        align: "left" | "center" | "right";
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        align: {
            type: import("vue").PropType<TdDividerProps["align"]>;
            default: TdDividerProps["align"];
            validator(val: TdDividerProps["align"]): boolean;
        };
        content: {
            type: import("vue").PropType<TdDividerProps["content"]>;
        };
        dashed: BooleanConstructor;
        default: {
            type: import("vue").PropType<TdDividerProps["default"]>;
        };
        layout: {
            type: import("vue").PropType<TdDividerProps["layout"]>;
            default: TdDividerProps["layout"];
            validator(val: TdDividerProps["layout"]): boolean;
        };
        size: {
            type: import("vue").PropType<TdDividerProps["size"]>;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        layout: "vertical" | "horizontal";
        dashed: boolean;
        align: "left" | "center" | "right";
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    align: {
        type: import("vue").PropType<TdDividerProps["align"]>;
        default: TdDividerProps["align"];
        validator(val: TdDividerProps["align"]): boolean;
    };
    content: {
        type: import("vue").PropType<TdDividerProps["content"]>;
    };
    dashed: BooleanConstructor;
    default: {
        type: import("vue").PropType<TdDividerProps["default"]>;
    };
    layout: {
        type: import("vue").PropType<TdDividerProps["layout"]>;
        default: TdDividerProps["layout"];
        validator(val: TdDividerProps["layout"]): boolean;
    };
    size: {
        type: import("vue").PropType<TdDividerProps["size"]>;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    layout: "vertical" | "horizontal";
    dashed: boolean;
    align: "left" | "center" | "right";
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Divider;
