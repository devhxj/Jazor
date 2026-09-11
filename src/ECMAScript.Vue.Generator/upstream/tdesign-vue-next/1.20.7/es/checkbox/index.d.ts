import { TdCheckboxProps, TdCheckboxGroupProps } from './type';
import './style';
export * from './type';
export type CheckboxProps = TdCheckboxProps;
export type CheckboxGroupProps = TdCheckboxGroupProps;
export declare const Checkbox: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        needRipple: BooleanConstructor;
        stopLabelTrigger: BooleanConstructor;
        index: NumberConstructor;
        data: ObjectConstructor;
        checkAll: BooleanConstructor;
        checked: {
            type: BooleanConstructor;
            default: any;
        };
        modelValue: {
            type: BooleanConstructor;
            default: any;
        };
        defaultChecked: BooleanConstructor;
        default: {
            type: import("vue").PropType<TdCheckboxProps["default"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        indeterminate: BooleanConstructor;
        label: {
            type: import("vue").PropType<TdCheckboxProps["label"]>;
        };
        lazyLoad: BooleanConstructor;
        name: {
            type: StringConstructor;
            default: string;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        title: {
            type: StringConstructor;
            default: string;
        };
        value: {
            type: import("vue").PropType<TdCheckboxProps["value"]>;
        };
        onChange: import("vue").PropType<TdCheckboxProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        title: string;
        disabled: boolean;
        checked: boolean;
        indeterminate: boolean;
        name: string;
        modelValue: boolean;
        defaultChecked: boolean;
        readonly: boolean;
        needRipple: boolean;
        stopLabelTrigger: boolean;
        checkAll: boolean;
        lazyLoad: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        needRipple: BooleanConstructor;
        stopLabelTrigger: BooleanConstructor;
        index: NumberConstructor;
        data: ObjectConstructor;
        checkAll: BooleanConstructor;
        checked: {
            type: BooleanConstructor;
            default: any;
        };
        modelValue: {
            type: BooleanConstructor;
            default: any;
        };
        defaultChecked: BooleanConstructor;
        default: {
            type: import("vue").PropType<TdCheckboxProps["default"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        indeterminate: BooleanConstructor;
        label: {
            type: import("vue").PropType<TdCheckboxProps["label"]>;
        };
        lazyLoad: BooleanConstructor;
        name: {
            type: StringConstructor;
            default: string;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        title: {
            type: StringConstructor;
            default: string;
        };
        value: {
            type: import("vue").PropType<TdCheckboxProps["value"]>;
        };
        onChange: import("vue").PropType<TdCheckboxProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        title: string;
        disabled: boolean;
        checked: boolean;
        indeterminate: boolean;
        name: string;
        modelValue: boolean;
        defaultChecked: boolean;
        readonly: boolean;
        needRipple: boolean;
        stopLabelTrigger: boolean;
        checkAll: boolean;
        lazyLoad: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    needRipple: BooleanConstructor;
    stopLabelTrigger: BooleanConstructor;
    index: NumberConstructor;
    data: ObjectConstructor;
    checkAll: BooleanConstructor;
    checked: {
        type: BooleanConstructor;
        default: any;
    };
    modelValue: {
        type: BooleanConstructor;
        default: any;
    };
    defaultChecked: BooleanConstructor;
    default: {
        type: import("vue").PropType<TdCheckboxProps["default"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    indeterminate: BooleanConstructor;
    label: {
        type: import("vue").PropType<TdCheckboxProps["label"]>;
    };
    lazyLoad: BooleanConstructor;
    name: {
        type: StringConstructor;
        default: string;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    title: {
        type: StringConstructor;
        default: string;
    };
    value: {
        type: import("vue").PropType<TdCheckboxProps["value"]>;
    };
    onChange: import("vue").PropType<TdCheckboxProps["onChange"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    title: string;
    disabled: boolean;
    checked: boolean;
    indeterminate: boolean;
    name: string;
    modelValue: boolean;
    defaultChecked: boolean;
    readonly: boolean;
    needRipple: boolean;
    stopLabelTrigger: boolean;
    checkAll: boolean;
    lazyLoad: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const CheckboxGroup: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        lazyLoad: BooleanConstructor;
        max: {
            type: NumberConstructor;
            default: any;
        };
        name: {
            type: StringConstructor;
            default: string;
        };
        options: {
            type: import("vue").PropType<TdCheckboxGroupProps["options"]>;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        value: {
            type: import("vue").PropType<TdCheckboxGroupProps["value"]>;
            default: TdCheckboxGroupProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdCheckboxGroupProps["value"]>;
            default: TdCheckboxGroupProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdCheckboxGroupProps["defaultValue"]>;
            default: () => TdCheckboxGroupProps["defaultValue"];
        };
        onChange: import("vue").PropType<TdCheckboxGroupProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").CheckboxGroupValue;
        max: number;
        disabled: boolean;
        name: string;
        defaultValue: import("./type").CheckboxGroupValue;
        modelValue: import("./type").CheckboxGroupValue;
        readonly: boolean;
        lazyLoad: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        lazyLoad: BooleanConstructor;
        max: {
            type: NumberConstructor;
            default: any;
        };
        name: {
            type: StringConstructor;
            default: string;
        };
        options: {
            type: import("vue").PropType<TdCheckboxGroupProps["options"]>;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        value: {
            type: import("vue").PropType<TdCheckboxGroupProps["value"]>;
            default: TdCheckboxGroupProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdCheckboxGroupProps["value"]>;
            default: TdCheckboxGroupProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdCheckboxGroupProps["defaultValue"]>;
            default: () => TdCheckboxGroupProps["defaultValue"];
        };
        onChange: import("vue").PropType<TdCheckboxGroupProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").CheckboxGroupValue;
        max: number;
        disabled: boolean;
        name: string;
        defaultValue: import("./type").CheckboxGroupValue;
        modelValue: import("./type").CheckboxGroupValue;
        readonly: boolean;
        lazyLoad: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    lazyLoad: BooleanConstructor;
    max: {
        type: NumberConstructor;
        default: any;
    };
    name: {
        type: StringConstructor;
        default: string;
    };
    options: {
        type: import("vue").PropType<TdCheckboxGroupProps["options"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    value: {
        type: import("vue").PropType<TdCheckboxGroupProps["value"]>;
        default: TdCheckboxGroupProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdCheckboxGroupProps["value"]>;
        default: TdCheckboxGroupProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdCheckboxGroupProps["defaultValue"]>;
        default: () => TdCheckboxGroupProps["defaultValue"];
    };
    onChange: import("vue").PropType<TdCheckboxGroupProps["onChange"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").CheckboxGroupValue;
    max: number;
    disabled: boolean;
    name: string;
    defaultValue: import("./type").CheckboxGroupValue;
    modelValue: import("./type").CheckboxGroupValue;
    readonly: boolean;
    lazyLoad: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Checkbox;
