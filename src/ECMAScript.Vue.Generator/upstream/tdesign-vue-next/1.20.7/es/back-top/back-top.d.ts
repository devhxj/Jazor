declare const _default: import("vue").DefineComponent<{
    container?: import("..").AttachNode;
    content?: string | import("..").TNode;
    default?: string | import("..").TNode;
    duration?: number;
    offset?: Array<string | number>;
    shape?: import("./type").BackTopShapeEnum;
    size?: "medium" | "small";
    target?: import("..").AttachNode;
    theme?: "light" | "primary" | "dark";
    visibleHeight?: string | number;
    onClick?: (context: {
        e: MouseEvent;
    }) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    container?: import("..").AttachNode;
    content?: string | import("..").TNode;
    default?: string | import("..").TNode;
    duration?: number;
    offset?: Array<string | number>;
    shape?: import("./type").BackTopShapeEnum;
    size?: "medium" | "small";
    target?: import("..").AttachNode;
    theme?: "light" | "primary" | "dark";
    visibleHeight?: string | number;
    onClick?: (context: {
        e: MouseEvent;
    }) => void;
}> & Readonly<{}>, {
    size: "small" | "medium";
    offset: (string | number)[];
    duration: number;
    theme: "primary" | "dark" | "light";
    container: import("..").AttachNode;
    target: import("..").AttachNode;
    shape: import("./type").BackTopShapeEnum;
    visibleHeight: string | number;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
