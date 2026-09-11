import { ComputedRef } from 'vue';
import dayjs from 'dayjs';
import { TdDatePickerProps, DateValue } from '../type';
export declare function useSingle(props: TdDatePickerProps): {
    year: import("vue").Ref<number, number>;
    month: import("vue").Ref<number, number>;
    value: import("vue").Ref<DateValue | import("..").DateMultipleValue, DateValue | import("..").DateMultipleValue>;
    time: import("vue").Ref<any, any>;
    inputValue: import("vue").Ref<string | number | string[] | Date | number[] | Date[], string | number | string[] | Date | number[] | Date[]>;
    popupVisible: import("vue").Ref<boolean, boolean>;
    inputProps: ComputedRef<{
        size: import("../..").SizeEnum;
        ref: import("vue").Ref<any, any>;
        readonly: boolean;
        class: {
            [x: string]: boolean;
        }[];
        align?: "left" | "center" | "right";
        allowInputOverMax?: boolean;
        autoWidth?: boolean;
        autocomplete?: string;
        autofocus?: boolean;
        borderless?: boolean;
        clearable?: boolean;
        disabled?: boolean;
        format?: import("../..").InputFormatType;
        inputClass?: import("../..").ClassName;
        label?: string | import("../..").TNode;
        maxcharacter?: number;
        maxlength?: string | number;
        name?: string;
        placeholder?: string;
        prefixIcon?: import("../..").TNode;
        showClearIconOnEmpty?: boolean;
        showLimitNumber?: boolean;
        spellCheck?: boolean;
        status?: "default" | "success" | "warning" | "error";
        suffix?: string | import("../..").TNode;
        suffixIcon?: import("../..").TNode;
        tips?: string | import("../..").TNode;
        type?: "text" | "number" | "url" | "tel" | "password" | "search" | "submit" | "hidden";
        value?: import("../..").InputValue;
        defaultValue?: import("../..").InputValue;
        modelValue?: import("../..").InputValue;
        onBlur?: (value: import("../..").InputValue, context: {
            e: FocusEvent;
        }) => void;
        onChange?: (value: import("../..").InputValue, context?: {
            e?: InputEvent | MouseEvent | CompositionEvent;
            trigger: "input" | "initial" | "clear";
        }) => void;
        onClear?: (context: {
            e: MouseEvent;
        }) => void;
        onClick?: (context: {
            e: MouseEvent;
        }) => void;
        onCompositionend?: (value: string, context: {
            e: CompositionEvent;
        }) => void;
        onCompositionstart?: (value: string, context: {
            e: CompositionEvent;
        }) => void;
        onEnter?: (value: import("../..").InputValue, context: {
            e: KeyboardEvent;
        }) => void;
        onFocus?: (value: import("../..").InputValue, context: {
            e: FocusEvent;
        }) => void;
        onKeydown?: (value: import("../..").InputValue, context: {
            e: KeyboardEvent;
        }) => void;
        onKeypress?: (value: import("../..").InputValue, context: {
            e: KeyboardEvent;
        }) => void;
        onKeyup?: (value: import("../..").InputValue, context: {
            e: KeyboardEvent;
        }) => void;
        onMouseenter?: (context: {
            e: MouseEvent;
        }) => void;
        onMouseleave?: (context: {
            e: MouseEvent;
        }) => void;
        onPaste?: (context: {
            e: ClipboardEvent;
            pasteValue: string;
        }) => void;
        onValidate?: (context: {
            error?: "exceed-maximum" | "below-minimum";
        }) => void;
        onWheel?: (context: {
            e: WheelEvent;
        }) => void;
    }>;
    popupProps: ComputedRef<{
        disabled: boolean;
        overlayInnerStyle: import("../..").Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => import("../..").Styles);
        overlayClassName: import("../..").ClassName[];
        onVisibleChange: (visible: boolean, context: any) => void;
        default?: string | import("../..").TNode;
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
    inputRef: import("vue").Ref<any, any>;
    cacheValue: import("vue").Ref<string | number | string[] | Date | number[] | Date[], string | number | string[] | Date | number[] | Date[]>;
    isHoverCell: import("vue").Ref<boolean, boolean>;
    onChange: import("@tdesign/shared-hooks").ChangeHandler<DateValue | import("..").DateMultipleValue, [context: {
        dayjsValue?: dayjs.Dayjs;
        trigger?: import("..").DatePickerTriggerSource;
    }]>;
};
