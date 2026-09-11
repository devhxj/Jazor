import { PropType } from 'vue';
import { TdTabsProps, TdTabPanelProps } from './type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    index: NumberConstructor;
    active: {
        type: BooleanConstructor;
    };
    theme: {
        type: PropType<TdTabsProps["theme"]>;
        default: TdTabsProps["theme"];
        validator(val: TdTabsProps["theme"]): boolean;
    };
    size: {
        type: PropType<TdTabsProps["size"]>;
        default: TdTabsProps["size"];
        validator(val: TdTabsProps["size"]): boolean;
    };
    placement: {
        type: PropType<TdTabsProps["placement"]>;
        default: TdTabsProps["placement"];
        validator(val: TdTabsProps["placement"]): boolean;
    };
    label: {
        type: any;
    };
    disabled: BooleanConstructor;
    removable: BooleanConstructor;
    value: {
        type: PropType<TdTabPanelProps["value"]>;
    };
    draggable: {
        type: BooleanConstructor;
        default: boolean;
    };
    onClick: PropType<Function>;
    onTabRemove: PropType<TdTabsProps["onRemove"]>;
    onTabPanelRemove: PropType<TdTabPanelProps["onRemove"]>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    index: NumberConstructor;
    active: {
        type: BooleanConstructor;
    };
    theme: {
        type: PropType<TdTabsProps["theme"]>;
        default: TdTabsProps["theme"];
        validator(val: TdTabsProps["theme"]): boolean;
    };
    size: {
        type: PropType<TdTabsProps["size"]>;
        default: TdTabsProps["size"];
        validator(val: TdTabsProps["size"]): boolean;
    };
    placement: {
        type: PropType<TdTabsProps["placement"]>;
        default: TdTabsProps["placement"];
        validator(val: TdTabsProps["placement"]): boolean;
    };
    label: {
        type: any;
    };
    disabled: BooleanConstructor;
    removable: BooleanConstructor;
    value: {
        type: PropType<TdTabPanelProps["value"]>;
    };
    draggable: {
        type: BooleanConstructor;
        default: boolean;
    };
    onClick: PropType<Function>;
    onTabRemove: PropType<TdTabsProps["onRemove"]>;
    onTabPanelRemove: PropType<TdTabPanelProps["onRemove"]>;
}>> & Readonly<{}>, {
    size: "medium" | "large";
    label: any;
    active: boolean;
    disabled: boolean;
    draggable: boolean;
    theme: "card" | "normal";
    placement: "left" | "right" | "top" | "bottom";
    removable: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
