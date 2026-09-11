import { SetupContext, ComputedRef, Ref } from 'vue';
import { ClassName, Styles } from '../../common';
import { BaseTableCol, TableRowData, TdBaseTableProps } from '../type';
import { TableRowFixedClasses, RowAndColFixedPosition, TableColFixedClasses } from '../types';
export declare function getColumnFixedStyles(col: TdBaseTableProps['columns'][0], index: number, rowAndColFixedPosition: RowAndColFixedPosition, tableColFixedClasses: TableColFixedClasses): {
    style?: Styles;
    classes?: ClassName;
};
export declare function getRowFixedStyles(rowId: string | number, rowIndex: number, rowLength: number, fixedRows: TdBaseTableProps['fixedRows'], rowAndColFixedPosition: RowAndColFixedPosition, tableRowFixedClasses: TableRowFixedClasses, virtualTranslateY?: number): {
    style: Styles;
    classes: ClassName;
};
export default function useFixed(props: TdBaseTableProps, context: SetupContext, finalColumns: ComputedRef<BaseTableCol<TableRowData>[]>, affixRef: Record<string, Ref>): {
    tableWidth: Ref<number, number>;
    tableElmWidth: Ref<number, number>;
    thWidthList: Ref<{
        [colKey: string]: number;
    }, {
        [colKey: string]: number;
    }>;
    isFixedHeader: Ref<boolean, boolean>;
    isWidthOverflow: Ref<boolean, boolean>;
    tableContentRef: Ref<HTMLDivElement, HTMLDivElement>;
    isFixedColumn: Ref<boolean, boolean>;
    showColumnShadow: {
        left: boolean;
        right: boolean;
    };
    rowAndColFixedPosition: Ref<Map<string | number, {
        left?: number;
        right?: number;
        top?: number;
        bottom?: number;
        parent?: any;
        children?: string[];
        width?: number;
        height?: number;
        col?: {
            align?: "left" | "right" | "center";
            attrs?: import("..").BaseTableColumnAttributes<TableRowData>;
            cell?: string | ((h: typeof import("vue").h, props: import("..").BaseTableCellParams<TableRowData>) => import("../..").TNodeReturnValue);
            children?: any[];
            className?: any;
            colKey?: string;
            colspan?: number;
            ellipsis?: boolean | ((h: typeof import("vue").h, props: import("..").BaseTableCellParams<TableRowData>) => import("../..").TNodeReturnValue) | {
                delay?: number;
                destroyOnClose?: boolean;
                duration?: number;
                placement?: "mouse" | import("../..").PopupPlacement;
                showArrow?: boolean;
                theme?: "default" | "primary" | "success" | "danger" | "warning" | "light";
                default?: string | import("../..").TNode;
                disabled?: boolean;
                visible?: boolean;
                modelValue?: boolean;
                onScroll?: (context: {
                    e: WheelEvent;
                }) => void;
                attach?: import("../..").AttachNode;
                content?: string | import("../..").TNode;
                overlayClassName?: any;
                overlayStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                overlayInnerClassName?: any;
                overlayInnerStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                popperOptions?: object;
                trigger?: "hover" | "click" | "focus" | "mousedown" | "context-menu";
                triggerElement?: string | import("../..").TNode;
                onOverlayClick?: (context: {
                    e: MouseEvent;
                }) => void;
                onScrollToBottom?: (context: {
                    e: WheelEvent;
                }) => void;
                onVisibleChange?: (visible: boolean, context: import("../..").PopupVisibleChangeContext) => void;
                hideEmptyPopup?: boolean;
                defaultVisible?: boolean;
                zIndex?: number;
            } | {
                props: {
                    delay?: number;
                    destroyOnClose?: boolean;
                    duration?: number;
                    placement?: "mouse" | import("../..").PopupPlacement;
                    showArrow?: boolean;
                    theme?: "default" | "primary" | "success" | "danger" | "warning" | "light";
                    default?: string | import("../..").TNode;
                    disabled?: boolean;
                    visible?: boolean;
                    modelValue?: boolean;
                    onScroll?: (context: {
                        e: WheelEvent;
                    }) => void;
                    attach?: import("../..").AttachNode;
                    content?: string | import("../..").TNode;
                    overlayClassName?: any;
                    overlayStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                    overlayInnerClassName?: any;
                    overlayInnerStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                    popperOptions?: object;
                    trigger?: "hover" | "click" | "focus" | "mousedown" | "context-menu";
                    triggerElement?: string | import("../..").TNode;
                    onOverlayClick?: (context: {
                        e: MouseEvent;
                    }) => void;
                    onScrollToBottom?: (context: {
                        e: WheelEvent;
                    }) => void;
                    onVisibleChange?: (visible: boolean, context: import("../..").PopupVisibleChangeContext) => void;
                    hideEmptyPopup?: boolean;
                    defaultVisible?: boolean;
                    zIndex?: number;
                };
                content: (h: typeof import("vue").h, props: import("..").BaseTableCellParams<TableRowData>) => import("../..").TNodeReturnValue;
            };
            ellipsisTitle?: boolean | ((h: typeof import("vue").h, props: import("..").BaseTableColParams<TableRowData>) => import("../..").TNodeReturnValue) | {
                delay?: number;
                destroyOnClose?: boolean;
                duration?: number;
                placement?: "mouse" | import("../..").PopupPlacement;
                showArrow?: boolean;
                theme?: "default" | "primary" | "success" | "danger" | "warning" | "light";
                default?: string | import("../..").TNode;
                disabled?: boolean;
                visible?: boolean;
                modelValue?: boolean;
                onScroll?: (context: {
                    e: WheelEvent;
                }) => void;
                attach?: import("../..").AttachNode;
                content?: string | import("../..").TNode;
                overlayClassName?: any;
                overlayStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                overlayInnerClassName?: any;
                overlayInnerStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                popperOptions?: object;
                trigger?: "hover" | "click" | "focus" | "mousedown" | "context-menu";
                triggerElement?: string | import("../..").TNode;
                onOverlayClick?: (context: {
                    e: MouseEvent;
                }) => void;
                onScrollToBottom?: (context: {
                    e: WheelEvent;
                }) => void;
                onVisibleChange?: (visible: boolean, context: import("../..").PopupVisibleChangeContext) => void;
                hideEmptyPopup?: boolean;
                defaultVisible?: boolean;
                zIndex?: number;
            } | {
                props: {
                    delay?: number;
                    destroyOnClose?: boolean;
                    duration?: number;
                    placement?: "mouse" | import("../..").PopupPlacement;
                    showArrow?: boolean;
                    theme?: "default" | "primary" | "success" | "danger" | "warning" | "light";
                    default?: string | import("../..").TNode;
                    disabled?: boolean;
                    visible?: boolean;
                    modelValue?: boolean;
                    onScroll?: (context: {
                        e: WheelEvent;
                    }) => void;
                    attach?: import("../..").AttachNode;
                    content?: string | import("../..").TNode;
                    overlayClassName?: any;
                    overlayStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                    overlayInnerClassName?: any;
                    overlayInnerStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                    popperOptions?: object;
                    trigger?: "hover" | "click" | "focus" | "mousedown" | "context-menu";
                    triggerElement?: string | import("../..").TNode;
                    onOverlayClick?: (context: {
                        e: MouseEvent;
                    }) => void;
                    onScrollToBottom?: (context: {
                        e: WheelEvent;
                    }) => void;
                    onVisibleChange?: (visible: boolean, context: import("../..").PopupVisibleChangeContext) => void;
                    hideEmptyPopup?: boolean;
                    defaultVisible?: boolean;
                    zIndex?: number;
                };
                content: (h: typeof import("vue").h, props: import("..").BaseTableColParams<TableRowData>) => import("../..").TNodeReturnValue;
            };
            fixed?: "left" | "right";
            foot?: string | import("../..").TNode<{
                col: BaseTableCol;
                colIndex: number;
            }>;
            minWidth?: string | number;
            render?: (h: typeof import("vue").h, props: import("..").BaseTableRenderParams<TableRowData>) => import("../..").TNodeReturnValue;
            resizable?: boolean;
            resize?: {
                minWidth: number;
                maxWidth: number;
            };
            stopPropagation?: boolean;
            thClassName?: any;
            title?: string | import("../..").TNode<{
                col: BaseTableCol;
                colIndex: number;
            }>;
            width?: string | number;
        };
        index?: number;
        lastLeftFixedCol?: boolean;
        firstRightFixedCol?: boolean;
    }> & Omit<RowAndColFixedPosition, keyof Map<any, any>>, RowAndColFixedPosition | (Map<string | number, {
        left?: number;
        right?: number;
        top?: number;
        bottom?: number;
        parent?: any;
        children?: string[];
        width?: number;
        height?: number;
        col?: {
            align?: "left" | "right" | "center";
            attrs?: import("..").BaseTableColumnAttributes<TableRowData>;
            cell?: string | ((h: typeof import("vue").h, props: import("..").BaseTableCellParams<TableRowData>) => import("../..").TNodeReturnValue);
            children?: any[];
            className?: any;
            colKey?: string;
            colspan?: number;
            ellipsis?: boolean | ((h: typeof import("vue").h, props: import("..").BaseTableCellParams<TableRowData>) => import("../..").TNodeReturnValue) | {
                delay?: number;
                destroyOnClose?: boolean;
                duration?: number;
                placement?: "mouse" | import("../..").PopupPlacement;
                showArrow?: boolean;
                theme?: "default" | "primary" | "success" | "danger" | "warning" | "light";
                default?: string | import("../..").TNode;
                disabled?: boolean;
                visible?: boolean;
                modelValue?: boolean;
                onScroll?: (context: {
                    e: WheelEvent;
                }) => void;
                attach?: import("../..").AttachNode;
                content?: string | import("../..").TNode;
                overlayClassName?: any;
                overlayStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                overlayInnerClassName?: any;
                overlayInnerStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                popperOptions?: object;
                trigger?: "hover" | "click" | "focus" | "mousedown" | "context-menu";
                triggerElement?: string | import("../..").TNode;
                onOverlayClick?: (context: {
                    e: MouseEvent;
                }) => void;
                onScrollToBottom?: (context: {
                    e: WheelEvent;
                }) => void;
                onVisibleChange?: (visible: boolean, context: import("../..").PopupVisibleChangeContext) => void;
                hideEmptyPopup?: boolean;
                defaultVisible?: boolean;
                zIndex?: number;
            } | {
                props: {
                    delay?: number;
                    destroyOnClose?: boolean;
                    duration?: number;
                    placement?: "mouse" | import("../..").PopupPlacement;
                    showArrow?: boolean;
                    theme?: "default" | "primary" | "success" | "danger" | "warning" | "light";
                    default?: string | import("../..").TNode;
                    disabled?: boolean;
                    visible?: boolean;
                    modelValue?: boolean;
                    onScroll?: (context: {
                        e: WheelEvent;
                    }) => void;
                    attach?: import("../..").AttachNode;
                    content?: string | import("../..").TNode;
                    overlayClassName?: any;
                    overlayStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                    overlayInnerClassName?: any;
                    overlayInnerStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                    popperOptions?: object;
                    trigger?: "hover" | "click" | "focus" | "mousedown" | "context-menu";
                    triggerElement?: string | import("../..").TNode;
                    onOverlayClick?: (context: {
                        e: MouseEvent;
                    }) => void;
                    onScrollToBottom?: (context: {
                        e: WheelEvent;
                    }) => void;
                    onVisibleChange?: (visible: boolean, context: import("../..").PopupVisibleChangeContext) => void;
                    hideEmptyPopup?: boolean;
                    defaultVisible?: boolean;
                    zIndex?: number;
                };
                content: (h: typeof import("vue").h, props: import("..").BaseTableCellParams<TableRowData>) => import("../..").TNodeReturnValue;
            };
            ellipsisTitle?: boolean | ((h: typeof import("vue").h, props: import("..").BaseTableColParams<TableRowData>) => import("../..").TNodeReturnValue) | {
                delay?: number;
                destroyOnClose?: boolean;
                duration?: number;
                placement?: "mouse" | import("../..").PopupPlacement;
                showArrow?: boolean;
                theme?: "default" | "primary" | "success" | "danger" | "warning" | "light";
                default?: string | import("../..").TNode;
                disabled?: boolean;
                visible?: boolean;
                modelValue?: boolean;
                onScroll?: (context: {
                    e: WheelEvent;
                }) => void;
                attach?: import("../..").AttachNode;
                content?: string | import("../..").TNode;
                overlayClassName?: any;
                overlayStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                overlayInnerClassName?: any;
                overlayInnerStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                popperOptions?: object;
                trigger?: "hover" | "click" | "focus" | "mousedown" | "context-menu";
                triggerElement?: string | import("../..").TNode;
                onOverlayClick?: (context: {
                    e: MouseEvent;
                }) => void;
                onScrollToBottom?: (context: {
                    e: WheelEvent;
                }) => void;
                onVisibleChange?: (visible: boolean, context: import("../..").PopupVisibleChangeContext) => void;
                hideEmptyPopup?: boolean;
                defaultVisible?: boolean;
                zIndex?: number;
            } | {
                props: {
                    delay?: number;
                    destroyOnClose?: boolean;
                    duration?: number;
                    placement?: "mouse" | import("../..").PopupPlacement;
                    showArrow?: boolean;
                    theme?: "default" | "primary" | "success" | "danger" | "warning" | "light";
                    default?: string | import("../..").TNode;
                    disabled?: boolean;
                    visible?: boolean;
                    modelValue?: boolean;
                    onScroll?: (context: {
                        e: WheelEvent;
                    }) => void;
                    attach?: import("../..").AttachNode;
                    content?: string | import("../..").TNode;
                    overlayClassName?: any;
                    overlayStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                    overlayInnerClassName?: any;
                    overlayInnerStyle?: Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => Styles);
                    popperOptions?: object;
                    trigger?: "hover" | "click" | "focus" | "mousedown" | "context-menu";
                    triggerElement?: string | import("../..").TNode;
                    onOverlayClick?: (context: {
                        e: MouseEvent;
                    }) => void;
                    onScrollToBottom?: (context: {
                        e: WheelEvent;
                    }) => void;
                    onVisibleChange?: (visible: boolean, context: import("../..").PopupVisibleChangeContext) => void;
                    hideEmptyPopup?: boolean;
                    defaultVisible?: boolean;
                    zIndex?: number;
                };
                content: (h: typeof import("vue").h, props: import("..").BaseTableColParams<TableRowData>) => import("../..").TNodeReturnValue;
            };
            fixed?: "left" | "right";
            foot?: string | import("../..").TNode<{
                col: BaseTableCol;
                colIndex: number;
            }>;
            minWidth?: string | number;
            render?: (h: typeof import("vue").h, props: import("..").BaseTableRenderParams<TableRowData>) => import("../..").TNodeReturnValue;
            resizable?: boolean;
            resize?: {
                minWidth: number;
                maxWidth: number;
            };
            stopPropagation?: boolean;
            thClassName?: any;
            title?: string | import("../..").TNode<{
                col: BaseTableCol;
                colIndex: number;
            }>;
            width?: string | number;
        };
        index?: number;
        lastLeftFixedCol?: boolean;
        firstRightFixedCol?: boolean;
    }> & Omit<RowAndColFixedPosition, keyof Map<any, any>>)>;
    virtualScrollHeaderPos: Ref<{
        left: number;
        top: number;
    }, {
        left: number;
        top: number;
    } | {
        left: number;
        top: number;
    }>;
    scrollbarWidth: Ref<number, number>;
    setData: (dataSource: TableRowData[]) => void;
    refreshTable: () => void;
    setTableElmWidth: (width: number) => void;
    emitScrollEvent: (e: WheelEvent) => void;
    updateThWidthListHandler: () => void;
    updateColumnFixedShadow: (target: HTMLElement, extra?: {
        skipScrollLimit?: boolean;
    }) => void;
    setUseFixedTableElmRef: (val: HTMLTableElement) => void;
    getThWidthList: (type?: "default" | "calculate") => {
        [colKey: string]: number;
    };
    updateThWidthList: (trList: HTMLCollection | {
        [colKey: string]: number;
    }) => {
        [colKey: string]: number;
    };
    addTableResizeObserver: (tableElement: HTMLDivElement) => void;
    updateTableAfterColumnResize: () => void;
};
