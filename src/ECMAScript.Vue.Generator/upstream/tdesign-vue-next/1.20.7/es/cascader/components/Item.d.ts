import { PropType } from 'vue';
import { CascaderContextType, TreeNode, TdCascaderProps } from '../types';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    node: {
        type: PropType<TreeNode>;
        default(): {};
    };
    optionChild: {
        type: PropType<TdCascaderProps["option"]>;
    };
    cascaderContext: {
        type: PropType<CascaderContextType>;
    };
    onChange: PropType<() => void>;
    onClick: PropType<() => void>;
    onMouseenter: PropType<() => void>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    node: {
        type: PropType<TreeNode>;
        default(): {};
    };
    optionChild: {
        type: PropType<TdCascaderProps["option"]>;
    };
    cascaderContext: {
        type: PropType<CascaderContextType>;
    };
    onChange: PropType<() => void>;
    onClick: PropType<() => void>;
    onMouseenter: PropType<() => void>;
}>> & Readonly<{}>, {
    node: TreeNode;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
