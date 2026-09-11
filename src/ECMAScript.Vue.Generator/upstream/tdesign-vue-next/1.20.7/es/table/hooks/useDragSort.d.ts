import { SetupContext, ComputedRef } from 'vue';
import { TdPrimaryTableProps } from '../type';
import { BaseTableColumns } from '../types';
export default function useDragSort(props: TdPrimaryTableProps, context: SetupContext, params: ComputedRef<{
    showElement: boolean;
}>): {
    innerPagination: import("vue").ShallowRef<import("../..").TdPaginationProps, import("../..").TdPaginationProps>;
    isRowDraggable: ComputedRef<boolean>;
    isRowHandlerDraggable: ComputedRef<boolean>;
    isColDraggable: ComputedRef<boolean>;
    setDragSortPrimaryTableRef: (primaryTableElement: any) => void;
    setDragSortColumns: (val: BaseTableColumns) => void;
};
