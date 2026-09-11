import { TdDateRangePickerProps, DateRangePickerPartial } from '../type';
export declare const PARTIAL_MAP: Record<'first' | 'second', DateRangePickerPartial>;
export declare function useRange(props: TdDateRangePickerProps): {
    year: import("vue").Ref<number[], number[]>;
    month: import("vue").Ref<number[], number[]>;
    value: import("vue").ComputedRef<import("..").DateRangeValue>;
    time: import("vue").Ref<string[], string[]>;
    inputValue: import("vue").Ref<string | number | string[] | Date | number[] | Date[], string | number | string[] | Date | number[] | Date[]>;
    popupVisible: import("vue").Ref<boolean, boolean>;
    rangeInputProps: import("vue").ComputedRef<{
        size: import("../..").SizeEnum;
        ref: import("vue").Ref<any, any>;
        borderless: boolean;
        clearable: boolean;
        prefixIcon: () => any;
        readonly: boolean;
        separator: string;
        placeholder: string | string[];
        suffixIcon: () => any;
        class: {
            [x: string]: boolean;
        };
        onClick: ({ position }: any) => void;
        onClear: (context: {
            e: MouseEvent;
        } | MouseEvent) => void;
        onBlur: (newVal: string[], { e, position }: {
            e: MouseEvent;
            position: "first" | "second";
        }) => void;
        onFocus: (newVal: string[], { e, position }: {
            e: MouseEvent;
            position: "first" | "second";
        }) => void;
        onChange: (newVal: string[], { e, position }: {
            e: MouseEvent;
            position: "first" | "second";
        }) => void;
        onEnter: (newVal: string[]) => void;
        activeIndex?: number;
        disabled?: boolean | Array<boolean>;
        format?: import("../..").InputFormatType | Array<import("../..").InputFormatType>;
        inputProps?: import("../..").InputProps | Array<import("../..").InputProps>;
        label?: string | import("../..").TNode;
        showClearIconOnEmpty?: boolean;
        status?: "default" | "success" | "warning" | "error";
        suffix?: string | import("../..").TNode;
        tips?: string | import("../..").TNode;
        value?: import("../..").RangeInputValue;
        defaultValue?: import("../..").RangeInputValue;
        modelValue?: import("../..").RangeInputValue;
        onMouseenter?: (context: {
            e: MouseEvent;
        }) => void;
        onMouseleave?: (context: {
            e: MouseEvent;
        }) => void;
    }>;
    popupProps: import("vue").ComputedRef<{
        overlayInnerStyle: import("../..").Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => import("../..").Styles);
        overlayClassName: import("../..").ClassName[];
        onVisibleChange: (visible: boolean, context: any) => void;
        default?: string | import("../..").TNode;
        disabled?: boolean;
        visible?: boolean;
        modelValue?: boolean;
        onScroll?: (context: {
            e: WheelEvent;
        }) => void;
        attach?: import("../..").AttachNode;
        content?: string | import("../..").TNode;
        overlayStyle?: import("../..").Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => import("../..").Styles);
        delay?: number | Array<number>;
        overlayInnerClassName?: import("../..").ClassName;
        placement?: import("../..").PopupPlacement;
        popperOptions?: object;
        trigger?: "hover" | "click" | "focus" | "mousedown" | "context-menu";
        triggerElement?: string | import("../..").TNode;
        onOverlayClick?: (context: {
            e: MouseEvent;
        }) => void;
        onScrollToBottom?: (context: {
            e: WheelEvent;
        }) => void;
        destroyOnClose?: boolean;
        hideEmptyPopup?: boolean;
        showArrow?: boolean;
        defaultVisible?: boolean;
        zIndex?: number;
        expandAnimation: boolean;
    }>;
    isHoverCell: import("vue").Ref<boolean, boolean>;
    activeIndex: import("vue").Ref<0 | 1, 0 | 1>;
    isFirstValueSelected: import("vue").Ref<boolean, boolean>;
    cacheValue: import("vue").Ref<string | number | string[] | Date | number[] | Date[], string | number | string[] | Date | number[] | Date[]>;
    onRawChange: import("@tdesign/shared-hooks").ChangeHandler<import("..").DateRangeValue, [context: {
        dayjsValue?: import("dayjs").Dayjs[];
        trigger?: import("..").DatePickerTriggerSource;
    }]>;
};
