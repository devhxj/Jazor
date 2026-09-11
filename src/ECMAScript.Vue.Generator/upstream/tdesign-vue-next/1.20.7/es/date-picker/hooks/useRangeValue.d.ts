import { TdDateRangePickerProps } from '../type';
export declare function useRangeValue(props: TdDateRangePickerProps): {
    year: import("vue").Ref<number[], number[]>;
    month: import("vue").Ref<number[], number[]>;
    value: import("vue").ComputedRef<import("..").DateRangeValue>;
    time: import("vue").Ref<string[], string[]>;
    isFirstValueSelected: import("vue").Ref<boolean, boolean>;
    cacheValue: import("vue").Ref<string | number | string[] | Date | number[] | Date[], string | number | string[] | Date | number[] | Date[]>;
    onRawChange: import("@tdesign/shared-hooks").ChangeHandler<import("..").DateRangeValue, [context: {
        dayjsValue?: import("dayjs").Dayjs[];
        trigger?: import("..").DatePickerTriggerSource;
    }]>;
};
