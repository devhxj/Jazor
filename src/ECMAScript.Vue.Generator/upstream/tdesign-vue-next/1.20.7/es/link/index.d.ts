import { TdLinkProps } from './type';
import './style';
export * from './type';
export type LinkProps = TdLinkProps;
export declare const Link: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        content: {
            type: import("vue").PropType<TdLinkProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdLinkProps["default"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        download: {
            type: import("vue").PropType<TdLinkProps["download"]>;
        };
        hover: {
            type: import("vue").PropType<TdLinkProps["hover"]>;
            default: TdLinkProps["hover"];
            validator(val: TdLinkProps["hover"]): boolean;
        };
        href: {
            type: StringConstructor;
            default: string;
        };
        prefixIcon: {
            type: import("vue").PropType<TdLinkProps["prefixIcon"]>;
        };
        size: {
            type: import("vue").PropType<TdLinkProps["size"]>;
            default: TdLinkProps["size"];
            validator(val: TdLinkProps["size"]): boolean;
        };
        suffixIcon: {
            type: import("vue").PropType<TdLinkProps["suffixIcon"]>;
        };
        target: {
            type: StringConstructor;
            default: string;
        };
        theme: {
            type: import("vue").PropType<TdLinkProps["theme"]>;
            default: TdLinkProps["theme"];
            validator(val: TdLinkProps["theme"]): boolean;
        };
        underline: BooleanConstructor;
        onClick: import("vue").PropType<TdLinkProps["onClick"]>;
    }>> & Readonly<{
        onClick?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "click"[], import("vue").PublicProps, {
        size: import("..").SizeEnum;
        disabled: boolean;
        theme: "default" | "primary" | "success" | "warning" | "danger";
        href: string;
        target: string;
        hover: "color" | "underline";
        underline: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        content: {
            type: import("vue").PropType<TdLinkProps["content"]>;
        };
        default: {
            type: import("vue").PropType<TdLinkProps["default"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: any;
        };
        download: {
            type: import("vue").PropType<TdLinkProps["download"]>;
        };
        hover: {
            type: import("vue").PropType<TdLinkProps["hover"]>;
            default: TdLinkProps["hover"];
            validator(val: TdLinkProps["hover"]): boolean;
        };
        href: {
            type: StringConstructor;
            default: string;
        };
        prefixIcon: {
            type: import("vue").PropType<TdLinkProps["prefixIcon"]>;
        };
        size: {
            type: import("vue").PropType<TdLinkProps["size"]>;
            default: TdLinkProps["size"];
            validator(val: TdLinkProps["size"]): boolean;
        };
        suffixIcon: {
            type: import("vue").PropType<TdLinkProps["suffixIcon"]>;
        };
        target: {
            type: StringConstructor;
            default: string;
        };
        theme: {
            type: import("vue").PropType<TdLinkProps["theme"]>;
            default: TdLinkProps["theme"];
            validator(val: TdLinkProps["theme"]): boolean;
        };
        underline: BooleanConstructor;
        onClick: import("vue").PropType<TdLinkProps["onClick"]>;
    }>> & Readonly<{
        onClick?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        size: import("..").SizeEnum;
        disabled: boolean;
        theme: "default" | "primary" | "success" | "warning" | "danger";
        href: string;
        target: string;
        hover: "color" | "underline";
        underline: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    content: {
        type: import("vue").PropType<TdLinkProps["content"]>;
    };
    default: {
        type: import("vue").PropType<TdLinkProps["default"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    download: {
        type: import("vue").PropType<TdLinkProps["download"]>;
    };
    hover: {
        type: import("vue").PropType<TdLinkProps["hover"]>;
        default: TdLinkProps["hover"];
        validator(val: TdLinkProps["hover"]): boolean;
    };
    href: {
        type: StringConstructor;
        default: string;
    };
    prefixIcon: {
        type: import("vue").PropType<TdLinkProps["prefixIcon"]>;
    };
    size: {
        type: import("vue").PropType<TdLinkProps["size"]>;
        default: TdLinkProps["size"];
        validator(val: TdLinkProps["size"]): boolean;
    };
    suffixIcon: {
        type: import("vue").PropType<TdLinkProps["suffixIcon"]>;
    };
    target: {
        type: StringConstructor;
        default: string;
    };
    theme: {
        type: import("vue").PropType<TdLinkProps["theme"]>;
        default: TdLinkProps["theme"];
        validator(val: TdLinkProps["theme"]): boolean;
    };
    underline: BooleanConstructor;
    onClick: import("vue").PropType<TdLinkProps["onClick"]>;
}>> & Readonly<{
    onClick?: (...args: any[]) => any;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "click"[], "click", {
    size: import("..").SizeEnum;
    disabled: boolean;
    theme: "default" | "primary" | "success" | "warning" | "danger";
    href: string;
    target: string;
    hover: "color" | "underline";
    underline: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Link;
