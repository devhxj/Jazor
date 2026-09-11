import { TdTabsProps } from './type';
import TTabPanel from './tab-panel';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    theme: {
        type: import("vue").PropType<TdTabsProps["theme"]>;
        default: TdTabsProps["theme"];
        validator(val: TdTabsProps["theme"]): boolean;
    };
    panels: {
        type: {
            new (): Array<InstanceType<typeof TTabPanel>>;
        };
        default: () => Array<InstanceType<typeof TTabPanel>>;
    };
    action: (ObjectConstructor | ArrayConstructor | StringConstructor | BooleanConstructor)[];
    value: {
        type: import("vue").PropType<TdTabsProps["value"]>;
        default: TdTabsProps["value"];
    };
    placement: {
        type: import("vue").PropType<TdTabsProps["placement"]>;
        default: TdTabsProps["placement"];
        validator(val: TdTabsProps["placement"]): boolean;
    };
    scrollPosition: {
        type: import("vue").PropType<TdTabsProps["scrollPosition"]>;
        default: TdTabsProps["scrollPosition"];
        validator(val: TdTabsProps["scrollPosition"]): boolean;
    };
    size: {
        type: import("vue").PropType<TdTabsProps["size"]>;
        default: TdTabsProps["size"];
        validator(val: TdTabsProps["size"]): boolean;
    };
    disabled: BooleanConstructor;
    addable: BooleanConstructor;
    onChange: import("vue").PropType<(value: import("./type").TabValue) => void>;
    onAdd: import("vue").PropType<(context: {
        e: MouseEvent;
    }) => void>;
    onRemove: import("vue").PropType<(options: {
        value: import("./type").TabValue;
        index: number;
        e: MouseEvent;
    }) => void>;
    dragSort: BooleanConstructor;
    onDragSort: import("vue").PropType<(context: import("./type").TabsDragSortContext) => void>;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    theme: {
        type: import("vue").PropType<TdTabsProps["theme"]>;
        default: TdTabsProps["theme"];
        validator(val: TdTabsProps["theme"]): boolean;
    };
    panels: {
        type: {
            new (): Array<InstanceType<typeof TTabPanel>>;
        };
        default: () => Array<InstanceType<typeof TTabPanel>>;
    };
    action: (ObjectConstructor | ArrayConstructor | StringConstructor | BooleanConstructor)[];
    value: {
        type: import("vue").PropType<TdTabsProps["value"]>;
        default: TdTabsProps["value"];
    };
    placement: {
        type: import("vue").PropType<TdTabsProps["placement"]>;
        default: TdTabsProps["placement"];
        validator(val: TdTabsProps["placement"]): boolean;
    };
    scrollPosition: {
        type: import("vue").PropType<TdTabsProps["scrollPosition"]>;
        default: TdTabsProps["scrollPosition"];
        validator(val: TdTabsProps["scrollPosition"]): boolean;
    };
    size: {
        type: import("vue").PropType<TdTabsProps["size"]>;
        default: TdTabsProps["size"];
        validator(val: TdTabsProps["size"]): boolean;
    };
    disabled: BooleanConstructor;
    addable: BooleanConstructor;
    onChange: import("vue").PropType<(value: import("./type").TabValue) => void>;
    onAdd: import("vue").PropType<(context: {
        e: MouseEvent;
    }) => void>;
    onRemove: import("vue").PropType<(options: {
        value: import("./type").TabValue;
        index: number;
        e: MouseEvent;
    }) => void>;
    dragSort: BooleanConstructor;
    onDragSort: import("vue").PropType<(context: import("./type").TabsDragSortContext) => void>;
}>> & Readonly<{}>, {
    value: import("./type").TabValue;
    size: "medium" | "large";
    disabled: boolean;
    theme: "card" | "normal";
    placement: "left" | "right" | "top" | "bottom";
    dragSort: boolean;
    scrollPosition: "center" | "start" | "end" | "auto";
    addable: boolean;
    panels: import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        default: {
            type: import("vue").PropType<import("./type").TdTabPanelProps["default"]>;
        };
        destroyOnHide: {
            type: BooleanConstructor;
            default: boolean;
        };
        disabled: BooleanConstructor;
        draggable: {
            type: BooleanConstructor;
            default: boolean;
        };
        label: {
            type: import("vue").PropType<import("./type").TdTabPanelProps["label"]>;
        };
        lazy: BooleanConstructor;
        panel: {
            type: import("vue").PropType<import("./type").TdTabPanelProps["panel"]>;
        };
        removable: BooleanConstructor;
        value: {
            type: import("vue").PropType<import("./type").TdTabPanelProps["value"]>;
        };
        onRemove: import("vue").PropType<import("./type").TdTabPanelProps["onRemove"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        disabled: boolean;
        draggable: boolean;
        lazy: boolean;
        destroyOnHide: boolean;
        removable: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        default: {
            type: import("vue").PropType<import("./type").TdTabPanelProps["default"]>;
        };
        destroyOnHide: {
            type: BooleanConstructor;
            default: boolean;
        };
        disabled: BooleanConstructor;
        draggable: {
            type: BooleanConstructor;
            default: boolean;
        };
        label: {
            type: import("vue").PropType<import("./type").TdTabPanelProps["label"]>;
        };
        lazy: BooleanConstructor;
        panel: {
            type: import("vue").PropType<import("./type").TdTabPanelProps["panel"]>;
        };
        removable: BooleanConstructor;
        value: {
            type: import("vue").PropType<import("./type").TdTabPanelProps["value"]>;
        };
        onRemove: import("vue").PropType<import("./type").TdTabPanelProps["onRemove"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        disabled: boolean;
        draggable: boolean;
        lazy: boolean;
        destroyOnHide: boolean;
        removable: boolean;
    }>[];
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
