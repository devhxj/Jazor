import { PropType } from 'vue';
import { SearchOption, TdTransferProps } from '../types';
declare const _default: import("vue").DefineComponent<{
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
}, () => import("vue/jsx-runtime").JSX.Element, unknown, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps, Readonly<import("vue").ExtractPropTypes<{
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
}>>, {
    value: string;
    search: boolean | import("../..").InputProps | SearchOption[];
    placeholder: string;
}, {}>;
export default _default;
