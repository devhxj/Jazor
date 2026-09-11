declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    showInput: {
        type: BooleanConstructor;
        default: boolean;
    };
    keepWrapperWidth: {
        type: BooleanConstructor;
        default: boolean;
    };
    align: {
        type: import("vue").PropType<import("./type").TdInputProps["align"]>;
        default: import("./type").TdInputProps["align"];
        validator(val: import("./type").TdInputProps["align"]): boolean;
    };
    allowInputOverMax: BooleanConstructor;
    autoWidth: BooleanConstructor;
    autocomplete: {
        type: StringConstructor;
        default: any;
    };
    autofocus: BooleanConstructor;
    borderless: BooleanConstructor;
    clearable: BooleanConstructor;
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    format: {
        type: import("vue").PropType<import("./type").TdInputProps["format"]>;
    };
    inputClass: {
        type: import("vue").PropType<import("./type").TdInputProps["inputClass"]>;
    };
    label: {
        type: import("vue").PropType<import("./type").TdInputProps["label"]>;
    };
    maxcharacter: {
        type: NumberConstructor;
    };
    maxlength: {
        type: import("vue").PropType<import("./type").TdInputProps["maxlength"]>;
    };
    name: {
        type: StringConstructor;
        default: string;
    };
    placeholder: {
        type: StringConstructor;
        default: any;
    };
    prefixIcon: {
        type: import("vue").PropType<import("./type").TdInputProps["prefixIcon"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    showClearIconOnEmpty: BooleanConstructor;
    showLimitNumber: BooleanConstructor;
    size: {
        type: import("vue").PropType<import("./type").TdInputProps["size"]>;
        default: import("./type").TdInputProps["size"];
        validator(val: import("./type").TdInputProps["size"]): boolean;
    };
    spellCheck: BooleanConstructor;
    status: {
        type: import("vue").PropType<import("./type").TdInputProps["status"]>;
        default: import("./type").TdInputProps["status"];
        validator(val: import("./type").TdInputProps["status"]): boolean;
    };
    suffix: {
        type: import("vue").PropType<import("./type").TdInputProps["suffix"]>;
    };
    suffixIcon: {
        type: import("vue").PropType<import("./type").TdInputProps["suffixIcon"]>;
    };
    tips: {
        type: import("vue").PropType<import("./type").TdInputProps["tips"]>;
    };
    type: {
        type: import("vue").PropType<import("./type").TdInputProps["type"]>;
        default: import("./type").TdInputProps["type"];
        validator(val: import("./type").TdInputProps["type"]): boolean;
    };
    value: {
        type: import("vue").PropType<import("./type").TdInputProps["value"]>;
        default: import("./type").TdInputProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<import("./type").TdInputProps["value"]>;
        default: import("./type").TdInputProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<import("./type").TdInputProps["defaultValue"]>;
    };
    onBlur: import("vue").PropType<import("./type").TdInputProps["onBlur"]>;
    onChange: import("vue").PropType<import("./type").TdInputProps["onChange"]>;
    onClear: import("vue").PropType<import("./type").TdInputProps["onClear"]>;
    onClick: import("vue").PropType<import("./type").TdInputProps["onClick"]>;
    onCompositionend: import("vue").PropType<import("./type").TdInputProps["onCompositionend"]>;
    onCompositionstart: import("vue").PropType<import("./type").TdInputProps["onCompositionstart"]>;
    onEnter: import("vue").PropType<import("./type").TdInputProps["onEnter"]>;
    onFocus: import("vue").PropType<import("./type").TdInputProps["onFocus"]>;
    onKeydown: import("vue").PropType<import("./type").TdInputProps["onKeydown"]>;
    onKeypress: import("vue").PropType<import("./type").TdInputProps["onKeypress"]>;
    onKeyup: import("vue").PropType<import("./type").TdInputProps["onKeyup"]>;
    onMouseenter: import("vue").PropType<import("./type").TdInputProps["onMouseenter"]>;
    onMouseleave: import("vue").PropType<import("./type").TdInputProps["onMouseleave"]>;
    onPaste: import("vue").PropType<import("./type").TdInputProps["onPaste"]>;
    onValidate: import("vue").PropType<import("./type").TdInputProps["onValidate"]>;
    onWheel: import("vue").PropType<import("./type").TdInputProps["onWheel"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    showInput: {
        type: BooleanConstructor;
        default: boolean;
    };
    keepWrapperWidth: {
        type: BooleanConstructor;
        default: boolean;
    };
    align: {
        type: import("vue").PropType<import("./type").TdInputProps["align"]>;
        default: import("./type").TdInputProps["align"];
        validator(val: import("./type").TdInputProps["align"]): boolean;
    };
    allowInputOverMax: BooleanConstructor;
    autoWidth: BooleanConstructor;
    autocomplete: {
        type: StringConstructor;
        default: any;
    };
    autofocus: BooleanConstructor;
    borderless: BooleanConstructor;
    clearable: BooleanConstructor;
    disabled: {
        type: BooleanConstructor;
        default: any;
    };
    format: {
        type: import("vue").PropType<import("./type").TdInputProps["format"]>;
    };
    inputClass: {
        type: import("vue").PropType<import("./type").TdInputProps["inputClass"]>;
    };
    label: {
        type: import("vue").PropType<import("./type").TdInputProps["label"]>;
    };
    maxcharacter: {
        type: NumberConstructor;
    };
    maxlength: {
        type: import("vue").PropType<import("./type").TdInputProps["maxlength"]>;
    };
    name: {
        type: StringConstructor;
        default: string;
    };
    placeholder: {
        type: StringConstructor;
        default: any;
    };
    prefixIcon: {
        type: import("vue").PropType<import("./type").TdInputProps["prefixIcon"]>;
    };
    readonly: {
        type: BooleanConstructor;
        default: any;
    };
    showClearIconOnEmpty: BooleanConstructor;
    showLimitNumber: BooleanConstructor;
    size: {
        type: import("vue").PropType<import("./type").TdInputProps["size"]>;
        default: import("./type").TdInputProps["size"];
        validator(val: import("./type").TdInputProps["size"]): boolean;
    };
    spellCheck: BooleanConstructor;
    status: {
        type: import("vue").PropType<import("./type").TdInputProps["status"]>;
        default: import("./type").TdInputProps["status"];
        validator(val: import("./type").TdInputProps["status"]): boolean;
    };
    suffix: {
        type: import("vue").PropType<import("./type").TdInputProps["suffix"]>;
    };
    suffixIcon: {
        type: import("vue").PropType<import("./type").TdInputProps["suffixIcon"]>;
    };
    tips: {
        type: import("vue").PropType<import("./type").TdInputProps["tips"]>;
    };
    type: {
        type: import("vue").PropType<import("./type").TdInputProps["type"]>;
        default: import("./type").TdInputProps["type"];
        validator(val: import("./type").TdInputProps["type"]): boolean;
    };
    value: {
        type: import("vue").PropType<import("./type").TdInputProps["value"]>;
        default: import("./type").TdInputProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<import("./type").TdInputProps["value"]>;
        default: import("./type").TdInputProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<import("./type").TdInputProps["defaultValue"]>;
    };
    onBlur: import("vue").PropType<import("./type").TdInputProps["onBlur"]>;
    onChange: import("vue").PropType<import("./type").TdInputProps["onChange"]>;
    onClear: import("vue").PropType<import("./type").TdInputProps["onClear"]>;
    onClick: import("vue").PropType<import("./type").TdInputProps["onClick"]>;
    onCompositionend: import("vue").PropType<import("./type").TdInputProps["onCompositionend"]>;
    onCompositionstart: import("vue").PropType<import("./type").TdInputProps["onCompositionstart"]>;
    onEnter: import("vue").PropType<import("./type").TdInputProps["onEnter"]>;
    onFocus: import("vue").PropType<import("./type").TdInputProps["onFocus"]>;
    onKeydown: import("vue").PropType<import("./type").TdInputProps["onKeydown"]>;
    onKeypress: import("vue").PropType<import("./type").TdInputProps["onKeypress"]>;
    onKeyup: import("vue").PropType<import("./type").TdInputProps["onKeyup"]>;
    onMouseenter: import("vue").PropType<import("./type").TdInputProps["onMouseenter"]>;
    onMouseleave: import("vue").PropType<import("./type").TdInputProps["onMouseleave"]>;
    onPaste: import("vue").PropType<import("./type").TdInputProps["onPaste"]>;
    onValidate: import("vue").PropType<import("./type").TdInputProps["onValidate"]>;
    onWheel: import("vue").PropType<import("./type").TdInputProps["onWheel"]>;
}>> & Readonly<{}>, {
    value: import("./type").InputValue;
    type: "number" | "search" | "submit" | "url" | "text" | "hidden" | "tel" | "password";
    size: import("..").SizeEnum;
    status: "default" | "error" | "success" | "warning";
    disabled: boolean;
    name: string;
    autocomplete: string;
    placeholder: string;
    autofocus: boolean;
    modelValue: import("./type").InputValue;
    readonly: boolean;
    align: "left" | "center" | "right";
    allowInputOverMax: boolean;
    showInput: boolean;
    keepWrapperWidth: boolean;
    autoWidth: boolean;
    borderless: boolean;
    clearable: boolean;
    showClearIconOnEmpty: boolean;
    showLimitNumber: boolean;
    spellCheck: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
