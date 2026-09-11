import { TdCollapseProps, TdCollapsePanelProps } from './type';
import './style';
export * from './type';
export type CollapseProps = TdCollapseProps;
export type CollapsePanelProps = TdCollapsePanelProps;
export declare const Collapse: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        borderless?: boolean;
        defaultExpandAll?: boolean;
        disabled?: boolean;
        expandIcon?: boolean | import("..").TNode;
        expandIconPlacement?: "left" | "right";
        expandMutex?: boolean;
        expandOnRowClick?: boolean;
        value?: import("./type").CollapseValue;
        defaultValue?: import("./type").CollapseValue;
        modelValue?: import("./type").CollapseValue;
        onChange?: (value: import("./type").CollapseValue) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        value: import("./type").CollapseValue;
        disabled: boolean;
        expandMutex: boolean;
        modelValue: import("./type").CollapseValue;
        borderless: boolean;
        expandIcon: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        expandOnRowClick: boolean;
        expandIconPlacement: "left" | "right";
        defaultExpandAll: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        borderless?: boolean;
        defaultExpandAll?: boolean;
        disabled?: boolean;
        expandIcon?: boolean | import("..").TNode;
        expandIconPlacement?: "left" | "right";
        expandMutex?: boolean;
        expandOnRowClick?: boolean;
        value?: import("./type").CollapseValue;
        defaultValue?: import("./type").CollapseValue;
        modelValue?: import("./type").CollapseValue;
        onChange?: (value: import("./type").CollapseValue) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        value: import("./type").CollapseValue;
        disabled: boolean;
        expandMutex: boolean;
        modelValue: import("./type").CollapseValue;
        borderless: boolean;
        expandIcon: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        expandOnRowClick: boolean;
        expandIconPlacement: "left" | "right";
        defaultExpandAll: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    borderless?: boolean;
    defaultExpandAll?: boolean;
    disabled?: boolean;
    expandIcon?: boolean | import("..").TNode;
    expandIconPlacement?: "left" | "right";
    expandMutex?: boolean;
    expandOnRowClick?: boolean;
    value?: import("./type").CollapseValue;
    defaultValue?: import("./type").CollapseValue;
    modelValue?: import("./type").CollapseValue;
    onChange?: (value: import("./type").CollapseValue) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    value: import("./type").CollapseValue;
    disabled: boolean;
    expandMutex: boolean;
    modelValue: import("./type").CollapseValue;
    borderless: boolean;
    expandIcon: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    expandOnRowClick: boolean;
    expandIconPlacement: "left" | "right";
    defaultExpandAll: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const CollapsePanel: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        content?: string | import("..").TNode;
        default?: string | import("..").TNode;
        destroyOnCollapse?: boolean;
        disabled?: boolean;
        expandIcon?: boolean | import("..").TNode;
        header?: string | import("..").TNode;
        headerRightContent?: string | import("..").TNode;
        value?: string | number;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        disabled: boolean;
        expandIcon: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        destroyOnCollapse: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        content?: string | import("..").TNode;
        default?: string | import("..").TNode;
        destroyOnCollapse?: boolean;
        disabled?: boolean;
        expandIcon?: boolean | import("..").TNode;
        header?: string | import("..").TNode;
        headerRightContent?: string | import("..").TNode;
        value?: string | number;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        disabled: boolean;
        expandIcon: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        destroyOnCollapse: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    content?: string | import("..").TNode;
    default?: string | import("..").TNode;
    destroyOnCollapse?: boolean;
    disabled?: boolean;
    expandIcon?: boolean | import("..").TNode;
    header?: string | import("..").TNode;
    headerRightContent?: string | import("..").TNode;
    value?: string | number;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    disabled: boolean;
    expandIcon: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    destroyOnCollapse: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Collapse;
