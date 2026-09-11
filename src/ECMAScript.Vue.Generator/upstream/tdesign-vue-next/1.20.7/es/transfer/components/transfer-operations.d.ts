import { PropType } from 'vue';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    leftDisabled: {
        type: PropType<boolean>;
        required: true;
    };
    rightDisabled: {
        type: PropType<boolean>;
        required: true;
    };
    operation: {
        type: PropType<import("../type").TdTransferProps["operation"]>;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, ("moveToRight" | "moveToLeft")[], "moveToRight" | "moveToLeft", import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    leftDisabled: {
        type: PropType<boolean>;
        required: true;
    };
    rightDisabled: {
        type: PropType<boolean>;
        required: true;
    };
    operation: {
        type: PropType<import("../type").TdTransferProps["operation"]>;
    };
}>> & Readonly<{
    onMoveToRight?: (...args: any[]) => any;
    onMoveToLeft?: (...args: any[]) => any;
}>, {}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
