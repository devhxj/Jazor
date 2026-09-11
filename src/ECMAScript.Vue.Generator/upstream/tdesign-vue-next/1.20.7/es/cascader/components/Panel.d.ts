import { PropType } from 'vue';
import { CascaderContextType } from '../types';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    option: {
        type: PropType<import("..").TdCascaderProps["option"]>;
    };
    options: {
        type: PropType<import("..").TdCascaderProps["options"]>;
        default: () => import("..").TdCascaderProps["options"];
    };
    empty: {
        type: PropType<import("..").TdCascaderProps["empty"]>;
    };
    trigger: {
        type: PropType<import("..").TdCascaderProps["trigger"]>;
        default: import("..").TdCascaderProps["trigger"];
        validator(val: import("..").TdCascaderProps["trigger"]): boolean;
    };
    onChange: PropType<(value: import("..").CascaderValue<import("../..").TreeOptionData>, context: import("..").CascaderChangeContext<import("../..").TreeOptionData>) => void>;
    loading: BooleanConstructor;
    loadingText: {
        type: PropType<import("..").TdCascaderProps["loadingText"]>;
    };
    cascaderContext: {
        type: PropType<CascaderContextType>;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    option: {
        type: PropType<import("..").TdCascaderProps["option"]>;
    };
    options: {
        type: PropType<import("..").TdCascaderProps["options"]>;
        default: () => import("..").TdCascaderProps["options"];
    };
    empty: {
        type: PropType<import("..").TdCascaderProps["empty"]>;
    };
    trigger: {
        type: PropType<import("..").TdCascaderProps["trigger"]>;
        default: import("..").TdCascaderProps["trigger"];
        validator(val: import("..").TdCascaderProps["trigger"]): boolean;
    };
    onChange: PropType<(value: import("..").CascaderValue<import("../..").TreeOptionData>, context: import("..").CascaderChangeContext<import("../..").TreeOptionData>) => void>;
    loading: BooleanConstructor;
    loadingText: {
        type: PropType<import("..").TdCascaderProps["loadingText"]>;
    };
    cascaderContext: {
        type: PropType<CascaderContextType>;
    };
}>> & Readonly<{}>, {
    loading: boolean;
    options: import("../..").TreeOptionData[];
    trigger: "click" | "hover";
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
