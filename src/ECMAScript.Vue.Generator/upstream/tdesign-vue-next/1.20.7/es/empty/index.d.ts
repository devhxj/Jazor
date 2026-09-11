import type { TdEmptyProps } from './type';
import './style';
export * from './type';
export type EmptyProps = TdEmptyProps;
export declare const Empty: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        action?: import("..").TNode;
        description?: string | import("..").TNode;
        image?: string | import("..").ImageProps | import("..").TNode;
        imageStyle?: import("..").Styles;
        size?: import("..").SizeEnum;
        title?: string | import("..").TNode;
        type?: "empty" | "success" | "fail" | "network-error" | "maintenance";
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        type: "empty" | "success" | "fail" | "network-error" | "maintenance";
        size: import("..").SizeEnum;
    }, true, {}, {}, {
        TImage: {
            new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
                onClick: FunctionConstructor;
                alt: {
                    type: StringConstructor;
                    default: string;
                };
                error: {
                    type: import("vue").PropType<import("..").TdImageProps["error"]>;
                };
                fallback: {
                    type: StringConstructor;
                    default: string;
                };
                fit: {
                    type: import("vue").PropType<import("..").TdImageProps["fit"]>;
                    default: import("..").TdImageProps["fit"];
                    validator(val: import("..").TdImageProps["fit"]): boolean;
                };
                gallery: BooleanConstructor;
                lazy: BooleanConstructor;
                loading: {
                    type: import("vue").PropType<import("..").TdImageProps["loading"]>;
                };
                overlayContent: {
                    type: import("vue").PropType<import("..").TdImageProps["overlayContent"]>;
                };
                overlayTrigger: {
                    type: import("vue").PropType<import("..").TdImageProps["overlayTrigger"]>;
                    default: import("..").TdImageProps["overlayTrigger"];
                    validator(val: import("..").TdImageProps["overlayTrigger"]): boolean;
                };
                placeholder: {
                    type: import("vue").PropType<import("..").TdImageProps["placeholder"]>;
                };
                position: {
                    type: StringConstructor;
                    default: string;
                };
                referrerpolicy: {
                    type: import("vue").PropType<import("..").TdImageProps["referrerpolicy"]>;
                    default: import("..").TdImageProps["referrerpolicy"];
                    validator(val: import("..").TdImageProps["referrerpolicy"]): boolean;
                };
                shape: {
                    type: import("vue").PropType<import("..").TdImageProps["shape"]>;
                    default: import("..").TdImageProps["shape"];
                    validator(val: import("..").TdImageProps["shape"]): boolean;
                };
                src: {
                    type: import("vue").PropType<import("..").TdImageProps["src"]>;
                };
                srcset: {
                    type: import("vue").PropType<import("..").TdImageProps["srcset"]>;
                };
                onError: import("vue").PropType<import("..").TdImageProps["onError"]>;
                onLoad: import("vue").PropType<import("..").TdImageProps["onLoad"]>;
            }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
                position: string;
                lazy: boolean;
                shape: "circle" | "round" | "square";
                fit: "fill" | "none" | "contain" | "cover" | "scale-down";
                overlayTrigger: "always" | "hover";
                referrerpolicy: "no-referrer" | "origin" | "no-referrer-when-downgrade" | "origin-when-cross-origin" | "same-origin" | "strict-origin" | "strict-origin-when-cross-origin" | "unsafe-url";
                alt: string;
                fallback: string;
                gallery: boolean;
            }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
                P: {};
                B: {};
                D: {};
                C: {};
                M: {};
                Defaults: {};
            }, Readonly<import("vue").ExtractPropTypes<{
                onClick: FunctionConstructor;
                alt: {
                    type: StringConstructor;
                    default: string;
                };
                error: {
                    type: import("vue").PropType<import("..").TdImageProps["error"]>;
                };
                fallback: {
                    type: StringConstructor;
                    default: string;
                };
                fit: {
                    type: import("vue").PropType<import("..").TdImageProps["fit"]>;
                    default: import("..").TdImageProps["fit"];
                    validator(val: import("..").TdImageProps["fit"]): boolean;
                };
                gallery: BooleanConstructor;
                lazy: BooleanConstructor;
                loading: {
                    type: import("vue").PropType<import("..").TdImageProps["loading"]>;
                };
                overlayContent: {
                    type: import("vue").PropType<import("..").TdImageProps["overlayContent"]>;
                };
                overlayTrigger: {
                    type: import("vue").PropType<import("..").TdImageProps["overlayTrigger"]>;
                    default: import("..").TdImageProps["overlayTrigger"];
                    validator(val: import("..").TdImageProps["overlayTrigger"]): boolean;
                };
                placeholder: {
                    type: import("vue").PropType<import("..").TdImageProps["placeholder"]>;
                };
                position: {
                    type: StringConstructor;
                    default: string;
                };
                referrerpolicy: {
                    type: import("vue").PropType<import("..").TdImageProps["referrerpolicy"]>;
                    default: import("..").TdImageProps["referrerpolicy"];
                    validator(val: import("..").TdImageProps["referrerpolicy"]): boolean;
                };
                shape: {
                    type: import("vue").PropType<import("..").TdImageProps["shape"]>;
                    default: import("..").TdImageProps["shape"];
                    validator(val: import("..").TdImageProps["shape"]): boolean;
                };
                src: {
                    type: import("vue").PropType<import("..").TdImageProps["src"]>;
                };
                srcset: {
                    type: import("vue").PropType<import("..").TdImageProps["srcset"]>;
                };
                onError: import("vue").PropType<import("..").TdImageProps["onError"]>;
                onLoad: import("vue").PropType<import("..").TdImageProps["onLoad"]>;
            }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
                position: string;
                lazy: boolean;
                shape: "circle" | "round" | "square";
                fit: "fill" | "none" | "contain" | "cover" | "scale-down";
                overlayTrigger: "always" | "hover";
                referrerpolicy: "no-referrer" | "origin" | "no-referrer-when-downgrade" | "origin-when-cross-origin" | "same-origin" | "strict-origin" | "strict-origin-when-cross-origin" | "unsafe-url";
                alt: string;
                fallback: string;
                gallery: boolean;
            }>;
            __isFragment?: never;
            __isTeleport?: never;
            __isSuspense?: never;
        } & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
            onClick: FunctionConstructor;
            alt: {
                type: StringConstructor;
                default: string;
            };
            error: {
                type: import("vue").PropType<import("..").TdImageProps["error"]>;
            };
            fallback: {
                type: StringConstructor;
                default: string;
            };
            fit: {
                type: import("vue").PropType<import("..").TdImageProps["fit"]>;
                default: import("..").TdImageProps["fit"];
                validator(val: import("..").TdImageProps["fit"]): boolean;
            };
            gallery: BooleanConstructor;
            lazy: BooleanConstructor;
            loading: {
                type: import("vue").PropType<import("..").TdImageProps["loading"]>;
            };
            overlayContent: {
                type: import("vue").PropType<import("..").TdImageProps["overlayContent"]>;
            };
            overlayTrigger: {
                type: import("vue").PropType<import("..").TdImageProps["overlayTrigger"]>;
                default: import("..").TdImageProps["overlayTrigger"];
                validator(val: import("..").TdImageProps["overlayTrigger"]): boolean;
            };
            placeholder: {
                type: import("vue").PropType<import("..").TdImageProps["placeholder"]>;
            };
            position: {
                type: StringConstructor;
                default: string;
            };
            referrerpolicy: {
                type: import("vue").PropType<import("..").TdImageProps["referrerpolicy"]>;
                default: import("..").TdImageProps["referrerpolicy"];
                validator(val: import("..").TdImageProps["referrerpolicy"]): boolean;
            };
            shape: {
                type: import("vue").PropType<import("..").TdImageProps["shape"]>;
                default: import("..").TdImageProps["shape"];
                validator(val: import("..").TdImageProps["shape"]): boolean;
            };
            src: {
                type: import("vue").PropType<import("..").TdImageProps["src"]>;
            };
            srcset: {
                type: import("vue").PropType<import("..").TdImageProps["srcset"]>;
            };
            onError: import("vue").PropType<import("..").TdImageProps["onError"]>;
            onLoad: import("vue").PropType<import("..").TdImageProps["onLoad"]>;
        }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
            position: string;
            lazy: boolean;
            shape: "circle" | "round" | "square";
            fit: "fill" | "none" | "contain" | "cover" | "scale-down";
            overlayTrigger: "always" | "hover";
            referrerpolicy: "no-referrer" | "origin" | "no-referrer-when-downgrade" | "origin-when-cross-origin" | "same-origin" | "strict-origin" | "strict-origin-when-cross-origin" | "unsafe-url";
            alt: string;
            fallback: string;
            gallery: boolean;
        }, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
    } & import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        action?: import("..").TNode;
        description?: string | import("..").TNode;
        image?: string | import("..").ImageProps | import("..").TNode;
        imageStyle?: import("..").Styles;
        size?: import("..").SizeEnum;
        title?: string | import("..").TNode;
        type?: "empty" | "success" | "fail" | "network-error" | "maintenance";
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        type: "empty" | "success" | "fail" | "network-error" | "maintenance";
        size: import("..").SizeEnum;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    action?: import("..").TNode;
    description?: string | import("..").TNode;
    image?: string | import("..").ImageProps | import("..").TNode;
    imageStyle?: import("..").Styles;
    size?: import("..").SizeEnum;
    title?: string | import("..").TNode;
    type?: "empty" | "success" | "fail" | "network-error" | "maintenance";
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    type: "empty" | "success" | "fail" | "network-error" | "maintenance";
    size: import("..").SizeEnum;
}, {}, string, {}, {
    TImage: {
        new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
            onClick: FunctionConstructor;
            alt: {
                type: StringConstructor;
                default: string;
            };
            error: {
                type: import("vue").PropType<import("..").TdImageProps["error"]>;
            };
            fallback: {
                type: StringConstructor;
                default: string;
            };
            fit: {
                type: import("vue").PropType<import("..").TdImageProps["fit"]>;
                default: import("..").TdImageProps["fit"];
                validator(val: import("..").TdImageProps["fit"]): boolean;
            };
            gallery: BooleanConstructor;
            lazy: BooleanConstructor;
            loading: {
                type: import("vue").PropType<import("..").TdImageProps["loading"]>;
            };
            overlayContent: {
                type: import("vue").PropType<import("..").TdImageProps["overlayContent"]>;
            };
            overlayTrigger: {
                type: import("vue").PropType<import("..").TdImageProps["overlayTrigger"]>;
                default: import("..").TdImageProps["overlayTrigger"];
                validator(val: import("..").TdImageProps["overlayTrigger"]): boolean;
            };
            placeholder: {
                type: import("vue").PropType<import("..").TdImageProps["placeholder"]>;
            };
            position: {
                type: StringConstructor;
                default: string;
            };
            referrerpolicy: {
                type: import("vue").PropType<import("..").TdImageProps["referrerpolicy"]>;
                default: import("..").TdImageProps["referrerpolicy"];
                validator(val: import("..").TdImageProps["referrerpolicy"]): boolean;
            };
            shape: {
                type: import("vue").PropType<import("..").TdImageProps["shape"]>;
                default: import("..").TdImageProps["shape"];
                validator(val: import("..").TdImageProps["shape"]): boolean;
            };
            src: {
                type: import("vue").PropType<import("..").TdImageProps["src"]>;
            };
            srcset: {
                type: import("vue").PropType<import("..").TdImageProps["srcset"]>;
            };
            onError: import("vue").PropType<import("..").TdImageProps["onError"]>;
            onLoad: import("vue").PropType<import("..").TdImageProps["onLoad"]>;
        }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
            position: string;
            lazy: boolean;
            shape: "circle" | "round" | "square";
            fit: "fill" | "none" | "contain" | "cover" | "scale-down";
            overlayTrigger: "always" | "hover";
            referrerpolicy: "no-referrer" | "origin" | "no-referrer-when-downgrade" | "origin-when-cross-origin" | "same-origin" | "strict-origin" | "strict-origin-when-cross-origin" | "unsafe-url";
            alt: string;
            fallback: string;
            gallery: boolean;
        }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
            P: {};
            B: {};
            D: {};
            C: {};
            M: {};
            Defaults: {};
        }, Readonly<import("vue").ExtractPropTypes<{
            onClick: FunctionConstructor;
            alt: {
                type: StringConstructor;
                default: string;
            };
            error: {
                type: import("vue").PropType<import("..").TdImageProps["error"]>;
            };
            fallback: {
                type: StringConstructor;
                default: string;
            };
            fit: {
                type: import("vue").PropType<import("..").TdImageProps["fit"]>;
                default: import("..").TdImageProps["fit"];
                validator(val: import("..").TdImageProps["fit"]): boolean;
            };
            gallery: BooleanConstructor;
            lazy: BooleanConstructor;
            loading: {
                type: import("vue").PropType<import("..").TdImageProps["loading"]>;
            };
            overlayContent: {
                type: import("vue").PropType<import("..").TdImageProps["overlayContent"]>;
            };
            overlayTrigger: {
                type: import("vue").PropType<import("..").TdImageProps["overlayTrigger"]>;
                default: import("..").TdImageProps["overlayTrigger"];
                validator(val: import("..").TdImageProps["overlayTrigger"]): boolean;
            };
            placeholder: {
                type: import("vue").PropType<import("..").TdImageProps["placeholder"]>;
            };
            position: {
                type: StringConstructor;
                default: string;
            };
            referrerpolicy: {
                type: import("vue").PropType<import("..").TdImageProps["referrerpolicy"]>;
                default: import("..").TdImageProps["referrerpolicy"];
                validator(val: import("..").TdImageProps["referrerpolicy"]): boolean;
            };
            shape: {
                type: import("vue").PropType<import("..").TdImageProps["shape"]>;
                default: import("..").TdImageProps["shape"];
                validator(val: import("..").TdImageProps["shape"]): boolean;
            };
            src: {
                type: import("vue").PropType<import("..").TdImageProps["src"]>;
            };
            srcset: {
                type: import("vue").PropType<import("..").TdImageProps["srcset"]>;
            };
            onError: import("vue").PropType<import("..").TdImageProps["onError"]>;
            onLoad: import("vue").PropType<import("..").TdImageProps["onLoad"]>;
        }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
            position: string;
            lazy: boolean;
            shape: "circle" | "round" | "square";
            fit: "fill" | "none" | "contain" | "cover" | "scale-down";
            overlayTrigger: "always" | "hover";
            referrerpolicy: "no-referrer" | "origin" | "no-referrer-when-downgrade" | "origin-when-cross-origin" | "same-origin" | "strict-origin" | "strict-origin-when-cross-origin" | "unsafe-url";
            alt: string;
            fallback: string;
            gallery: boolean;
        }>;
        __isFragment?: never;
        __isTeleport?: never;
        __isSuspense?: never;
    } & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
        onClick: FunctionConstructor;
        alt: {
            type: StringConstructor;
            default: string;
        };
        error: {
            type: import("vue").PropType<import("..").TdImageProps["error"]>;
        };
        fallback: {
            type: StringConstructor;
            default: string;
        };
        fit: {
            type: import("vue").PropType<import("..").TdImageProps["fit"]>;
            default: import("..").TdImageProps["fit"];
            validator(val: import("..").TdImageProps["fit"]): boolean;
        };
        gallery: BooleanConstructor;
        lazy: BooleanConstructor;
        loading: {
            type: import("vue").PropType<import("..").TdImageProps["loading"]>;
        };
        overlayContent: {
            type: import("vue").PropType<import("..").TdImageProps["overlayContent"]>;
        };
        overlayTrigger: {
            type: import("vue").PropType<import("..").TdImageProps["overlayTrigger"]>;
            default: import("..").TdImageProps["overlayTrigger"];
            validator(val: import("..").TdImageProps["overlayTrigger"]): boolean;
        };
        placeholder: {
            type: import("vue").PropType<import("..").TdImageProps["placeholder"]>;
        };
        position: {
            type: StringConstructor;
            default: string;
        };
        referrerpolicy: {
            type: import("vue").PropType<import("..").TdImageProps["referrerpolicy"]>;
            default: import("..").TdImageProps["referrerpolicy"];
            validator(val: import("..").TdImageProps["referrerpolicy"]): boolean;
        };
        shape: {
            type: import("vue").PropType<import("..").TdImageProps["shape"]>;
            default: import("..").TdImageProps["shape"];
            validator(val: import("..").TdImageProps["shape"]): boolean;
        };
        src: {
            type: import("vue").PropType<import("..").TdImageProps["src"]>;
        };
        srcset: {
            type: import("vue").PropType<import("..").TdImageProps["srcset"]>;
        };
        onError: import("vue").PropType<import("..").TdImageProps["onError"]>;
        onLoad: import("vue").PropType<import("..").TdImageProps["onLoad"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
        position: string;
        lazy: boolean;
        shape: "circle" | "round" | "square";
        fit: "fill" | "none" | "contain" | "cover" | "scale-down";
        overlayTrigger: "always" | "hover";
        referrerpolicy: "no-referrer" | "origin" | "no-referrer-when-downgrade" | "origin-when-cross-origin" | "same-origin" | "strict-origin" | "strict-origin-when-cross-origin" | "unsafe-url";
        alt: string;
        fallback: string;
        gallery: boolean;
    }, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
} & import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Empty;
