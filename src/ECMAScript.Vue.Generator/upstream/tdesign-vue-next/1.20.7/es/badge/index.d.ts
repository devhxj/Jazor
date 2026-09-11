import { TdBadgeProps } from './type';
import './style';
export * from './type';
export type BadgeProps = TdBadgeProps;
export declare const Badge: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        color: {
            type: StringConstructor;
            default: string;
        };
        content: {
            type: import("vue").PropType<TdBadgeProps["content"]>;
        };
        count: {
            type: import("vue").PropType<TdBadgeProps["count"]>;
            default: number;
        };
        default: {
            type: import("vue").PropType<TdBadgeProps["default"]>;
        };
        dot: BooleanConstructor;
        maxCount: {
            type: NumberConstructor;
            default: number;
        };
        offset: {
            type: import("vue").PropType<TdBadgeProps["offset"]>;
        };
        shape: {
            type: import("vue").PropType<TdBadgeProps["shape"]>;
            default: TdBadgeProps["shape"];
            validator(val: TdBadgeProps["shape"]): boolean;
        };
        showZero: BooleanConstructor;
        size: {
            type: import("vue").PropType<TdBadgeProps["size"]>;
            default: TdBadgeProps["size"];
            validator(val: TdBadgeProps["size"]): boolean;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        color: string;
        count: string | number | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        size: "small" | "medium";
        shape: "circle" | "round";
        dot: boolean;
        maxCount: number;
        showZero: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        color: {
            type: StringConstructor;
            default: string;
        };
        content: {
            type: import("vue").PropType<TdBadgeProps["content"]>;
        };
        count: {
            type: import("vue").PropType<TdBadgeProps["count"]>;
            default: number;
        };
        default: {
            type: import("vue").PropType<TdBadgeProps["default"]>;
        };
        dot: BooleanConstructor;
        maxCount: {
            type: NumberConstructor;
            default: number;
        };
        offset: {
            type: import("vue").PropType<TdBadgeProps["offset"]>;
        };
        shape: {
            type: import("vue").PropType<TdBadgeProps["shape"]>;
            default: TdBadgeProps["shape"];
            validator(val: TdBadgeProps["shape"]): boolean;
        };
        showZero: BooleanConstructor;
        size: {
            type: import("vue").PropType<TdBadgeProps["size"]>;
            default: TdBadgeProps["size"];
            validator(val: TdBadgeProps["size"]): boolean;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        color: string;
        count: string | number | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        size: "small" | "medium";
        shape: "circle" | "round";
        dot: boolean;
        maxCount: number;
        showZero: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    color: {
        type: StringConstructor;
        default: string;
    };
    content: {
        type: import("vue").PropType<TdBadgeProps["content"]>;
    };
    count: {
        type: import("vue").PropType<TdBadgeProps["count"]>;
        default: number;
    };
    default: {
        type: import("vue").PropType<TdBadgeProps["default"]>;
    };
    dot: BooleanConstructor;
    maxCount: {
        type: NumberConstructor;
        default: number;
    };
    offset: {
        type: import("vue").PropType<TdBadgeProps["offset"]>;
    };
    shape: {
        type: import("vue").PropType<TdBadgeProps["shape"]>;
        default: TdBadgeProps["shape"];
        validator(val: TdBadgeProps["shape"]): boolean;
    };
    showZero: BooleanConstructor;
    size: {
        type: import("vue").PropType<TdBadgeProps["size"]>;
        default: TdBadgeProps["size"];
        validator(val: TdBadgeProps["size"]): boolean;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    color: string;
    count: string | number | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    size: "small" | "medium";
    shape: "circle" | "round";
    dot: boolean;
    maxCount: number;
    showZero: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Badge;
