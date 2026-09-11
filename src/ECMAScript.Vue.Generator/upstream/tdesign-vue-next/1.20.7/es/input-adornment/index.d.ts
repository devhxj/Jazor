import { TdInputAdornmentProps } from './type';
import './style';
export * from './type';
export type InputAdornmentProps = TdInputAdornmentProps;
export declare const InputAdornment: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        append: {
            type: import("vue").PropType<TdInputAdornmentProps["append"]>;
        };
        prepend: {
            type: import("vue").PropType<TdInputAdornmentProps["prepend"]>;
        };
    }>> & Readonly<{}>, () => string | number | boolean | void | import("vue/jsx-runtime").JSX.Element | import("vue").VNodeArrayChildren, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {}, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        append: {
            type: import("vue").PropType<TdInputAdornmentProps["append"]>;
        };
        prepend: {
            type: import("vue").PropType<TdInputAdornmentProps["prepend"]>;
        };
    }>> & Readonly<{}>, () => string | number | boolean | void | import("vue/jsx-runtime").JSX.Element | import("vue").VNodeArrayChildren, {}, {}, {}, {}>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    append: {
        type: import("vue").PropType<TdInputAdornmentProps["append"]>;
    };
    prepend: {
        type: import("vue").PropType<TdInputAdornmentProps["prepend"]>;
    };
}>> & Readonly<{}>, () => string | number | boolean | void | import("vue/jsx-runtime").JSX.Element | import("vue").VNodeArrayChildren, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default InputAdornment;
