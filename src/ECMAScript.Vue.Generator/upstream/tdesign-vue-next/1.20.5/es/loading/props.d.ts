import { TdLoadingProps } from './type';
import { PropType } from 'vue';
declare const _default: {
    attach: {
        type: PropType<TdLoadingProps["attach"]>;
        default: TdLoadingProps["attach"];
    };
    content: {
        type: PropType<TdLoadingProps["content"]>;
    };
    default: {
        type: PropType<TdLoadingProps["default"]>;
    };
    delay: {
        type: NumberConstructor;
        default: any;
    };
    fullscreen: BooleanConstructor;
    indicator: {
        type: PropType<TdLoadingProps["indicator"]>;
        default: TdLoadingProps["indicator"];
    };
    inheritColor: {
        type: BooleanConstructor;
        default: any;
    };
    loading: {
        type: BooleanConstructor;
        default: boolean;
    };
    preventScrollThrough: {
        type: BooleanConstructor;
        default: any;
    };
    showOverlay: {
        type: BooleanConstructor;
        default: any;
    };
    size: {
        type: StringConstructor;
        default: any;
    };
    text: {
        type: PropType<TdLoadingProps["text"]>;
    };
    zIndex: {
        type: NumberConstructor;
    };
};
export default _default;
