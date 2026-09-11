import { TdSwitchProps } from './type';
import './style';
export * from './type';
export type SwitchProps = TdSwitchProps;
export declare const Switch: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        beforeChange: {
            type: import("vue").PropType<TdSwitchProps["beforeChange"]>;
        };
        customValue: {
            type: import("vue").PropType<TdSwitchProps["customValue"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        label: {
            type: import("vue").PropType<TdSwitchProps["label"]>;
            default: () => TdSwitchProps["label"];
        };
        loading: BooleanConstructor;
        size: {
            type: import("vue").PropType<TdSwitchProps["size"]>;
            default: TdSwitchProps["size"];
            validator(val: TdSwitchProps["size"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdSwitchProps["value"]>;
            default: TdSwitchProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdSwitchProps["value"]>;
            default: TdSwitchProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdSwitchProps["defaultValue"]>;
        };
        onChange: import("vue").PropType<TdSwitchProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        loading: boolean;
        value: import("./type").SwitchValue;
        size: "small" | "medium" | "large";
        label: (string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue))[] | ((h: typeof import("vue").h, props: {
            value: import("./type").SwitchValue;
        }) => import("..").TNodeReturnValue);
        disabled: boolean;
        modelValue: import("./type").SwitchValue;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        beforeChange: {
            type: import("vue").PropType<TdSwitchProps["beforeChange"]>;
        };
        customValue: {
            type: import("vue").PropType<TdSwitchProps["customValue"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        label: {
            type: import("vue").PropType<TdSwitchProps["label"]>;
            default: () => TdSwitchProps["label"];
        };
        loading: BooleanConstructor;
        size: {
            type: import("vue").PropType<TdSwitchProps["size"]>;
            default: TdSwitchProps["size"];
            validator(val: TdSwitchProps["size"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdSwitchProps["value"]>;
            default: TdSwitchProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdSwitchProps["value"]>;
            default: TdSwitchProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdSwitchProps["defaultValue"]>;
        };
        onChange: import("vue").PropType<TdSwitchProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        loading: boolean;
        value: import("./type").SwitchValue;
        size: "small" | "medium" | "large";
        label: (string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue))[] | ((h: typeof import("vue").h, props: {
            value: import("./type").SwitchValue;
        }) => import("..").TNodeReturnValue);
        disabled: boolean;
        modelValue: import("./type").SwitchValue;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    beforeChange: {
        type: import("vue").PropType<TdSwitchProps["beforeChange"]>;
    };
    customValue: {
        type: import("vue").PropType<TdSwitchProps["customValue"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    label: {
        type: import("vue").PropType<TdSwitchProps["label"]>;
        default: () => TdSwitchProps["label"];
    };
    loading: BooleanConstructor;
    size: {
        type: import("vue").PropType<TdSwitchProps["size"]>;
        default: TdSwitchProps["size"];
        validator(val: TdSwitchProps["size"]): boolean;
    };
    value: {
        type: import("vue").PropType<TdSwitchProps["value"]>;
        default: TdSwitchProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdSwitchProps["value"]>;
        default: TdSwitchProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdSwitchProps["defaultValue"]>;
    };
    onChange: import("vue").PropType<TdSwitchProps["onChange"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    loading: boolean;
    value: import("./type").SwitchValue;
    size: "small" | "medium" | "large";
    label: (string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue))[] | ((h: typeof import("vue").h, props: {
        value: import("./type").SwitchValue;
    }) => import("..").TNodeReturnValue);
    disabled: boolean;
    modelValue: import("./type").SwitchValue;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Switch;
