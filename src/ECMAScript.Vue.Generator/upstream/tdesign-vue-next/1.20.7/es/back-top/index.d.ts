import { TdBackTopProps } from './type';
import './style';
export * from './type';
export type BackTopProps = TdBackTopProps;
export declare const BackTop: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
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
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        size: "small" | "medium";
        offset: (string | number)[];
        duration: number;
        theme: "primary" | "dark" | "light";
        container: import("..").AttachNode;
        target: import("..").AttachNode;
        shape: import("./type").BackTopShapeEnum;
        visibleHeight: string | number;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
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
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        size: "small" | "medium";
        offset: (string | number)[];
        duration: number;
        theme: "primary" | "dark" | "light";
        container: import("..").AttachNode;
        target: import("..").AttachNode;
        shape: import("./type").BackTopShapeEnum;
        visibleHeight: string | number;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
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
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    size: "small" | "medium";
    offset: (string | number)[];
    duration: number;
    theme: "primary" | "dark" | "light";
    container: import("..").AttachNode;
    target: import("..").AttachNode;
    shape: import("./type").BackTopShapeEnum;
    visibleHeight: string | number;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default BackTop;
