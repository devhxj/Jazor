import { PropType } from 'vue';
import { EmptyType, SearchOption, TransferValue, TdTransferProps, TransferListType, TransferItemOption } from '../types';
import { CheckboxProps } from '../../checkbox';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    checkboxProps: {
        type: PropType<CheckboxProps>;
        default: () => {};
    };
    dataSource: {
        type: PropType<Array<TransferItemOption>>;
        default(): Array<TransferItemOption>;
    };
    listType: {
        type: PropType<TransferListType>;
        default: string;
    };
    title: {
        type: (StringConstructor | FunctionConstructor)[];
    };
    checkedValue: {
        type: PropType<Array<TransferValue>>;
        default(): Array<TransferValue>;
    };
    disabled: {
        type: BooleanConstructor;
        default: boolean;
    };
    search: {
        type: PropType<SearchOption>;
        default: TdTransferProps["search"];
    };
    transferItem: PropType<TdTransferProps["transferItem"]>;
    empty: {
        type: PropType<EmptyType>;
    };
    pagination: (ObjectConstructor | BooleanConstructor)[];
    footer: (StringConstructor | FunctionConstructor)[];
    checkAll: BooleanConstructor;
    isTreeMode: {
        type: PropType<boolean>;
        default: boolean;
    };
    onCheckedChange: PropType<(event: Array<TransferValue>) => void>;
    onPageChange: FunctionConstructor;
    onScroll: FunctionConstructor;
    onSearch: FunctionConstructor;
    onDataChange: PropType<(data: Array<TransferValue>, movedValue: Array<TransferValue>) => void>;
    draggable: BooleanConstructor;
    currentValue: {
        type: PropType<Array<TransferValue>>;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    checkboxProps: {
        type: PropType<CheckboxProps>;
        default: () => {};
    };
    dataSource: {
        type: PropType<Array<TransferItemOption>>;
        default(): Array<TransferItemOption>;
    };
    listType: {
        type: PropType<TransferListType>;
        default: string;
    };
    title: {
        type: (StringConstructor | FunctionConstructor)[];
    };
    checkedValue: {
        type: PropType<Array<TransferValue>>;
        default(): Array<TransferValue>;
    };
    disabled: {
        type: BooleanConstructor;
        default: boolean;
    };
    search: {
        type: PropType<SearchOption>;
        default: TdTransferProps["search"];
    };
    transferItem: PropType<TdTransferProps["transferItem"]>;
    empty: {
        type: PropType<EmptyType>;
    };
    pagination: (ObjectConstructor | BooleanConstructor)[];
    footer: (StringConstructor | FunctionConstructor)[];
    checkAll: BooleanConstructor;
    isTreeMode: {
        type: PropType<boolean>;
        default: boolean;
    };
    onCheckedChange: PropType<(event: Array<TransferValue>) => void>;
    onPageChange: FunctionConstructor;
    onScroll: FunctionConstructor;
    onSearch: FunctionConstructor;
    onDataChange: PropType<(data: Array<TransferValue>, movedValue: Array<TransferValue>) => void>;
    draggable: BooleanConstructor;
    currentValue: {
        type: PropType<Array<TransferValue>>;
    };
}>> & Readonly<{}>, {
    search: boolean | import("../..").InputProps | SearchOption[];
    disabled: boolean;
    draggable: boolean;
    checkAll: boolean;
    isTreeMode: boolean;
    checkboxProps: import("../..").TdCheckboxProps;
    dataSource: TransferItemOption[];
    listType: TransferListType;
    checkedValue: TransferValue[];
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
