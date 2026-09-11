declare const _default: import("vue").DefineComponent<{
    content?: string | import("..").TNode;
    default?: string | import("..").TNode;
    disabled?: boolean;
    href?: string;
    icon?: import("..").TNode;
    replace?: boolean;
    router?: Record<string, any>;
    routerLink?: boolean;
    target?: "_blank" | "_self" | "_parent" | "_top";
    to?: string | import("./type").MenuRoute;
    value?: import("./type").MenuValue;
    onClick?: (context: {
        e: MouseEvent;
        value: import("./type").MenuValue;
    }) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "click"[], "click", import("vue").PublicProps, Readonly<{
    content?: string | import("..").TNode;
    default?: string | import("..").TNode;
    disabled?: boolean;
    href?: string;
    icon?: import("..").TNode;
    replace?: boolean;
    router?: Record<string, any>;
    routerLink?: boolean;
    target?: "_blank" | "_self" | "_parent" | "_top";
    to?: string | import("./type").MenuRoute;
    value?: import("./type").MenuValue;
    onClick?: (context: {
        e: MouseEvent;
        value: import("./type").MenuValue;
    }) => void;
}> & Readonly<{
    onClick?: (...args: any[]) => any;
}>, {
    replace: boolean;
    disabled: boolean;
    href: string;
    target: "_self" | "_blank" | "_parent" | "_top";
    routerLink: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
