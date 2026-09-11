import { TdTransferProps } from './type';
import './style';
export type TransferProps = TdTransferProps;
export declare const Transfer: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        checkboxProps: {
            type: import("vue").PropType<TdTransferProps["checkboxProps"]>;
        };
        checked: {
            type: import("vue").PropType<TdTransferProps["checked"]>;
            default: TdTransferProps["checked"];
        };
        defaultChecked: {
            type: import("vue").PropType<TdTransferProps["defaultChecked"]>;
            default: () => TdTransferProps["defaultChecked"];
        };
        data: {
            type: import("vue").PropType<TdTransferProps["data"]>;
            default: () => TdTransferProps["data"];
        };
        direction: {
            type: import("vue").PropType<TdTransferProps["direction"]>;
            default: TdTransferProps["direction"];
            validator(val: TdTransferProps["direction"]): boolean;
        };
        disabled: {
            type: import("vue").PropType<TdTransferProps["disabled"]>;
            default: any;
        };
        empty: {
            type: import("vue").PropType<TdTransferProps["empty"]>;
            default: TdTransferProps["empty"];
        };
        footer: {
            type: import("vue").PropType<TdTransferProps["footer"]>;
        };
        keys: {
            type: import("vue").PropType<TdTransferProps["keys"]>;
        };
        operation: {
            type: import("vue").PropType<TdTransferProps["operation"]>;
        };
        pagination: {
            type: import("vue").PropType<TdTransferProps["pagination"]>;
        };
        search: {
            type: import("vue").PropType<TdTransferProps["search"]>;
            default: TdTransferProps["search"];
        };
        showCheckAll: {
            type: import("vue").PropType<TdTransferProps["showCheckAll"]>;
            default: TdTransferProps["showCheckAll"];
        };
        targetDraggable: BooleanConstructor;
        targetSort: {
            type: import("vue").PropType<TdTransferProps["targetSort"]>;
            default: TdTransferProps["targetSort"];
            validator(val: TdTransferProps["targetSort"]): boolean;
        };
        title: {
            type: import("vue").PropType<TdTransferProps["title"]>;
            default: () => TdTransferProps["title"];
        };
        transferItem: {
            type: import("vue").PropType<TdTransferProps["transferItem"]>;
        };
        value: {
            type: import("vue").PropType<TdTransferProps["value"]>;
            default: TdTransferProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdTransferProps["value"]>;
            default: TdTransferProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdTransferProps["defaultValue"]>;
            default: () => TdTransferProps["defaultValue"];
        };
        onChange: import("vue").PropType<TdTransferProps["onChange"]>;
        onCheckedChange: import("vue").PropType<TdTransferProps["onCheckedChange"]>;
        onPageChange: import("vue").PropType<TdTransferProps["onPageChange"]>;
        onScroll: import("vue").PropType<TdTransferProps["onScroll"]>;
        onSearch: import("vue").PropType<TdTransferProps["onSearch"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        empty: import("./type").EmptyType | import("./type").EmptyType[];
        search: boolean | import("..").InputProps | import("./type").SearchOption[];
        value: import("./type").TransferValue[];
        data: import("./type").DataOption[];
        title: import("./type").TitleType[] | ((h: typeof import("vue").h, props: {
            type: import("./type").TransferListType;
        }) => import("..").TNodeReturnValue);
        direction: "left" | "right" | "both";
        disabled: boolean | boolean[];
        checked: import("./type").TransferValue[];
        defaultValue: import("./type").TransferValue[];
        modelValue: import("./type").TransferValue[];
        defaultChecked: import("./type").TransferValue[];
        showCheckAll: boolean | boolean[];
        targetSort: "push" | "unshift" | "original";
        targetDraggable: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        checkboxProps: {
            type: import("vue").PropType<TdTransferProps["checkboxProps"]>;
        };
        checked: {
            type: import("vue").PropType<TdTransferProps["checked"]>;
            default: TdTransferProps["checked"];
        };
        defaultChecked: {
            type: import("vue").PropType<TdTransferProps["defaultChecked"]>;
            default: () => TdTransferProps["defaultChecked"];
        };
        data: {
            type: import("vue").PropType<TdTransferProps["data"]>;
            default: () => TdTransferProps["data"];
        };
        direction: {
            type: import("vue").PropType<TdTransferProps["direction"]>;
            default: TdTransferProps["direction"];
            validator(val: TdTransferProps["direction"]): boolean;
        };
        disabled: {
            type: import("vue").PropType<TdTransferProps["disabled"]>;
            default: any;
        };
        empty: {
            type: import("vue").PropType<TdTransferProps["empty"]>;
            default: TdTransferProps["empty"];
        };
        footer: {
            type: import("vue").PropType<TdTransferProps["footer"]>;
        };
        keys: {
            type: import("vue").PropType<TdTransferProps["keys"]>;
        };
        operation: {
            type: import("vue").PropType<TdTransferProps["operation"]>;
        };
        pagination: {
            type: import("vue").PropType<TdTransferProps["pagination"]>;
        };
        search: {
            type: import("vue").PropType<TdTransferProps["search"]>;
            default: TdTransferProps["search"];
        };
        showCheckAll: {
            type: import("vue").PropType<TdTransferProps["showCheckAll"]>;
            default: TdTransferProps["showCheckAll"];
        };
        targetDraggable: BooleanConstructor;
        targetSort: {
            type: import("vue").PropType<TdTransferProps["targetSort"]>;
            default: TdTransferProps["targetSort"];
            validator(val: TdTransferProps["targetSort"]): boolean;
        };
        title: {
            type: import("vue").PropType<TdTransferProps["title"]>;
            default: () => TdTransferProps["title"];
        };
        transferItem: {
            type: import("vue").PropType<TdTransferProps["transferItem"]>;
        };
        value: {
            type: import("vue").PropType<TdTransferProps["value"]>;
            default: TdTransferProps["value"];
        };
        modelValue: {
            type: import("vue").PropType<TdTransferProps["value"]>;
            default: TdTransferProps["value"];
        };
        defaultValue: {
            type: import("vue").PropType<TdTransferProps["defaultValue"]>;
            default: () => TdTransferProps["defaultValue"];
        };
        onChange: import("vue").PropType<TdTransferProps["onChange"]>;
        onCheckedChange: import("vue").PropType<TdTransferProps["onCheckedChange"]>;
        onPageChange: import("vue").PropType<TdTransferProps["onPageChange"]>;
        onScroll: import("vue").PropType<TdTransferProps["onScroll"]>;
        onSearch: import("vue").PropType<TdTransferProps["onSearch"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        empty: import("./type").EmptyType | import("./type").EmptyType[];
        search: boolean | import("..").InputProps | import("./type").SearchOption[];
        value: import("./type").TransferValue[];
        data: import("./type").DataOption[];
        title: import("./type").TitleType[] | ((h: typeof import("vue").h, props: {
            type: import("./type").TransferListType;
        }) => import("..").TNodeReturnValue);
        direction: "left" | "right" | "both";
        disabled: boolean | boolean[];
        checked: import("./type").TransferValue[];
        defaultValue: import("./type").TransferValue[];
        modelValue: import("./type").TransferValue[];
        defaultChecked: import("./type").TransferValue[];
        showCheckAll: boolean | boolean[];
        targetSort: "push" | "unshift" | "original";
        targetDraggable: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    checkboxProps: {
        type: import("vue").PropType<TdTransferProps["checkboxProps"]>;
    };
    checked: {
        type: import("vue").PropType<TdTransferProps["checked"]>;
        default: TdTransferProps["checked"];
    };
    defaultChecked: {
        type: import("vue").PropType<TdTransferProps["defaultChecked"]>;
        default: () => TdTransferProps["defaultChecked"];
    };
    data: {
        type: import("vue").PropType<TdTransferProps["data"]>;
        default: () => TdTransferProps["data"];
    };
    direction: {
        type: import("vue").PropType<TdTransferProps["direction"]>;
        default: TdTransferProps["direction"];
        validator(val: TdTransferProps["direction"]): boolean;
    };
    disabled: {
        type: import("vue").PropType<TdTransferProps["disabled"]>;
        default: any;
    };
    empty: {
        type: import("vue").PropType<TdTransferProps["empty"]>;
        default: TdTransferProps["empty"];
    };
    footer: {
        type: import("vue").PropType<TdTransferProps["footer"]>;
    };
    keys: {
        type: import("vue").PropType<TdTransferProps["keys"]>;
    };
    operation: {
        type: import("vue").PropType<TdTransferProps["operation"]>;
    };
    pagination: {
        type: import("vue").PropType<TdTransferProps["pagination"]>;
    };
    search: {
        type: import("vue").PropType<TdTransferProps["search"]>;
        default: TdTransferProps["search"];
    };
    showCheckAll: {
        type: import("vue").PropType<TdTransferProps["showCheckAll"]>;
        default: TdTransferProps["showCheckAll"];
    };
    targetDraggable: BooleanConstructor;
    targetSort: {
        type: import("vue").PropType<TdTransferProps["targetSort"]>;
        default: TdTransferProps["targetSort"];
        validator(val: TdTransferProps["targetSort"]): boolean;
    };
    title: {
        type: import("vue").PropType<TdTransferProps["title"]>;
        default: () => TdTransferProps["title"];
    };
    transferItem: {
        type: import("vue").PropType<TdTransferProps["transferItem"]>;
    };
    value: {
        type: import("vue").PropType<TdTransferProps["value"]>;
        default: TdTransferProps["value"];
    };
    modelValue: {
        type: import("vue").PropType<TdTransferProps["value"]>;
        default: TdTransferProps["value"];
    };
    defaultValue: {
        type: import("vue").PropType<TdTransferProps["defaultValue"]>;
        default: () => TdTransferProps["defaultValue"];
    };
    onChange: import("vue").PropType<TdTransferProps["onChange"]>;
    onCheckedChange: import("vue").PropType<TdTransferProps["onCheckedChange"]>;
    onPageChange: import("vue").PropType<TdTransferProps["onPageChange"]>;
    onScroll: import("vue").PropType<TdTransferProps["onScroll"]>;
    onSearch: import("vue").PropType<TdTransferProps["onSearch"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    empty: import("./type").EmptyType | import("./type").EmptyType[];
    search: boolean | import("..").InputProps | import("./type").SearchOption[];
    value: import("./type").TransferValue[];
    data: import("./type").DataOption[];
    title: import("./type").TitleType[] | ((h: typeof import("vue").h, props: {
        type: import("./type").TransferListType;
    }) => import("..").TNodeReturnValue);
    direction: "left" | "right" | "both";
    disabled: boolean | boolean[];
    checked: import("./type").TransferValue[];
    defaultValue: import("./type").TransferValue[];
    modelValue: import("./type").TransferValue[];
    defaultChecked: import("./type").TransferValue[];
    showCheckAll: boolean | boolean[];
    targetSort: "push" | "unshift" | "original";
    targetDraggable: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Transfer;
