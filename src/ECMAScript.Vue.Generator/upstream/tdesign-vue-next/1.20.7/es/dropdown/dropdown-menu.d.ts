import { DropdownOption } from './type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    direction: {
        type: import("vue").PropType<import("./type").TdDropdownProps["direction"]>;
        default: import("./type").TdDropdownProps["direction"];
        validator(val: import("./type").TdDropdownProps["direction"]): boolean;
    };
    disabled: BooleanConstructor;
    hideAfterItemClick: {
        type: BooleanConstructor;
        default: boolean;
    };
    maxColumnWidth: {
        type: import("vue").PropType<import("./type").TdDropdownProps["maxColumnWidth"]>;
        default: import("./type").TdDropdownProps["maxColumnWidth"];
    };
    maxHeight: {
        type: NumberConstructor;
        default: number;
    };
    minColumnWidth: {
        type: import("vue").PropType<import("./type").TdDropdownProps["minColumnWidth"]>;
        default: import("./type").TdDropdownProps["minColumnWidth"];
    };
    options: {
        type: import("vue").PropType<import("./type").TdDropdownProps["options"]>;
        default: () => import("./type").TdDropdownProps["options"];
    };
    panelBottomContent: {
        type: import("vue").PropType<import("./type").TdDropdownProps["panelBottomContent"]>;
    };
    panelTopContent: {
        type: import("vue").PropType<import("./type").TdDropdownProps["panelTopContent"]>;
    };
    placement: {
        type: import("vue").PropType<import("./type").TdDropdownProps["placement"]>;
        default: import("./type").TdDropdownProps["placement"];
        validator(val: import("./type").TdDropdownProps["placement"]): boolean;
    };
    popupProps: {
        type: import("vue").PropType<import("./type").TdDropdownProps["popupProps"]>;
    };
    trigger: {
        type: import("vue").PropType<import("./type").TdDropdownProps["trigger"]>;
        default: import("./type").TdDropdownProps["trigger"];
        validator(val: import("./type").TdDropdownProps["trigger"]): boolean;
    };
    onClick: import("vue").PropType<import("./type").TdDropdownProps["onClick"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    direction: {
        type: import("vue").PropType<import("./type").TdDropdownProps["direction"]>;
        default: import("./type").TdDropdownProps["direction"];
        validator(val: import("./type").TdDropdownProps["direction"]): boolean;
    };
    disabled: BooleanConstructor;
    hideAfterItemClick: {
        type: BooleanConstructor;
        default: boolean;
    };
    maxColumnWidth: {
        type: import("vue").PropType<import("./type").TdDropdownProps["maxColumnWidth"]>;
        default: import("./type").TdDropdownProps["maxColumnWidth"];
    };
    maxHeight: {
        type: NumberConstructor;
        default: number;
    };
    minColumnWidth: {
        type: import("vue").PropType<import("./type").TdDropdownProps["minColumnWidth"]>;
        default: import("./type").TdDropdownProps["minColumnWidth"];
    };
    options: {
        type: import("vue").PropType<import("./type").TdDropdownProps["options"]>;
        default: () => import("./type").TdDropdownProps["options"];
    };
    panelBottomContent: {
        type: import("vue").PropType<import("./type").TdDropdownProps["panelBottomContent"]>;
    };
    panelTopContent: {
        type: import("vue").PropType<import("./type").TdDropdownProps["panelTopContent"]>;
    };
    placement: {
        type: import("vue").PropType<import("./type").TdDropdownProps["placement"]>;
        default: import("./type").TdDropdownProps["placement"];
        validator(val: import("./type").TdDropdownProps["placement"]): boolean;
    };
    popupProps: {
        type: import("vue").PropType<import("./type").TdDropdownProps["popupProps"]>;
    };
    trigger: {
        type: import("vue").PropType<import("./type").TdDropdownProps["trigger"]>;
        default: import("./type").TdDropdownProps["trigger"];
        validator(val: import("./type").TdDropdownProps["trigger"]): boolean;
    };
    onClick: import("vue").PropType<import("./type").TdDropdownProps["onClick"]>;
}>> & Readonly<{}>, {
    maxHeight: number;
    direction: "left" | "right";
    disabled: boolean;
    options: DropdownOption[];
    placement: "left" | "right" | "top" | "bottom" | "top-left" | "bottom-left" | "top-right" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
    trigger: "click" | "focus" | "hover" | "context-menu";
    maxColumnWidth: string | number;
    minColumnWidth: string | number;
    hideAfterItemClick: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
