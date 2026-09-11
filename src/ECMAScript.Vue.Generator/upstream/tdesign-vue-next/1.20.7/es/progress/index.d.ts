import { TdProgressProps } from './type';
import './style';
export type ProgressProps = TdProgressProps;
export * from './type';
export declare const Progress: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        color: {
            type: import("vue").PropType<TdProgressProps["color"]>;
            default: TdProgressProps["color"];
        };
        label: {
            type: import("vue").PropType<TdProgressProps["label"]>;
            default: TdProgressProps["label"];
        };
        percentage: {
            type: NumberConstructor;
            default: number;
        };
        size: {
            type: import("vue").PropType<TdProgressProps["size"]>;
            default: TdProgressProps["size"];
        };
        status: {
            type: import("vue").PropType<TdProgressProps["status"]>;
            validator(val: TdProgressProps["status"]): boolean;
        };
        strokeWidth: {
            type: import("vue").PropType<TdProgressProps["strokeWidth"]>;
        };
        theme: {
            type: import("vue").PropType<TdProgressProps["theme"]>;
            default: TdProgressProps["theme"];
            validator(val: TdProgressProps["theme"]): boolean;
        };
        trackColor: {
            type: StringConstructor;
            default: string;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        color: string | Record<string, string> | string[];
        size: string | number;
        label: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        theme: import("./type").ProgressTheme;
        percentage: number;
        trackColor: string;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        color: {
            type: import("vue").PropType<TdProgressProps["color"]>;
            default: TdProgressProps["color"];
        };
        label: {
            type: import("vue").PropType<TdProgressProps["label"]>;
            default: TdProgressProps["label"];
        };
        percentage: {
            type: NumberConstructor;
            default: number;
        };
        size: {
            type: import("vue").PropType<TdProgressProps["size"]>;
            default: TdProgressProps["size"];
        };
        status: {
            type: import("vue").PropType<TdProgressProps["status"]>;
            validator(val: TdProgressProps["status"]): boolean;
        };
        strokeWidth: {
            type: import("vue").PropType<TdProgressProps["strokeWidth"]>;
        };
        theme: {
            type: import("vue").PropType<TdProgressProps["theme"]>;
            default: TdProgressProps["theme"];
            validator(val: TdProgressProps["theme"]): boolean;
        };
        trackColor: {
            type: StringConstructor;
            default: string;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        color: string | Record<string, string> | string[];
        size: string | number;
        label: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        theme: import("./type").ProgressTheme;
        percentage: number;
        trackColor: string;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    color: {
        type: import("vue").PropType<TdProgressProps["color"]>;
        default: TdProgressProps["color"];
    };
    label: {
        type: import("vue").PropType<TdProgressProps["label"]>;
        default: TdProgressProps["label"];
    };
    percentage: {
        type: NumberConstructor;
        default: number;
    };
    size: {
        type: import("vue").PropType<TdProgressProps["size"]>;
        default: TdProgressProps["size"];
    };
    status: {
        type: import("vue").PropType<TdProgressProps["status"]>;
        validator(val: TdProgressProps["status"]): boolean;
    };
    strokeWidth: {
        type: import("vue").PropType<TdProgressProps["strokeWidth"]>;
    };
    theme: {
        type: import("vue").PropType<TdProgressProps["theme"]>;
        default: TdProgressProps["theme"];
        validator(val: TdProgressProps["theme"]): boolean;
    };
    trackColor: {
        type: StringConstructor;
        default: string;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    color: string | Record<string, string> | string[];
    size: string | number;
    label: string | boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    theme: import("./type").ProgressTheme;
    percentage: number;
    trackColor: string;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Progress;
