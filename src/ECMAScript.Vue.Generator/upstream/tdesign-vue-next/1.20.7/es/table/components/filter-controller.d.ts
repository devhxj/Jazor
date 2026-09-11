import { PopupProps } from '../../popup';
import { PrimaryTableCol, FilterValue, TdPrimaryTableProps } from '../type';
import type { AttachNode } from '../../common';
import type { TableConfig } from '../../config-provider';
export interface TableFilterControllerProps {
    locale: TableConfig;
    tFilterValue: FilterValue;
    innerFilterValue: FilterValue;
    tableFilterClasses: {
        filterable: string;
        popup: string;
        icon: string;
        popupContent: string;
        result: string;
        inner: string;
        bottomButtons: string;
        contentInner: string;
        iconWrap: string;
    };
    isFocusClass: string;
    column: PrimaryTableCol;
    colIndex: number;
    primaryTableElement: any;
    popupProps: PopupProps;
    attach?: AttachNode;
    onVisibleChange: (val: boolean) => void;
    onInnerFilterChange: (val: any, column: PrimaryTableCol) => void;
    filterIcon?: TdPrimaryTableProps['filterIcon'];
}
declare const _default: import("vue").DefineComponent<{
    locale: TableConfig;
    tFilterValue: FilterValue;
    innerFilterValue: FilterValue;
    tableFilterClasses: {
        filterable: string;
        popup: string;
        icon: string;
        popupContent: string;
        result: string;
        inner: string;
        bottomButtons: string;
        contentInner: string;
        iconWrap: string;
    };
    isFocusClass: string;
    column: PrimaryTableCol;
    colIndex: number;
    primaryTableElement: any;
    popupProps: PopupProps;
    attach?: AttachNode;
    onVisibleChange: (val: boolean) => void;
    onInnerFilterChange: (val: any, column: PrimaryTableCol) => void;
    filterIcon?: TdPrimaryTableProps["filterIcon"];
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, ("reset" | "confirm" | "inner-filter-change")[], "reset" | "confirm" | "inner-filter-change", import("vue").PublicProps, Readonly<{
    locale: TableConfig;
    tFilterValue: FilterValue;
    innerFilterValue: FilterValue;
    tableFilterClasses: {
        filterable: string;
        popup: string;
        icon: string;
        popupContent: string;
        result: string;
        inner: string;
        bottomButtons: string;
        contentInner: string;
        iconWrap: string;
    };
    isFocusClass: string;
    column: PrimaryTableCol;
    colIndex: number;
    primaryTableElement: any;
    popupProps: PopupProps;
    attach?: AttachNode;
    onVisibleChange: (val: boolean) => void;
    onInnerFilterChange: (val: any, column: PrimaryTableCol) => void;
    filterIcon?: TdPrimaryTableProps["filterIcon"];
}> & Readonly<{
    onReset?: (...args: any[]) => any;
    onConfirm?: (...args: any[]) => any;
    "onInner-filter-change"?: (...args: any[]) => any;
}>, {}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
