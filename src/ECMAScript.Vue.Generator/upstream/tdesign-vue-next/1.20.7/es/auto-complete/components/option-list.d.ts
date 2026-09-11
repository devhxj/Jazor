import { PropType } from 'vue';
import { CommonClassNameType } from '@tdesign/shared-hooks';
import { TdAutoCompleteProps } from '../type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    sizeClassNames: PropType<CommonClassNameType["SIZE"]>;
    value: StringConstructor;
    size: PropType<TdAutoCompleteProps["size"]>;
    options: PropType<TdAutoCompleteProps["options"]>;
    popupVisible: BooleanConstructor;
    highlightKeyword: BooleanConstructor;
    filterable: BooleanConstructor;
    filter: PropType<TdAutoCompleteProps["filter"]>;
    empty: PropType<TdAutoCompleteProps["empty"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "select"[], "select", import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    sizeClassNames: PropType<CommonClassNameType["SIZE"]>;
    value: StringConstructor;
    size: PropType<TdAutoCompleteProps["size"]>;
    options: PropType<TdAutoCompleteProps["options"]>;
    popupVisible: BooleanConstructor;
    highlightKeyword: BooleanConstructor;
    filterable: BooleanConstructor;
    filter: PropType<TdAutoCompleteProps["filter"]>;
    empty: PropType<TdAutoCompleteProps["empty"]>;
}>> & Readonly<{
    onSelect?: (...args: any[]) => any;
}>, {
    popupVisible: boolean;
    filterable: boolean;
    highlightKeyword: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
