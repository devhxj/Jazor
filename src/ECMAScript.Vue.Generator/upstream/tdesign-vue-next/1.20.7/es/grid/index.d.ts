import { TdRowProps, TdColProps } from './type';
import './style';
export * from './type';
export type ColProps = TdColProps;
export type RowProps = TdRowProps;
export declare const Row: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        align: {
            type: import("vue").PropType<TdRowProps["align"]>;
            default: TdRowProps["align"];
            validator(val: TdRowProps["align"]): boolean;
        };
        gutter: {
            type: import("vue").PropType<TdRowProps["gutter"]>;
            default: TdRowProps["gutter"];
        };
        justify: {
            type: import("vue").PropType<TdRowProps["justify"]>;
            default: TdRowProps["justify"];
            validator(val: TdRowProps["justify"]): boolean;
        };
        tag: {
            type: StringConstructor;
            default: string;
        };
    }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
        [key: string]: any;
    }>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        tag: string;
        justify: "center" | "start" | "end" | "space-around" | "space-between";
        align: "center" | "top" | "middle" | "bottom" | "start" | "end" | "stretch" | "baseline";
        gutter: number | import("./type").GutterObject | (number | import("./type").GutterObject)[];
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        align: {
            type: import("vue").PropType<TdRowProps["align"]>;
            default: TdRowProps["align"];
            validator(val: TdRowProps["align"]): boolean;
        };
        gutter: {
            type: import("vue").PropType<TdRowProps["gutter"]>;
            default: TdRowProps["gutter"];
        };
        justify: {
            type: import("vue").PropType<TdRowProps["justify"]>;
            default: TdRowProps["justify"];
            validator(val: TdRowProps["justify"]): boolean;
        };
        tag: {
            type: StringConstructor;
            default: string;
        };
    }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
        [key: string]: any;
    }>, {}, {}, {}, {
        tag: string;
        justify: "center" | "start" | "end" | "space-around" | "space-between";
        align: "center" | "top" | "middle" | "bottom" | "start" | "end" | "stretch" | "baseline";
        gutter: number | import("./type").GutterObject | (number | import("./type").GutterObject)[];
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    align: {
        type: import("vue").PropType<TdRowProps["align"]>;
        default: TdRowProps["align"];
        validator(val: TdRowProps["align"]): boolean;
    };
    gutter: {
        type: import("vue").PropType<TdRowProps["gutter"]>;
        default: TdRowProps["gutter"];
    };
    justify: {
        type: import("vue").PropType<TdRowProps["justify"]>;
        default: TdRowProps["justify"];
        validator(val: TdRowProps["justify"]): boolean;
    };
    tag: {
        type: StringConstructor;
        default: string;
    };
}>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
    [key: string]: any;
}>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    tag: string;
    justify: "center" | "start" | "end" | "space-around" | "space-between";
    align: "center" | "top" | "middle" | "bottom" | "start" | "end" | "stretch" | "baseline";
    gutter: number | import("./type").GutterObject | (number | import("./type").GutterObject)[];
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const Col: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        flex: {
            type: import("vue").PropType<TdColProps["flex"]>;
        };
        lg: {
            type: import("vue").PropType<TdColProps["lg"]>;
        };
        md: {
            type: import("vue").PropType<TdColProps["md"]>;
        };
        offset: {
            type: NumberConstructor;
            default: number;
        };
        order: {
            type: NumberConstructor;
            default: number;
        };
        pull: {
            type: NumberConstructor;
            default: number;
        };
        push: {
            type: NumberConstructor;
            default: number;
        };
        sm: {
            type: import("vue").PropType<TdColProps["sm"]>;
        };
        span: {
            type: NumberConstructor;
        };
        tag: {
            type: StringConstructor;
            default: string;
        };
        xl: {
            type: import("vue").PropType<TdColProps["xl"]>;
        };
        xs: {
            type: import("vue").PropType<TdColProps["xs"]>;
        };
        xxl: {
            type: import("vue").PropType<TdColProps["xxl"]>;
        };
    }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
        [key: string]: any;
    }>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        tag: string;
        push: number;
        offset: number;
        order: number;
        pull: number;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        flex: {
            type: import("vue").PropType<TdColProps["flex"]>;
        };
        lg: {
            type: import("vue").PropType<TdColProps["lg"]>;
        };
        md: {
            type: import("vue").PropType<TdColProps["md"]>;
        };
        offset: {
            type: NumberConstructor;
            default: number;
        };
        order: {
            type: NumberConstructor;
            default: number;
        };
        pull: {
            type: NumberConstructor;
            default: number;
        };
        push: {
            type: NumberConstructor;
            default: number;
        };
        sm: {
            type: import("vue").PropType<TdColProps["sm"]>;
        };
        span: {
            type: NumberConstructor;
        };
        tag: {
            type: StringConstructor;
            default: string;
        };
        xl: {
            type: import("vue").PropType<TdColProps["xl"]>;
        };
        xs: {
            type: import("vue").PropType<TdColProps["xs"]>;
        };
        xxl: {
            type: import("vue").PropType<TdColProps["xxl"]>;
        };
    }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
        [key: string]: any;
    }>, {}, {}, {}, {
        tag: string;
        push: number;
        offset: number;
        order: number;
        pull: number;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    flex: {
        type: import("vue").PropType<TdColProps["flex"]>;
    };
    lg: {
        type: import("vue").PropType<TdColProps["lg"]>;
    };
    md: {
        type: import("vue").PropType<TdColProps["md"]>;
    };
    offset: {
        type: NumberConstructor;
        default: number;
    };
    order: {
        type: NumberConstructor;
        default: number;
    };
    pull: {
        type: NumberConstructor;
        default: number;
    };
    push: {
        type: NumberConstructor;
        default: number;
    };
    sm: {
        type: import("vue").PropType<TdColProps["sm"]>;
    };
    span: {
        type: NumberConstructor;
    };
    tag: {
        type: StringConstructor;
        default: string;
    };
    xl: {
        type: import("vue").PropType<TdColProps["xl"]>;
    };
    xs: {
        type: import("vue").PropType<TdColProps["xs"]>;
    };
    xxl: {
        type: import("vue").PropType<TdColProps["xxl"]>;
    };
}>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
    [key: string]: any;
}>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    tag: string;
    push: number;
    offset: number;
    order: number;
    pull: number;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
declare const _default: {
    Row: {
        new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
            align: {
                type: import("vue").PropType<TdRowProps["align"]>;
                default: TdRowProps["align"];
                validator(val: TdRowProps["align"]): boolean;
            };
            gutter: {
                type: import("vue").PropType<TdRowProps["gutter"]>;
                default: TdRowProps["gutter"];
            };
            justify: {
                type: import("vue").PropType<TdRowProps["justify"]>;
                default: TdRowProps["justify"];
                validator(val: TdRowProps["justify"]): boolean;
            };
            tag: {
                type: StringConstructor;
                default: string;
            };
        }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
            [key: string]: any;
        }>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
            tag: string;
            justify: "center" | "start" | "end" | "space-around" | "space-between";
            align: "center" | "top" | "middle" | "bottom" | "start" | "end" | "stretch" | "baseline";
            gutter: number | import("./type").GutterObject | (number | import("./type").GutterObject)[];
        }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
            P: {};
            B: {};
            D: {};
            C: {};
            M: {};
            Defaults: {};
        }, Readonly<import("vue").ExtractPropTypes<{
            align: {
                type: import("vue").PropType<TdRowProps["align"]>;
                default: TdRowProps["align"];
                validator(val: TdRowProps["align"]): boolean;
            };
            gutter: {
                type: import("vue").PropType<TdRowProps["gutter"]>;
                default: TdRowProps["gutter"];
            };
            justify: {
                type: import("vue").PropType<TdRowProps["justify"]>;
                default: TdRowProps["justify"];
                validator(val: TdRowProps["justify"]): boolean;
            };
            tag: {
                type: StringConstructor;
                default: string;
            };
        }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
            [key: string]: any;
        }>, {}, {}, {}, {
            tag: string;
            justify: "center" | "start" | "end" | "space-around" | "space-between";
            align: "center" | "top" | "middle" | "bottom" | "start" | "end" | "stretch" | "baseline";
            gutter: number | import("./type").GutterObject | (number | import("./type").GutterObject)[];
        }>;
        __isFragment?: never;
        __isTeleport?: never;
        __isSuspense?: never;
    } & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
        align: {
            type: import("vue").PropType<TdRowProps["align"]>;
            default: TdRowProps["align"];
            validator(val: TdRowProps["align"]): boolean;
        };
        gutter: {
            type: import("vue").PropType<TdRowProps["gutter"]>;
            default: TdRowProps["gutter"];
        };
        justify: {
            type: import("vue").PropType<TdRowProps["justify"]>;
            default: TdRowProps["justify"];
            validator(val: TdRowProps["justify"]): boolean;
        };
        tag: {
            type: StringConstructor;
            default: string;
        };
    }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
        [key: string]: any;
    }>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
        tag: string;
        justify: "center" | "start" | "end" | "space-around" | "space-between";
        align: "center" | "top" | "middle" | "bottom" | "start" | "end" | "stretch" | "baseline";
        gutter: number | import("./type").GutterObject | (number | import("./type").GutterObject)[];
    }, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
    Col: {
        new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
            flex: {
                type: import("vue").PropType<TdColProps["flex"]>;
            };
            lg: {
                type: import("vue").PropType<TdColProps["lg"]>;
            };
            md: {
                type: import("vue").PropType<TdColProps["md"]>;
            };
            offset: {
                type: NumberConstructor;
                default: number;
            };
            order: {
                type: NumberConstructor;
                default: number;
            };
            pull: {
                type: NumberConstructor;
                default: number;
            };
            push: {
                type: NumberConstructor;
                default: number;
            };
            sm: {
                type: import("vue").PropType<TdColProps["sm"]>;
            };
            span: {
                type: NumberConstructor;
            };
            tag: {
                type: StringConstructor;
                default: string;
            };
            xl: {
                type: import("vue").PropType<TdColProps["xl"]>;
            };
            xs: {
                type: import("vue").PropType<TdColProps["xs"]>;
            };
            xxl: {
                type: import("vue").PropType<TdColProps["xxl"]>;
            };
        }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
            [key: string]: any;
        }>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
            tag: string;
            push: number;
            offset: number;
            order: number;
            pull: number;
        }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
            P: {};
            B: {};
            D: {};
            C: {};
            M: {};
            Defaults: {};
        }, Readonly<import("vue").ExtractPropTypes<{
            flex: {
                type: import("vue").PropType<TdColProps["flex"]>;
            };
            lg: {
                type: import("vue").PropType<TdColProps["lg"]>;
            };
            md: {
                type: import("vue").PropType<TdColProps["md"]>;
            };
            offset: {
                type: NumberConstructor;
                default: number;
            };
            order: {
                type: NumberConstructor;
                default: number;
            };
            pull: {
                type: NumberConstructor;
                default: number;
            };
            push: {
                type: NumberConstructor;
                default: number;
            };
            sm: {
                type: import("vue").PropType<TdColProps["sm"]>;
            };
            span: {
                type: NumberConstructor;
            };
            tag: {
                type: StringConstructor;
                default: string;
            };
            xl: {
                type: import("vue").PropType<TdColProps["xl"]>;
            };
            xs: {
                type: import("vue").PropType<TdColProps["xs"]>;
            };
            xxl: {
                type: import("vue").PropType<TdColProps["xxl"]>;
            };
        }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
            [key: string]: any;
        }>, {}, {}, {}, {
            tag: string;
            push: number;
            offset: number;
            order: number;
            pull: number;
        }>;
        __isFragment?: never;
        __isTeleport?: never;
        __isSuspense?: never;
    } & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
        flex: {
            type: import("vue").PropType<TdColProps["flex"]>;
        };
        lg: {
            type: import("vue").PropType<TdColProps["lg"]>;
        };
        md: {
            type: import("vue").PropType<TdColProps["md"]>;
        };
        offset: {
            type: NumberConstructor;
            default: number;
        };
        order: {
            type: NumberConstructor;
            default: number;
        };
        pull: {
            type: NumberConstructor;
            default: number;
        };
        push: {
            type: NumberConstructor;
            default: number;
        };
        sm: {
            type: import("vue").PropType<TdColProps["sm"]>;
        };
        span: {
            type: NumberConstructor;
        };
        tag: {
            type: StringConstructor;
            default: string;
        };
        xl: {
            type: import("vue").PropType<TdColProps["xl"]>;
        };
        xs: {
            type: import("vue").PropType<TdColProps["xs"]>;
        };
        xxl: {
            type: import("vue").PropType<TdColProps["xxl"]>;
        };
    }>> & Readonly<{}>, () => import("vue").VNode<import("vue").RendererNode, import("vue").RendererElement, {
        [key: string]: any;
    }>, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
        tag: string;
        push: number;
        offset: number;
        order: number;
        pull: number;
    }, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
};
export default _default;
