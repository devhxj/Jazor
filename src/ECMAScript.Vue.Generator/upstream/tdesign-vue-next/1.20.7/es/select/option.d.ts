declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    createAble: BooleanConstructor;
    multiple: BooleanConstructor;
    index: NumberConstructor;
    rowIndex: NumberConstructor;
    trs: MapConstructor;
    scrollType: StringConstructor;
    isVirtual: BooleanConstructor;
    bufferSize: NumberConstructor;
    checkAll: BooleanConstructor;
    onRowMounted: FunctionConstructor;
    content: {
        type: import("vue").PropType<import("./type").TdOptionProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdOptionProps["default"]>;
    };
    disabled: BooleanConstructor;
    label: {
        type: StringConstructor;
        default: string;
    };
    title: {
        type: StringConstructor;
        default: string;
    };
    value: {
        type: import("vue").PropType<import("./type").TdOptionProps["value"]>;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "row-mounted"[], "row-mounted", import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    createAble: BooleanConstructor;
    multiple: BooleanConstructor;
    index: NumberConstructor;
    rowIndex: NumberConstructor;
    trs: MapConstructor;
    scrollType: StringConstructor;
    isVirtual: BooleanConstructor;
    bufferSize: NumberConstructor;
    checkAll: BooleanConstructor;
    onRowMounted: FunctionConstructor;
    content: {
        type: import("vue").PropType<import("./type").TdOptionProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("./type").TdOptionProps["default"]>;
    };
    disabled: BooleanConstructor;
    label: {
        type: StringConstructor;
        default: string;
    };
    title: {
        type: StringConstructor;
        default: string;
    };
    value: {
        type: import("vue").PropType<import("./type").TdOptionProps["value"]>;
    };
}>> & Readonly<{
    "onRow-mounted"?: (...args: any[]) => any;
}>, {
    multiple: boolean;
    label: string;
    title: string;
    disabled: boolean;
    checkAll: boolean;
    createAble: boolean;
    isVirtual: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
