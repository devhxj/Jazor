import { ComponentPublicInstance } from 'vue';
import { ANCHOR_CONTAINER } from './utils';
export interface Anchor extends ComponentPublicInstance {
    scrollContainer: ANCHOR_CONTAINER;
    handleScrollLock: boolean;
}
declare const _default: import("vue").DefineComponent<{
    affixProps?: import("..").AffixProps;
    bounds?: number;
    container?: import("..").ScrollContainer;
    cursor?: import("..").TNode;
    size?: "small" | "medium" | "large";
    targetOffset?: number;
    onChange?: (currentLink: string, prevLink: string) => void;
    onClick?: (link: {
        href: string;
        title: string;
        e: MouseEvent;
    }) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    affixProps?: import("..").AffixProps;
    bounds?: number;
    container?: import("..").ScrollContainer;
    cursor?: import("..").TNode;
    size?: "small" | "medium" | "large";
    targetOffset?: number;
    onChange?: (currentLink: string, prevLink: string) => void;
    onClick?: (link: {
        href: string;
        title: string;
        e: MouseEvent;
    }) => void;
}> & Readonly<{}>, {
    size: "small" | "medium" | "large";
    container: import("..").ScrollContainer;
    bounds: number;
    targetOffset: number;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
