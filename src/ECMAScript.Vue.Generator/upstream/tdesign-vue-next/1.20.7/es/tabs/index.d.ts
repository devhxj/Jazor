import { TdTabsProps, TdTabPanelProps } from './type';
import './style';
export * from './type';
export type TabsProps = TdTabsProps;
export type TabPanelProps = TdTabPanelProps;
export declare const Tabs: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        action: {
            type: import("vue").PropType<TdTabsProps["action"]>;
        };
        addable: BooleanConstructor;
        disabled: BooleanConstructor;
        dragSort: BooleanConstructor;
        list: {
            type: import("vue").PropType<TdTabsProps["list"]>;
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
        theme: {
            type: import("vue").PropType<TdTabsProps["theme"]>;
            default: TdTabsProps["theme"];
            validator(val: TdTabsProps["theme"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdTabsProps["value"]>;
            default: TdTabsProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdTabsProps["value"]>;
            default: TdTabsProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdTabsProps["defaultValue"]>;
        };
        onAdd: import("vue").PropType<TdTabsProps["onAdd"]>;
        onChange: import("vue").PropType<TdTabsProps["onChange"]>;
        onDragSort: import("vue").PropType<TdTabsProps["onDragSort"]>;
        onRemove: import("vue").PropType<TdTabsProps["onRemove"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").TabValue;
        size: "medium" | "large";
        disabled: boolean;
        theme: "card" | "normal";
        modelValue: import("./type").TabValue;
        placement: "left" | "right" | "top" | "bottom";
        dragSort: boolean;
        scrollPosition: "center" | "start" | "end" | "auto";
        addable: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        action: {
            type: import("vue").PropType<TdTabsProps["action"]>;
        };
        addable: BooleanConstructor;
        disabled: BooleanConstructor;
        dragSort: BooleanConstructor;
        list: {
            type: import("vue").PropType<TdTabsProps["list"]>;
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
        theme: {
            type: import("vue").PropType<TdTabsProps["theme"]>;
            default: TdTabsProps["theme"];
            validator(val: TdTabsProps["theme"]): boolean;
        };
        value: {
            type: import("vue").PropType<TdTabsProps["value"]>;
            default: TdTabsProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdTabsProps["value"]>;
            default: TdTabsProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdTabsProps["defaultValue"]>;
        };
        onAdd: import("vue").PropType<TdTabsProps["onAdd"]>;
        onChange: import("vue").PropType<TdTabsProps["onChange"]>;
        onDragSort: import("vue").PropType<TdTabsProps["onDragSort"]>;
        onRemove: import("vue").PropType<TdTabsProps["onRemove"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").TabValue;
        size: "medium" | "large";
        disabled: boolean;
        theme: "card" | "normal";
        modelValue: import("./type").TabValue;
        placement: "left" | "right" | "top" | "bottom";
        dragSort: boolean;
        scrollPosition: "center" | "start" | "end" | "auto";
        addable: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    action: {
        type: import("vue").PropType<TdTabsProps["action"]>;
    };
    addable: BooleanConstructor;
    disabled: BooleanConstructor;
    dragSort: BooleanConstructor;
    list: {
        type: import("vue").PropType<TdTabsProps["list"]>;
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
    theme: {
        type: import("vue").PropType<TdTabsProps["theme"]>;
        default: TdTabsProps["theme"];
        validator(val: TdTabsProps["theme"]): boolean;
    };
    value: {
        type: import("vue").PropType<TdTabsProps["value"]>;
        default: TdTabsProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdTabsProps["value"]>;
        default: TdTabsProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdTabsProps["defaultValue"]>;
    };
    onAdd: import("vue").PropType<TdTabsProps["onAdd"]>;
    onChange: import("vue").PropType<TdTabsProps["onChange"]>;
    onDragSort: import("vue").PropType<TdTabsProps["onDragSort"]>;
    onRemove: import("vue").PropType<TdTabsProps["onRemove"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").TabValue;
    size: "medium" | "large";
    disabled: boolean;
    theme: "card" | "normal";
    modelValue: import("./type").TabValue;
    placement: "left" | "right" | "top" | "bottom";
    dragSort: boolean;
    scrollPosition: "center" | "start" | "end" | "auto";
    addable: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const TabPanel: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        default: {
            type: import("vue").PropType<TdTabPanelProps["default"]>;
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
            type: import("vue").PropType<TdTabPanelProps["label"]>;
        };
        lazy: BooleanConstructor;
        panel: {
            type: import("vue").PropType<TdTabPanelProps["panel"]>;
        };
        removable: BooleanConstructor;
        value: {
            type: import("vue").PropType<TdTabPanelProps["value"]>;
        };
        onRemove: import("vue").PropType<TdTabPanelProps["onRemove"]>;
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
            type: import("vue").PropType<TdTabPanelProps["default"]>;
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
            type: import("vue").PropType<TdTabPanelProps["label"]>;
        };
        lazy: BooleanConstructor;
        panel: {
            type: import("vue").PropType<TdTabPanelProps["panel"]>;
        };
        removable: BooleanConstructor;
        value: {
            type: import("vue").PropType<TdTabPanelProps["value"]>;
        };
        onRemove: import("vue").PropType<TdTabPanelProps["onRemove"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        disabled: boolean;
        draggable: boolean;
        lazy: boolean;
        destroyOnHide: boolean;
        removable: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    default: {
        type: import("vue").PropType<TdTabPanelProps["default"]>;
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
        type: import("vue").PropType<TdTabPanelProps["label"]>;
    };
    lazy: BooleanConstructor;
    panel: {
        type: import("vue").PropType<TdTabPanelProps["panel"]>;
    };
    removable: BooleanConstructor;
    value: {
        type: import("vue").PropType<TdTabPanelProps["value"]>;
    };
    onRemove: import("vue").PropType<TdTabPanelProps["onRemove"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    disabled: boolean;
    draggable: boolean;
    lazy: boolean;
    destroyOnHide: boolean;
    removable: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Tabs;
