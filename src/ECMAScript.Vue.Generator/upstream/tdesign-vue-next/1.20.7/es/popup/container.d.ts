import { PropType } from 'vue';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    parent: ObjectConstructor;
    visible: BooleanConstructor;
    attach: {
        type: PropType<import("./type").TdPopupProps["attach"]>;
        default: import("./type").TdPopupProps["attach"];
    };
    forwardRef: PropType<(el: HTMLElement) => void>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, ("resize" | "contentMounted")[], "resize" | "contentMounted", import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    parent: ObjectConstructor;
    visible: BooleanConstructor;
    attach: {
        type: PropType<import("./type").TdPopupProps["attach"]>;
        default: import("./type").TdPopupProps["attach"];
    };
    forwardRef: PropType<(el: HTMLElement) => void>;
}>> & Readonly<{
    onResize?: (...args: any[]) => any;
    onContentMounted?: (...args: any[]) => any;
}>, {
    visible: boolean;
    attach: import("..").AttachNode;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
