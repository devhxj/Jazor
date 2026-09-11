import { Ref } from 'vue';
import { BaseTableProps } from '../types';
export declare function useHoverKeyboardEvent(props: BaseTableProps, tableRef: Ref<HTMLDivElement>): {
    hoverRow: Ref<string | number, string | number>;
    needKeyboardRowHover: import("vue").ComputedRef<boolean>;
    clearHoverRow: () => void;
    addRowHoverKeyboardListener: () => void;
    removeRowHoverKeyboardListener: () => void;
    tableRefTabIndex: Ref<number, number>;
};
export default useHoverKeyboardEvent;
