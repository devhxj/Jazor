import { TdButtonProps } from './type';
import './style';
export * from './type';
export type ButtonProps = TdButtonProps;
export declare const Button: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        block: BooleanConstructor;
        content: {
            type: import("vue").PropType<TdButtonProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdButtonProps["default"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        form: {
            type: StringConstructor;
            default: any;
        };
        ghost: BooleanConstructor;
        href: {
            type: StringConstructor;
            default: string;
        };
        icon: {
            type: import("vue").PropType<TdButtonProps["icon"]>;
        };
        loading: BooleanConstructor;
        loadingProps: {
            type: import("vue").PropType<TdButtonProps["loadingProps"]>;
        };
        shape: {
            type: import("vue").PropType<TdButtonProps["shape"]>;
            default: TdButtonProps["shape"];
            validator(val: TdButtonProps["shape"]): boolean;
        };
        size: {
            type: import("vue").PropType<TdButtonProps["size"]>;
            default: TdButtonProps["size"];
            validator(val: TdButtonProps["size"]): boolean;
        };
        suffix: {
            type: import("vue").PropType<TdButtonProps["suffix"]>;
        };
        tag: {
            type: import("vue").PropType<TdButtonProps["tag"]>;
            validator(val: TdButtonProps["tag"]): boolean;
        };
        theme: {
            type: import("vue").PropType<TdButtonProps["theme"]>;
            validator(val: TdButtonProps["theme"]): boolean;
        };
        type: {
            type: import("vue").PropType<TdButtonProps["type"]>;
            default: TdButtonProps["type"];
            validator(val: TdButtonProps["type"]): boolean;
        };
        variant: {
            type: import("vue").PropType<TdButtonProps["variant"]>;
            default: TdButtonProps["variant"];
            validator(val: TdButtonProps["variant"]): boolean;
        };
        onClick: import("vue").PropType<TdButtonProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
        [key: string]: any;
    }>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        form: string;
        loading: boolean;
        type: "button" | "reset" | "submit";
        size: import("..").SizeEnum;
        disabled: boolean;
        href: string;
        block: boolean;
        variant: "base" | "text" | "outline" | "dashed";
        shape: "circle" | "round" | "square" | "rectangle";
        ghost: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        block: BooleanConstructor;
        content: {
            type: import("vue").PropType<TdButtonProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdButtonProps["default"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        form: {
            type: StringConstructor;
            default: any;
        };
        ghost: BooleanConstructor;
        href: {
            type: StringConstructor;
            default: string;
        };
        icon: {
            type: import("vue").PropType<TdButtonProps["icon"]>;
        };
        loading: BooleanConstructor;
        loadingProps: {
            type: import("vue").PropType<TdButtonProps["loadingProps"]>;
        };
        shape: {
            type: import("vue").PropType<TdButtonProps["shape"]>;
            default: TdButtonProps["shape"];
            validator(val: TdButtonProps["shape"]): boolean;
        };
        size: {
            type: import("vue").PropType<TdButtonProps["size"]>;
            default: TdButtonProps["size"];
            validator(val: TdButtonProps["size"]): boolean;
        };
        suffix: {
            type: import("vue").PropType<TdButtonProps["suffix"]>;
        };
        tag: {
            type: import("vue").PropType<TdButtonProps["tag"]>;
            validator(val: TdButtonProps["tag"]): boolean;
        };
        theme: {
            type: import("vue").PropType<TdButtonProps["theme"]>;
            validator(val: TdButtonProps["theme"]): boolean;
        };
        type: {
            type: import("vue").PropType<TdButtonProps["type"]>;
            default: TdButtonProps["type"];
            validator(val: TdButtonProps["type"]): boolean;
        };
        variant: {
            type: import("vue").PropType<TdButtonProps["variant"]>;
            default: TdButtonProps["variant"];
            validator(val: TdButtonProps["variant"]): boolean;
        };
        onClick: import("vue").PropType<TdButtonProps["onClick"]>;
    }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
        [key: string]: any;
    }>, {}, {}, {}, {
        form: string;
        loading: boolean;
        type: "button" | "reset" | "submit";
        size: import("..").SizeEnum;
        disabled: boolean;
        href: string;
        block: boolean;
        variant: "base" | "text" | "outline" | "dashed";
        shape: "circle" | "round" | "square" | "rectangle";
        ghost: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    block: BooleanConstructor;
    content: {
        type: import("vue").PropType<TdButtonProps["content"]>;
    };
    default: {
        type: import("vue").PropType<TdButtonProps["default"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    form: {
        type: StringConstructor;
        default: any;
    };
    ghost: BooleanConstructor;
    href: {
        type: StringConstructor;
        default: string;
    };
    icon: {
        type: import("vue").PropType<TdButtonProps["icon"]>;
    };
    loading: BooleanConstructor;
    loadingProps: {
        type: import("vue").PropType<TdButtonProps["loadingProps"]>;
    };
    shape: {
        type: import("vue").PropType<TdButtonProps["shape"]>;
        default: TdButtonProps["shape"];
        validator(val: TdButtonProps["shape"]): boolean;
    };
    size: {
        type: import("vue").PropType<TdButtonProps["size"]>;
        default: TdButtonProps["size"];
        validator(val: TdButtonProps["size"]): boolean;
    };
    suffix: {
        type: import("vue").PropType<TdButtonProps["suffix"]>;
    };
    tag: {
        type: import("vue").PropType<TdButtonProps["tag"]>;
        validator(val: TdButtonProps["tag"]): boolean;
    };
    theme: {
        type: import("vue").PropType<TdButtonProps["theme"]>;
        validator(val: TdButtonProps["theme"]): boolean;
    };
    type: {
        type: import("vue").PropType<TdButtonProps["type"]>;
        default: TdButtonProps["type"];
        validator(val: TdButtonProps["type"]): boolean;
    };
    variant: {
        type: import("vue").PropType<TdButtonProps["variant"]>;
        default: TdButtonProps["variant"];
        validator(val: TdButtonProps["variant"]): boolean;
    };
    onClick: import("vue").PropType<TdButtonProps["onClick"]>;
}>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
    [key: string]: any;
}>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    form: string;
    loading: boolean;
    type: "button" | "reset" | "submit";
    size: import("..").SizeEnum;
    disabled: boolean;
    href: string;
    block: boolean;
    variant: "base" | "text" | "outline" | "dashed";
    shape: "circle" | "round" | "square" | "rectangle";
    ghost: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Button;
