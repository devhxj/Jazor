import { TdPaginationProps, TdPaginationMiniProps } from './type';
import './style';
export * from './type';
export type PaginationProps = TdPaginationProps;
export type PaginationMiniProps = TdPaginationMiniProps;
export declare const Pagination: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        current?: number;
        defaultCurrent?: number;
        modelValue?: number;
        disabled?: boolean;
        foldedMaxPageBtn?: number;
        maxPageBtn?: number;
        pageEllipsisMode?: "mid" | "both-ends";
        pageSize?: number;
        defaultPageSize?: number;
        pageSizeOptions?: Array<number | {
            label: string;
            value: number;
        }>;
        selectProps?: import("..").SelectProps;
        showFirstAndLastPageBtn?: boolean;
        showJumper?: boolean;
        showPageNumber?: boolean;
        showPageSize?: boolean;
        showPreviousAndNextBtn?: boolean;
        size?: "small" | "medium";
        theme?: "default" | "simple";
        total?: number;
        totalContent?: boolean | import("..").TNode;
        onChange?: (pageInfo: import("./type").PageInfo) => void;
        onCurrentChange?: (current: number, pageInfo: import("./type").PageInfo) => void;
        onPageSizeChange?: (pageSize: number, pageInfo: import("./type").PageInfo) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        size: "small" | "medium";
        disabled: boolean;
        theme: "default" | "simple";
        modelValue: number;
        current: number;
        total: number;
        pageEllipsisMode: "mid" | "both-ends";
        pageSizeOptions: (number | {
            label: string;
            value: number;
        })[];
        totalContent: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        defaultCurrent: number;
        foldedMaxPageBtn: number;
        maxPageBtn: number;
        pageSize: number;
        defaultPageSize: number;
        showFirstAndLastPageBtn: boolean;
        showJumper: boolean;
        showPageNumber: boolean;
        showPageSize: boolean;
        showPreviousAndNextBtn: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        current?: number;
        defaultCurrent?: number;
        modelValue?: number;
        disabled?: boolean;
        foldedMaxPageBtn?: number;
        maxPageBtn?: number;
        pageEllipsisMode?: "mid" | "both-ends";
        pageSize?: number;
        defaultPageSize?: number;
        pageSizeOptions?: Array<number | {
            label: string;
            value: number;
        }>;
        selectProps?: import("..").SelectProps;
        showFirstAndLastPageBtn?: boolean;
        showJumper?: boolean;
        showPageNumber?: boolean;
        showPageSize?: boolean;
        showPreviousAndNextBtn?: boolean;
        size?: "small" | "medium";
        theme?: "default" | "simple";
        total?: number;
        totalContent?: boolean | import("..").TNode;
        onChange?: (pageInfo: import("./type").PageInfo) => void;
        onCurrentChange?: (current: number, pageInfo: import("./type").PageInfo) => void;
        onPageSizeChange?: (pageSize: number, pageInfo: import("./type").PageInfo) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        size: "small" | "medium";
        disabled: boolean;
        theme: "default" | "simple";
        modelValue: number;
        current: number;
        total: number;
        pageEllipsisMode: "mid" | "both-ends";
        pageSizeOptions: (number | {
            label: string;
            value: number;
        })[];
        totalContent: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
        defaultCurrent: number;
        foldedMaxPageBtn: number;
        maxPageBtn: number;
        pageSize: number;
        defaultPageSize: number;
        showFirstAndLastPageBtn: boolean;
        showJumper: boolean;
        showPageNumber: boolean;
        showPageSize: boolean;
        showPreviousAndNextBtn: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    current?: number;
    defaultCurrent?: number;
    modelValue?: number;
    disabled?: boolean;
    foldedMaxPageBtn?: number;
    maxPageBtn?: number;
    pageEllipsisMode?: "mid" | "both-ends";
    pageSize?: number;
    defaultPageSize?: number;
    pageSizeOptions?: Array<number | {
        label: string;
        value: number;
    }>;
    selectProps?: import("..").SelectProps;
    showFirstAndLastPageBtn?: boolean;
    showJumper?: boolean;
    showPageNumber?: boolean;
    showPageSize?: boolean;
    showPreviousAndNextBtn?: boolean;
    size?: "small" | "medium";
    theme?: "default" | "simple";
    total?: number;
    totalContent?: boolean | import("..").TNode;
    onChange?: (pageInfo: import("./type").PageInfo) => void;
    onCurrentChange?: (current: number, pageInfo: import("./type").PageInfo) => void;
    onPageSizeChange?: (pageSize: number, pageInfo: import("./type").PageInfo) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    size: "small" | "medium";
    disabled: boolean;
    theme: "default" | "simple";
    modelValue: number;
    current: number;
    total: number;
    pageEllipsisMode: "mid" | "both-ends";
    pageSizeOptions: (number | {
        label: string;
        value: number;
    })[];
    totalContent: boolean | ((h: typeof import("vue").h) => import("..").TNodeReturnValue);
    defaultCurrent: number;
    foldedMaxPageBtn: number;
    maxPageBtn: number;
    pageSize: number;
    defaultPageSize: number;
    showFirstAndLastPageBtn: boolean;
    showJumper: boolean;
    showPageNumber: boolean;
    showPageSize: boolean;
    showPreviousAndNextBtn: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const PaginationMini: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        disabled: {
            type: import("vue").PropType<TdPaginationMiniProps["disabled"]>;
        };
        layout: {
            type: import("vue").PropType<TdPaginationMiniProps["layout"]>;
            default: TdPaginationMiniProps["layout"];
            validator(val: TdPaginationMiniProps["layout"]): boolean;
        };
        showCurrent: {
            type: BooleanConstructor;
            default: boolean;
        };
        size: {
            type: import("vue").PropType<TdPaginationMiniProps["size"]>;
            default: TdPaginationMiniProps["size"];
            validator(val: TdPaginationMiniProps["size"]): boolean;
        };
        tips: {
            type: import("vue").PropType<TdPaginationMiniProps["tips"]>;
        };
        variant: {
            type: import("vue").PropType<TdPaginationMiniProps["variant"]>;
            default: TdPaginationMiniProps["variant"];
            validator(val: TdPaginationMiniProps["variant"]): boolean;
        };
        onChange: import("vue").PropType<TdPaginationMiniProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        layout: "vertical" | "horizontal";
        size: import("..").SizeEnum;
        variant: "text" | "outline";
        showCurrent: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        disabled: {
            type: import("vue").PropType<TdPaginationMiniProps["disabled"]>;
        };
        layout: {
            type: import("vue").PropType<TdPaginationMiniProps["layout"]>;
            default: TdPaginationMiniProps["layout"];
            validator(val: TdPaginationMiniProps["layout"]): boolean;
        };
        showCurrent: {
            type: BooleanConstructor;
            default: boolean;
        };
        size: {
            type: import("vue").PropType<TdPaginationMiniProps["size"]>;
            default: TdPaginationMiniProps["size"];
            validator(val: TdPaginationMiniProps["size"]): boolean;
        };
        tips: {
            type: import("vue").PropType<TdPaginationMiniProps["tips"]>;
        };
        variant: {
            type: import("vue").PropType<TdPaginationMiniProps["variant"]>;
            default: TdPaginationMiniProps["variant"];
            validator(val: TdPaginationMiniProps["variant"]): boolean;
        };
        onChange: import("vue").PropType<TdPaginationMiniProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        layout: "vertical" | "horizontal";
        size: import("..").SizeEnum;
        variant: "text" | "outline";
        showCurrent: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    disabled: {
        type: import("vue").PropType<TdPaginationMiniProps["disabled"]>;
    };
    layout: {
        type: import("vue").PropType<TdPaginationMiniProps["layout"]>;
        default: TdPaginationMiniProps["layout"];
        validator(val: TdPaginationMiniProps["layout"]): boolean;
    };
    showCurrent: {
        type: BooleanConstructor;
        default: boolean;
    };
    size: {
        type: import("vue").PropType<TdPaginationMiniProps["size"]>;
        default: TdPaginationMiniProps["size"];
        validator(val: TdPaginationMiniProps["size"]): boolean;
    };
    tips: {
        type: import("vue").PropType<TdPaginationMiniProps["tips"]>;
    };
    variant: {
        type: import("vue").PropType<TdPaginationMiniProps["variant"]>;
        default: TdPaginationMiniProps["variant"];
        validator(val: TdPaginationMiniProps["variant"]): boolean;
    };
    onChange: import("vue").PropType<TdPaginationMiniProps["onChange"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    layout: "vertical" | "horizontal";
    size: import("..").SizeEnum;
    variant: "text" | "outline";
    showCurrent: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Pagination;
