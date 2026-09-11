import { PropType } from 'vue';
import { SearchOption, TdTransferProps } from '../types';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    value: {
        type: StringConstructor;
        default: string;
    };
    search: {
        type: PropType<SearchOption>;
        default: TdTransferProps["search"];
    };
    placeholder: {
        type: StringConstructor;
        default: string;
    };
    onChange: FunctionConstructor;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    value: {
        type: StringConstructor;
        default: string;
    };
    search: {
        type: PropType<SearchOption>;
        default: TdTransferProps["search"];
    };
    placeholder: {
        type: StringConstructor;
        default: string;
    };
    onChange: FunctionConstructor;
}>> & Readonly<{}>, {
    search: boolean | import("../..").InputProps | SearchOption[];
    value: string;
    placeholder: string;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
