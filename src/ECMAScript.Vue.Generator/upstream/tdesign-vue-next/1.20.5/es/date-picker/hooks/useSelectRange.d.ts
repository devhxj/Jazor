import { type Ref } from 'vue';
import type { TdDatePickerProps } from '../type';
export declare function useSelectRange(props: {
    range: TdDatePickerProps['range'];
    mode: TdDatePickerProps['mode'];
    year: Ref<number>;
    month?: Ref<number | undefined>;
}): {
    rangeBounds: import("vue").ComputedRef<{
        min: Date | null;
        max: Date | null;
    }>;
    monthHasAnyAllowed: (year: number, month: number) => boolean;
    yearHasAnyAllowed: (year: number) => boolean;
    decadeHasAnyAllowed: (decadeEndYear: number) => boolean;
    paginationDisabled: import("vue").ComputedRef<{
        prev: boolean;
        next: boolean;
    }>;
    canLoadMoreTop: (firstValue: number) => boolean;
    canLoadMoreBottom: (lastValue: number) => boolean;
};
