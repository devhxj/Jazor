import { SetupContext } from 'vue';
import { SortInfo, TdPrimaryTableProps, PrimaryTableCol, TableRowData } from '../type';
export type SortMap = Record<string, SortInfo & {
    index: number;
}>;
export default function useSorter(props: TdPrimaryTableProps, { slots }: SetupContext): {
    renderSortIcon: ({ col }: {
        col: PrimaryTableCol<TableRowData>;
        colIndex: number;
    }) => import("vue/jsx-runtime").JSX.Element;
};
