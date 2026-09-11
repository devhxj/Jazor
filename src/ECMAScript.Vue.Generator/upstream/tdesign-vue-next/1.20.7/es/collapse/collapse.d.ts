import { CollapseValue } from './type';
declare const _default: import("vue").DefineComponent<{
    borderless?: boolean;
    defaultExpandAll?: boolean;
    disabled?: boolean;
    expandIcon?: boolean | import("..").TNode;
    expandIconPlacement?: "left" | "right";
    expandMutex?: boolean;
    expandOnRowClick?: boolean;
    value?: CollapseValue;
    defaultValue?: CollapseValue;
    modelValue?: CollapseValue;
    onChange?: (value: CollapseValue) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    borderless?: boolean;
    defaultExpandAll?: boolean;
    disabled?: boolean;
    expandIcon?: boolean | import("..").TNode;
    expandIconPlacement?: "left" | "right";
    expandMutex?: boolean;
    expandOnRowClick?: boolean;
    value?: CollapseValue;
    defaultValue?: CollapseValue;
    modelValue?: CollapseValue;
    onChange?: (value: CollapseValue) => void;
}> & Readonly<{}>, {
    value: CollapseValue;
    disabled: boolean;
    expandMutex: boolean;
    modelValue: CollapseValue;
    borderless: boolean;
    expandIcon: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    expandOnRowClick: boolean;
    expandIconPlacement: "left" | "right";
    defaultExpandAll: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
