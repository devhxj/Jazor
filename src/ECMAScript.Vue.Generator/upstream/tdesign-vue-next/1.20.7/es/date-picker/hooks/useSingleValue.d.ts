import { TdDatePickerProps, DateMultipleValue, DateValue } from '../type';
export declare function useSingleValue(props: TdDatePickerProps): {
    year: import("vue").Ref<number, number>;
    month: import("vue").Ref<number, number>;
    value: import("vue").Ref<DateValue | DateMultipleValue, DateValue | DateMultipleValue>;
    time: import("vue").Ref<any, any>;
    cacheValue: import("vue").Ref<string | number | string[] | Date | number[] | Date[], string | number | string[] | Date | number[] | Date[]>;
    onChange: import("@tdesign/shared-hooks").ChangeHandler<DateValue | DateMultipleValue, [context: {
        dayjsValue?: import("dayjs").Dayjs;
        trigger?: import("..").DatePickerTriggerSource;
    }]>;
};
