import { Ref } from 'vue';
import { TdPaginationProps } from '../type';
export declare function useMoreAction(props: TdPaginationProps, pageCount: Ref<number>, innerCurrent: Ref<number>): {
    prevMore: Ref<boolean, boolean>;
    nextMore: Ref<boolean, boolean>;
    curPageLeftCount: import("vue").ComputedRef<number>;
    curPageRightCount: import("vue").ComputedRef<number>;
    isPrevMoreShow: import("vue").ComputedRef<boolean>;
    isNextMoreShow: import("vue").ComputedRef<boolean>;
};
