import { TdRadioProps, TdRadioGroupProps } from './type';
import './style';
export * from './type';
export type RadioProps = TdRadioProps;
export type RadioGroupProps = TdRadioGroupProps;
export declare const Radio: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        allowUncheck: BooleanConstructor;
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
            type: import("vue").PropType<TdRadioProps["default"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        label: {
            type: import("vue").PropType<TdRadioProps["label"]>;
        };
        name: {
            type: StringConstructor;
            default: string;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        value: {
            type: import("vue").PropType<TdRadioProps["value"]>;
            default: TdRadioProps["value"];
        };
        onChange: import("vue").PropType<TdRadioProps["onChange"]>;
        onClick: import("vue").PropType<TdRadioProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").RadioValue;
        disabled: boolean;
        checked: boolean;
        name: string;
        allowUncheck: boolean;
        modelValue: boolean;
        defaultChecked: boolean;
        readonly: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        allowUncheck: BooleanConstructor;
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
            type: import("vue").PropType<TdRadioProps["default"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        label: {
            type: import("vue").PropType<TdRadioProps["label"]>;
        };
        name: {
            type: StringConstructor;
            default: string;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        value: {
            type: import("vue").PropType<TdRadioProps["value"]>;
            default: TdRadioProps["value"];
        };
        onChange: import("vue").PropType<TdRadioProps["onChange"]>;
        onClick: import("vue").PropType<TdRadioProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").RadioValue;
        disabled: boolean;
        checked: boolean;
        name: string;
        allowUncheck: boolean;
        modelValue: boolean;
        defaultChecked: boolean;
        readonly: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    allowUncheck: BooleanConstructor;
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
        type: import("vue").PropType<TdRadioProps["default"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    label: {
        type: import("vue").PropType<TdRadioProps["label"]>;
    };
    name: {
        type: StringConstructor;
        default: string;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    value: {
        type: import("vue").PropType<TdRadioProps["value"]>;
        default: TdRadioProps["value"];
    };
    onChange: import("vue").PropType<TdRadioProps["onChange"]>;
    onClick: import("vue").PropType<TdRadioProps["onClick"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").RadioValue;
    disabled: boolean;
    checked: boolean;
    name: string;
    allowUncheck: boolean;
    modelValue: boolean;
    defaultChecked: boolean;
    readonly: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const RadioGroup: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        allowUncheck: BooleanConstructor;
        direction: {
            type: import("vue").PropType<TdRadioGroupProps["direction"]>;
            default: TdRadioGroupProps["direction"];
            validator(val: TdRadioGroupProps["direction"]): boolean;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        name: {
            type: StringConstructor;
            default: string;
        };
        options: {
            type: import("vue").PropType<TdRadioGroupProps["options"]>;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        size: {
            type: import("vue").PropType<TdRadioGroupProps["size"]>;
            default: TdRadioGroupProps["size"];
            validator(val: TdRadioGroupProps["size"]): boolean;
        };
        theme: {
            type: import("vue").PropType<TdRadioGroupProps["theme"]>;
            default: TdRadioGroupProps["theme"];
            validator(val: TdRadioGroupProps["theme"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdRadioGroupProps["value"]>;
            default: TdRadioGroupProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdRadioGroupProps["value"]>;
            default: TdRadioGroupProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdRadioGroupProps["defaultValue"]>;
            default: TdRadioGroupProps["defaultValue"];
        };
        variant: {
            type: import("vue").PropType<TdRadioGroupProps["variant"]>;
            default: TdRadioGroupProps["variant"];
            validator(val: TdRadioGroupProps["variant"]): boolean;
        };
        onChange: import("vue").PropType<TdRadioGroupProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").RadioValue;
        size: import("..").SizeEnum;
        direction: "vertical" | "horizontal";
        disabled: boolean;
        name: string;
        defaultValue: import("./type").RadioValue;
        theme: "button" | "radio";
        allowUncheck: boolean;
        modelValue: import("./type").RadioValue;
        readonly: boolean;
        variant: "outline" | "primary-filled" | "default-filled";
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        allowUncheck: BooleanConstructor;
        direction: {
            type: import("vue").PropType<TdRadioGroupProps["direction"]>;
            default: TdRadioGroupProps["direction"];
            validator(val: TdRadioGroupProps["direction"]): boolean;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        name: {
            type: StringConstructor;
            default: string;
        };
        options: {
            type: import("vue").PropType<TdRadioGroupProps["options"]>;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        size: {
            type: import("vue").PropType<TdRadioGroupProps["size"]>;
            default: TdRadioGroupProps["size"];
            validator(val: TdRadioGroupProps["size"]): boolean;
        };
        theme: {
            type: import("vue").PropType<TdRadioGroupProps["theme"]>;
            default: TdRadioGroupProps["theme"];
            validator(val: TdRadioGroupProps["theme"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdRadioGroupProps["value"]>;
            default: TdRadioGroupProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdRadioGroupProps["value"]>;
            default: TdRadioGroupProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdRadioGroupProps["defaultValue"]>;
            default: TdRadioGroupProps["defaultValue"];
        };
        variant: {
            type: import("vue").PropType<TdRadioGroupProps["variant"]>;
            default: TdRadioGroupProps["variant"];
            validator(val: TdRadioGroupProps["variant"]): boolean;
        };
        onChange: import("vue").PropType<TdRadioGroupProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").RadioValue;
        size: import("..").SizeEnum;
        direction: "vertical" | "horizontal";
        disabled: boolean;
        name: string;
        defaultValue: import("./type").RadioValue;
        theme: "button" | "radio";
        allowUncheck: boolean;
        modelValue: import("./type").RadioValue;
        readonly: boolean;
        variant: "outline" | "primary-filled" | "default-filled";
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    allowUncheck: BooleanConstructor;
    direction: {
        type: import("vue").PropType<TdRadioGroupProps["direction"]>;
        default: TdRadioGroupProps["direction"];
        validator(val: TdRadioGroupProps["direction"]): boolean;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    name: {
        type: StringConstructor;
        default: string;
    };
    options: {
        type: import("vue").PropType<TdRadioGroupProps["options"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    size: {
        type: import("vue").PropType<TdRadioGroupProps["size"]>;
        default: TdRadioGroupProps["size"];
        validator(val: TdRadioGroupProps["size"]): boolean;
    };
    theme: {
        type: import("vue").PropType<TdRadioGroupProps["theme"]>;
        default: TdRadioGroupProps["theme"];
        validator(val: TdRadioGroupProps["theme"]): boolean;
    };
    value: {
        type: import("vue").PropType<TdRadioGroupProps["value"]>;
        default: TdRadioGroupProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdRadioGroupProps["value"]>;
        default: TdRadioGroupProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdRadioGroupProps["defaultValue"]>;
        default: TdRadioGroupProps["defaultValue"];
    };
    variant: {
        type: import("vue").PropType<TdRadioGroupProps["variant"]>;
        default: TdRadioGroupProps["variant"];
        validator(val: TdRadioGroupProps["variant"]): boolean;
    };
    onChange: import("vue").PropType<TdRadioGroupProps["onChange"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").RadioValue;
    size: import("..").SizeEnum;
    direction: "vertical" | "horizontal";
    disabled: boolean;
    name: string;
    defaultValue: import("./type").RadioValue;
    theme: "button" | "radio";
    allowUncheck: boolean;
    modelValue: import("./type").RadioValue;
    readonly: boolean;
    variant: "outline" | "primary-filled" | "default-filled";
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const RadioButton: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        allowUncheck: BooleanConstructor;
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
            type: import("vue").PropType<TdRadioProps["default"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        label: {
            type: import("vue").PropType<TdRadioProps["label"]>;
        };
        name: {
            type: StringConstructor;
            default: string;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        value: {
            type: import("vue").PropType<TdRadioProps["value"]>;
            default: TdRadioProps["value"];
        };
        onChange: import("vue").PropType<TdRadioProps["onChange"]>;
        onClick: import("vue").PropType<TdRadioProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").RadioValue;
        disabled: boolean;
        checked: boolean;
        name: string;
        allowUncheck: boolean;
        modelValue: boolean;
        defaultChecked: boolean;
        readonly: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        allowUncheck: BooleanConstructor;
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
            type: import("vue").PropType<TdRadioProps["default"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        label: {
            type: import("vue").PropType<TdRadioProps["label"]>;
        };
        name: {
            type: StringConstructor;
            default: string;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        value: {
            type: import("vue").PropType<TdRadioProps["value"]>;
            default: TdRadioProps["value"];
        };
        onChange: import("vue").PropType<TdRadioProps["onChange"]>;
        onClick: import("vue").PropType<TdRadioProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").RadioValue;
        disabled: boolean;
        checked: boolean;
        name: string;
        allowUncheck: boolean;
        modelValue: boolean;
        defaultChecked: boolean;
        readonly: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    allowUncheck: BooleanConstructor;
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
        type: import("vue").PropType<TdRadioProps["default"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    label: {
        type: import("vue").PropType<TdRadioProps["label"]>;
    };
    name: {
        type: StringConstructor;
        default: string;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    value: {
        type: import("vue").PropType<TdRadioProps["value"]>;
        default: TdRadioProps["value"];
    };
    onChange: import("vue").PropType<TdRadioProps["onChange"]>;
    onClick: import("vue").PropType<TdRadioProps["onClick"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").RadioValue;
    disabled: boolean;
    checked: boolean;
    name: string;
    allowUncheck: boolean;
    modelValue: boolean;
    defaultChecked: boolean;
    readonly: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Radio;
