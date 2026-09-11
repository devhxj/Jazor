import { TdAffixProps } from './type';
import './style';
export * from './type';
export declare const Affix: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        container: {
            type: import("vue").PropType<TdAffixProps["container"]>;
            default: () => TdAffixProps["container"];
        };
        content: {
            type: import("vue").PropType<TdAffixProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdAffixProps["default"]>;
        };
        offsetBottom: {
            type: NumberConstructor;
            default: number;
        };
        offsetTop: {
            type: NumberConstructor;
            default: number;
        };
        zIndex: {
            type: NumberConstructor;
        };
        onFixedChange: import("vue").PropType<TdAffixProps["onFixedChange"]>;
    }>> & Readonly<{
        onFixedChange?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "fixedChange"[], import("vue").PublicProps, {
        offsetTop: number;
        container: import("..").ScrollContainer;
        offsetBottom: number;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        container: {
            type: import("vue").PropType<TdAffixProps["container"]>;
            default: () => TdAffixProps["container"];
        };
        content: {
            type: import("vue").PropType<TdAffixProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdAffixProps["default"]>;
        };
        offsetBottom: {
            type: NumberConstructor;
            default: number;
        };
        offsetTop: {
            type: NumberConstructor;
            default: number;
        };
        zIndex: {
            type: NumberConstructor;
        };
        onFixedChange: import("vue").PropType<TdAffixProps["onFixedChange"]>;
    }>> & Readonly<{
        onFixedChange?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        offsetTop: number;
        container: import("..").ScrollContainer;
        offsetBottom: number;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    container: {
        type: import("vue").PropType<TdAffixProps["container"]>;
        default: () => TdAffixProps["container"];
    };
    content: {
        type: import("vue").PropType<TdAffixProps["content"]>;
    };
    default: {
        type: import("vue").PropType<TdAffixProps["default"]>;
    };
    offsetBottom: {
        type: NumberConstructor;
        default: number;
    };
    offsetTop: {
        type: NumberConstructor;
        default: number;
    };
    zIndex: {
        type: NumberConstructor;
    };
    onFixedChange: import("vue").PropType<TdAffixProps["onFixedChange"]>;
}>> & Readonly<{
    onFixedChange?: (...args: any[]) => any;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "fixedChange"[], "fixedChange", {
    offsetTop: number;
    container: import("..").ScrollContainer;
    offsetBottom: number;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export type AffixProps = TdAffixProps;
export default Affix;
