declare const _default: import("vue").DefineComponent<{
    asyncLoading?: string | import("..").TNode;
    footer?: string | import("..").TNode;
    header?: string | import("..").TNode;
    layout?: "horizontal" | "vertical";
    scroll?: import("..").TScroll;
    size?: "small" | "medium" | "large";
    split?: boolean;
    stripe?: boolean;
    onLoadMore?: (options: {
        e: MouseEvent;
    }) => void;
    onScroll?: (options: {
        e: Event | WheelEvent;
        scrollTop: number;
        scrollBottom: number;
    }) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    asyncLoading?: string | import("..").TNode;
    footer?: string | import("..").TNode;
    header?: string | import("..").TNode;
    layout?: "horizontal" | "vertical";
    scroll?: import("..").TScroll;
    size?: "small" | "medium" | "large";
    split?: boolean;
    stripe?: boolean;
    onLoadMore?: (options: {
        e: MouseEvent;
    }) => void;
    onScroll?: (options: {
        e: Event | WheelEvent;
        scrollTop: number;
        scrollBottom: number;
    }) => void;
}> & Readonly<{}>, {
    layout: "vertical" | "horizontal";
    split: boolean;
    size: "small" | "medium" | "large";
    stripe: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
