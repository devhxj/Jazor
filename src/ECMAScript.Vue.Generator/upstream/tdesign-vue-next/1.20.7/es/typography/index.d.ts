export * from './type';
import './style';
export declare const Typography: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {}, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {}>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const Text: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        code: BooleanConstructor;
        content: {
            type: import("vue").PropType<import("./type").TdTextProps["content"]>;
        };
        copyable: {
            type: import("vue").PropType<import("./type").TdTextProps["copyable"]>;
            default: import("./type").TdTextProps["copyable"];
        };
        default: {
            type: import("vue").PropType<import("./type").TdTextProps["default"]>;
        };
        delete: BooleanConstructor;
        disabled: BooleanConstructor;
        ellipsis: {
            type: import("vue").PropType<import("./type").TdTextProps["ellipsis"]>;
            default: import("./type").TdTextProps["ellipsis"];
        };
        italic: BooleanConstructor;
        keyboard: BooleanConstructor;
        mark: {
            type: import("vue").PropType<import("./type").TdTextProps["mark"]>;
            default: import("./type").TdTextProps["mark"];
        };
        strong: BooleanConstructor;
        theme: {
            type: import("vue").PropType<import("./type").TdTextProps["theme"]>;
            validator(val: import("./type").TdTextProps["theme"]): boolean;
        };
        underline: BooleanConstructor;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        code: boolean;
        mark: string | boolean;
        strong: boolean;
        disabled: boolean;
        delete: boolean;
        italic: boolean;
        underline: boolean;
        ellipsis: boolean | import("./type").TypographyEllipsis;
        copyable: boolean | import("./type").TypographyCopyable;
        keyboard: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        code: BooleanConstructor;
        content: {
            type: import("vue").PropType<import("./type").TdTextProps["content"]>;
        };
        copyable: {
            type: import("vue").PropType<import("./type").TdTextProps["copyable"]>;
            default: import("./type").TdTextProps["copyable"];
        };
        default: {
            type: import("vue").PropType<import("./type").TdTextProps["default"]>;
        };
        delete: BooleanConstructor;
        disabled: BooleanConstructor;
        ellipsis: {
            type: import("vue").PropType<import("./type").TdTextProps["ellipsis"]>;
            default: import("./type").TdTextProps["ellipsis"];
        };
        italic: BooleanConstructor;
        keyboard: BooleanConstructor;
        mark: {
            type: import("vue").PropType<import("./type").TdTextProps["mark"]>;
            default: import("./type").TdTextProps["mark"];
        };
        strong: BooleanConstructor;
        theme: {
            type: import("vue").PropType<import("./type").TdTextProps["theme"]>;
            validator(val: import("./type").TdTextProps["theme"]): boolean;
        };
        underline: BooleanConstructor;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        code: boolean;
        mark: string | boolean;
        strong: boolean;
        disabled: boolean;
        delete: boolean;
        italic: boolean;
        underline: boolean;
        ellipsis: boolean | import("./type").TypographyEllipsis;
        copyable: boolean | import("./type").TypographyCopyable;
        keyboard: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    code: BooleanConstructor;
    content: {
        type: import("vue").PropType<import("./type").TdTextProps["content"]>;
    };
    copyable: {
        type: import("vue").PropType<import("./type").TdTextProps["copyable"]>;
        default: import("./type").TdTextProps["copyable"];
    };
    default: {
        type: import("vue").PropType<import("./type").TdTextProps["default"]>;
    };
    delete: BooleanConstructor;
    disabled: BooleanConstructor;
    ellipsis: {
        type: import("vue").PropType<import("./type").TdTextProps["ellipsis"]>;
        default: import("./type").TdTextProps["ellipsis"];
    };
    italic: BooleanConstructor;
    keyboard: BooleanConstructor;
    mark: {
        type: import("vue").PropType<import("./type").TdTextProps["mark"]>;
        default: import("./type").TdTextProps["mark"];
    };
    strong: BooleanConstructor;
    theme: {
        type: import("vue").PropType<import("./type").TdTextProps["theme"]>;
        validator(val: import("./type").TdTextProps["theme"]): boolean;
    };
    underline: BooleanConstructor;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    code: boolean;
    mark: string | boolean;
    strong: boolean;
    disabled: boolean;
    delete: boolean;
    italic: boolean;
    underline: boolean;
    ellipsis: boolean | import("./type").TypographyEllipsis;
    copyable: boolean | import("./type").TypographyCopyable;
    keyboard: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const Title: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        content: {
            type: import("vue").PropType<import("./type").TdTitleProps["content"]>;
        };
        default: {
            type: import("vue").PropType<import("./type").TdTitleProps["default"]>;
        };
        ellipsis: {
            type: import("vue").PropType<import("./type").TdTitleProps["ellipsis"]>;
            default: import("./type").TdTitleProps["ellipsis"];
        };
        level: {
            type: import("vue").PropType<import("./type").TdTitleProps["level"]>;
            default: import("./type").TdTitleProps["level"];
            validator(val: import("./type").TdTitleProps["level"]): boolean;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        level: "h1" | "h2" | "h3" | "h4" | "h5" | "h6";
        ellipsis: boolean | import("./type").TypographyEllipsis;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        content: {
            type: import("vue").PropType<import("./type").TdTitleProps["content"]>;
        };
        default: {
            type: import("vue").PropType<import("./type").TdTitleProps["default"]>;
        };
        ellipsis: {
            type: import("vue").PropType<import("./type").TdTitleProps["ellipsis"]>;
            default: import("./type").TdTitleProps["ellipsis"];
        };
        level: {
            type: import("vue").PropType<import("./type").TdTitleProps["level"]>;
            default: import("./type").TdTitleProps["level"];
            validator(val: import("./type").TdTitleProps["level"]): boolean;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        level: "h1" | "h2" | "h3" | "h4" | "h5" | "h6";
        ellipsis: boolean | import("./type").TypographyEllipsis;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    content: {
        type: import("vue").PropType<import("./type").TdTitleProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdTitleProps["default"]>;
    };
    ellipsis: {
        type: import("vue").PropType<import("./type").TdTitleProps["ellipsis"]>;
        default: import("./type").TdTitleProps["ellipsis"];
    };
    level: {
        type: import("vue").PropType<import("./type").TdTitleProps["level"]>;
        default: import("./type").TdTitleProps["level"];
        validator(val: import("./type").TdTitleProps["level"]): boolean;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    level: "h1" | "h2" | "h3" | "h4" | "h5" | "h6";
    ellipsis: boolean | import("./type").TypographyEllipsis;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const Paragraph: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        content: {
            type: import("vue").PropType<import("./type").TdParagraphProps["content"]>;
        };
        default: {
            type: import("vue").PropType<import("./type").TdParagraphProps["default"]>;
        };
        ellipsis: {
            type: import("vue").PropType<import("./type").TdParagraphProps["ellipsis"]>;
            default: import("./type").TdParagraphProps["ellipsis"];
        };
        style: {
            type: import("vue").PropType<Record<string, string | number>>;
            default: () => {};
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        style: Record<string, string | number>;
        ellipsis: boolean | import("./type").TypographyEllipsis;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        content: {
            type: import("vue").PropType<import("./type").TdParagraphProps["content"]>;
        };
        default: {
            type: import("vue").PropType<import("./type").TdParagraphProps["default"]>;
        };
        ellipsis: {
            type: import("vue").PropType<import("./type").TdParagraphProps["ellipsis"]>;
            default: import("./type").TdParagraphProps["ellipsis"];
        };
        style: {
            type: import("vue").PropType<Record<string, string | number>>;
            default: () => {};
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        style: Record<string, string | number>;
        ellipsis: boolean | import("./type").TypographyEllipsis;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    content: {
        type: import("vue").PropType<import("./type").TdParagraphProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdParagraphProps["default"]>;
    };
    ellipsis: {
        type: import("vue").PropType<import("./type").TdParagraphProps["ellipsis"]>;
        default: import("./type").TdParagraphProps["ellipsis"];
    };
    style: {
        type: import("vue").PropType<Record<string, string | number>>;
        default: () => {};
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    style: Record<string, string | number>;
    ellipsis: boolean | import("./type").TypographyEllipsis;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Typography;
