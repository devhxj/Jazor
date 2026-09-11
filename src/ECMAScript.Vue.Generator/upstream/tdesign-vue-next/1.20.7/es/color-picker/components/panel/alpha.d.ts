import { PropType } from 'vue';
import { Color } from '../../utils';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    color: {
        type: PropType<Color>;
    };
    disabled: {
        type: BooleanConstructor;
        default: boolean;
    };
    onChange: {
        type: FunctionConstructor;
        default: () => () => void;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    color: {
        type: PropType<Color>;
    };
    disabled: {
        type: BooleanConstructor;
        default: boolean;
    };
    onChange: {
        type: FunctionConstructor;
        default: () => () => void;
    };
}>> & Readonly<{}>, {
    disabled: boolean;
    onChange: Function;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
