import { TdDatePickerProps, TdDateRangePickerProps, TdDatePickerPanelProps, TdDateRangePickerPanelProps } from './type';
import './style';
export * from './type';
export type DatePickerProps = TdDatePickerProps;
export interface DatePickerPanelProps extends TdDatePickerPanelProps, Pick<TdDatePickerProps, 'modelValue'> {
}
export type DateRangePickerProps = TdDateRangePickerProps;
export interface DateRangePickerPanelProps extends TdDateRangePickerPanelProps, Pick<TdDateRangePickerProps, 'modelValue'> {
}
export declare const DatePicker: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        allowInput: BooleanConstructor;
        borderless: BooleanConstructor;
        cell: {
            type: import("vue").PropType<TdDatePickerProps["cell"]>;
        };
        clearable: BooleanConstructor;
        defaultTime: {
            type: StringConstructor;
            default: string;
        };
        disableDate: {
            type: import("vue").PropType<TdDatePickerProps["disableDate"]>;
        };
        disableTime: {
            type: import("vue").PropType<TdDatePickerProps["disableTime"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: TdDatePickerProps["disabled"];
        };
        enableTimePicker: BooleanConstructor;
        firstDayOfWeek: {
            type: NumberConstructor;
            validator(val: TdDatePickerProps["firstDayOfWeek"]): boolean;
        };
        format: {
            type: StringConstructor;
            default: any;
        };
        inputProps: {
            type: import("vue").PropType<TdDatePickerProps["inputProps"]>;
        };
        label: {
            type: import("vue").PropType<TdDatePickerProps["label"]>;
        };
        mode: {
            type: import("vue").PropType<TdDatePickerProps["mode"]>;
            default: TdDatePickerProps["mode"];
            validator(val: TdDatePickerProps["mode"]): boolean;
        };
        multiple: BooleanConstructor;
        needConfirm: {
            type: BooleanConstructor;
            default: boolean;
        };
        panelActiveDate: {
            type: import("vue").PropType<TdDatePickerProps["panelActiveDate"]>;
            default: TdDatePickerProps["panelActiveDate"];
        };
        placeholder: {
            type: import("vue").PropType<TdDatePickerProps["placeholder"]>;
            default: TdDatePickerProps["placeholder"];
        };
        popupProps: {
            type: import("vue").PropType<TdDatePickerProps["popupProps"]>;
        };
        prefixIcon: {
            type: import("vue").PropType<TdDatePickerProps["prefixIcon"]>;
        };
        presets: {
            type: import("vue").PropType<TdDatePickerProps["presets"]>;
        };
        presetsPlacement: {
            type: import("vue").PropType<TdDatePickerProps["presetsPlacement"]>;
            default: TdDatePickerProps["presetsPlacement"];
            validator(val: TdDatePickerProps["presetsPlacement"]): boolean;
        };
        range: {
            type: import("vue").PropType<TdDatePickerProps["range"]>;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        selectInputProps: {
            type: import("vue").PropType<TdDatePickerProps["selectInputProps"]>;
        };
        size: {
            type: import("vue").PropType<TdDatePickerProps["size"]>;
            default: TdDatePickerProps["size"];
            validator(val: TdDatePickerProps["size"]): boolean;
        };
        status: {
            type: import("vue").PropType<TdDatePickerProps["status"]>;
            default: TdDatePickerProps["status"];
            validator(val: TdDatePickerProps["status"]): boolean;
        };
        suffixIcon: {
            type: import("vue").PropType<TdDatePickerProps["suffixIcon"]>;
        };
        timePickerProps: {
            type: import("vue").PropType<TdDatePickerProps["timePickerProps"]>;
        };
        tips: {
            type: import("vue").PropType<TdDatePickerProps["tips"]>;
        };
        value: {
            type: import("vue").PropType<TdDatePickerProps["value"]>;
            default: TdDatePickerProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdDatePickerProps["value"]>;
            default: TdDatePickerProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdDatePickerProps["defaultValue"]>;
            default: TdDatePickerProps["defaultValue"];
        };
        valueDisplay: {
            type: import("vue").PropType<TdDatePickerProps["valueDisplay"]>;
        };
        valueType: {
            type: import("vue").PropType<TdDatePickerProps["valueType"]>;
            default: TdDatePickerProps["valueType"];
        };
        onBlur: import("vue").PropType<TdDatePickerProps["onBlur"]>;
        onChange: import("vue").PropType<TdDatePickerProps["onChange"]>;
        onClear: import("vue").PropType<TdDatePickerProps["onClear"]>;
        onConfirm: import("vue").PropType<TdDatePickerProps["onConfirm"]>;
        onFocus: import("vue").PropType<TdDatePickerProps["onFocus"]>;
        onMonthChange: import("vue").PropType<TdDatePickerProps["onMonthChange"]>;
        onPick: import("vue").PropType<TdDatePickerProps["onPick"]>;
        onPresetClick: import("vue").PropType<TdDatePickerProps["onPresetClick"]>;
        onYearChange: import("vue").PropType<TdDatePickerProps["onYearChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").DateValue | import("./type").DateMultipleValue;
        format: string;
        defaultTime: string;
        mode: "date" | "month" | "year" | "quarter" | "week";
        valueType: import("./type").DatePickerValueType;
        enableTimePicker: boolean;
        multiple: boolean;
        size: import("..").SizeEnum;
        status: "default" | "error" | "success" | "warning";
        disabled: boolean;
        defaultValue: import("./type").DateValue | import("./type").DateMultipleValue;
        placeholder: string;
        modelValue: import("./type").DateValue | import("./type").DateMultipleValue;
        readonly: boolean;
        borderless: boolean;
        clearable: boolean;
        allowInput: boolean;
        presetsPlacement: "left" | "right" | "top" | "bottom";
        needConfirm: boolean;
        panelActiveDate: import("./type").PanelActiveDate;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        allowInput: BooleanConstructor;
        borderless: BooleanConstructor;
        cell: {
            type: import("vue").PropType<TdDatePickerProps["cell"]>;
        };
        clearable: BooleanConstructor;
        defaultTime: {
            type: StringConstructor;
            default: string;
        };
        disableDate: {
            type: import("vue").PropType<TdDatePickerProps["disableDate"]>;
        };
        disableTime: {
            type: import("vue").PropType<TdDatePickerProps["disableTime"]>;
        };
        disabled: {
            type: BooleanConstructor;
            default: TdDatePickerProps["disabled"];
        };
        enableTimePicker: BooleanConstructor;
        firstDayOfWeek: {
            type: NumberConstructor;
            validator(val: TdDatePickerProps["firstDayOfWeek"]): boolean;
        };
        format: {
            type: StringConstructor;
            default: any;
        };
        inputProps: {
            type: import("vue").PropType<TdDatePickerProps["inputProps"]>;
        };
        label: {
            type: import("vue").PropType<TdDatePickerProps["label"]>;
        };
        mode: {
            type: import("vue").PropType<TdDatePickerProps["mode"]>;
            default: TdDatePickerProps["mode"];
            validator(val: TdDatePickerProps["mode"]): boolean;
        };
        multiple: BooleanConstructor;
        needConfirm: {
            type: BooleanConstructor;
            default: boolean;
        };
        panelActiveDate: {
            type: import("vue").PropType<TdDatePickerProps["panelActiveDate"]>;
            default: TdDatePickerProps["panelActiveDate"];
        };
        placeholder: {
            type: import("vue").PropType<TdDatePickerProps["placeholder"]>;
            default: TdDatePickerProps["placeholder"];
        };
        popupProps: {
            type: import("vue").PropType<TdDatePickerProps["popupProps"]>;
        };
        prefixIcon: {
            type: import("vue").PropType<TdDatePickerProps["prefixIcon"]>;
        };
        presets: {
            type: import("vue").PropType<TdDatePickerProps["presets"]>;
        };
        presetsPlacement: {
            type: import("vue").PropType<TdDatePickerProps["presetsPlacement"]>;
            default: TdDatePickerProps["presetsPlacement"];
            validator(val: TdDatePickerProps["presetsPlacement"]): boolean;
        };
        range: {
            type: import("vue").PropType<TdDatePickerProps["range"]>;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        selectInputProps: {
            type: import("vue").PropType<TdDatePickerProps["selectInputProps"]>;
        };
        size: {
            type: import("vue").PropType<TdDatePickerProps["size"]>;
            default: TdDatePickerProps["size"];
            validator(val: TdDatePickerProps["size"]): boolean;
        };
        status: {
            type: import("vue").PropType<TdDatePickerProps["status"]>;
            default: TdDatePickerProps["status"];
            validator(val: TdDatePickerProps["status"]): boolean;
        };
        suffixIcon: {
            type: import("vue").PropType<TdDatePickerProps["suffixIcon"]>;
        };
        timePickerProps: {
            type: import("vue").PropType<TdDatePickerProps["timePickerProps"]>;
        };
        tips: {
            type: import("vue").PropType<TdDatePickerProps["tips"]>;
        };
        value: {
            type: import("vue").PropType<TdDatePickerProps["value"]>;
            default: TdDatePickerProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdDatePickerProps["value"]>;
            default: TdDatePickerProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdDatePickerProps["defaultValue"]>;
            default: TdDatePickerProps["defaultValue"];
        };
        valueDisplay: {
            type: import("vue").PropType<TdDatePickerProps["valueDisplay"]>;
        };
        valueType: {
            type: import("vue").PropType<TdDatePickerProps["valueType"]>;
            default: TdDatePickerProps["valueType"];
        };
        onBlur: import("vue").PropType<TdDatePickerProps["onBlur"]>;
        onChange: import("vue").PropType<TdDatePickerProps["onChange"]>;
        onClear: import("vue").PropType<TdDatePickerProps["onClear"]>;
        onConfirm: import("vue").PropType<TdDatePickerProps["onConfirm"]>;
        onFocus: import("vue").PropType<TdDatePickerProps["onFocus"]>;
        onMonthChange: import("vue").PropType<TdDatePickerProps["onMonthChange"]>;
        onPick: import("vue").PropType<TdDatePickerProps["onPick"]>;
        onPresetClick: import("vue").PropType<TdDatePickerProps["onPresetClick"]>;
        onYearChange: import("vue").PropType<TdDatePickerProps["onYearChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").DateValue | import("./type").DateMultipleValue;
        format: string;
        defaultTime: string;
        mode: "date" | "month" | "year" | "quarter" | "week";
        valueType: import("./type").DatePickerValueType;
        enableTimePicker: boolean;
        multiple: boolean;
        size: import("..").SizeEnum;
        status: "default" | "error" | "success" | "warning";
        disabled: boolean;
        defaultValue: import("./type").DateValue | import("./type").DateMultipleValue;
        placeholder: string;
        modelValue: import("./type").DateValue | import("./type").DateMultipleValue;
        readonly: boolean;
        borderless: boolean;
        clearable: boolean;
        allowInput: boolean;
        presetsPlacement: "left" | "right" | "top" | "bottom";
        needConfirm: boolean;
        panelActiveDate: import("./type").PanelActiveDate;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    allowInput: BooleanConstructor;
    borderless: BooleanConstructor;
    cell: {
        type: import("vue").PropType<TdDatePickerProps["cell"]>;
    };
    clearable: BooleanConstructor;
    defaultTime: {
        type: StringConstructor;
        default: string;
    };
    disableDate: {
        type: import("vue").PropType<TdDatePickerProps["disableDate"]>;
    };
    disableTime: {
        type: import("vue").PropType<TdDatePickerProps["disableTime"]>;
    };
    disabled: {
        type: BooleanConstructor;
        default: TdDatePickerProps["disabled"];
    };
    enableTimePicker: BooleanConstructor;
    firstDayOfWeek: {
        type: NumberConstructor;
        validator(val: TdDatePickerProps["firstDayOfWeek"]): boolean;
    };
    format: {
        type: StringConstructor;
        default: any;
    };
    inputProps: {
        type: import("vue").PropType<TdDatePickerProps["inputProps"]>;
    };
    label: {
        type: import("vue").PropType<TdDatePickerProps["label"]>;
    };
    mode: {
        type: import("vue").PropType<TdDatePickerProps["mode"]>;
        default: TdDatePickerProps["mode"];
        validator(val: TdDatePickerProps["mode"]): boolean;
    };
    multiple: BooleanConstructor;
    needConfirm: {
        type: BooleanConstructor;
        default: boolean;
    };
    panelActiveDate: {
        type: import("vue").PropType<TdDatePickerProps["panelActiveDate"]>;
        default: TdDatePickerProps["panelActiveDate"];
    };
    placeholder: {
        type: import("vue").PropType<TdDatePickerProps["placeholder"]>;
        default: TdDatePickerProps["placeholder"];
    };
    popupProps: {
        type: import("vue").PropType<TdDatePickerProps["popupProps"]>;
    };
    prefixIcon: {
        type: import("vue").PropType<TdDatePickerProps["prefixIcon"]>;
    };
    presets: {
        type: import("vue").PropType<TdDatePickerProps["presets"]>;
    };
    presetsPlacement: {
        type: import("vue").PropType<TdDatePickerProps["presetsPlacement"]>;
        default: TdDatePickerProps["presetsPlacement"];
        validator(val: TdDatePickerProps["presetsPlacement"]): boolean;
    };
    range: {
        type: import("vue").PropType<TdDatePickerProps["range"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    selectInputProps: {
        type: import("vue").PropType<TdDatePickerProps["selectInputProps"]>;
    };
    size: {
        type: import("vue").PropType<TdDatePickerProps["size"]>;
        default: TdDatePickerProps["size"];
        validator(val: TdDatePickerProps["size"]): boolean;
    };
    status: {
        type: import("vue").PropType<TdDatePickerProps["status"]>;
        default: TdDatePickerProps["status"];
        validator(val: TdDatePickerProps["status"]): boolean;
    };
    suffixIcon: {
        type: import("vue").PropType<TdDatePickerProps["suffixIcon"]>;
    };
    timePickerProps: {
        type: import("vue").PropType<TdDatePickerProps["timePickerProps"]>;
    };
    tips: {
        type: import("vue").PropType<TdDatePickerProps["tips"]>;
    };
    value: {
        type: import("vue").PropType<TdDatePickerProps["value"]>;
        default: TdDatePickerProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdDatePickerProps["value"]>;
        default: TdDatePickerProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdDatePickerProps["defaultValue"]>;
        default: TdDatePickerProps["defaultValue"];
    };
    valueDisplay: {
        type: import("vue").PropType<TdDatePickerProps["valueDisplay"]>;
    };
    valueType: {
        type: import("vue").PropType<TdDatePickerProps["valueType"]>;
        default: TdDatePickerProps["valueType"];
    };
    onBlur: import("vue").PropType<TdDatePickerProps["onBlur"]>;
    onChange: import("vue").PropType<TdDatePickerProps["onChange"]>;
    onClear: import("vue").PropType<TdDatePickerProps["onClear"]>;
    onConfirm: import("vue").PropType<TdDatePickerProps["onConfirm"]>;
    onFocus: import("vue").PropType<TdDatePickerProps["onFocus"]>;
    onMonthChange: import("vue").PropType<TdDatePickerProps["onMonthChange"]>;
    onPick: import("vue").PropType<TdDatePickerProps["onPick"]>;
    onPresetClick: import("vue").PropType<TdDatePickerProps["onPresetClick"]>;
    onYearChange: import("vue").PropType<TdDatePickerProps["onYearChange"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").DateValue | import("./type").DateMultipleValue;
    format: string;
    defaultTime: string;
    mode: "date" | "month" | "year" | "quarter" | "week";
    valueType: import("./type").DatePickerValueType;
    enableTimePicker: boolean;
    multiple: boolean;
    size: import("..").SizeEnum;
    status: "default" | "error" | "success" | "warning";
    disabled: boolean;
    defaultValue: import("./type").DateValue | import("./type").DateMultipleValue;
    placeholder: string;
    modelValue: import("./type").DateValue | import("./type").DateMultipleValue;
    readonly: boolean;
    borderless: boolean;
    clearable: boolean;
    allowInput: boolean;
    presetsPlacement: "left" | "right" | "top" | "bottom";
    needConfirm: boolean;
    panelActiveDate: import("./type").PanelActiveDate;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const DatePickerPanel: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        defaultTime?: string;
        onCellClick?: (context: {
            date: Date;
            e: MouseEvent;
        }) => void;
        onChange?: (value: import("./type").DateValue, context: {
            dayjsValue?: import("dayjs").Dayjs;
            e?: MouseEvent;
            trigger?: import("./type").DatePickerTriggerSource;
        }) => void;
        onConfirm?: (context: {
            date: Date;
            e: MouseEvent;
        }) => void;
        onMonthChange?: (context: {
            month: number;
            date: Date;
            e?: MouseEvent;
            trigger: import("./type").DatePickerMonthChangeTrigger;
        }) => void;
        onPanelClick?: (context: {
            e: MouseEvent;
        }) => void;
        onPresetClick?: (context: {
            preset: import("./type").PresetDate;
            e: MouseEvent;
        }) => void;
        onTimeChange?: (context: {
            time: string;
            date: Date;
            trigger: import("./type").DatePickerTimeChangeTrigger;
            e?: MouseEvent;
        }) => void;
        onYearChange?: (context: {
            year: number;
            date: Date;
            trigger: import("./type").DatePickerYearChangeTrigger;
            e?: MouseEvent;
        }) => void;
        value?: import("./type").DateValue | import("./type").DateMultipleValue;
        format?: string;
        mode?: "year" | "quarter" | "month" | "week" | "date";
        enableTimePicker?: boolean;
        firstDayOfWeek?: number;
        disableDate?: import("./type").DisableDate;
        defaultValue?: import("./type").DateValue | import("./type").DateMultipleValue;
        disableTime?: (time: Date) => Partial<{
            hour: Array<number>;
            minute: Array<number>;
            second: Array<number>;
            millisecond: Array<number>;
        }>;
        presets?: import("..").TNode | import("./type").PresetDate;
        presetsPlacement?: "left" | "top" | "right" | "bottom";
        timePickerProps?: import("..").TimePickerProps;
        needConfirm?: boolean;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").DateValue | import("./type").DateMultipleValue;
        format: string;
        defaultTime: string;
        mode: "date" | "month" | "year" | "quarter" | "week";
        enableTimePicker: boolean;
        disabled: boolean;
        defaultValue: import("./type").DateValue | import("./type").DateMultipleValue;
        modelValue: import("./type").DateValue | import("./type").DateMultipleValue;
        presetsPlacement: "left" | "right" | "top" | "bottom";
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        defaultTime?: string;
        onCellClick?: (context: {
            date: Date;
            e: MouseEvent;
        }) => void;
        onChange?: (value: import("./type").DateValue, context: {
            dayjsValue?: import("dayjs").Dayjs;
            e?: MouseEvent;
            trigger?: import("./type").DatePickerTriggerSource;
        }) => void;
        onConfirm?: (context: {
            date: Date;
            e: MouseEvent;
        }) => void;
        onMonthChange?: (context: {
            month: number;
            date: Date;
            e?: MouseEvent;
            trigger: import("./type").DatePickerMonthChangeTrigger;
        }) => void;
        onPanelClick?: (context: {
            e: MouseEvent;
        }) => void;
        onPresetClick?: (context: {
            preset: import("./type").PresetDate;
            e: MouseEvent;
        }) => void;
        onTimeChange?: (context: {
            time: string;
            date: Date;
            trigger: import("./type").DatePickerTimeChangeTrigger;
            e?: MouseEvent;
        }) => void;
        onYearChange?: (context: {
            year: number;
            date: Date;
            trigger: import("./type").DatePickerYearChangeTrigger;
            e?: MouseEvent;
        }) => void;
        value?: import("./type").DateValue | import("./type").DateMultipleValue;
        format?: string;
        mode?: "year" | "quarter" | "month" | "week" | "date";
        enableTimePicker?: boolean;
        firstDayOfWeek?: number;
        disableDate?: import("./type").DisableDate;
        defaultValue?: import("./type").DateValue | import("./type").DateMultipleValue;
        disableTime?: (time: Date) => Partial<{
            hour: Array<number>;
            minute: Array<number>;
            second: Array<number>;
            millisecond: Array<number>;
        }>;
        presets?: import("..").TNode | import("./type").PresetDate;
        presetsPlacement?: "left" | "top" | "right" | "bottom";
        timePickerProps?: import("..").TimePickerProps;
        needConfirm?: boolean;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").DateValue | import("./type").DateMultipleValue;
        format: string;
        defaultTime: string;
        mode: "date" | "month" | "year" | "quarter" | "week";
        enableTimePicker: boolean;
        disabled: boolean;
        defaultValue: import("./type").DateValue | import("./type").DateMultipleValue;
        modelValue: import("./type").DateValue | import("./type").DateMultipleValue;
        presetsPlacement: "left" | "right" | "top" | "bottom";
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    defaultTime?: string;
    onCellClick?: (context: {
        date: Date;
        e: MouseEvent;
    }) => void;
    onChange?: (value: import("./type").DateValue, context: {
        dayjsValue?: import("dayjs").Dayjs;
        e?: MouseEvent;
        trigger?: import("./type").DatePickerTriggerSource;
    }) => void;
    onConfirm?: (context: {
        date: Date;
        e: MouseEvent;
    }) => void;
    onMonthChange?: (context: {
        month: number;
        date: Date;
        e?: MouseEvent;
        trigger: import("./type").DatePickerMonthChangeTrigger;
    }) => void;
    onPanelClick?: (context: {
        e: MouseEvent;
    }) => void;
    onPresetClick?: (context: {
        preset: import("./type").PresetDate;
        e: MouseEvent;
    }) => void;
    onTimeChange?: (context: {
        time: string;
        date: Date;
        trigger: import("./type").DatePickerTimeChangeTrigger;
        e?: MouseEvent;
    }) => void;
    onYearChange?: (context: {
        year: number;
        date: Date;
        trigger: import("./type").DatePickerYearChangeTrigger;
        e?: MouseEvent;
    }) => void;
    value?: import("./type").DateValue | import("./type").DateMultipleValue;
    format?: string;
    mode?: "year" | "quarter" | "month" | "week" | "date";
    enableTimePicker?: boolean;
    firstDayOfWeek?: number;
    disableDate?: import("./type").DisableDate;
    defaultValue?: import("./type").DateValue | import("./type").DateMultipleValue;
    disableTime?: (time: Date) => Partial<{
        hour: Array<number>;
        minute: Array<number>;
        second: Array<number>;
        millisecond: Array<number>;
    }>;
    presets?: import("..").TNode | import("./type").PresetDate;
    presetsPlacement?: "left" | "top" | "right" | "bottom";
    timePickerProps?: import("..").TimePickerProps;
    needConfirm?: boolean;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").DateValue | import("./type").DateMultipleValue;
    format: string;
    defaultTime: string;
    mode: "date" | "month" | "year" | "quarter" | "week";
    enableTimePicker: boolean;
    disabled: boolean;
    defaultValue: import("./type").DateValue | import("./type").DateMultipleValue;
    modelValue: import("./type").DateValue | import("./type").DateMultipleValue;
    presetsPlacement: "left" | "right" | "top" | "bottom";
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const DateRangePicker: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        allowInput: BooleanConstructor;
        borderless: BooleanConstructor;
        cancelRangeSelectLimit: BooleanConstructor;
        cell: {
            type: import("vue").PropType<TdDateRangePickerProps["cell"]>;
        };
        clearable: BooleanConstructor;
        defaultTime: {
            type: import("vue").PropType<TdDateRangePickerProps["defaultTime"]>;
            default: () => TdDateRangePickerProps["defaultTime"];
        };
        disableDate: {
            type: import("vue").PropType<TdDateRangePickerProps["disableDate"]>;
        };
        disableTime: {
            type: import("vue").PropType<TdDateRangePickerProps["disableTime"]>;
        };
        disabled: {
            type: import("vue").PropType<TdDateRangePickerProps["disabled"]>;
            default: any;
        };
        enableTimePicker: BooleanConstructor;
        firstDayOfWeek: {
            type: NumberConstructor;
            validator(val: TdDateRangePickerProps["firstDayOfWeek"]): boolean;
        };
        format: {
            type: StringConstructor;
            default: string;
        };
        label: {
            type: import("vue").PropType<TdDateRangePickerProps["label"]>;
        };
        mode: {
            type: import("vue").PropType<TdDateRangePickerProps["mode"]>;
            default: TdDateRangePickerProps["mode"];
            validator(val: TdDateRangePickerProps["mode"]): boolean;
        };
        needConfirm: {
            type: BooleanConstructor;
            default: boolean;
        };
        panelActiveDate: {
            type: import("vue").PropType<TdDateRangePickerProps["panelActiveDate"]>;
            default: TdDateRangePickerProps["panelActiveDate"];
        };
        panelPreselection: {
            type: BooleanConstructor;
            default: boolean;
        };
        placeholder: {
            type: import("vue").PropType<TdDateRangePickerProps["placeholder"]>;
        };
        popupProps: {
            type: import("vue").PropType<TdDateRangePickerProps["popupProps"]>;
        };
        prefixIcon: {
            type: import("vue").PropType<TdDateRangePickerProps["prefixIcon"]>;
        };
        presets: {
            type: import("vue").PropType<TdDateRangePickerProps["presets"]>;
        };
        presetsPlacement: {
            type: import("vue").PropType<TdDateRangePickerProps["presetsPlacement"]>;
            default: TdDateRangePickerProps["presetsPlacement"];
            validator(val: TdDateRangePickerProps["presetsPlacement"]): boolean;
        };
        range: {
            type: import("vue").PropType<TdDateRangePickerProps["range"]>;
        };
        rangeInputProps: {
            type: import("vue").PropType<TdDateRangePickerProps["rangeInputProps"]>;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        separator: {
            type: StringConstructor;
            default: string;
        };
        size: {
            type: import("vue").PropType<TdDateRangePickerProps["size"]>;
            default: TdDateRangePickerProps["size"];
            validator(val: TdDateRangePickerProps["size"]): boolean;
        };
        status: {
            type: import("vue").PropType<TdDateRangePickerProps["status"]>;
            default: TdDateRangePickerProps["status"];
            validator(val: TdDateRangePickerProps["status"]): boolean;
        };
        suffixIcon: {
            type: import("vue").PropType<TdDateRangePickerProps["suffixIcon"]>;
        };
        timePickerProps: {
            type: import("vue").PropType<TdDateRangePickerProps["timePickerProps"]>;
        };
        tips: {
            type: import("vue").PropType<TdDateRangePickerProps["tips"]>;
        };
        value: {
            type: import("vue").PropType<TdDateRangePickerProps["value"]>;
            default: TdDateRangePickerProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdDateRangePickerProps["value"]>;
            default: TdDateRangePickerProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdDateRangePickerProps["defaultValue"]>;
            default: () => TdDateRangePickerProps["defaultValue"];
        };
        valueType: {
            type: import("vue").PropType<TdDateRangePickerProps["valueType"]>;
            validator(val: TdDateRangePickerProps["valueType"]): boolean;
        };
        onBlur: import("vue").PropType<TdDateRangePickerProps["onBlur"]>;
        onChange: import("vue").PropType<TdDateRangePickerProps["onChange"]>;
        onConfirm: import("vue").PropType<TdDateRangePickerProps["onConfirm"]>;
        onFocus: import("vue").PropType<TdDateRangePickerProps["onFocus"]>;
        onInput: import("vue").PropType<TdDateRangePickerProps["onInput"]>;
        onMonthChange: import("vue").PropType<TdDateRangePickerProps["onMonthChange"]>;
        onPick: import("vue").PropType<TdDateRangePickerProps["onPick"]>;
        onPresetClick: import("vue").PropType<TdDateRangePickerProps["onPresetClick"]>;
        onYearChange: import("vue").PropType<TdDateRangePickerProps["onYearChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").DateRangeValue;
        format: string;
        defaultTime: string[];
        mode: "date" | "month" | "year" | "quarter" | "week";
        enableTimePicker: boolean;
        cancelRangeSelectLimit: boolean;
        size: import("..").SizeEnum;
        status: "default" | "error" | "success" | "warning";
        disabled: boolean | boolean[];
        defaultValue: import("./type").DateRangeValue;
        modelValue: import("./type").DateRangeValue;
        readonly: boolean;
        separator: string;
        borderless: boolean;
        clearable: boolean;
        allowInput: boolean;
        presetsPlacement: "left" | "right" | "top" | "bottom";
        needConfirm: boolean;
        panelActiveDate: import("./type").PanelActiveDate | [import("./type").PanelActiveDate, import("./type").PanelActiveDate];
        panelPreselection: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        allowInput: BooleanConstructor;
        borderless: BooleanConstructor;
        cancelRangeSelectLimit: BooleanConstructor;
        cell: {
            type: import("vue").PropType<TdDateRangePickerProps["cell"]>;
        };
        clearable: BooleanConstructor;
        defaultTime: {
            type: import("vue").PropType<TdDateRangePickerProps["defaultTime"]>;
            default: () => TdDateRangePickerProps["defaultTime"];
        };
        disableDate: {
            type: import("vue").PropType<TdDateRangePickerProps["disableDate"]>;
        };
        disableTime: {
            type: import("vue").PropType<TdDateRangePickerProps["disableTime"]>;
        };
        disabled: {
            type: import("vue").PropType<TdDateRangePickerProps["disabled"]>;
            default: any;
        };
        enableTimePicker: BooleanConstructor;
        firstDayOfWeek: {
            type: NumberConstructor;
            validator(val: TdDateRangePickerProps["firstDayOfWeek"]): boolean;
        };
        format: {
            type: StringConstructor;
            default: string;
        };
        label: {
            type: import("vue").PropType<TdDateRangePickerProps["label"]>;
        };
        mode: {
            type: import("vue").PropType<TdDateRangePickerProps["mode"]>;
            default: TdDateRangePickerProps["mode"];
            validator(val: TdDateRangePickerProps["mode"]): boolean;
        };
        needConfirm: {
            type: BooleanConstructor;
            default: boolean;
        };
        panelActiveDate: {
            type: import("vue").PropType<TdDateRangePickerProps["panelActiveDate"]>;
            default: TdDateRangePickerProps["panelActiveDate"];
        };
        panelPreselection: {
            type: BooleanConstructor;
            default: boolean;
        };
        placeholder: {
            type: import("vue").PropType<TdDateRangePickerProps["placeholder"]>;
        };
        popupProps: {
            type: import("vue").PropType<TdDateRangePickerProps["popupProps"]>;
        };
        prefixIcon: {
            type: import("vue").PropType<TdDateRangePickerProps["prefixIcon"]>;
        };
        presets: {
            type: import("vue").PropType<TdDateRangePickerProps["presets"]>;
        };
        presetsPlacement: {
            type: import("vue").PropType<TdDateRangePickerProps["presetsPlacement"]>;
            default: TdDateRangePickerProps["presetsPlacement"];
            validator(val: TdDateRangePickerProps["presetsPlacement"]): boolean;
        };
        range: {
            type: import("vue").PropType<TdDateRangePickerProps["range"]>;
        };
        rangeInputProps: {
            type: import("vue").PropType<TdDateRangePickerProps["rangeInputProps"]>;
        };
        readonly: {
            type: BooleanConstructor;
            default: any;
        };
        separator: {
            type: StringConstructor;
            default: string;
        };
        size: {
            type: import("vue").PropType<TdDateRangePickerProps["size"]>;
            default: TdDateRangePickerProps["size"];
            validator(val: TdDateRangePickerProps["size"]): boolean;
        };
        status: {
            type: import("vue").PropType<TdDateRangePickerProps["status"]>;
            default: TdDateRangePickerProps["status"];
            validator(val: TdDateRangePickerProps["status"]): boolean;
        };
        suffixIcon: {
            type: import("vue").PropType<TdDateRangePickerProps["suffixIcon"]>;
        };
        timePickerProps: {
            type: import("vue").PropType<TdDateRangePickerProps["timePickerProps"]>;
        };
        tips: {
            type: import("vue").PropType<TdDateRangePickerProps["tips"]>;
        };
        value: {
            type: import("vue").PropType<TdDateRangePickerProps["value"]>;
            default: TdDateRangePickerProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdDateRangePickerProps["value"]>;
            default: TdDateRangePickerProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdDateRangePickerProps["defaultValue"]>;
            default: () => TdDateRangePickerProps["defaultValue"];
        };
        valueType: {
            type: import("vue").PropType<TdDateRangePickerProps["valueType"]>;
            validator(val: TdDateRangePickerProps["valueType"]): boolean;
        };
        onBlur: import("vue").PropType<TdDateRangePickerProps["onBlur"]>;
        onChange: import("vue").PropType<TdDateRangePickerProps["onChange"]>;
        onConfirm: import("vue").PropType<TdDateRangePickerProps["onConfirm"]>;
        onFocus: import("vue").PropType<TdDateRangePickerProps["onFocus"]>;
        onInput: import("vue").PropType<TdDateRangePickerProps["onInput"]>;
        onMonthChange: import("vue").PropType<TdDateRangePickerProps["onMonthChange"]>;
        onPick: import("vue").PropType<TdDateRangePickerProps["onPick"]>;
        onPresetClick: import("vue").PropType<TdDateRangePickerProps["onPresetClick"]>;
        onYearChange: import("vue").PropType<TdDateRangePickerProps["onYearChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").DateRangeValue;
        format: string;
        defaultTime: string[];
        mode: "date" | "month" | "year" | "quarter" | "week";
        enableTimePicker: boolean;
        cancelRangeSelectLimit: boolean;
        size: import("..").SizeEnum;
        status: "default" | "error" | "success" | "warning";
        disabled: boolean | boolean[];
        defaultValue: import("./type").DateRangeValue;
        modelValue: import("./type").DateRangeValue;
        readonly: boolean;
        separator: string;
        borderless: boolean;
        clearable: boolean;
        allowInput: boolean;
        presetsPlacement: "left" | "right" | "top" | "bottom";
        needConfirm: boolean;
        panelActiveDate: import("./type").PanelActiveDate | [import("./type").PanelActiveDate, import("./type").PanelActiveDate];
        panelPreselection: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    allowInput: BooleanConstructor;
    borderless: BooleanConstructor;
    cancelRangeSelectLimit: BooleanConstructor;
    cell: {
        type: import("vue").PropType<TdDateRangePickerProps["cell"]>;
    };
    clearable: BooleanConstructor;
    defaultTime: {
        type: import("vue").PropType<TdDateRangePickerProps["defaultTime"]>;
        default: () => TdDateRangePickerProps["defaultTime"];
    };
    disableDate: {
        type: import("vue").PropType<TdDateRangePickerProps["disableDate"]>;
    };
    disableTime: {
        type: import("vue").PropType<TdDateRangePickerProps["disableTime"]>;
    };
    disabled: {
        type: import("vue").PropType<TdDateRangePickerProps["disabled"]>;
        default: any;
    };
    enableTimePicker: BooleanConstructor;
    firstDayOfWeek: {
        type: NumberConstructor;
        validator(val: TdDateRangePickerProps["firstDayOfWeek"]): boolean;
    };
    format: {
        type: StringConstructor;
        default: string;
    };
    label: {
        type: import("vue").PropType<TdDateRangePickerProps["label"]>;
    };
    mode: {
        type: import("vue").PropType<TdDateRangePickerProps["mode"]>;
        default: TdDateRangePickerProps["mode"];
        validator(val: TdDateRangePickerProps["mode"]): boolean;
    };
    needConfirm: {
        type: BooleanConstructor;
        default: boolean;
    };
    panelActiveDate: {
        type: import("vue").PropType<TdDateRangePickerProps["panelActiveDate"]>;
        default: TdDateRangePickerProps["panelActiveDate"];
    };
    panelPreselection: {
        type: BooleanConstructor;
        default: boolean;
    };
    placeholder: {
        type: import("vue").PropType<TdDateRangePickerProps["placeholder"]>;
    };
    popupProps: {
        type: import("vue").PropType<TdDateRangePickerProps["popupProps"]>;
    };
    prefixIcon: {
        type: import("vue").PropType<TdDateRangePickerProps["prefixIcon"]>;
    };
    presets: {
        type: import("vue").PropType<TdDateRangePickerProps["presets"]>;
    };
    presetsPlacement: {
        type: import("vue").PropType<TdDateRangePickerProps["presetsPlacement"]>;
        default: TdDateRangePickerProps["presetsPlacement"];
        validator(val: TdDateRangePickerProps["presetsPlacement"]): boolean;
    };
    range: {
        type: import("vue").PropType<TdDateRangePickerProps["range"]>;
    };
    rangeInputProps: {
        type: import("vue").PropType<TdDateRangePickerProps["rangeInputProps"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    separator: {
        type: StringConstructor;
        default: string;
    };
    size: {
        type: import("vue").PropType<TdDateRangePickerProps["size"]>;
        default: TdDateRangePickerProps["size"];
        validator(val: TdDateRangePickerProps["size"]): boolean;
    };
    status: {
        type: import("vue").PropType<TdDateRangePickerProps["status"]>;
        default: TdDateRangePickerProps["status"];
        validator(val: TdDateRangePickerProps["status"]): boolean;
    };
    suffixIcon: {
        type: import("vue").PropType<TdDateRangePickerProps["suffixIcon"]>;
    };
    timePickerProps: {
        type: import("vue").PropType<TdDateRangePickerProps["timePickerProps"]>;
    };
    tips: {
        type: import("vue").PropType<TdDateRangePickerProps["tips"]>;
    };
    value: {
        type: import("vue").PropType<TdDateRangePickerProps["value"]>;
        default: TdDateRangePickerProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdDateRangePickerProps["value"]>;
        default: TdDateRangePickerProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdDateRangePickerProps["defaultValue"]>;
        default: () => TdDateRangePickerProps["defaultValue"];
    };
    valueType: {
        type: import("vue").PropType<TdDateRangePickerProps["valueType"]>;
        validator(val: TdDateRangePickerProps["valueType"]): boolean;
    };
    onBlur: import("vue").PropType<TdDateRangePickerProps["onBlur"]>;
    onChange: import("vue").PropType<TdDateRangePickerProps["onChange"]>;
    onConfirm: import("vue").PropType<TdDateRangePickerProps["onConfirm"]>;
    onFocus: import("vue").PropType<TdDateRangePickerProps["onFocus"]>;
    onInput: import("vue").PropType<TdDateRangePickerProps["onInput"]>;
    onMonthChange: import("vue").PropType<TdDateRangePickerProps["onMonthChange"]>;
    onPick: import("vue").PropType<TdDateRangePickerProps["onPick"]>;
    onPresetClick: import("vue").PropType<TdDateRangePickerProps["onPresetClick"]>;
    onYearChange: import("vue").PropType<TdDateRangePickerProps["onYearChange"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").DateRangeValue;
    format: string;
    defaultTime: string[];
    mode: "date" | "month" | "year" | "quarter" | "week";
    enableTimePicker: boolean;
    cancelRangeSelectLimit: boolean;
    size: import("..").SizeEnum;
    status: "default" | "error" | "success" | "warning";
    disabled: boolean | boolean[];
    defaultValue: import("./type").DateRangeValue;
    modelValue: import("./type").DateRangeValue;
    readonly: boolean;
    separator: string;
    borderless: boolean;
    clearable: boolean;
    allowInput: boolean;
    presetsPlacement: "left" | "right" | "top" | "bottom";
    needConfirm: boolean;
    panelActiveDate: import("./type").PanelActiveDate | [import("./type").PanelActiveDate, import("./type").PanelActiveDate];
    panelPreselection: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const DateRangePickerPanel: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        defaultTime?: string[];
        onCellClick?: (context: {
            date: Date[];
            partial: import("./type").DateRangePickerPartial;
            e: MouseEvent;
        }) => void;
        onChange?: (value: import("./type").DateRangeValue, context: {
            dayjsValue?: import("dayjs").Dayjs[];
            partial: import("./type").DateRangePickerPartial;
            e?: MouseEvent;
            trigger?: import("./type").DatePickerTriggerSource;
        }) => void;
        onConfirm?: (context: {
            date: Date[];
            e: MouseEvent;
        }) => void;
        onMonthChange?: (context: {
            month: number;
            date: Date[];
            partial: import("./type").DateRangePickerPartial;
            e?: MouseEvent;
            trigger: import("./type").DatePickerMonthChangeTrigger;
        }) => void;
        onPanelClick?: (context: {
            e: MouseEvent;
        }) => void;
        onPresetClick?: (context: {
            preset: import("./type").PresetDate;
            e: MouseEvent;
        }) => void;
        onTimeChange?: (context: {
            time: string;
            date: Date[];
            partial: import("./type").DateRangePickerPartial;
            trigger: import("./type").DatePickerTimeChangeTrigger;
            e?: MouseEvent;
        }) => void;
        onYearChange?: (context: {
            year: number;
            date: Date[];
            partial: import("./type").DateRangePickerPartial;
            trigger: import("./type").DatePickerYearChangeTrigger;
            e?: MouseEvent;
        }) => void;
        value?: import("./type").DateRangeValue;
        format?: string;
        mode?: "year" | "quarter" | "month" | "week" | "date";
        enableTimePicker?: boolean;
        firstDayOfWeek?: number;
        disableDate?: import("./type").DisableRangeDate;
        defaultValue?: import("./type").DateRangeValue;
        range?: import("./type").PickerDateRange | import("./type").PickerDateRange[];
        presets?: import("..").TNode | import("./type").PresetRange;
        presetsPlacement?: "left" | "top" | "right" | "bottom";
        timePickerProps?: import("..").TimePickerProps;
        panelPreselection?: boolean;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").DateRangeValue;
        format: string;
        defaultTime: string[];
        mode: "date" | "month" | "year" | "quarter" | "week";
        enableTimePicker: boolean;
        disabled: boolean | boolean[];
        defaultValue: import("./type").DateRangeValue;
        modelValue: import("./type").DateRangeValue;
        presetsPlacement: "left" | "right" | "top" | "bottom";
        panelPreselection: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        defaultTime?: string[];
        onCellClick?: (context: {
            date: Date[];
            partial: import("./type").DateRangePickerPartial;
            e: MouseEvent;
        }) => void;
        onChange?: (value: import("./type").DateRangeValue, context: {
            dayjsValue?: import("dayjs").Dayjs[];
            partial: import("./type").DateRangePickerPartial;
            e?: MouseEvent;
            trigger?: import("./type").DatePickerTriggerSource;
        }) => void;
        onConfirm?: (context: {
            date: Date[];
            e: MouseEvent;
        }) => void;
        onMonthChange?: (context: {
            month: number;
            date: Date[];
            partial: import("./type").DateRangePickerPartial;
            e?: MouseEvent;
            trigger: import("./type").DatePickerMonthChangeTrigger;
        }) => void;
        onPanelClick?: (context: {
            e: MouseEvent;
        }) => void;
        onPresetClick?: (context: {
            preset: import("./type").PresetDate;
            e: MouseEvent;
        }) => void;
        onTimeChange?: (context: {
            time: string;
            date: Date[];
            partial: import("./type").DateRangePickerPartial;
            trigger: import("./type").DatePickerTimeChangeTrigger;
            e?: MouseEvent;
        }) => void;
        onYearChange?: (context: {
            year: number;
            date: Date[];
            partial: import("./type").DateRangePickerPartial;
            trigger: import("./type").DatePickerYearChangeTrigger;
            e?: MouseEvent;
        }) => void;
        value?: import("./type").DateRangeValue;
        format?: string;
        mode?: "year" | "quarter" | "month" | "week" | "date";
        enableTimePicker?: boolean;
        firstDayOfWeek?: number;
        disableDate?: import("./type").DisableRangeDate;
        defaultValue?: import("./type").DateRangeValue;
        range?: import("./type").PickerDateRange | import("./type").PickerDateRange[];
        presets?: import("..").TNode | import("./type").PresetRange;
        presetsPlacement?: "left" | "top" | "right" | "bottom";
        timePickerProps?: import("..").TimePickerProps;
        panelPreselection?: boolean;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").DateRangeValue;
        format: string;
        defaultTime: string[];
        mode: "date" | "month" | "year" | "quarter" | "week";
        enableTimePicker: boolean;
        disabled: boolean | boolean[];
        defaultValue: import("./type").DateRangeValue;
        modelValue: import("./type").DateRangeValue;
        presetsPlacement: "left" | "right" | "top" | "bottom";
        panelPreselection: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    defaultTime?: string[];
    onCellClick?: (context: {
        date: Date[];
        partial: import("./type").DateRangePickerPartial;
        e: MouseEvent;
    }) => void;
    onChange?: (value: import("./type").DateRangeValue, context: {
        dayjsValue?: import("dayjs").Dayjs[];
        partial: import("./type").DateRangePickerPartial;
        e?: MouseEvent;
        trigger?: import("./type").DatePickerTriggerSource;
    }) => void;
    onConfirm?: (context: {
        date: Date[];
        e: MouseEvent;
    }) => void;
    onMonthChange?: (context: {
        month: number;
        date: Date[];
        partial: import("./type").DateRangePickerPartial;
        e?: MouseEvent;
        trigger: import("./type").DatePickerMonthChangeTrigger;
    }) => void;
    onPanelClick?: (context: {
        e: MouseEvent;
    }) => void;
    onPresetClick?: (context: {
        preset: import("./type").PresetDate;
        e: MouseEvent;
    }) => void;
    onTimeChange?: (context: {
        time: string;
        date: Date[];
        partial: import("./type").DateRangePickerPartial;
        trigger: import("./type").DatePickerTimeChangeTrigger;
        e?: MouseEvent;
    }) => void;
    onYearChange?: (context: {
        year: number;
        date: Date[];
        partial: import("./type").DateRangePickerPartial;
        trigger: import("./type").DatePickerYearChangeTrigger;
        e?: MouseEvent;
    }) => void;
    value?: import("./type").DateRangeValue;
    format?: string;
    mode?: "year" | "quarter" | "month" | "week" | "date";
    enableTimePicker?: boolean;
    firstDayOfWeek?: number;
    disableDate?: import("./type").DisableRangeDate;
    defaultValue?: import("./type").DateRangeValue;
    range?: import("./type").PickerDateRange | import("./type").PickerDateRange[];
    presets?: import("..").TNode | import("./type").PresetRange;
    presetsPlacement?: "left" | "top" | "right" | "bottom";
    timePickerProps?: import("..").TimePickerProps;
    panelPreselection?: boolean;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").DateRangeValue;
    format: string;
    defaultTime: string[];
    mode: "date" | "month" | "year" | "quarter" | "week";
    enableTimePicker: boolean;
    disabled: boolean | boolean[];
    defaultValue: import("./type").DateRangeValue;
    modelValue: import("./type").DateRangeValue;
    presetsPlacement: "left" | "right" | "top" | "bottom";
    panelPreselection: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default DatePicker;
