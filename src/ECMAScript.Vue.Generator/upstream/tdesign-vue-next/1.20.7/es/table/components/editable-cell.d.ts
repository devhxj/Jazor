import { TableRowData, PrimaryTableCol, PrimaryTableRowEditContext, PrimaryTableRowValidateContext, TdBaseTableProps } from '../type';
import { TableClassName } from '../hooks/useClassName';
import { AllValidateResult } from '../../form/type';
export interface OnEditableChangeContext<T> extends PrimaryTableRowEditContext<T> {
    isEdit: boolean;
    validateEdit: (trigger: 'self' | 'parent') => Promise<true | AllValidateResult[]>;
}
export interface EditableCellInstance {
    clearValidateCellData: () => void;
}
export interface EditableCellProps {
    rowKey: string;
    row: TableRowData;
    rowIndex: number;
    col: PrimaryTableCol<TableRowData>;
    colIndex: number;
    oldCell: PrimaryTableCol<TableRowData>['cell'];
    tableBaseClass?: TableClassName['tableBaseClass'];
    editable?: boolean;
    readonly?: boolean;
    errors?: AllValidateResult[];
    cellEmptyContent?: TdBaseTableProps['cellEmptyContent'];
    cellKey?: string;
    onCellInstanceChange?: (cellKey: string, instance: EditableCellInstance | null) => void;
    onChange?: (context: PrimaryTableRowEditContext<TableRowData>) => void;
    onValidate?: (context: PrimaryTableRowValidateContext<TableRowData>) => void;
    onRuleChange?: (context: PrimaryTableRowEditContext<TableRowData>) => void;
    onEditableChange?: (context: OnEditableChangeContext<TableRowData>) => void;
}
declare const _default: import("vue").DefineComponent<{
    rowKey: string;
    row: TableRowData;
    rowIndex: number;
    col: PrimaryTableCol<TableRowData>;
    colIndex: number;
    oldCell: PrimaryTableCol<TableRowData>["cell"];
    tableBaseClass?: TableClassName["tableBaseClass"];
    editable?: boolean;
    readonly?: boolean;
    errors?: AllValidateResult[];
    cellEmptyContent?: TdBaseTableProps["cellEmptyContent"];
    cellKey?: string;
    onCellInstanceChange?: (cellKey: string, instance: EditableCellInstance | null) => void;
    onChange?: (context: PrimaryTableRowEditContext<TableRowData>) => void;
    onValidate?: (context: PrimaryTableRowValidateContext<TableRowData>) => void;
    onRuleChange?: (context: PrimaryTableRowEditContext<TableRowData>) => void;
    onEditableChange?: (context: OnEditableChangeContext<TableRowData>) => void;
}, () => any, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, "update-edited-cell", import("vue").PublicProps, Readonly<{
    rowKey: string;
    row: TableRowData;
    rowIndex: number;
    col: PrimaryTableCol<TableRowData>;
    colIndex: number;
    oldCell: PrimaryTableCol<TableRowData>["cell"];
    tableBaseClass?: TableClassName["tableBaseClass"];
    editable?: boolean;
    readonly?: boolean;
    errors?: AllValidateResult[];
    cellEmptyContent?: TdBaseTableProps["cellEmptyContent"];
    cellKey?: string;
    onCellInstanceChange?: (cellKey: string, instance: EditableCellInstance | null) => void;
    onChange?: (context: PrimaryTableRowEditContext<TableRowData>) => void;
    onValidate?: (context: PrimaryTableRowValidateContext<TableRowData>) => void;
    onRuleChange?: (context: PrimaryTableRowEditContext<TableRowData>) => void;
    onEditableChange?: (context: OnEditableChangeContext<TableRowData>) => void;
}> & Readonly<{}>, {
    readonly: boolean;
    editable: boolean;
    errors: AllValidateResult[];
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
