import { TdStatisticProps } from './type';
import './style';
export * from './type';
export type StatisticProps = TdStatisticProps;
export declare const Statistic: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        animation: {
            type: import("vue").PropType<TdStatisticProps["animation"]>;
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
            type: import("vue").PropType<TdStatisticProps["extra"]>;
        };
        format: {
            type: import("vue").PropType<TdStatisticProps["format"]>;
        };
        loading: BooleanConstructor;
        prefix: {
            type: import("vue").PropType<TdStatisticProps["prefix"]>;
        };
        separator: {
            type: StringConstructor;
            default: string;
        };
        suffix: {
            type: import("vue").PropType<TdStatisticProps["suffix"]>;
        };
        title: {
            type: import("vue").PropType<TdStatisticProps["title"]>;
        };
        trend: {
            type: import("vue").PropType<TdStatisticProps["trend"]>;
            validator(val: TdStatisticProps["trend"]): boolean;
        };
        trendPlacement: {
            type: import("vue").PropType<TdStatisticProps["trendPlacement"]>;
            default: TdStatisticProps["trendPlacement"];
            validator(val: TdStatisticProps["trendPlacement"]): boolean;
        };
        unit: {
            type: import("vue").PropType<TdStatisticProps["unit"]>;
        };
        value: {
            type: NumberConstructor;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        loading: boolean;
        color: string;
        separator: string;
        trendPlacement: "left" | "right";
        animationStart: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        animation: {
            type: import("vue").PropType<TdStatisticProps["animation"]>;
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
            type: import("vue").PropType<TdStatisticProps["extra"]>;
        };
        format: {
            type: import("vue").PropType<TdStatisticProps["format"]>;
        };
        loading: BooleanConstructor;
        prefix: {
            type: import("vue").PropType<TdStatisticProps["prefix"]>;
        };
        separator: {
            type: StringConstructor;
            default: string;
        };
        suffix: {
            type: import("vue").PropType<TdStatisticProps["suffix"]>;
        };
        title: {
            type: import("vue").PropType<TdStatisticProps["title"]>;
        };
        trend: {
            type: import("vue").PropType<TdStatisticProps["trend"]>;
            validator(val: TdStatisticProps["trend"]): boolean;
        };
        trendPlacement: {
            type: import("vue").PropType<TdStatisticProps["trendPlacement"]>;
            default: TdStatisticProps["trendPlacement"];
            validator(val: TdStatisticProps["trendPlacement"]): boolean;
        };
        unit: {
            type: import("vue").PropType<TdStatisticProps["unit"]>;
        };
        value: {
            type: NumberConstructor;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        loading: boolean;
        color: string;
        separator: string;
        trendPlacement: "left" | "right";
        animationStart: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    animation: {
        type: import("vue").PropType<TdStatisticProps["animation"]>;
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
        type: import("vue").PropType<TdStatisticProps["extra"]>;
    };
    format: {
        type: import("vue").PropType<TdStatisticProps["format"]>;
    };
    loading: BooleanConstructor;
    prefix: {
        type: import("vue").PropType<TdStatisticProps["prefix"]>;
    };
    separator: {
        type: StringConstructor;
        default: string;
    };
    suffix: {
        type: import("vue").PropType<TdStatisticProps["suffix"]>;
    };
    title: {
        type: import("vue").PropType<TdStatisticProps["title"]>;
    };
    trend: {
        type: import("vue").PropType<TdStatisticProps["trend"]>;
        validator(val: TdStatisticProps["trend"]): boolean;
    };
    trendPlacement: {
        type: import("vue").PropType<TdStatisticProps["trendPlacement"]>;
        default: TdStatisticProps["trendPlacement"];
        validator(val: TdStatisticProps["trendPlacement"]): boolean;
    };
    unit: {
        type: import("vue").PropType<TdStatisticProps["unit"]>;
    };
    value: {
        type: NumberConstructor;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    loading: boolean;
    color: string;
    separator: string;
    trendPlacement: "left" | "right";
    animationStart: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Statistic;
