import { TdTagProps, TdCheckTagProps, TdCheckTagGroupProps } from './type';
import './style';
export * from './type';
export type TagProps = TdTagProps;
export type CheckTagProps = TdCheckTagProps;
export type CheckTagGroupProps = TdCheckTagGroupProps;
export declare const Tag: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        closable: BooleanConstructor;
        color: {
            type: StringConstructor;
            default: string;
        };
        content: {
            type: import("vue").PropType<TdTagProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdTagProps["default"]>;
        };
        disabled: BooleanConstructor;
        icon: {
            type: import("vue").PropType<TdTagProps["icon"]>;
            default: any;
        };
        maxWidth: {
            type: import("vue").PropType<TdTagProps["maxWidth"]>;
        };
        shape: {
            type: import("vue").PropType<TdTagProps["shape"]>;
            default: TdTagProps["shape"];
            validator(val: TdTagProps["shape"]): boolean;
        };
        size: {
            type: import("vue").PropType<TdTagProps["size"]>;
            default: TdTagProps["size"];
            validator(val: TdTagProps["size"]): boolean;
        };
        theme: {
            type: import("vue").PropType<TdTagProps["theme"]>;
            default: TdTagProps["theme"];
            validator(val: TdTagProps["theme"]): boolean;
        };
        title: {
            type: import("vue").PropType<TdTagProps["title"]>;
        };
        variant: {
            type: import("vue").PropType<TdTagProps["variant"]>;
            default: TdTagProps["variant"];
            validator(val: TdTagProps["variant"]): boolean;
        };
        onClick: import("vue").PropType<TdTagProps["onClick"]>;
        onClose: import("vue").PropType<TdTagProps["onClose"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        icon: (h: typeof import("vue").h) => import("..").TNodeReturnValue;
        color: string;
        size: import("..").SizeEnum;
        disabled: boolean;
        theme: "default" | "primary" | "success" | "warning" | "danger";
        variant: "outline" | "dark" | "light" | "light-outline";
        shape: "mark" | "round" | "square";
        closable: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        closable: BooleanConstructor;
        color: {
            type: StringConstructor;
            default: string;
        };
        content: {
            type: import("vue").PropType<TdTagProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdTagProps["default"]>;
        };
        disabled: BooleanConstructor;
        icon: {
            type: import("vue").PropType<TdTagProps["icon"]>;
            default: any;
        };
        maxWidth: {
            type: import("vue").PropType<TdTagProps["maxWidth"]>;
        };
        shape: {
            type: import("vue").PropType<TdTagProps["shape"]>;
            default: TdTagProps["shape"];
            validator(val: TdTagProps["shape"]): boolean;
        };
        size: {
            type: import("vue").PropType<TdTagProps["size"]>;
            default: TdTagProps["size"];
            validator(val: TdTagProps["size"]): boolean;
        };
        theme: {
            type: import("vue").PropType<TdTagProps["theme"]>;
            default: TdTagProps["theme"];
            validator(val: TdTagProps["theme"]): boolean;
        };
        title: {
            type: import("vue").PropType<TdTagProps["title"]>;
        };
        variant: {
            type: import("vue").PropType<TdTagProps["variant"]>;
            default: TdTagProps["variant"];
            validator(val: TdTagProps["variant"]): boolean;
        };
        onClick: import("vue").PropType<TdTagProps["onClick"]>;
        onClose: import("vue").PropType<TdTagProps["onClose"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        icon: (h: typeof import("vue").h) => import("..").TNodeReturnValue;
        color: string;
        size: import("..").SizeEnum;
        disabled: boolean;
        theme: "default" | "primary" | "success" | "warning" | "danger";
        variant: "outline" | "dark" | "light" | "light-outline";
        shape: "mark" | "round" | "square";
        closable: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    closable: BooleanConstructor;
    color: {
        type: StringConstructor;
        default: string;
    };
    content: {
        type: import("vue").PropType<TdTagProps["content"]>;
    };
    default: {
        type: import("vue").PropType<TdTagProps["default"]>;
    };
    disabled: BooleanConstructor;
    icon: {
        type: import("vue").PropType<TdTagProps["icon"]>;
        default: any;
    };
    maxWidth: {
        type: import("vue").PropType<TdTagProps["maxWidth"]>;
    };
    shape: {
        type: import("vue").PropType<TdTagProps["shape"]>;
        default: TdTagProps["shape"];
        validator(val: TdTagProps["shape"]): boolean;
    };
    size: {
        type: import("vue").PropType<TdTagProps["size"]>;
        default: TdTagProps["size"];
        validator(val: TdTagProps["size"]): boolean;
    };
    theme: {
        type: import("vue").PropType<TdTagProps["theme"]>;
        default: TdTagProps["theme"];
        validator(val: TdTagProps["theme"]): boolean;
    };
    title: {
        type: import("vue").PropType<TdTagProps["title"]>;
    };
    variant: {
        type: import("vue").PropType<TdTagProps["variant"]>;
        default: TdTagProps["variant"];
        validator(val: TdTagProps["variant"]): boolean;
    };
    onClick: import("vue").PropType<TdTagProps["onClick"]>;
    onClose: import("vue").PropType<TdTagProps["onClose"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    icon: (h: typeof import("vue").h) => import("..").TNodeReturnValue;
    color: string;
    size: import("..").SizeEnum;
    disabled: boolean;
    theme: "default" | "primary" | "success" | "warning" | "danger";
    variant: "outline" | "dark" | "light" | "light-outline";
    shape: "mark" | "round" | "square";
    closable: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const CheckTag: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        checked?: boolean;
        defaultChecked?: boolean;
        modelValue?: boolean;
        checkedProps?: TdTagProps;
        content?: string | number | string[] | import("..").TNode;
        default?: string | import("..").TNode;
        disabled?: boolean;
        size?: import("..").SizeEnum;
        uncheckedProps?: TdTagProps;
        value?: string | number;
        onChange?: (checked: boolean, context: import("./type").CheckTagChangeContext) => void;
        onClick?: (context: {
            e: MouseEvent;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        size: import("..").SizeEnum;
        disabled: boolean;
        checked: boolean;
        modelValue: boolean;
        defaultChecked: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        checked?: boolean;
        defaultChecked?: boolean;
        modelValue?: boolean;
        checkedProps?: TdTagProps;
        content?: string | number | string[] | import("..").TNode;
        default?: string | import("..").TNode;
        disabled?: boolean;
        size?: import("..").SizeEnum;
        uncheckedProps?: TdTagProps;
        value?: string | number;
        onChange?: (checked: boolean, context: import("./type").CheckTagChangeContext) => void;
        onClick?: (context: {
            e: MouseEvent;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        size: import("..").SizeEnum;
        disabled: boolean;
        checked: boolean;
        modelValue: boolean;
        defaultChecked: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    checked?: boolean;
    defaultChecked?: boolean;
    modelValue?: boolean;
    checkedProps?: TdTagProps;
    content?: string | number | string[] | import("..").TNode;
    default?: string | import("..").TNode;
    disabled?: boolean;
    size?: import("..").SizeEnum;
    uncheckedProps?: TdTagProps;
    value?: string | number;
    onChange?: (checked: boolean, context: import("./type").CheckTagChangeContext) => void;
    onClick?: (context: {
        e: MouseEvent;
    }) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    size: import("..").SizeEnum;
    disabled: boolean;
    checked: boolean;
    modelValue: boolean;
    defaultChecked: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const CheckTagGroup: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        checkedProps: {
            type: import("vue").PropType<TdCheckTagGroupProps["checkedProps"]>;
        };
        multiple: BooleanConstructor;
        options: {
            type: import("vue").PropType<TdCheckTagGroupProps["options"]>;
        };
        uncheckedProps: {
            type: import("vue").PropType<TdCheckTagGroupProps["uncheckedProps"]>;
        };
        value: {
            type: import("vue").PropType<TdCheckTagGroupProps["value"]>;
            default: TdCheckTagGroupProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdCheckTagGroupProps["value"]>;
            default: TdCheckTagGroupProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdCheckTagGroupProps["defaultValue"]>;
            default: () => TdCheckTagGroupProps["defaultValue"];
        };
        onChange: import("vue").PropType<TdCheckTagGroupProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").CheckTagGroupValue;
        multiple: boolean;
        defaultValue: import("./type").CheckTagGroupValue;
        modelValue: import("./type").CheckTagGroupValue;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        checkedProps: {
            type: import("vue").PropType<TdCheckTagGroupProps["checkedProps"]>;
        };
        multiple: BooleanConstructor;
        options: {
            type: import("vue").PropType<TdCheckTagGroupProps["options"]>;
        };
        uncheckedProps: {
            type: import("vue").PropType<TdCheckTagGroupProps["uncheckedProps"]>;
        };
        value: {
            type: import("vue").PropType<TdCheckTagGroupProps["value"]>;
            default: TdCheckTagGroupProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdCheckTagGroupProps["value"]>;
            default: TdCheckTagGroupProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdCheckTagGroupProps["defaultValue"]>;
            default: () => TdCheckTagGroupProps["defaultValue"];
        };
        onChange: import("vue").PropType<TdCheckTagGroupProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").CheckTagGroupValue;
        multiple: boolean;
        defaultValue: import("./type").CheckTagGroupValue;
        modelValue: import("./type").CheckTagGroupValue;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    checkedProps: {
        type: import("vue").PropType<TdCheckTagGroupProps["checkedProps"]>;
    };
    multiple: BooleanConstructor;
    options: {
        type: import("vue").PropType<TdCheckTagGroupProps["options"]>;
    };
    uncheckedProps: {
        type: import("vue").PropType<TdCheckTagGroupProps["uncheckedProps"]>;
    };
    value: {
        type: import("vue").PropType<TdCheckTagGroupProps["value"]>;
        default: TdCheckTagGroupProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdCheckTagGroupProps["value"]>;
        default: TdCheckTagGroupProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdCheckTagGroupProps["defaultValue"]>;
        default: () => TdCheckTagGroupProps["defaultValue"];
    };
    onChange: import("vue").PropType<TdCheckTagGroupProps["onChange"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").CheckTagGroupValue;
    multiple: boolean;
    defaultValue: import("./type").CheckTagGroupValue;
    modelValue: import("./type").CheckTagGroupValue;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Tag;
