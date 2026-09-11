import { SetupContext } from 'vue';
import { BaseTableCellParams, TableRowData, RowspanColspan, TdPrimaryTableProps, TdBaseTableProps } from '../type';
import { RowAndColFixedPosition } from '../types';
import { SkipSpansValue } from '../hooks/useRowspanAndColspan';
import { PaginationProps } from '../../pagination';
import type { VirtualScrollConfig } from '@tdesign/shared-hooks';
import { AttachNode } from '../../common';
export interface RenderTdExtra {
    rowAndColFixedPosition: RowAndColFixedPosition;
    columnLength: number;
    dataLength: number;
    cellSpans: RowspanColspan;
    cellEmptyContent: TdBaseTableProps['cellEmptyContent'];
}
export interface RenderEllipsisCellParams {
    cellNode: any;
}
export type TrCommonProps = Pick<TdPrimaryTableProps, TrPropsKeys>;
export declare const TABLE_PROPS: readonly ["rowKey", "rowClassName", "columns", "fixedRows", "footData", "rowAttributes", "rowspanAndColspan", "scroll", "cellEmptyContent", "pagination", "attach", "onCellClick", "onRowClick", "onRowDblclick", "onRowMouseover", "onRowMousedown", "onRowMouseenter", "onRowMouseleave", "onRowMouseup"];
export type TrPropsKeys = typeof TABLE_PROPS[number];
export interface TrProps extends TrCommonProps {
    rowKey: string;
    row: TableRowData;
    rowIndex: number;
    ellipsisOverlayClassName: string;
    classPrefix: string;
    dataLength: number;
    rowAndColFixedPosition?: RowAndColFixedPosition;
    skipSpansMap?: Map<string, SkipSpansValue>;
    tableElm?: any;
    tableContentElm?: any;
    cellEmptyContent: TdBaseTableProps['cellEmptyContent'];
    virtualConfig: VirtualScrollConfig;
    attach?: AttachNode;
    active?: boolean;
    isHover?: boolean;
    reviveCells?: Map<number, {
        row: TableRowData;
        rowspan: number;
        colspan: number;
    }>;
}
export declare const ROW_LISTENERS: readonly ["click", "dblclick", "mouseover", "mousedown", "mouseenter", "mouseleave", "mouseup"];
export declare function renderCell(params: BaseTableCellParams<TableRowData>, slots: SetupContext['slots'], extra?: {
    cellEmptyContent?: TdBaseTableProps['cellEmptyContent'];
    pagination?: PaginationProps;
}): any;
declare const _default: import("vue").DefineComponent<{
    rowKey: string;
    row: TableRowData;
    rowIndex: number;
    ellipsisOverlayClassName: string;
    classPrefix: string;
    dataLength: number;
    rowAndColFixedPosition?: RowAndColFixedPosition;
    skipSpansMap?: Map<string, SkipSpansValue>;
    tableElm?: any;
    tableContentElm?: any;
    cellEmptyContent: TdBaseTableProps["cellEmptyContent"];
    virtualConfig: VirtualScrollConfig;
    attach?: AttachNode;
    active?: boolean;
    isHover?: boolean;
    reviveCells?: Map<number, {
        row: TableRowData;
        rowspan: number;
        colspan: number;
    }>;
    pagination?: PaginationProps;
    scroll?: import("../..").TScroll;
    columns?: import("..").PrimaryTableCol<TableRowData>[];
    onCellClick?: (context: import("..").PrimaryTableCellEventContext<TableRowData>) => void;
    fixedRows?: Array<number>;
    footData?: TableRowData[];
    rowAttributes?: import("..").TableRowAttributes<TableRowData>;
    rowClassName?: import("../..").ClassName | ((params: import("..").RowClassNameParams<TableRowData>) => import("../..").ClassName);
    rowspanAndColspan?: import("..").TableRowspanAndColspanFunc<TableRowData>;
    onRowClick?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowDblclick?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowMousedown?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowMouseenter?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowMouseleave?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowMouseover?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowMouseup?: (context: import("..").RowEventContext<TableRowData>) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, "row-mounted", import("vue").PublicProps, Readonly<{
    rowKey: string;
    row: TableRowData;
    rowIndex: number;
    ellipsisOverlayClassName: string;
    classPrefix: string;
    dataLength: number;
    rowAndColFixedPosition?: RowAndColFixedPosition;
    skipSpansMap?: Map<string, SkipSpansValue>;
    tableElm?: any;
    tableContentElm?: any;
    cellEmptyContent: TdBaseTableProps["cellEmptyContent"];
    virtualConfig: VirtualScrollConfig;
    attach?: AttachNode;
    active?: boolean;
    isHover?: boolean;
    reviveCells?: Map<number, {
        row: TableRowData;
        rowspan: number;
        colspan: number;
    }>;
    pagination?: PaginationProps;
    scroll?: import("../..").TScroll;
    columns?: import("..").PrimaryTableCol<TableRowData>[];
    onCellClick?: (context: import("..").PrimaryTableCellEventContext<TableRowData>) => void;
    fixedRows?: Array<number>;
    footData?: TableRowData[];
    rowAttributes?: import("..").TableRowAttributes<TableRowData>;
    rowClassName?: import("../..").ClassName | ((params: import("..").RowClassNameParams<TableRowData>) => import("../..").ClassName);
    rowspanAndColspan?: import("..").TableRowspanAndColspanFunc<TableRowData>;
    onRowClick?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowDblclick?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowMousedown?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowMouseenter?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowMouseleave?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowMouseover?: (context: import("..").RowEventContext<TableRowData>) => void;
    onRowMouseup?: (context: import("..").RowEventContext<TableRowData>) => void;
}> & Readonly<{}>, {
    active: boolean;
    columns: import("..").BaseTableCol<TableRowData>[];
    isHover: boolean;
    footData: TableRowData[];
    rowKey: string;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
