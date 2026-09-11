import { InfinityScroll } from '../common';
import { BaseTableInstanceFunctions, EnhancedTableInstanceFunctions, PrimaryTableInstanceFunctions } from './type';
import './style';
export * from './type';
export * from './types';
export type AllTableInstanceFunctions = EnhancedTableInstanceFunctions & PrimaryTableInstanceFunctions & BaseTableInstanceFunctions;
export type TableScroll = InfinityScroll;
export declare const BaseTable: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        renderExpandedRow?: (params: import("./type").TableExpandedRowParams<import("./type").TableRowData>) => import("..").TNodeReturnValue;
        onLeafColumnsChange?: (columns: import("./types").BaseTableColumns) => void;
        onShowElementChange?: (show: boolean) => void;
        thDraggable?: boolean;
        activeRowKeys?: Array<string | number>;
        defaultActiveRowKeys?: Array<string | number>;
        activeRowType?: "single" | "multiple";
        allowResizeColumnWidth?: boolean;
        attach?: import("..").AttachNode;
        bordered?: boolean;
        bottomContent?: string | import("..").TNode;
        cellEmptyContent?: string | ((h: typeof import("vue").h, props: import("./type").BaseTableCellParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        columns?: import("./type").BaseTableCol<import("./type").TableRowData>[];
        data?: import("./type").TableRowData[];
        disableDataPage?: boolean;
        disableSpaceInactiveRow?: boolean;
        empty?: string | import("..").TNode;
        firstFullRow?: string | import("..").TNode;
        fixedRows?: Array<number>;
        footData?: import("./type").TableRowData[];
        footerAffixProps?: Partial<import("..").AffixProps>;
        footerAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        footerSummary?: string | import("..").TNode;
        headerAffixProps?: Partial<import("..").AffixProps>;
        headerAffixedTop?: boolean | Partial<import("..").AffixProps>;
        height?: string | number;
        horizontalScrollAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        hover?: boolean;
        keyboardRowHover?: boolean;
        lastFullRow?: string | import("..").TNode;
        lazyLoad?: boolean;
        loading?: boolean | import("..").TNode;
        loadingProps?: Partial<import("..").LoadingProps>;
        locale?: import("..").TableConfig;
        maxHeight?: string | number;
        pagination?: import("..").PaginationProps;
        paginationAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        resizable?: boolean;
        rowAttributes?: import("./type").TableRowAttributes<import("./type").TableRowData>;
        rowClassName?: import("..").ClassName | ((params: import("./type").RowClassNameParams<import("./type").TableRowData>) => import("..").ClassName);
        rowKey: string;
        rowspanAndColspan?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
        rowspanAndColspanInFooter?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
        scroll?: import("..").TScroll;
        showHeader?: boolean;
        size?: import("..").SizeEnum;
        stripe?: boolean;
        tableContentWidth?: string;
        tableLayout?: "auto" | "fixed";
        topContent?: string | import("..").TNode;
        verticalAlign?: "top" | "middle" | "bottom";
        onActiveChange?: (activeRowKeys: Array<string | number>, context: import("./type").ActiveChangeContext<import("./type").TableRowData>) => void;
        onActiveRowAction?: (context: import("./type").ActiveRowActionContext<import("./type").TableRowData>) => void;
        onCellClick?: (context: import("./type").BaseTableCellEventContext<import("./type").TableRowData>) => void;
        onColumnResizeChange?: (context: {
            columnsWidth: {
                [colKey: string]: number;
            };
        }) => void;
        onPageChange?: (pageInfo: import("..").PageInfo, newDataSource: import("./type").TableRowData[]) => void;
        onRowClick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowDblclick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMousedown?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseenter?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseleave?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseover?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseup?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onScroll?: (params: {
            e: WheelEvent;
        }) => void;
        onScrollX?: (params: {
            e: WheelEvent;
        }) => void;
        onScrollY?: (params: {
            e: WheelEvent;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        data: import("./type").TableRowData[];
        hover: boolean;
        tableLayout: "fixed" | "auto";
        verticalAlign: "top" | "middle" | "bottom";
        columns: import("./type").BaseTableCol<import("./type").TableRowData>[];
        lazyLoad: boolean;
        bordered: boolean;
        stripe: boolean;
        activeRowKeys: (string | number)[];
        defaultActiveRowKeys: (string | number)[];
        activeRowType: "multiple" | "single";
        allowResizeColumnWidth: boolean;
        disableDataPage: boolean;
        disableSpaceInactiveRow: boolean;
        footData: import("./type").TableRowData[];
        footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
        headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
        keyboardRowHover: boolean;
        resizable: boolean;
        rowKey: string;
        showHeader: boolean;
        tableContentWidth: string;
        thDraggable: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        renderExpandedRow?: (params: import("./type").TableExpandedRowParams<import("./type").TableRowData>) => import("..").TNodeReturnValue;
        onLeafColumnsChange?: (columns: import("./types").BaseTableColumns) => void;
        onShowElementChange?: (show: boolean) => void;
        thDraggable?: boolean;
        activeRowKeys?: Array<string | number>;
        defaultActiveRowKeys?: Array<string | number>;
        activeRowType?: "single" | "multiple";
        allowResizeColumnWidth?: boolean;
        attach?: import("..").AttachNode;
        bordered?: boolean;
        bottomContent?: string | import("..").TNode;
        cellEmptyContent?: string | ((h: typeof import("vue").h, props: import("./type").BaseTableCellParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        columns?: import("./type").BaseTableCol<import("./type").TableRowData>[];
        data?: import("./type").TableRowData[];
        disableDataPage?: boolean;
        disableSpaceInactiveRow?: boolean;
        empty?: string | import("..").TNode;
        firstFullRow?: string | import("..").TNode;
        fixedRows?: Array<number>;
        footData?: import("./type").TableRowData[];
        footerAffixProps?: Partial<import("..").AffixProps>;
        footerAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        footerSummary?: string | import("..").TNode;
        headerAffixProps?: Partial<import("..").AffixProps>;
        headerAffixedTop?: boolean | Partial<import("..").AffixProps>;
        height?: string | number;
        horizontalScrollAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        hover?: boolean;
        keyboardRowHover?: boolean;
        lastFullRow?: string | import("..").TNode;
        lazyLoad?: boolean;
        loading?: boolean | import("..").TNode;
        loadingProps?: Partial<import("..").LoadingProps>;
        locale?: import("..").TableConfig;
        maxHeight?: string | number;
        pagination?: import("..").PaginationProps;
        paginationAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        resizable?: boolean;
        rowAttributes?: import("./type").TableRowAttributes<import("./type").TableRowData>;
        rowClassName?: import("..").ClassName | ((params: import("./type").RowClassNameParams<import("./type").TableRowData>) => import("..").ClassName);
        rowKey: string;
        rowspanAndColspan?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
        rowspanAndColspanInFooter?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
        scroll?: import("..").TScroll;
        showHeader?: boolean;
        size?: import("..").SizeEnum;
        stripe?: boolean;
        tableContentWidth?: string;
        tableLayout?: "auto" | "fixed";
        topContent?: string | import("..").TNode;
        verticalAlign?: "top" | "middle" | "bottom";
        onActiveChange?: (activeRowKeys: Array<string | number>, context: import("./type").ActiveChangeContext<import("./type").TableRowData>) => void;
        onActiveRowAction?: (context: import("./type").ActiveRowActionContext<import("./type").TableRowData>) => void;
        onCellClick?: (context: import("./type").BaseTableCellEventContext<import("./type").TableRowData>) => void;
        onColumnResizeChange?: (context: {
            columnsWidth: {
                [colKey: string]: number;
            };
        }) => void;
        onPageChange?: (pageInfo: import("..").PageInfo, newDataSource: import("./type").TableRowData[]) => void;
        onRowClick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowDblclick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMousedown?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseenter?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseleave?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseover?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseup?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onScroll?: (params: {
            e: WheelEvent;
        }) => void;
        onScrollX?: (params: {
            e: WheelEvent;
        }) => void;
        onScrollY?: (params: {
            e: WheelEvent;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        data: import("./type").TableRowData[];
        hover: boolean;
        tableLayout: "fixed" | "auto";
        verticalAlign: "top" | "middle" | "bottom";
        columns: import("./type").BaseTableCol<import("./type").TableRowData>[];
        lazyLoad: boolean;
        bordered: boolean;
        stripe: boolean;
        activeRowKeys: (string | number)[];
        defaultActiveRowKeys: (string | number)[];
        activeRowType: "multiple" | "single";
        allowResizeColumnWidth: boolean;
        disableDataPage: boolean;
        disableSpaceInactiveRow: boolean;
        footData: import("./type").TableRowData[];
        footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
        headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
        keyboardRowHover: boolean;
        resizable: boolean;
        rowKey: string;
        showHeader: boolean;
        tableContentWidth: string;
        thDraggable: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    renderExpandedRow?: (params: import("./type").TableExpandedRowParams<import("./type").TableRowData>) => import("..").TNodeReturnValue;
    onLeafColumnsChange?: (columns: import("./types").BaseTableColumns) => void;
    onShowElementChange?: (show: boolean) => void;
    thDraggable?: boolean;
    activeRowKeys?: Array<string | number>;
    defaultActiveRowKeys?: Array<string | number>;
    activeRowType?: "single" | "multiple";
    allowResizeColumnWidth?: boolean;
    attach?: import("..").AttachNode;
    bordered?: boolean;
    bottomContent?: string | import("..").TNode;
    cellEmptyContent?: string | ((h: typeof import("vue").h, props: import("./type").BaseTableCellParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
    columns?: import("./type").BaseTableCol<import("./type").TableRowData>[];
    data?: import("./type").TableRowData[];
    disableDataPage?: boolean;
    disableSpaceInactiveRow?: boolean;
    empty?: string | import("..").TNode;
    firstFullRow?: string | import("..").TNode;
    fixedRows?: Array<number>;
    footData?: import("./type").TableRowData[];
    footerAffixProps?: Partial<import("..").AffixProps>;
    footerAffixedBottom?: boolean | Partial<import("..").AffixProps>;
    footerSummary?: string | import("..").TNode;
    headerAffixProps?: Partial<import("..").AffixProps>;
    headerAffixedTop?: boolean | Partial<import("..").AffixProps>;
    height?: string | number;
    horizontalScrollAffixedBottom?: boolean | Partial<import("..").AffixProps>;
    hover?: boolean;
    keyboardRowHover?: boolean;
    lastFullRow?: string | import("..").TNode;
    lazyLoad?: boolean;
    loading?: boolean | import("..").TNode;
    loadingProps?: Partial<import("..").LoadingProps>;
    locale?: import("..").TableConfig;
    maxHeight?: string | number;
    pagination?: import("..").PaginationProps;
    paginationAffixedBottom?: boolean | Partial<import("..").AffixProps>;
    resizable?: boolean;
    rowAttributes?: import("./type").TableRowAttributes<import("./type").TableRowData>;
    rowClassName?: import("..").ClassName | ((params: import("./type").RowClassNameParams<import("./type").TableRowData>) => import("..").ClassName);
    rowKey: string;
    rowspanAndColspan?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
    rowspanAndColspanInFooter?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
    scroll?: import("..").TScroll;
    showHeader?: boolean;
    size?: import("..").SizeEnum;
    stripe?: boolean;
    tableContentWidth?: string;
    tableLayout?: "auto" | "fixed";
    topContent?: string | import("..").TNode;
    verticalAlign?: "top" | "middle" | "bottom";
    onActiveChange?: (activeRowKeys: Array<string | number>, context: import("./type").ActiveChangeContext<import("./type").TableRowData>) => void;
    onActiveRowAction?: (context: import("./type").ActiveRowActionContext<import("./type").TableRowData>) => void;
    onCellClick?: (context: import("./type").BaseTableCellEventContext<import("./type").TableRowData>) => void;
    onColumnResizeChange?: (context: {
        columnsWidth: {
            [colKey: string]: number;
        };
    }) => void;
    onPageChange?: (pageInfo: import("..").PageInfo, newDataSource: import("./type").TableRowData[]) => void;
    onRowClick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowDblclick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowMousedown?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowMouseenter?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowMouseleave?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowMouseover?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowMouseup?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onScroll?: (params: {
        e: WheelEvent;
    }) => void;
    onScrollX?: (params: {
        e: WheelEvent;
    }) => void;
    onScrollY?: (params: {
        e: WheelEvent;
    }) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, "show-element-change" | "update:activeRowKeys", {
    empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    data: import("./type").TableRowData[];
    hover: boolean;
    tableLayout: "fixed" | "auto";
    verticalAlign: "top" | "middle" | "bottom";
    columns: import("./type").BaseTableCol<import("./type").TableRowData>[];
    lazyLoad: boolean;
    bordered: boolean;
    stripe: boolean;
    activeRowKeys: (string | number)[];
    defaultActiveRowKeys: (string | number)[];
    activeRowType: "multiple" | "single";
    allowResizeColumnWidth: boolean;
    disableDataPage: boolean;
    disableSpaceInactiveRow: boolean;
    footData: import("./type").TableRowData[];
    footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
    headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
    keyboardRowHover: boolean;
    resizable: boolean;
    rowKey: string;
    showHeader: boolean;
    tableContentWidth: string;
    thDraggable: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const PrimaryTable: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        asyncLoading: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["asyncLoading"]>;
        };
        columnController: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["columnController"]>;
        };
        columnControllerVisible: {
            type: BooleanConstructor;
            default: any;
        };
        defaultColumnControllerVisible: {
            type: BooleanConstructor;
            default: any;
        };
        columns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["columns"]>;
            default: () => import("./type").TdPrimaryTableProps["columns"];
        };
        displayColumns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["displayColumns"]>;
            default: import("./type").TdPrimaryTableProps["displayColumns"];
        };
        defaultDisplayColumns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultDisplayColumns"]>;
        };
        dragSort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSort"]>;
            validator(val: import("./type").TdPrimaryTableProps["dragSort"]): boolean;
        };
        dragSortOptions: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSortOptions"]>;
        };
        editableCellState: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableCellState"]>;
        };
        editableRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableRowKeys"]>;
        };
        expandIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandIcon"]>;
            default: import("./type").TdPrimaryTableProps["expandIcon"];
        };
        expandOnRowClick: BooleanConstructor;
        expandedRow: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRow"]>;
        };
        expandedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRowKeys"]>;
            default: import("./type").TdPrimaryTableProps["expandedRowKeys"];
        };
        defaultExpandedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"]>;
            default: () => import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"];
        };
        filterIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterIcon"]>;
        };
        filterRow: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterRow"]>;
        };
        filterValue: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterValue"]>;
            default: import("./type").TdPrimaryTableProps["filterValue"];
        };
        defaultFilterValue: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultFilterValue"]>;
        };
        hideSortTips: BooleanConstructor;
        indeterminateSelectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["indeterminateSelectedRowKeys"]>;
        };
        multipleSort: BooleanConstructor;
        reserveSelectedRowOnPaginate: {
            type: BooleanConstructor;
            default: boolean;
        };
        rowSelectionAllowUncheck: BooleanConstructor;
        rowSelectionType: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["rowSelectionType"]>;
            validator(val: import("./type").TdPrimaryTableProps["rowSelectionType"]): boolean;
        };
        selectOnRowClick: BooleanConstructor;
        selectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["selectedRowKeys"]>;
            default: import("./type").TdPrimaryTableProps["selectedRowKeys"];
        };
        defaultSelectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"]>;
            default: () => import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"];
        };
        showSortColumnBgColor: BooleanConstructor;
        sort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["sort"]>;
            default: import("./type").TdPrimaryTableProps["sort"];
        };
        defaultSort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSort"]>;
        };
        sortIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["sortIcon"]>;
        };
        sortOnRowDraggable: BooleanConstructor;
        onAsyncLoadingClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onAsyncLoadingClick"]>;
        onCellClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onCellClick"]>;
        onChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onChange"]>;
        onColumnChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnChange"]>;
        onColumnControllerVisibleChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnControllerVisibleChange"]>;
        onDataChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDataChange"]>;
        onDisplayColumnsChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDisplayColumnsChange"]>;
        onDragSort: import("vue").PropType<import("./type").TdPrimaryTableProps["onDragSort"]>;
        onExpandChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onExpandChange"]>;
        onFilterChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onFilterChange"]>;
        onRowEdit: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowEdit"]>;
        onRowValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowValidate"]>;
        onSelectChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSelectChange"]>;
        onSortChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSortChange"]>;
        onValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onValidate"]>;
        activeRowKeys: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowKeys"]>;
            default: import("./type").TdBaseTableProps["activeRowKeys"];
        };
        defaultActiveRowKeys: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["defaultActiveRowKeys"]>;
            default: () => import("./type").TdBaseTableProps["defaultActiveRowKeys"];
        };
        activeRowType: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowType"]>;
            default: import("./type").TdBaseTableProps["activeRowType"];
        };
        allowResizeColumnWidth: {
            type: BooleanConstructor;
            default: any;
        };
        attach: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["attach"]>;
        };
        bordered: BooleanConstructor;
        bottomContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["bottomContent"]>;
        };
        cellEmptyContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["cellEmptyContent"]>;
        };
        data: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["data"]>;
            default: () => import("./type").TdBaseTableProps["data"];
        };
        disableDataPage: BooleanConstructor;
        disableSpaceInactiveRow: {
            type: BooleanConstructor;
            default: any;
        };
        empty: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["empty"]>;
            default: import("./type").TdBaseTableProps["empty"];
        };
        firstFullRow: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["firstFullRow"]>;
        };
        fixedRows: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["fixedRows"]>;
        };
        footData: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footData"]>;
            default: () => import("./type").TdBaseTableProps["footData"];
        };
        footerAffixProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixProps"]>;
        };
        footerAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixedBottom"]>;
            default: import("./type").TdBaseTableProps["footerAffixedBottom"];
        };
        footerSummary: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerSummary"]>;
        };
        headerAffixProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixProps"]>;
        };
        headerAffixedTop: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixedTop"]>;
            default: import("./type").TdBaseTableProps["headerAffixedTop"];
        };
        height: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["height"]>;
        };
        horizontalScrollAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["horizontalScrollAffixedBottom"]>;
        };
        hover: BooleanConstructor;
        keyboardRowHover: {
            type: BooleanConstructor;
            default: boolean;
        };
        lastFullRow: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["lastFullRow"]>;
        };
        lazyLoad: BooleanConstructor;
        loading: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["loading"]>;
            default: import("./type").TdBaseTableProps["loading"];
        };
        loadingProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["loadingProps"]>;
        };
        locale: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["locale"]>;
        };
        maxHeight: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["maxHeight"]>;
        };
        pagination: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["pagination"]>;
        };
        paginationAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["paginationAffixedBottom"]>;
        };
        resizable: BooleanConstructor;
        rowAttributes: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowAttributes"]>;
        };
        rowClassName: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowClassName"]>;
        };
        rowKey: {
            type: StringConstructor;
            default: string;
            required: boolean;
        };
        rowspanAndColspan: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspan"]>;
        };
        rowspanAndColspanInFooter: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspanInFooter"]>;
        };
        scroll: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["scroll"]>;
        };
        showHeader: {
            type: BooleanConstructor;
            default: boolean;
        };
        size: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["size"]>;
            validator(val: import("./type").TdBaseTableProps["size"]): boolean;
        };
        stripe: BooleanConstructor;
        tableContentWidth: {
            type: StringConstructor;
            default: string;
        };
        tableLayout: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["tableLayout"]>;
            default: import("./type").TdBaseTableProps["tableLayout"];
            validator(val: import("./type").TdBaseTableProps["tableLayout"]): boolean;
        };
        topContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["topContent"]>;
        };
        verticalAlign: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["verticalAlign"]>;
            default: import("./type").TdBaseTableProps["verticalAlign"];
            validator(val: import("./type").TdBaseTableProps["verticalAlign"]): boolean;
        };
        onActiveChange: import("vue").PropType<import("./type").TdBaseTableProps["onActiveChange"]>;
        onActiveRowAction: import("vue").PropType<import("./type").TdBaseTableProps["onActiveRowAction"]>;
        onColumnResizeChange: import("vue").PropType<import("./type").TdBaseTableProps["onColumnResizeChange"]>;
        onPageChange: import("vue").PropType<import("./type").TdBaseTableProps["onPageChange"]>;
        onRowClick: import("vue").PropType<import("./type").TdBaseTableProps["onRowClick"]>;
        onRowDblclick: import("vue").PropType<import("./type").TdBaseTableProps["onRowDblclick"]>;
        onRowMousedown: import("vue").PropType<import("./type").TdBaseTableProps["onRowMousedown"]>;
        onRowMouseenter: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseenter"]>;
        onRowMouseleave: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseleave"]>;
        onRowMouseover: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseover"]>;
        onRowMouseup: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseup"]>;
        onScroll: import("vue").PropType<import("./type").TdBaseTableProps["onScroll"]>;
        onScrollX: import("vue").PropType<import("./type").TdBaseTableProps["onScrollX"]>;
        onScrollY: import("vue").PropType<import("./type").TdBaseTableProps["onScrollY"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        sort: import("./type").TableSort;
        data: import("./type").TableRowData[];
        hover: boolean;
        tableLayout: "fixed" | "auto";
        verticalAlign: "top" | "middle" | "bottom";
        columns: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
        lazyLoad: boolean;
        bordered: boolean;
        stripe: boolean;
        activeRowKeys: (string | number)[];
        defaultActiveRowKeys: (string | number)[];
        activeRowType: "multiple" | "single";
        allowResizeColumnWidth: boolean;
        disableDataPage: boolean;
        disableSpaceInactiveRow: boolean;
        footData: import("./type").TableRowData[];
        footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
        headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
        keyboardRowHover: boolean;
        resizable: boolean;
        rowKey: string;
        showHeader: boolean;
        tableContentWidth: string;
        columnControllerVisible: boolean;
        defaultColumnControllerVisible: boolean;
        displayColumns: import("..").CheckboxGroupValue;
        expandIcon: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        expandOnRowClick: boolean;
        expandedRowKeys: (string | number)[];
        defaultExpandedRowKeys: (string | number)[];
        filterValue: import("./type").FilterValue;
        hideSortTips: boolean;
        multipleSort: boolean;
        reserveSelectedRowOnPaginate: boolean;
        rowSelectionAllowUncheck: boolean;
        selectOnRowClick: boolean;
        selectedRowKeys: (string | number)[];
        defaultSelectedRowKeys: (string | number)[];
        showSortColumnBgColor: boolean;
        sortOnRowDraggable: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        asyncLoading: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["asyncLoading"]>;
        };
        columnController: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["columnController"]>;
        };
        columnControllerVisible: {
            type: BooleanConstructor;
            default: any;
        };
        defaultColumnControllerVisible: {
            type: BooleanConstructor;
            default: any;
        };
        columns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["columns"]>;
            default: () => import("./type").TdPrimaryTableProps["columns"];
        };
        displayColumns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["displayColumns"]>;
            default: import("./type").TdPrimaryTableProps["displayColumns"];
        };
        defaultDisplayColumns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultDisplayColumns"]>;
        };
        dragSort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSort"]>;
            validator(val: import("./type").TdPrimaryTableProps["dragSort"]): boolean;
        };
        dragSortOptions: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSortOptions"]>;
        };
        editableCellState: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableCellState"]>;
        };
        editableRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableRowKeys"]>;
        };
        expandIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandIcon"]>;
            default: import("./type").TdPrimaryTableProps["expandIcon"];
        };
        expandOnRowClick: BooleanConstructor;
        expandedRow: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRow"]>;
        };
        expandedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRowKeys"]>;
            default: import("./type").TdPrimaryTableProps["expandedRowKeys"];
        };
        defaultExpandedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"]>;
            default: () => import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"];
        };
        filterIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterIcon"]>;
        };
        filterRow: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterRow"]>;
        };
        filterValue: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterValue"]>;
            default: import("./type").TdPrimaryTableProps["filterValue"];
        };
        defaultFilterValue: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultFilterValue"]>;
        };
        hideSortTips: BooleanConstructor;
        indeterminateSelectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["indeterminateSelectedRowKeys"]>;
        };
        multipleSort: BooleanConstructor;
        reserveSelectedRowOnPaginate: {
            type: BooleanConstructor;
            default: boolean;
        };
        rowSelectionAllowUncheck: BooleanConstructor;
        rowSelectionType: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["rowSelectionType"]>;
            validator(val: import("./type").TdPrimaryTableProps["rowSelectionType"]): boolean;
        };
        selectOnRowClick: BooleanConstructor;
        selectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["selectedRowKeys"]>;
            default: import("./type").TdPrimaryTableProps["selectedRowKeys"];
        };
        defaultSelectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"]>;
            default: () => import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"];
        };
        showSortColumnBgColor: BooleanConstructor;
        sort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["sort"]>;
            default: import("./type").TdPrimaryTableProps["sort"];
        };
        defaultSort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSort"]>;
        };
        sortIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["sortIcon"]>;
        };
        sortOnRowDraggable: BooleanConstructor;
        onAsyncLoadingClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onAsyncLoadingClick"]>;
        onCellClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onCellClick"]>;
        onChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onChange"]>;
        onColumnChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnChange"]>;
        onColumnControllerVisibleChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnControllerVisibleChange"]>;
        onDataChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDataChange"]>;
        onDisplayColumnsChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDisplayColumnsChange"]>;
        onDragSort: import("vue").PropType<import("./type").TdPrimaryTableProps["onDragSort"]>;
        onExpandChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onExpandChange"]>;
        onFilterChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onFilterChange"]>;
        onRowEdit: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowEdit"]>;
        onRowValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowValidate"]>;
        onSelectChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSelectChange"]>;
        onSortChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSortChange"]>;
        onValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onValidate"]>;
        activeRowKeys: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowKeys"]>;
            default: import("./type").TdBaseTableProps["activeRowKeys"];
        };
        defaultActiveRowKeys: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["defaultActiveRowKeys"]>;
            default: () => import("./type").TdBaseTableProps["defaultActiveRowKeys"];
        };
        activeRowType: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowType"]>;
            default: import("./type").TdBaseTableProps["activeRowType"];
        };
        allowResizeColumnWidth: {
            type: BooleanConstructor;
            default: any;
        };
        attach: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["attach"]>;
        };
        bordered: BooleanConstructor;
        bottomContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["bottomContent"]>;
        };
        cellEmptyContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["cellEmptyContent"]>;
        };
        data: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["data"]>;
            default: () => import("./type").TdBaseTableProps["data"];
        };
        disableDataPage: BooleanConstructor;
        disableSpaceInactiveRow: {
            type: BooleanConstructor;
            default: any;
        };
        empty: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["empty"]>;
            default: import("./type").TdBaseTableProps["empty"];
        };
        firstFullRow: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["firstFullRow"]>;
        };
        fixedRows: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["fixedRows"]>;
        };
        footData: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footData"]>;
            default: () => import("./type").TdBaseTableProps["footData"];
        };
        footerAffixProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixProps"]>;
        };
        footerAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixedBottom"]>;
            default: import("./type").TdBaseTableProps["footerAffixedBottom"];
        };
        footerSummary: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerSummary"]>;
        };
        headerAffixProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixProps"]>;
        };
        headerAffixedTop: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixedTop"]>;
            default: import("./type").TdBaseTableProps["headerAffixedTop"];
        };
        height: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["height"]>;
        };
        horizontalScrollAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["horizontalScrollAffixedBottom"]>;
        };
        hover: BooleanConstructor;
        keyboardRowHover: {
            type: BooleanConstructor;
            default: boolean;
        };
        lastFullRow: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["lastFullRow"]>;
        };
        lazyLoad: BooleanConstructor;
        loading: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["loading"]>;
            default: import("./type").TdBaseTableProps["loading"];
        };
        loadingProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["loadingProps"]>;
        };
        locale: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["locale"]>;
        };
        maxHeight: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["maxHeight"]>;
        };
        pagination: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["pagination"]>;
        };
        paginationAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["paginationAffixedBottom"]>;
        };
        resizable: BooleanConstructor;
        rowAttributes: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowAttributes"]>;
        };
        rowClassName: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowClassName"]>;
        };
        rowKey: {
            type: StringConstructor;
            default: string;
            required: boolean;
        };
        rowspanAndColspan: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspan"]>;
        };
        rowspanAndColspanInFooter: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspanInFooter"]>;
        };
        scroll: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["scroll"]>;
        };
        showHeader: {
            type: BooleanConstructor;
            default: boolean;
        };
        size: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["size"]>;
            validator(val: import("./type").TdBaseTableProps["size"]): boolean;
        };
        stripe: BooleanConstructor;
        tableContentWidth: {
            type: StringConstructor;
            default: string;
        };
        tableLayout: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["tableLayout"]>;
            default: import("./type").TdBaseTableProps["tableLayout"];
            validator(val: import("./type").TdBaseTableProps["tableLayout"]): boolean;
        };
        topContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["topContent"]>;
        };
        verticalAlign: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["verticalAlign"]>;
            default: import("./type").TdBaseTableProps["verticalAlign"];
            validator(val: import("./type").TdBaseTableProps["verticalAlign"]): boolean;
        };
        onActiveChange: import("vue").PropType<import("./type").TdBaseTableProps["onActiveChange"]>;
        onActiveRowAction: import("vue").PropType<import("./type").TdBaseTableProps["onActiveRowAction"]>;
        onColumnResizeChange: import("vue").PropType<import("./type").TdBaseTableProps["onColumnResizeChange"]>;
        onPageChange: import("vue").PropType<import("./type").TdBaseTableProps["onPageChange"]>;
        onRowClick: import("vue").PropType<import("./type").TdBaseTableProps["onRowClick"]>;
        onRowDblclick: import("vue").PropType<import("./type").TdBaseTableProps["onRowDblclick"]>;
        onRowMousedown: import("vue").PropType<import("./type").TdBaseTableProps["onRowMousedown"]>;
        onRowMouseenter: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseenter"]>;
        onRowMouseleave: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseleave"]>;
        onRowMouseover: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseover"]>;
        onRowMouseup: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseup"]>;
        onScroll: import("vue").PropType<import("./type").TdBaseTableProps["onScroll"]>;
        onScrollX: import("vue").PropType<import("./type").TdBaseTableProps["onScrollX"]>;
        onScrollY: import("vue").PropType<import("./type").TdBaseTableProps["onScrollY"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        sort: import("./type").TableSort;
        data: import("./type").TableRowData[];
        hover: boolean;
        tableLayout: "fixed" | "auto";
        verticalAlign: "top" | "middle" | "bottom";
        columns: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
        lazyLoad: boolean;
        bordered: boolean;
        stripe: boolean;
        activeRowKeys: (string | number)[];
        defaultActiveRowKeys: (string | number)[];
        activeRowType: "multiple" | "single";
        allowResizeColumnWidth: boolean;
        disableDataPage: boolean;
        disableSpaceInactiveRow: boolean;
        footData: import("./type").TableRowData[];
        footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
        headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
        keyboardRowHover: boolean;
        resizable: boolean;
        rowKey: string;
        showHeader: boolean;
        tableContentWidth: string;
        columnControllerVisible: boolean;
        defaultColumnControllerVisible: boolean;
        displayColumns: import("..").CheckboxGroupValue;
        expandIcon: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        expandOnRowClick: boolean;
        expandedRowKeys: (string | number)[];
        defaultExpandedRowKeys: (string | number)[];
        filterValue: import("./type").FilterValue;
        hideSortTips: boolean;
        multipleSort: boolean;
        reserveSelectedRowOnPaginate: boolean;
        rowSelectionAllowUncheck: boolean;
        selectOnRowClick: boolean;
        selectedRowKeys: (string | number)[];
        defaultSelectedRowKeys: (string | number)[];
        showSortColumnBgColor: boolean;
        sortOnRowDraggable: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    asyncLoading: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["asyncLoading"]>;
    };
    columnController: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["columnController"]>;
    };
    columnControllerVisible: {
        type: BooleanConstructor;
        default: any;
    };
    defaultColumnControllerVisible: {
        type: BooleanConstructor;
        default: any;
    };
    columns: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["columns"]>;
        default: () => import("./type").TdPrimaryTableProps["columns"];
    };
    displayColumns: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["displayColumns"]>;
        default: import("./type").TdPrimaryTableProps["displayColumns"];
    };
    defaultDisplayColumns: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultDisplayColumns"]>;
    };
    dragSort: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSort"]>;
        validator(val: import("./type").TdPrimaryTableProps["dragSort"]): boolean;
    };
    dragSortOptions: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSortOptions"]>;
    };
    editableCellState: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableCellState"]>;
    };
    editableRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableRowKeys"]>;
    };
    expandIcon: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandIcon"]>;
        default: import("./type").TdPrimaryTableProps["expandIcon"];
    };
    expandOnRowClick: BooleanConstructor;
    expandedRow: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRow"]>;
    };
    expandedRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRowKeys"]>;
        default: import("./type").TdPrimaryTableProps["expandedRowKeys"];
    };
    defaultExpandedRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"]>;
        default: () => import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"];
    };
    filterIcon: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterIcon"]>;
    };
    filterRow: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterRow"]>;
    };
    filterValue: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterValue"]>;
        default: import("./type").TdPrimaryTableProps["filterValue"];
    };
    defaultFilterValue: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultFilterValue"]>;
    };
    hideSortTips: BooleanConstructor;
    indeterminateSelectedRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["indeterminateSelectedRowKeys"]>;
    };
    multipleSort: BooleanConstructor;
    reserveSelectedRowOnPaginate: {
        type: BooleanConstructor;
        default: boolean;
    };
    rowSelectionAllowUncheck: BooleanConstructor;
    rowSelectionType: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["rowSelectionType"]>;
        validator(val: import("./type").TdPrimaryTableProps["rowSelectionType"]): boolean;
    };
    selectOnRowClick: BooleanConstructor;
    selectedRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["selectedRowKeys"]>;
        default: import("./type").TdPrimaryTableProps["selectedRowKeys"];
    };
    defaultSelectedRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"]>;
        default: () => import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"];
    };
    showSortColumnBgColor: BooleanConstructor;
    sort: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["sort"]>;
        default: import("./type").TdPrimaryTableProps["sort"];
    };
    defaultSort: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSort"]>;
    };
    sortIcon: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["sortIcon"]>;
    };
    sortOnRowDraggable: BooleanConstructor;
    onAsyncLoadingClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onAsyncLoadingClick"]>;
    onCellClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onCellClick"]>;
    onChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onChange"]>;
    onColumnChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnChange"]>;
    onColumnControllerVisibleChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnControllerVisibleChange"]>;
    onDataChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDataChange"]>;
    onDisplayColumnsChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDisplayColumnsChange"]>;
    onDragSort: import("vue").PropType<import("./type").TdPrimaryTableProps["onDragSort"]>;
    onExpandChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onExpandChange"]>;
    onFilterChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onFilterChange"]>;
    onRowEdit: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowEdit"]>;
    onRowValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowValidate"]>;
    onSelectChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSelectChange"]>;
    onSortChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSortChange"]>;
    onValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onValidate"]>;
    activeRowKeys: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowKeys"]>;
        default: import("./type").TdBaseTableProps["activeRowKeys"];
    };
    defaultActiveRowKeys: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["defaultActiveRowKeys"]>;
        default: () => import("./type").TdBaseTableProps["defaultActiveRowKeys"];
    };
    activeRowType: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowType"]>;
        default: import("./type").TdBaseTableProps["activeRowType"];
    };
    allowResizeColumnWidth: {
        type: BooleanConstructor;
        default: any;
    };
    attach: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["attach"]>;
    };
    bordered: BooleanConstructor;
    bottomContent: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["bottomContent"]>;
    };
    cellEmptyContent: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["cellEmptyContent"]>;
    };
    data: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["data"]>;
        default: () => import("./type").TdBaseTableProps["data"];
    };
    disableDataPage: BooleanConstructor;
    disableSpaceInactiveRow: {
        type: BooleanConstructor;
        default: any;
    };
    empty: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["empty"]>;
        default: import("./type").TdBaseTableProps["empty"];
    };
    firstFullRow: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["firstFullRow"]>;
    };
    fixedRows: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["fixedRows"]>;
    };
    footData: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["footData"]>;
        default: () => import("./type").TdBaseTableProps["footData"];
    };
    footerAffixProps: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixProps"]>;
    };
    footerAffixedBottom: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixedBottom"]>;
        default: import("./type").TdBaseTableProps["footerAffixedBottom"];
    };
    footerSummary: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["footerSummary"]>;
    };
    headerAffixProps: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixProps"]>;
    };
    headerAffixedTop: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixedTop"]>;
        default: import("./type").TdBaseTableProps["headerAffixedTop"];
    };
    height: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["height"]>;
    };
    horizontalScrollAffixedBottom: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["horizontalScrollAffixedBottom"]>;
    };
    hover: BooleanConstructor;
    keyboardRowHover: {
        type: BooleanConstructor;
        default: boolean;
    };
    lastFullRow: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["lastFullRow"]>;
    };
    lazyLoad: BooleanConstructor;
    loading: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["loading"]>;
        default: import("./type").TdBaseTableProps["loading"];
    };
    loadingProps: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["loadingProps"]>;
    };
    locale: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["locale"]>;
    };
    maxHeight: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["maxHeight"]>;
    };
    pagination: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["pagination"]>;
    };
    paginationAffixedBottom: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["paginationAffixedBottom"]>;
    };
    resizable: BooleanConstructor;
    rowAttributes: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["rowAttributes"]>;
    };
    rowClassName: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["rowClassName"]>;
    };
    rowKey: {
        type: StringConstructor;
        default: string;
        required: boolean;
    };
    rowspanAndColspan: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspan"]>;
    };
    rowspanAndColspanInFooter: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspanInFooter"]>;
    };
    scroll: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["scroll"]>;
    };
    showHeader: {
        type: BooleanConstructor;
        default: boolean;
    };
    size: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["size"]>;
        validator(val: import("./type").TdBaseTableProps["size"]): boolean;
    };
    stripe: BooleanConstructor;
    tableContentWidth: {
        type: StringConstructor;
        default: string;
    };
    tableLayout: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["tableLayout"]>;
        default: import("./type").TdBaseTableProps["tableLayout"];
        validator(val: import("./type").TdBaseTableProps["tableLayout"]): boolean;
    };
    topContent: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["topContent"]>;
    };
    verticalAlign: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["verticalAlign"]>;
        default: import("./type").TdBaseTableProps["verticalAlign"];
        validator(val: import("./type").TdBaseTableProps["verticalAlign"]): boolean;
    };
    onActiveChange: import("vue").PropType<import("./type").TdBaseTableProps["onActiveChange"]>;
    onActiveRowAction: import("vue").PropType<import("./type").TdBaseTableProps["onActiveRowAction"]>;
    onColumnResizeChange: import("vue").PropType<import("./type").TdBaseTableProps["onColumnResizeChange"]>;
    onPageChange: import("vue").PropType<import("./type").TdBaseTableProps["onPageChange"]>;
    onRowClick: import("vue").PropType<import("./type").TdBaseTableProps["onRowClick"]>;
    onRowDblclick: import("vue").PropType<import("./type").TdBaseTableProps["onRowDblclick"]>;
    onRowMousedown: import("vue").PropType<import("./type").TdBaseTableProps["onRowMousedown"]>;
    onRowMouseenter: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseenter"]>;
    onRowMouseleave: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseleave"]>;
    onRowMouseover: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseover"]>;
    onRowMouseup: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseup"]>;
    onScroll: import("vue").PropType<import("./type").TdBaseTableProps["onScroll"]>;
    onScrollX: import("vue").PropType<import("./type").TdBaseTableProps["onScrollX"]>;
    onScrollY: import("vue").PropType<import("./type").TdBaseTableProps["onScrollY"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    sort: import("./type").TableSort;
    data: import("./type").TableRowData[];
    hover: boolean;
    tableLayout: "fixed" | "auto";
    verticalAlign: "top" | "middle" | "bottom";
    columns: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
    lazyLoad: boolean;
    bordered: boolean;
    stripe: boolean;
    activeRowKeys: (string | number)[];
    defaultActiveRowKeys: (string | number)[];
    activeRowType: "multiple" | "single";
    allowResizeColumnWidth: boolean;
    disableDataPage: boolean;
    disableSpaceInactiveRow: boolean;
    footData: import("./type").TableRowData[];
    footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
    headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
    keyboardRowHover: boolean;
    resizable: boolean;
    rowKey: string;
    showHeader: boolean;
    tableContentWidth: string;
    columnControllerVisible: boolean;
    defaultColumnControllerVisible: boolean;
    displayColumns: import("..").CheckboxGroupValue;
    expandIcon: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
    expandOnRowClick: boolean;
    expandedRowKeys: (string | number)[];
    defaultExpandedRowKeys: (string | number)[];
    filterValue: import("./type").FilterValue;
    hideSortTips: boolean;
    multipleSort: boolean;
    reserveSelectedRowOnPaginate: boolean;
    rowSelectionAllowUncheck: boolean;
    selectOnRowClick: boolean;
    selectedRowKeys: (string | number)[];
    defaultSelectedRowKeys: (string | number)[];
    showSortColumnBgColor: boolean;
    sortOnRowDraggable: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const EnhancedTable: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        beforeDragSort?: (context: import("./type").DragSortContext<import("./type").TableRowData>) => boolean;
        expandedTreeNodes?: Array<string | number>;
        defaultExpandedTreeNodes?: Array<string | number>;
        tree?: import("./type").TableTreeConfig;
        treeExpandAndFoldIcon?: (h: typeof import("vue").h, props: {
            type: "expand" | "fold";
            row: import("./type").TableRowData;
        }) => import("..").TNodeReturnValue;
        onAbnormalDragSort?: (context: import("./type").TableAbnormalDragSortContext<import("./type").TableRowData>) => void;
        onExpandedTreeNodesChange?: (expandedTreeNodes: Array<string | number>, options: import("./type").TableTreeNodeExpandOptions<import("./type").TableRowData>) => void;
        onTreeExpandChange?: (context: import("./type").TableTreeExpandChangeContext<import("./type").TableRowData>) => void;
        asyncLoading?: "loading" | "load-more" | import("..").TNode;
        columnController?: import("./type").TableColumnController;
        columnControllerVisible?: boolean;
        defaultColumnControllerVisible?: boolean;
        columns?: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
        displayColumns?: import("..").CheckboxGroupValue;
        defaultDisplayColumns?: import("..").CheckboxGroupValue;
        dragSort?: "row" | "row-handler" | "col" | "row-handler-col" | "drag-col";
        dragSortOptions?: import("sortablejs").SortableOptions;
        editableCellState?: import("./type").EditableCellType<import("./type").TableRowData>;
        editableRowKeys?: Array<string | number>;
        expandIcon?: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        expandOnRowClick?: boolean;
        expandedRow?: (h: typeof import("vue").h, props: import("./type").TableExpandedRowParams<import("./type").TableRowData>) => import("..").TNodeReturnValue;
        expandedRowKeys?: Array<string | number>;
        defaultExpandedRowKeys?: Array<string | number>;
        filterIcon?: (h: typeof import("vue").h, props: {
            col: import("./type").PrimaryTableCol<import("./type").TableRowData>;
            colIndex: number;
        }) => import("..").TNodeReturnValue;
        filterRow?: string | import("..").TNode;
        filterValue?: import("./type").FilterValue;
        defaultFilterValue?: import("./type").FilterValue;
        hideSortTips?: boolean;
        indeterminateSelectedRowKeys?: Array<string | number>;
        multipleSort?: boolean;
        reserveSelectedRowOnPaginate?: boolean;
        rowSelectionAllowUncheck?: boolean;
        rowSelectionType?: "single" | "multiple";
        selectOnRowClick?: boolean;
        selectedRowKeys?: Array<string | number>;
        defaultSelectedRowKeys?: Array<string | number>;
        showSortColumnBgColor?: boolean;
        sort?: import("./type").TableSort;
        defaultSort?: import("./type").TableSort;
        sortIcon?: import("..").TNode;
        sortOnRowDraggable?: boolean;
        onAsyncLoadingClick?: (context: {
            status: "loading" | "load-more";
        }) => void;
        onCellClick?: (context: import("./type").PrimaryTableCellEventContext<import("./type").TableRowData>) => void;
        onChange?: (data: import("./type").TableChangeData, context: import("./type").TableChangeContext<import("./type").TableRowData>) => void;
        onColumnChange?: (context: import("./type").PrimaryTableColumnChange<import("./type").TableRowData>) => void;
        onColumnControllerVisibleChange?: (visible: boolean, context: {
            trigger: "cancel" | "confirm" | "open";
        }) => void;
        onDataChange?: (data: import("./type").TableRowData[], context: import("./type").TableDataChangeContext) => void;
        onDisplayColumnsChange?: (value: import("..").CheckboxGroupValue) => void;
        onDragSort?: (context: import("./type").DragSortContext<import("./type").TableRowData>) => void;
        onExpandChange?: (expandedRowKeys: Array<string | number>, options: import("./type").ExpandOptions<import("./type").TableRowData>) => void;
        onFilterChange?: (filterValue: import("./type").FilterValue, context: import("./type").TableFilterChangeContext<import("./type").TableRowData>) => void;
        onRowEdit?: (context: import("./type").PrimaryTableRowEditContext<import("./type").TableRowData>) => void;
        onRowValidate?: (context: import("./type").PrimaryTableRowValidateContext<import("./type").TableRowData>) => void;
        onSelectChange?: (selectedRowKeys: Array<string | number>, options: import("./type").SelectOptions<import("./type").TableRowData>) => void;
        onSortChange?: (sort: import("./type").TableSort, options: import("./type").SortOptions<import("./type").TableRowData>) => void;
        onValidate?: (context: import("./type").PrimaryTableValidateContext) => void;
        empty?: string | import("..").TNode;
        loading?: boolean | import("..").TNode;
        pagination?: import("..").PaginationProps;
        scroll?: import("..").TScroll;
        maxHeight?: string | number;
        size?: import("..").SizeEnum;
        data?: import("./type").TableRowData[];
        height?: string | number;
        onScroll?: (params: {
            e: WheelEvent;
        }) => void;
        attach?: import("..").AttachNode;
        hover?: boolean;
        tableLayout?: "auto" | "fixed";
        verticalAlign?: "top" | "middle" | "bottom";
        lazyLoad?: boolean;
        locale?: import("..").TableConfig;
        loadingProps?: Partial<import("..").LoadingProps>;
        fixedRows?: Array<number>;
        bordered?: boolean;
        onPageChange?: (pageInfo: import("..").PageInfo, newDataSource: import("./type").TableRowData[]) => void;
        stripe?: boolean;
        activeRowKeys?: Array<string | number>;
        defaultActiveRowKeys?: Array<string | number>;
        activeRowType?: "single" | "multiple";
        allowResizeColumnWidth?: boolean;
        bottomContent?: string | import("..").TNode;
        cellEmptyContent?: string | ((h: typeof import("vue").h, props: import("./type").BaseTableCellParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        disableDataPage?: boolean;
        disableSpaceInactiveRow?: boolean;
        firstFullRow?: string | import("..").TNode;
        footData?: import("./type").TableRowData[];
        footerAffixProps?: Partial<import("..").AffixProps>;
        footerAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        footerSummary?: string | import("..").TNode;
        headerAffixProps?: Partial<import("..").AffixProps>;
        headerAffixedTop?: boolean | Partial<import("..").AffixProps>;
        horizontalScrollAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        keyboardRowHover?: boolean;
        lastFullRow?: string | import("..").TNode;
        paginationAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        resizable?: boolean;
        rowAttributes?: import("./type").TableRowAttributes<import("./type").TableRowData>;
        rowClassName?: import("..").ClassName | ((params: import("./type").RowClassNameParams<import("./type").TableRowData>) => import("..").ClassName);
        rowKey: string;
        rowspanAndColspan?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
        rowspanAndColspanInFooter?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
        showHeader?: boolean;
        tableContentWidth?: string;
        topContent?: string | import("..").TNode;
        onActiveChange?: (activeRowKeys: Array<string | number>, context: import("./type").ActiveChangeContext<import("./type").TableRowData>) => void;
        onActiveRowAction?: (context: import("./type").ActiveRowActionContext<import("./type").TableRowData>) => void;
        onColumnResizeChange?: (context: {
            columnsWidth: {
                [colKey: string]: number;
            };
        }) => void;
        onRowClick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowDblclick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMousedown?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseenter?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseleave?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseover?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseup?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onScrollX?: (params: {
            e: WheelEvent;
        }) => void;
        onScrollY?: (params: {
            e: WheelEvent;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        sort: import("./type").TableSort;
        data: import("./type").TableRowData[];
        hover: boolean;
        tableLayout: "fixed" | "auto";
        verticalAlign: "top" | "middle" | "bottom";
        columns: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
        lazyLoad: boolean;
        bordered: boolean;
        stripe: boolean;
        activeRowKeys: (string | number)[];
        defaultActiveRowKeys: (string | number)[];
        activeRowType: "multiple" | "single";
        allowResizeColumnWidth: boolean;
        disableDataPage: boolean;
        disableSpaceInactiveRow: boolean;
        footData: import("./type").TableRowData[];
        footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
        headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
        keyboardRowHover: boolean;
        resizable: boolean;
        rowKey: string;
        showHeader: boolean;
        tableContentWidth: string;
        columnControllerVisible: boolean;
        defaultColumnControllerVisible: boolean;
        displayColumns: import("..").CheckboxGroupValue;
        expandIcon: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        expandOnRowClick: boolean;
        expandedRowKeys: (string | number)[];
        defaultExpandedRowKeys: (string | number)[];
        filterValue: import("./type").FilterValue;
        hideSortTips: boolean;
        multipleSort: boolean;
        reserveSelectedRowOnPaginate: boolean;
        rowSelectionAllowUncheck: boolean;
        selectOnRowClick: boolean;
        selectedRowKeys: (string | number)[];
        defaultSelectedRowKeys: (string | number)[];
        showSortColumnBgColor: boolean;
        sortOnRowDraggable: boolean;
        expandedTreeNodes: (string | number)[];
        defaultExpandedTreeNodes: (string | number)[];
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        beforeDragSort?: (context: import("./type").DragSortContext<import("./type").TableRowData>) => boolean;
        expandedTreeNodes?: Array<string | number>;
        defaultExpandedTreeNodes?: Array<string | number>;
        tree?: import("./type").TableTreeConfig;
        treeExpandAndFoldIcon?: (h: typeof import("vue").h, props: {
            type: "expand" | "fold";
            row: import("./type").TableRowData;
        }) => import("..").TNodeReturnValue;
        onAbnormalDragSort?: (context: import("./type").TableAbnormalDragSortContext<import("./type").TableRowData>) => void;
        onExpandedTreeNodesChange?: (expandedTreeNodes: Array<string | number>, options: import("./type").TableTreeNodeExpandOptions<import("./type").TableRowData>) => void;
        onTreeExpandChange?: (context: import("./type").TableTreeExpandChangeContext<import("./type").TableRowData>) => void;
        asyncLoading?: "loading" | "load-more" | import("..").TNode;
        columnController?: import("./type").TableColumnController;
        columnControllerVisible?: boolean;
        defaultColumnControllerVisible?: boolean;
        columns?: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
        displayColumns?: import("..").CheckboxGroupValue;
        defaultDisplayColumns?: import("..").CheckboxGroupValue;
        dragSort?: "row" | "row-handler" | "col" | "row-handler-col" | "drag-col";
        dragSortOptions?: import("sortablejs").SortableOptions;
        editableCellState?: import("./type").EditableCellType<import("./type").TableRowData>;
        editableRowKeys?: Array<string | number>;
        expandIcon?: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        expandOnRowClick?: boolean;
        expandedRow?: (h: typeof import("vue").h, props: import("./type").TableExpandedRowParams<import("./type").TableRowData>) => import("..").TNodeReturnValue;
        expandedRowKeys?: Array<string | number>;
        defaultExpandedRowKeys?: Array<string | number>;
        filterIcon?: (h: typeof import("vue").h, props: {
            col: import("./type").PrimaryTableCol<import("./type").TableRowData>;
            colIndex: number;
        }) => import("..").TNodeReturnValue;
        filterRow?: string | import("..").TNode;
        filterValue?: import("./type").FilterValue;
        defaultFilterValue?: import("./type").FilterValue;
        hideSortTips?: boolean;
        indeterminateSelectedRowKeys?: Array<string | number>;
        multipleSort?: boolean;
        reserveSelectedRowOnPaginate?: boolean;
        rowSelectionAllowUncheck?: boolean;
        rowSelectionType?: "single" | "multiple";
        selectOnRowClick?: boolean;
        selectedRowKeys?: Array<string | number>;
        defaultSelectedRowKeys?: Array<string | number>;
        showSortColumnBgColor?: boolean;
        sort?: import("./type").TableSort;
        defaultSort?: import("./type").TableSort;
        sortIcon?: import("..").TNode;
        sortOnRowDraggable?: boolean;
        onAsyncLoadingClick?: (context: {
            status: "loading" | "load-more";
        }) => void;
        onCellClick?: (context: import("./type").PrimaryTableCellEventContext<import("./type").TableRowData>) => void;
        onChange?: (data: import("./type").TableChangeData, context: import("./type").TableChangeContext<import("./type").TableRowData>) => void;
        onColumnChange?: (context: import("./type").PrimaryTableColumnChange<import("./type").TableRowData>) => void;
        onColumnControllerVisibleChange?: (visible: boolean, context: {
            trigger: "cancel" | "confirm" | "open";
        }) => void;
        onDataChange?: (data: import("./type").TableRowData[], context: import("./type").TableDataChangeContext) => void;
        onDisplayColumnsChange?: (value: import("..").CheckboxGroupValue) => void;
        onDragSort?: (context: import("./type").DragSortContext<import("./type").TableRowData>) => void;
        onExpandChange?: (expandedRowKeys: Array<string | number>, options: import("./type").ExpandOptions<import("./type").TableRowData>) => void;
        onFilterChange?: (filterValue: import("./type").FilterValue, context: import("./type").TableFilterChangeContext<import("./type").TableRowData>) => void;
        onRowEdit?: (context: import("./type").PrimaryTableRowEditContext<import("./type").TableRowData>) => void;
        onRowValidate?: (context: import("./type").PrimaryTableRowValidateContext<import("./type").TableRowData>) => void;
        onSelectChange?: (selectedRowKeys: Array<string | number>, options: import("./type").SelectOptions<import("./type").TableRowData>) => void;
        onSortChange?: (sort: import("./type").TableSort, options: import("./type").SortOptions<import("./type").TableRowData>) => void;
        onValidate?: (context: import("./type").PrimaryTableValidateContext) => void;
        empty?: string | import("..").TNode;
        loading?: boolean | import("..").TNode;
        pagination?: import("..").PaginationProps;
        scroll?: import("..").TScroll;
        maxHeight?: string | number;
        size?: import("..").SizeEnum;
        data?: import("./type").TableRowData[];
        height?: string | number;
        onScroll?: (params: {
            e: WheelEvent;
        }) => void;
        attach?: import("..").AttachNode;
        hover?: boolean;
        tableLayout?: "auto" | "fixed";
        verticalAlign?: "top" | "middle" | "bottom";
        lazyLoad?: boolean;
        locale?: import("..").TableConfig;
        loadingProps?: Partial<import("..").LoadingProps>;
        fixedRows?: Array<number>;
        bordered?: boolean;
        onPageChange?: (pageInfo: import("..").PageInfo, newDataSource: import("./type").TableRowData[]) => void;
        stripe?: boolean;
        activeRowKeys?: Array<string | number>;
        defaultActiveRowKeys?: Array<string | number>;
        activeRowType?: "single" | "multiple";
        allowResizeColumnWidth?: boolean;
        bottomContent?: string | import("..").TNode;
        cellEmptyContent?: string | ((h: typeof import("vue").h, props: import("./type").BaseTableCellParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        disableDataPage?: boolean;
        disableSpaceInactiveRow?: boolean;
        firstFullRow?: string | import("..").TNode;
        footData?: import("./type").TableRowData[];
        footerAffixProps?: Partial<import("..").AffixProps>;
        footerAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        footerSummary?: string | import("..").TNode;
        headerAffixProps?: Partial<import("..").AffixProps>;
        headerAffixedTop?: boolean | Partial<import("..").AffixProps>;
        horizontalScrollAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        keyboardRowHover?: boolean;
        lastFullRow?: string | import("..").TNode;
        paginationAffixedBottom?: boolean | Partial<import("..").AffixProps>;
        resizable?: boolean;
        rowAttributes?: import("./type").TableRowAttributes<import("./type").TableRowData>;
        rowClassName?: import("..").ClassName | ((params: import("./type").RowClassNameParams<import("./type").TableRowData>) => import("..").ClassName);
        rowKey: string;
        rowspanAndColspan?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
        rowspanAndColspanInFooter?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
        showHeader?: boolean;
        tableContentWidth?: string;
        topContent?: string | import("..").TNode;
        onActiveChange?: (activeRowKeys: Array<string | number>, context: import("./type").ActiveChangeContext<import("./type").TableRowData>) => void;
        onActiveRowAction?: (context: import("./type").ActiveRowActionContext<import("./type").TableRowData>) => void;
        onColumnResizeChange?: (context: {
            columnsWidth: {
                [colKey: string]: number;
            };
        }) => void;
        onRowClick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowDblclick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMousedown?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseenter?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseleave?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseover?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onRowMouseup?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
        onScrollX?: (params: {
            e: WheelEvent;
        }) => void;
        onScrollY?: (params: {
            e: WheelEvent;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        sort: import("./type").TableSort;
        data: import("./type").TableRowData[];
        hover: boolean;
        tableLayout: "fixed" | "auto";
        verticalAlign: "top" | "middle" | "bottom";
        columns: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
        lazyLoad: boolean;
        bordered: boolean;
        stripe: boolean;
        activeRowKeys: (string | number)[];
        defaultActiveRowKeys: (string | number)[];
        activeRowType: "multiple" | "single";
        allowResizeColumnWidth: boolean;
        disableDataPage: boolean;
        disableSpaceInactiveRow: boolean;
        footData: import("./type").TableRowData[];
        footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
        headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
        keyboardRowHover: boolean;
        resizable: boolean;
        rowKey: string;
        showHeader: boolean;
        tableContentWidth: string;
        columnControllerVisible: boolean;
        defaultColumnControllerVisible: boolean;
        displayColumns: import("..").CheckboxGroupValue;
        expandIcon: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        expandOnRowClick: boolean;
        expandedRowKeys: (string | number)[];
        defaultExpandedRowKeys: (string | number)[];
        filterValue: import("./type").FilterValue;
        hideSortTips: boolean;
        multipleSort: boolean;
        reserveSelectedRowOnPaginate: boolean;
        rowSelectionAllowUncheck: boolean;
        selectOnRowClick: boolean;
        selectedRowKeys: (string | number)[];
        defaultSelectedRowKeys: (string | number)[];
        showSortColumnBgColor: boolean;
        sortOnRowDraggable: boolean;
        expandedTreeNodes: (string | number)[];
        defaultExpandedTreeNodes: (string | number)[];
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    beforeDragSort?: (context: import("./type").DragSortContext<import("./type").TableRowData>) => boolean;
    expandedTreeNodes?: Array<string | number>;
    defaultExpandedTreeNodes?: Array<string | number>;
    tree?: import("./type").TableTreeConfig;
    treeExpandAndFoldIcon?: (h: typeof import("vue").h, props: {
        type: "expand" | "fold";
        row: import("./type").TableRowData;
    }) => import("..").TNodeReturnValue;
    onAbnormalDragSort?: (context: import("./type").TableAbnormalDragSortContext<import("./type").TableRowData>) => void;
    onExpandedTreeNodesChange?: (expandedTreeNodes: Array<string | number>, options: import("./type").TableTreeNodeExpandOptions<import("./type").TableRowData>) => void;
    onTreeExpandChange?: (context: import("./type").TableTreeExpandChangeContext<import("./type").TableRowData>) => void;
    asyncLoading?: "loading" | "load-more" | import("..").TNode;
    columnController?: import("./type").TableColumnController;
    columnControllerVisible?: boolean;
    defaultColumnControllerVisible?: boolean;
    columns?: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
    displayColumns?: import("..").CheckboxGroupValue;
    defaultDisplayColumns?: import("..").CheckboxGroupValue;
    dragSort?: "row" | "row-handler" | "col" | "row-handler-col" | "drag-col";
    dragSortOptions?: import("sortablejs").SortableOptions;
    editableCellState?: import("./type").EditableCellType<import("./type").TableRowData>;
    editableRowKeys?: Array<string | number>;
    expandIcon?: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
    expandOnRowClick?: boolean;
    expandedRow?: (h: typeof import("vue").h, props: import("./type").TableExpandedRowParams<import("./type").TableRowData>) => import("..").TNodeReturnValue;
    expandedRowKeys?: Array<string | number>;
    defaultExpandedRowKeys?: Array<string | number>;
    filterIcon?: (h: typeof import("vue").h, props: {
        col: import("./type").PrimaryTableCol<import("./type").TableRowData>;
        colIndex: number;
    }) => import("..").TNodeReturnValue;
    filterRow?: string | import("..").TNode;
    filterValue?: import("./type").FilterValue;
    defaultFilterValue?: import("./type").FilterValue;
    hideSortTips?: boolean;
    indeterminateSelectedRowKeys?: Array<string | number>;
    multipleSort?: boolean;
    reserveSelectedRowOnPaginate?: boolean;
    rowSelectionAllowUncheck?: boolean;
    rowSelectionType?: "single" | "multiple";
    selectOnRowClick?: boolean;
    selectedRowKeys?: Array<string | number>;
    defaultSelectedRowKeys?: Array<string | number>;
    showSortColumnBgColor?: boolean;
    sort?: import("./type").TableSort;
    defaultSort?: import("./type").TableSort;
    sortIcon?: import("..").TNode;
    sortOnRowDraggable?: boolean;
    onAsyncLoadingClick?: (context: {
        status: "loading" | "load-more";
    }) => void;
    onCellClick?: (context: import("./type").PrimaryTableCellEventContext<import("./type").TableRowData>) => void;
    onChange?: (data: import("./type").TableChangeData, context: import("./type").TableChangeContext<import("./type").TableRowData>) => void;
    onColumnChange?: (context: import("./type").PrimaryTableColumnChange<import("./type").TableRowData>) => void;
    onColumnControllerVisibleChange?: (visible: boolean, context: {
        trigger: "cancel" | "confirm" | "open";
    }) => void;
    onDataChange?: (data: import("./type").TableRowData[], context: import("./type").TableDataChangeContext) => void;
    onDisplayColumnsChange?: (value: import("..").CheckboxGroupValue) => void;
    onDragSort?: (context: import("./type").DragSortContext<import("./type").TableRowData>) => void;
    onExpandChange?: (expandedRowKeys: Array<string | number>, options: import("./type").ExpandOptions<import("./type").TableRowData>) => void;
    onFilterChange?: (filterValue: import("./type").FilterValue, context: import("./type").TableFilterChangeContext<import("./type").TableRowData>) => void;
    onRowEdit?: (context: import("./type").PrimaryTableRowEditContext<import("./type").TableRowData>) => void;
    onRowValidate?: (context: import("./type").PrimaryTableRowValidateContext<import("./type").TableRowData>) => void;
    onSelectChange?: (selectedRowKeys: Array<string | number>, options: import("./type").SelectOptions<import("./type").TableRowData>) => void;
    onSortChange?: (sort: import("./type").TableSort, options: import("./type").SortOptions<import("./type").TableRowData>) => void;
    onValidate?: (context: import("./type").PrimaryTableValidateContext) => void;
    empty?: string | import("..").TNode;
    loading?: boolean | import("..").TNode;
    pagination?: import("..").PaginationProps;
    scroll?: import("..").TScroll;
    maxHeight?: string | number;
    size?: import("..").SizeEnum;
    data?: import("./type").TableRowData[];
    height?: string | number;
    onScroll?: (params: {
        e: WheelEvent;
    }) => void;
    attach?: import("..").AttachNode;
    hover?: boolean;
    tableLayout?: "auto" | "fixed";
    verticalAlign?: "top" | "middle" | "bottom";
    lazyLoad?: boolean;
    locale?: import("..").TableConfig;
    loadingProps?: Partial<import("..").LoadingProps>;
    fixedRows?: Array<number>;
    bordered?: boolean;
    onPageChange?: (pageInfo: import("..").PageInfo, newDataSource: import("./type").TableRowData[]) => void;
    stripe?: boolean;
    activeRowKeys?: Array<string | number>;
    defaultActiveRowKeys?: Array<string | number>;
    activeRowType?: "single" | "multiple";
    allowResizeColumnWidth?: boolean;
    bottomContent?: string | import("..").TNode;
    cellEmptyContent?: string | ((h: typeof import("vue").h, props: import("./type").BaseTableCellParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
    disableDataPage?: boolean;
    disableSpaceInactiveRow?: boolean;
    firstFullRow?: string | import("..").TNode;
    footData?: import("./type").TableRowData[];
    footerAffixProps?: Partial<import("..").AffixProps>;
    footerAffixedBottom?: boolean | Partial<import("..").AffixProps>;
    footerSummary?: string | import("..").TNode;
    headerAffixProps?: Partial<import("..").AffixProps>;
    headerAffixedTop?: boolean | Partial<import("..").AffixProps>;
    horizontalScrollAffixedBottom?: boolean | Partial<import("..").AffixProps>;
    keyboardRowHover?: boolean;
    lastFullRow?: string | import("..").TNode;
    paginationAffixedBottom?: boolean | Partial<import("..").AffixProps>;
    resizable?: boolean;
    rowAttributes?: import("./type").TableRowAttributes<import("./type").TableRowData>;
    rowClassName?: import("..").ClassName | ((params: import("./type").RowClassNameParams<import("./type").TableRowData>) => import("..").ClassName);
    rowKey: string;
    rowspanAndColspan?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
    rowspanAndColspanInFooter?: import("./type").TableRowspanAndColspanFunc<import("./type").TableRowData>;
    showHeader?: boolean;
    tableContentWidth?: string;
    topContent?: string | import("..").TNode;
    onActiveChange?: (activeRowKeys: Array<string | number>, context: import("./type").ActiveChangeContext<import("./type").TableRowData>) => void;
    onActiveRowAction?: (context: import("./type").ActiveRowActionContext<import("./type").TableRowData>) => void;
    onColumnResizeChange?: (context: {
        columnsWidth: {
            [colKey: string]: number;
        };
    }) => void;
    onRowClick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowDblclick?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowMousedown?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowMouseenter?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowMouseleave?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowMouseover?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onRowMouseup?: (context: import("./type").RowEventContext<import("./type").TableRowData>) => void;
    onScrollX?: (params: {
        e: WheelEvent;
    }) => void;
    onScrollY?: (params: {
        e: WheelEvent;
    }) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    sort: import("./type").TableSort;
    data: import("./type").TableRowData[];
    hover: boolean;
    tableLayout: "fixed" | "auto";
    verticalAlign: "top" | "middle" | "bottom";
    columns: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
    lazyLoad: boolean;
    bordered: boolean;
    stripe: boolean;
    activeRowKeys: (string | number)[];
    defaultActiveRowKeys: (string | number)[];
    activeRowType: "multiple" | "single";
    allowResizeColumnWidth: boolean;
    disableDataPage: boolean;
    disableSpaceInactiveRow: boolean;
    footData: import("./type").TableRowData[];
    footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
    headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
    keyboardRowHover: boolean;
    resizable: boolean;
    rowKey: string;
    showHeader: boolean;
    tableContentWidth: string;
    columnControllerVisible: boolean;
    defaultColumnControllerVisible: boolean;
    displayColumns: import("..").CheckboxGroupValue;
    expandIcon: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
    expandOnRowClick: boolean;
    expandedRowKeys: (string | number)[];
    defaultExpandedRowKeys: (string | number)[];
    filterValue: import("./type").FilterValue;
    hideSortTips: boolean;
    multipleSort: boolean;
    reserveSelectedRowOnPaginate: boolean;
    rowSelectionAllowUncheck: boolean;
    selectOnRowClick: boolean;
    selectedRowKeys: (string | number)[];
    defaultSelectedRowKeys: (string | number)[];
    showSortColumnBgColor: boolean;
    sortOnRowDraggable: boolean;
    expandedTreeNodes: (string | number)[];
    defaultExpandedTreeNodes: (string | number)[];
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const Table: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        asyncLoading: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["asyncLoading"]>;
        };
        columnController: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["columnController"]>;
        };
        columnControllerVisible: {
            type: BooleanConstructor;
            default: any;
        };
        defaultColumnControllerVisible: {
            type: BooleanConstructor;
            default: any;
        };
        columns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["columns"]>;
            default: () => import("./type").TdPrimaryTableProps["columns"];
        };
        displayColumns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["displayColumns"]>;
            default: import("./type").TdPrimaryTableProps["displayColumns"];
        };
        defaultDisplayColumns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultDisplayColumns"]>;
        };
        dragSort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSort"]>;
            validator(val: import("./type").TdPrimaryTableProps["dragSort"]): boolean;
        };
        dragSortOptions: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSortOptions"]>;
        };
        editableCellState: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableCellState"]>;
        };
        editableRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableRowKeys"]>;
        };
        expandIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandIcon"]>;
            default: import("./type").TdPrimaryTableProps["expandIcon"];
        };
        expandOnRowClick: BooleanConstructor;
        expandedRow: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRow"]>;
        };
        expandedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRowKeys"]>;
            default: import("./type").TdPrimaryTableProps["expandedRowKeys"];
        };
        defaultExpandedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"]>;
            default: () => import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"];
        };
        filterIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterIcon"]>;
        };
        filterRow: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterRow"]>;
        };
        filterValue: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterValue"]>;
            default: import("./type").TdPrimaryTableProps["filterValue"];
        };
        defaultFilterValue: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultFilterValue"]>;
        };
        hideSortTips: BooleanConstructor;
        indeterminateSelectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["indeterminateSelectedRowKeys"]>;
        };
        multipleSort: BooleanConstructor;
        reserveSelectedRowOnPaginate: {
            type: BooleanConstructor;
            default: boolean;
        };
        rowSelectionAllowUncheck: BooleanConstructor;
        rowSelectionType: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["rowSelectionType"]>;
            validator(val: import("./type").TdPrimaryTableProps["rowSelectionType"]): boolean;
        };
        selectOnRowClick: BooleanConstructor;
        selectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["selectedRowKeys"]>;
            default: import("./type").TdPrimaryTableProps["selectedRowKeys"];
        };
        defaultSelectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"]>;
            default: () => import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"];
        };
        showSortColumnBgColor: BooleanConstructor;
        sort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["sort"]>;
            default: import("./type").TdPrimaryTableProps["sort"];
        };
        defaultSort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSort"]>;
        };
        sortIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["sortIcon"]>;
        };
        sortOnRowDraggable: BooleanConstructor;
        onAsyncLoadingClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onAsyncLoadingClick"]>;
        onCellClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onCellClick"]>;
        onChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onChange"]>;
        onColumnChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnChange"]>;
        onColumnControllerVisibleChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnControllerVisibleChange"]>;
        onDataChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDataChange"]>;
        onDisplayColumnsChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDisplayColumnsChange"]>;
        onDragSort: import("vue").PropType<import("./type").TdPrimaryTableProps["onDragSort"]>;
        onExpandChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onExpandChange"]>;
        onFilterChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onFilterChange"]>;
        onRowEdit: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowEdit"]>;
        onRowValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowValidate"]>;
        onSelectChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSelectChange"]>;
        onSortChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSortChange"]>;
        onValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onValidate"]>;
        activeRowKeys: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowKeys"]>;
            default: import("./type").TdBaseTableProps["activeRowKeys"];
        };
        defaultActiveRowKeys: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["defaultActiveRowKeys"]>;
            default: () => import("./type").TdBaseTableProps["defaultActiveRowKeys"];
        };
        activeRowType: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowType"]>;
            default: import("./type").TdBaseTableProps["activeRowType"];
        };
        allowResizeColumnWidth: {
            type: BooleanConstructor;
            default: any;
        };
        attach: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["attach"]>;
        };
        bordered: BooleanConstructor;
        bottomContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["bottomContent"]>;
        };
        cellEmptyContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["cellEmptyContent"]>;
        };
        data: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["data"]>;
            default: () => import("./type").TdBaseTableProps["data"];
        };
        disableDataPage: BooleanConstructor;
        disableSpaceInactiveRow: {
            type: BooleanConstructor;
            default: any;
        };
        empty: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["empty"]>;
            default: import("./type").TdBaseTableProps["empty"];
        };
        firstFullRow: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["firstFullRow"]>;
        };
        fixedRows: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["fixedRows"]>;
        };
        footData: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footData"]>;
            default: () => import("./type").TdBaseTableProps["footData"];
        };
        footerAffixProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixProps"]>;
        };
        footerAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixedBottom"]>;
            default: import("./type").TdBaseTableProps["footerAffixedBottom"];
        };
        footerSummary: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerSummary"]>;
        };
        headerAffixProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixProps"]>;
        };
        headerAffixedTop: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixedTop"]>;
            default: import("./type").TdBaseTableProps["headerAffixedTop"];
        };
        height: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["height"]>;
        };
        horizontalScrollAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["horizontalScrollAffixedBottom"]>;
        };
        hover: BooleanConstructor;
        keyboardRowHover: {
            type: BooleanConstructor;
            default: boolean;
        };
        lastFullRow: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["lastFullRow"]>;
        };
        lazyLoad: BooleanConstructor;
        loading: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["loading"]>;
            default: import("./type").TdBaseTableProps["loading"];
        };
        loadingProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["loadingProps"]>;
        };
        locale: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["locale"]>;
        };
        maxHeight: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["maxHeight"]>;
        };
        pagination: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["pagination"]>;
        };
        paginationAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["paginationAffixedBottom"]>;
        };
        resizable: BooleanConstructor;
        rowAttributes: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowAttributes"]>;
        };
        rowClassName: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowClassName"]>;
        };
        rowKey: {
            type: StringConstructor;
            default: string;
            required: boolean;
        };
        rowspanAndColspan: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspan"]>;
        };
        rowspanAndColspanInFooter: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspanInFooter"]>;
        };
        scroll: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["scroll"]>;
        };
        showHeader: {
            type: BooleanConstructor;
            default: boolean;
        };
        size: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["size"]>;
            validator(val: import("./type").TdBaseTableProps["size"]): boolean;
        };
        stripe: BooleanConstructor;
        tableContentWidth: {
            type: StringConstructor;
            default: string;
        };
        tableLayout: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["tableLayout"]>;
            default: import("./type").TdBaseTableProps["tableLayout"];
            validator(val: import("./type").TdBaseTableProps["tableLayout"]): boolean;
        };
        topContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["topContent"]>;
        };
        verticalAlign: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["verticalAlign"]>;
            default: import("./type").TdBaseTableProps["verticalAlign"];
            validator(val: import("./type").TdBaseTableProps["verticalAlign"]): boolean;
        };
        onActiveChange: import("vue").PropType<import("./type").TdBaseTableProps["onActiveChange"]>;
        onActiveRowAction: import("vue").PropType<import("./type").TdBaseTableProps["onActiveRowAction"]>;
        onColumnResizeChange: import("vue").PropType<import("./type").TdBaseTableProps["onColumnResizeChange"]>;
        onPageChange: import("vue").PropType<import("./type").TdBaseTableProps["onPageChange"]>;
        onRowClick: import("vue").PropType<import("./type").TdBaseTableProps["onRowClick"]>;
        onRowDblclick: import("vue").PropType<import("./type").TdBaseTableProps["onRowDblclick"]>;
        onRowMousedown: import("vue").PropType<import("./type").TdBaseTableProps["onRowMousedown"]>;
        onRowMouseenter: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseenter"]>;
        onRowMouseleave: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseleave"]>;
        onRowMouseover: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseover"]>;
        onRowMouseup: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseup"]>;
        onScroll: import("vue").PropType<import("./type").TdBaseTableProps["onScroll"]>;
        onScrollX: import("vue").PropType<import("./type").TdBaseTableProps["onScrollX"]>;
        onScrollY: import("vue").PropType<import("./type").TdBaseTableProps["onScrollY"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        sort: import("./type").TableSort;
        data: import("./type").TableRowData[];
        hover: boolean;
        tableLayout: "fixed" | "auto";
        verticalAlign: "top" | "middle" | "bottom";
        columns: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
        lazyLoad: boolean;
        bordered: boolean;
        stripe: boolean;
        activeRowKeys: (string | number)[];
        defaultActiveRowKeys: (string | number)[];
        activeRowType: "multiple" | "single";
        allowResizeColumnWidth: boolean;
        disableDataPage: boolean;
        disableSpaceInactiveRow: boolean;
        footData: import("./type").TableRowData[];
        footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
        headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
        keyboardRowHover: boolean;
        resizable: boolean;
        rowKey: string;
        showHeader: boolean;
        tableContentWidth: string;
        columnControllerVisible: boolean;
        defaultColumnControllerVisible: boolean;
        displayColumns: import("..").CheckboxGroupValue;
        expandIcon: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        expandOnRowClick: boolean;
        expandedRowKeys: (string | number)[];
        defaultExpandedRowKeys: (string | number)[];
        filterValue: import("./type").FilterValue;
        hideSortTips: boolean;
        multipleSort: boolean;
        reserveSelectedRowOnPaginate: boolean;
        rowSelectionAllowUncheck: boolean;
        selectOnRowClick: boolean;
        selectedRowKeys: (string | number)[];
        defaultSelectedRowKeys: (string | number)[];
        showSortColumnBgColor: boolean;
        sortOnRowDraggable: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        asyncLoading: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["asyncLoading"]>;
        };
        columnController: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["columnController"]>;
        };
        columnControllerVisible: {
            type: BooleanConstructor;
            default: any;
        };
        defaultColumnControllerVisible: {
            type: BooleanConstructor;
            default: any;
        };
        columns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["columns"]>;
            default: () => import("./type").TdPrimaryTableProps["columns"];
        };
        displayColumns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["displayColumns"]>;
            default: import("./type").TdPrimaryTableProps["displayColumns"];
        };
        defaultDisplayColumns: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultDisplayColumns"]>;
        };
        dragSort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSort"]>;
            validator(val: import("./type").TdPrimaryTableProps["dragSort"]): boolean;
        };
        dragSortOptions: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSortOptions"]>;
        };
        editableCellState: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableCellState"]>;
        };
        editableRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableRowKeys"]>;
        };
        expandIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandIcon"]>;
            default: import("./type").TdPrimaryTableProps["expandIcon"];
        };
        expandOnRowClick: BooleanConstructor;
        expandedRow: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRow"]>;
        };
        expandedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRowKeys"]>;
            default: import("./type").TdPrimaryTableProps["expandedRowKeys"];
        };
        defaultExpandedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"]>;
            default: () => import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"];
        };
        filterIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterIcon"]>;
        };
        filterRow: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterRow"]>;
        };
        filterValue: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterValue"]>;
            default: import("./type").TdPrimaryTableProps["filterValue"];
        };
        defaultFilterValue: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultFilterValue"]>;
        };
        hideSortTips: BooleanConstructor;
        indeterminateSelectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["indeterminateSelectedRowKeys"]>;
        };
        multipleSort: BooleanConstructor;
        reserveSelectedRowOnPaginate: {
            type: BooleanConstructor;
            default: boolean;
        };
        rowSelectionAllowUncheck: BooleanConstructor;
        rowSelectionType: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["rowSelectionType"]>;
            validator(val: import("./type").TdPrimaryTableProps["rowSelectionType"]): boolean;
        };
        selectOnRowClick: BooleanConstructor;
        selectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["selectedRowKeys"]>;
            default: import("./type").TdPrimaryTableProps["selectedRowKeys"];
        };
        defaultSelectedRowKeys: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"]>;
            default: () => import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"];
        };
        showSortColumnBgColor: BooleanConstructor;
        sort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["sort"]>;
            default: import("./type").TdPrimaryTableProps["sort"];
        };
        defaultSort: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSort"]>;
        };
        sortIcon: {
            type: import("vue").PropType<import("./type").TdPrimaryTableProps["sortIcon"]>;
        };
        sortOnRowDraggable: BooleanConstructor;
        onAsyncLoadingClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onAsyncLoadingClick"]>;
        onCellClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onCellClick"]>;
        onChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onChange"]>;
        onColumnChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnChange"]>;
        onColumnControllerVisibleChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnControllerVisibleChange"]>;
        onDataChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDataChange"]>;
        onDisplayColumnsChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDisplayColumnsChange"]>;
        onDragSort: import("vue").PropType<import("./type").TdPrimaryTableProps["onDragSort"]>;
        onExpandChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onExpandChange"]>;
        onFilterChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onFilterChange"]>;
        onRowEdit: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowEdit"]>;
        onRowValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowValidate"]>;
        onSelectChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSelectChange"]>;
        onSortChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSortChange"]>;
        onValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onValidate"]>;
        activeRowKeys: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowKeys"]>;
            default: import("./type").TdBaseTableProps["activeRowKeys"];
        };
        defaultActiveRowKeys: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["defaultActiveRowKeys"]>;
            default: () => import("./type").TdBaseTableProps["defaultActiveRowKeys"];
        };
        activeRowType: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowType"]>;
            default: import("./type").TdBaseTableProps["activeRowType"];
        };
        allowResizeColumnWidth: {
            type: BooleanConstructor;
            default: any;
        };
        attach: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["attach"]>;
        };
        bordered: BooleanConstructor;
        bottomContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["bottomContent"]>;
        };
        cellEmptyContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["cellEmptyContent"]>;
        };
        data: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["data"]>;
            default: () => import("./type").TdBaseTableProps["data"];
        };
        disableDataPage: BooleanConstructor;
        disableSpaceInactiveRow: {
            type: BooleanConstructor;
            default: any;
        };
        empty: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["empty"]>;
            default: import("./type").TdBaseTableProps["empty"];
        };
        firstFullRow: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["firstFullRow"]>;
        };
        fixedRows: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["fixedRows"]>;
        };
        footData: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footData"]>;
            default: () => import("./type").TdBaseTableProps["footData"];
        };
        footerAffixProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixProps"]>;
        };
        footerAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixedBottom"]>;
            default: import("./type").TdBaseTableProps["footerAffixedBottom"];
        };
        footerSummary: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["footerSummary"]>;
        };
        headerAffixProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixProps"]>;
        };
        headerAffixedTop: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixedTop"]>;
            default: import("./type").TdBaseTableProps["headerAffixedTop"];
        };
        height: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["height"]>;
        };
        horizontalScrollAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["horizontalScrollAffixedBottom"]>;
        };
        hover: BooleanConstructor;
        keyboardRowHover: {
            type: BooleanConstructor;
            default: boolean;
        };
        lastFullRow: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["lastFullRow"]>;
        };
        lazyLoad: BooleanConstructor;
        loading: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["loading"]>;
            default: import("./type").TdBaseTableProps["loading"];
        };
        loadingProps: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["loadingProps"]>;
        };
        locale: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["locale"]>;
        };
        maxHeight: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["maxHeight"]>;
        };
        pagination: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["pagination"]>;
        };
        paginationAffixedBottom: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["paginationAffixedBottom"]>;
        };
        resizable: BooleanConstructor;
        rowAttributes: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowAttributes"]>;
        };
        rowClassName: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowClassName"]>;
        };
        rowKey: {
            type: StringConstructor;
            default: string;
            required: boolean;
        };
        rowspanAndColspan: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspan"]>;
        };
        rowspanAndColspanInFooter: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspanInFooter"]>;
        };
        scroll: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["scroll"]>;
        };
        showHeader: {
            type: BooleanConstructor;
            default: boolean;
        };
        size: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["size"]>;
            validator(val: import("./type").TdBaseTableProps["size"]): boolean;
        };
        stripe: BooleanConstructor;
        tableContentWidth: {
            type: StringConstructor;
            default: string;
        };
        tableLayout: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["tableLayout"]>;
            default: import("./type").TdBaseTableProps["tableLayout"];
            validator(val: import("./type").TdBaseTableProps["tableLayout"]): boolean;
        };
        topContent: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["topContent"]>;
        };
        verticalAlign: {
            type: import("vue").PropType<import("./type").TdBaseTableProps["verticalAlign"]>;
            default: import("./type").TdBaseTableProps["verticalAlign"];
            validator(val: import("./type").TdBaseTableProps["verticalAlign"]): boolean;
        };
        onActiveChange: import("vue").PropType<import("./type").TdBaseTableProps["onActiveChange"]>;
        onActiveRowAction: import("vue").PropType<import("./type").TdBaseTableProps["onActiveRowAction"]>;
        onColumnResizeChange: import("vue").PropType<import("./type").TdBaseTableProps["onColumnResizeChange"]>;
        onPageChange: import("vue").PropType<import("./type").TdBaseTableProps["onPageChange"]>;
        onRowClick: import("vue").PropType<import("./type").TdBaseTableProps["onRowClick"]>;
        onRowDblclick: import("vue").PropType<import("./type").TdBaseTableProps["onRowDblclick"]>;
        onRowMousedown: import("vue").PropType<import("./type").TdBaseTableProps["onRowMousedown"]>;
        onRowMouseenter: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseenter"]>;
        onRowMouseleave: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseleave"]>;
        onRowMouseover: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseover"]>;
        onRowMouseup: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseup"]>;
        onScroll: import("vue").PropType<import("./type").TdBaseTableProps["onScroll"]>;
        onScrollX: import("vue").PropType<import("./type").TdBaseTableProps["onScrollX"]>;
        onScrollY: import("vue").PropType<import("./type").TdBaseTableProps["onScrollY"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        sort: import("./type").TableSort;
        data: import("./type").TableRowData[];
        hover: boolean;
        tableLayout: "fixed" | "auto";
        verticalAlign: "top" | "middle" | "bottom";
        columns: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
        lazyLoad: boolean;
        bordered: boolean;
        stripe: boolean;
        activeRowKeys: (string | number)[];
        defaultActiveRowKeys: (string | number)[];
        activeRowType: "multiple" | "single";
        allowResizeColumnWidth: boolean;
        disableDataPage: boolean;
        disableSpaceInactiveRow: boolean;
        footData: import("./type").TableRowData[];
        footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
        headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
        keyboardRowHover: boolean;
        resizable: boolean;
        rowKey: string;
        showHeader: boolean;
        tableContentWidth: string;
        columnControllerVisible: boolean;
        defaultColumnControllerVisible: boolean;
        displayColumns: import("..").CheckboxGroupValue;
        expandIcon: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
        expandOnRowClick: boolean;
        expandedRowKeys: (string | number)[];
        defaultExpandedRowKeys: (string | number)[];
        filterValue: import("./type").FilterValue;
        hideSortTips: boolean;
        multipleSort: boolean;
        reserveSelectedRowOnPaginate: boolean;
        rowSelectionAllowUncheck: boolean;
        selectOnRowClick: boolean;
        selectedRowKeys: (string | number)[];
        defaultSelectedRowKeys: (string | number)[];
        showSortColumnBgColor: boolean;
        sortOnRowDraggable: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    asyncLoading: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["asyncLoading"]>;
    };
    columnController: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["columnController"]>;
    };
    columnControllerVisible: {
        type: BooleanConstructor;
        default: any;
    };
    defaultColumnControllerVisible: {
        type: BooleanConstructor;
        default: any;
    };
    columns: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["columns"]>;
        default: () => import("./type").TdPrimaryTableProps["columns"];
    };
    displayColumns: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["displayColumns"]>;
        default: import("./type").TdPrimaryTableProps["displayColumns"];
    };
    defaultDisplayColumns: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultDisplayColumns"]>;
    };
    dragSort: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSort"]>;
        validator(val: import("./type").TdPrimaryTableProps["dragSort"]): boolean;
    };
    dragSortOptions: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["dragSortOptions"]>;
    };
    editableCellState: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableCellState"]>;
    };
    editableRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["editableRowKeys"]>;
    };
    expandIcon: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandIcon"]>;
        default: import("./type").TdPrimaryTableProps["expandIcon"];
    };
    expandOnRowClick: BooleanConstructor;
    expandedRow: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRow"]>;
    };
    expandedRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["expandedRowKeys"]>;
        default: import("./type").TdPrimaryTableProps["expandedRowKeys"];
    };
    defaultExpandedRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"]>;
        default: () => import("./type").TdPrimaryTableProps["defaultExpandedRowKeys"];
    };
    filterIcon: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterIcon"]>;
    };
    filterRow: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterRow"]>;
    };
    filterValue: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["filterValue"]>;
        default: import("./type").TdPrimaryTableProps["filterValue"];
    };
    defaultFilterValue: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultFilterValue"]>;
    };
    hideSortTips: BooleanConstructor;
    indeterminateSelectedRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["indeterminateSelectedRowKeys"]>;
    };
    multipleSort: BooleanConstructor;
    reserveSelectedRowOnPaginate: {
        type: BooleanConstructor;
        default: boolean;
    };
    rowSelectionAllowUncheck: BooleanConstructor;
    rowSelectionType: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["rowSelectionType"]>;
        validator(val: import("./type").TdPrimaryTableProps["rowSelectionType"]): boolean;
    };
    selectOnRowClick: BooleanConstructor;
    selectedRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["selectedRowKeys"]>;
        default: import("./type").TdPrimaryTableProps["selectedRowKeys"];
    };
    defaultSelectedRowKeys: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"]>;
        default: () => import("./type").TdPrimaryTableProps["defaultSelectedRowKeys"];
    };
    showSortColumnBgColor: BooleanConstructor;
    sort: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["sort"]>;
        default: import("./type").TdPrimaryTableProps["sort"];
    };
    defaultSort: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["defaultSort"]>;
    };
    sortIcon: {
        type: import("vue").PropType<import("./type").TdPrimaryTableProps["sortIcon"]>;
    };
    sortOnRowDraggable: BooleanConstructor;
    onAsyncLoadingClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onAsyncLoadingClick"]>;
    onCellClick: import("vue").PropType<import("./type").TdPrimaryTableProps["onCellClick"]>;
    onChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onChange"]>;
    onColumnChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnChange"]>;
    onColumnControllerVisibleChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onColumnControllerVisibleChange"]>;
    onDataChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDataChange"]>;
    onDisplayColumnsChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onDisplayColumnsChange"]>;
    onDragSort: import("vue").PropType<import("./type").TdPrimaryTableProps["onDragSort"]>;
    onExpandChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onExpandChange"]>;
    onFilterChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onFilterChange"]>;
    onRowEdit: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowEdit"]>;
    onRowValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onRowValidate"]>;
    onSelectChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSelectChange"]>;
    onSortChange: import("vue").PropType<import("./type").TdPrimaryTableProps["onSortChange"]>;
    onValidate: import("vue").PropType<import("./type").TdPrimaryTableProps["onValidate"]>;
    activeRowKeys: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowKeys"]>;
        default: import("./type").TdBaseTableProps["activeRowKeys"];
    };
    defaultActiveRowKeys: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["defaultActiveRowKeys"]>;
        default: () => import("./type").TdBaseTableProps["defaultActiveRowKeys"];
    };
    activeRowType: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["activeRowType"]>;
        default: import("./type").TdBaseTableProps["activeRowType"];
    };
    allowResizeColumnWidth: {
        type: BooleanConstructor;
        default: any;
    };
    attach: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["attach"]>;
    };
    bordered: BooleanConstructor;
    bottomContent: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["bottomContent"]>;
    };
    cellEmptyContent: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["cellEmptyContent"]>;
    };
    data: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["data"]>;
        default: () => import("./type").TdBaseTableProps["data"];
    };
    disableDataPage: BooleanConstructor;
    disableSpaceInactiveRow: {
        type: BooleanConstructor;
        default: any;
    };
    empty: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["empty"]>;
        default: import("./type").TdBaseTableProps["empty"];
    };
    firstFullRow: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["firstFullRow"]>;
    };
    fixedRows: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["fixedRows"]>;
    };
    footData: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["footData"]>;
        default: () => import("./type").TdBaseTableProps["footData"];
    };
    footerAffixProps: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixProps"]>;
    };
    footerAffixedBottom: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["footerAffixedBottom"]>;
        default: import("./type").TdBaseTableProps["footerAffixedBottom"];
    };
    footerSummary: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["footerSummary"]>;
    };
    headerAffixProps: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixProps"]>;
    };
    headerAffixedTop: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["headerAffixedTop"]>;
        default: import("./type").TdBaseTableProps["headerAffixedTop"];
    };
    height: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["height"]>;
    };
    horizontalScrollAffixedBottom: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["horizontalScrollAffixedBottom"]>;
    };
    hover: BooleanConstructor;
    keyboardRowHover: {
        type: BooleanConstructor;
        default: boolean;
    };
    lastFullRow: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["lastFullRow"]>;
    };
    lazyLoad: BooleanConstructor;
    loading: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["loading"]>;
        default: import("./type").TdBaseTableProps["loading"];
    };
    loadingProps: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["loadingProps"]>;
    };
    locale: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["locale"]>;
    };
    maxHeight: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["maxHeight"]>;
    };
    pagination: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["pagination"]>;
    };
    paginationAffixedBottom: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["paginationAffixedBottom"]>;
    };
    resizable: BooleanConstructor;
    rowAttributes: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["rowAttributes"]>;
    };
    rowClassName: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["rowClassName"]>;
    };
    rowKey: {
        type: StringConstructor;
        default: string;
        required: boolean;
    };
    rowspanAndColspan: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspan"]>;
    };
    rowspanAndColspanInFooter: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["rowspanAndColspanInFooter"]>;
    };
    scroll: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["scroll"]>;
    };
    showHeader: {
        type: BooleanConstructor;
        default: boolean;
    };
    size: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["size"]>;
        validator(val: import("./type").TdBaseTableProps["size"]): boolean;
    };
    stripe: BooleanConstructor;
    tableContentWidth: {
        type: StringConstructor;
        default: string;
    };
    tableLayout: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["tableLayout"]>;
        default: import("./type").TdBaseTableProps["tableLayout"];
        validator(val: import("./type").TdBaseTableProps["tableLayout"]): boolean;
    };
    topContent: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["topContent"]>;
    };
    verticalAlign: {
        type: import("vue").PropType<import("./type").TdBaseTableProps["verticalAlign"]>;
        default: import("./type").TdBaseTableProps["verticalAlign"];
        validator(val: import("./type").TdBaseTableProps["verticalAlign"]): boolean;
    };
    onActiveChange: import("vue").PropType<import("./type").TdBaseTableProps["onActiveChange"]>;
    onActiveRowAction: import("vue").PropType<import("./type").TdBaseTableProps["onActiveRowAction"]>;
    onColumnResizeChange: import("vue").PropType<import("./type").TdBaseTableProps["onColumnResizeChange"]>;
    onPageChange: import("vue").PropType<import("./type").TdBaseTableProps["onPageChange"]>;
    onRowClick: import("vue").PropType<import("./type").TdBaseTableProps["onRowClick"]>;
    onRowDblclick: import("vue").PropType<import("./type").TdBaseTableProps["onRowDblclick"]>;
    onRowMousedown: import("vue").PropType<import("./type").TdBaseTableProps["onRowMousedown"]>;
    onRowMouseenter: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseenter"]>;
    onRowMouseleave: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseleave"]>;
    onRowMouseover: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseover"]>;
    onRowMouseup: import("vue").PropType<import("./type").TdBaseTableProps["onRowMouseup"]>;
    onScroll: import("vue").PropType<import("./type").TdBaseTableProps["onScroll"]>;
    onScrollX: import("vue").PropType<import("./type").TdBaseTableProps["onScrollX"]>;
    onScrollY: import("vue").PropType<import("./type").TdBaseTableProps["onScrollY"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    empty: string | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    loading: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    sort: import("./type").TableSort;
    data: import("./type").TableRowData[];
    hover: boolean;
    tableLayout: "fixed" | "auto";
    verticalAlign: "top" | "middle" | "bottom";
    columns: import("./type").PrimaryTableCol<import("./type").TableRowData>[];
    lazyLoad: boolean;
    bordered: boolean;
    stripe: boolean;
    activeRowKeys: (string | number)[];
    defaultActiveRowKeys: (string | number)[];
    activeRowType: "multiple" | "single";
    allowResizeColumnWidth: boolean;
    disableDataPage: boolean;
    disableSpaceInactiveRow: boolean;
    footData: import("./type").TableRowData[];
    footerAffixedBottom: boolean | Partial<import("..").TdAffixProps>;
    headerAffixedTop: boolean | Partial<import("..").TdAffixProps>;
    keyboardRowHover: boolean;
    resizable: boolean;
    rowKey: string;
    showHeader: boolean;
    tableContentWidth: string;
    columnControllerVisible: boolean;
    defaultColumnControllerVisible: boolean;
    displayColumns: import("..").CheckboxGroupValue;
    expandIcon: boolean | ((h: typeof import("vue").h, props: import("./type").ExpandArrowRenderParams<import("./type").TableRowData>) => import("..").TNodeReturnValue);
    expandOnRowClick: boolean;
    expandedRowKeys: (string | number)[];
    defaultExpandedRowKeys: (string | number)[];
    filterValue: import("./type").FilterValue;
    hideSortTips: boolean;
    multipleSort: boolean;
    reserveSelectedRowOnPaginate: boolean;
    rowSelectionAllowUncheck: boolean;
    selectOnRowClick: boolean;
    selectedRowKeys: (string | number)[];
    defaultSelectedRowKeys: (string | number)[];
    showSortColumnBgColor: boolean;
    sortOnRowDraggable: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Table;
