import { PropType } from 'vue';
import { TdSliderProps } from './type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    mark: {
        type: PropType<TdSliderProps["marks"]>;
    };
    point: {
        type: NumberConstructor;
    };
    onClickMarkPoint: {
        type: FunctionConstructor;
        default: () => void;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    mark: {
        type: PropType<TdSliderProps["marks"]>;
    };
    point: {
        type: NumberConstructor;
    };
    onClickMarkPoint: {
        type: FunctionConstructor;
        default: () => void;
    };
}>> & Readonly<{}>, {
    onClickMarkPoint: Function;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
