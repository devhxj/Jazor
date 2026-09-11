import { PropType } from 'vue';
import { Color } from '../../utils';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    colors: {
        type: PropType<string[]>;
        default: () => PropType<string[]>;
    };
    title: {
        type: StringConstructor;
        default: string;
    };
    editable: {
        type: BooleanConstructor;
        default: boolean;
    };
    onSetColor: {
        type: FunctionConstructor;
        default: () => () => void;
    };
    handleAddColor: {
        type: FunctionConstructor;
        default: () => () => void;
    };
    disabled: BooleanConstructor;
    color: {
        type: PropType<Color>;
    };
    onChange: {
        type: FunctionConstructor;
        default: () => () => void;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    colors: {
        type: PropType<string[]>;
        default: () => PropType<string[]>;
    };
    title: {
        type: StringConstructor;
        default: string;
    };
    editable: {
        type: BooleanConstructor;
        default: boolean;
    };
    onSetColor: {
        type: FunctionConstructor;
        default: () => () => void;
    };
    handleAddColor: {
        type: FunctionConstructor;
        default: () => () => void;
    };
    disabled: BooleanConstructor;
    color: {
        type: PropType<Color>;
    };
    onChange: {
        type: FunctionConstructor;
        default: () => () => void;
    };
}>> & Readonly<{}>, {
    title: string;
    disabled: boolean;
    onChange: Function;
    colors: string[];
    editable: boolean;
    onSetColor: Function;
    handleAddColor: Function;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
