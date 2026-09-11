import { TdSkeletonProps } from './type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    animation: {
        type: import("vue").PropType<TdSkeletonProps["animation"]>;
        default: TdSkeletonProps["animation"];
        validator(val: TdSkeletonProps["animation"]): boolean;
    };
    content: {
        type: import("vue").PropType<TdSkeletonProps["content"]>;
    };
    default: {
        type: import("vue").PropType<TdSkeletonProps["default"]>;
    };
    delay: {
        type: NumberConstructor;
        default: number;
    };
    loading: {
        type: BooleanConstructor;
        default: boolean;
    };
    rowCol: {
        type: import("vue").PropType<TdSkeletonProps["rowCol"]>;
    };
    theme: {
        type: import("vue").PropType<TdSkeletonProps["theme"]>;
        default: TdSkeletonProps["theme"];
        validator(val: TdSkeletonProps["theme"]): boolean;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    animation: {
        type: import("vue").PropType<TdSkeletonProps["animation"]>;
        default: TdSkeletonProps["animation"];
        validator(val: TdSkeletonProps["animation"]): boolean;
    };
    content: {
        type: import("vue").PropType<TdSkeletonProps["content"]>;
    };
    default: {
        type: import("vue").PropType<TdSkeletonProps["default"]>;
    };
    delay: {
        type: NumberConstructor;
        default: number;
    };
    loading: {
        type: BooleanConstructor;
        default: boolean;
    };
    rowCol: {
        type: import("vue").PropType<TdSkeletonProps["rowCol"]>;
    };
    theme: {
        type: import("vue").PropType<TdSkeletonProps["theme"]>;
        default: TdSkeletonProps["theme"];
        validator(val: TdSkeletonProps["theme"]): boolean;
    };
}>> & Readonly<{}>, {
    loading: boolean;
    theme: "avatar" | "article" | "text" | "tab" | "paragraph" | "avatar-text";
    animation: "none" | "gradient" | "flashed";
    delay: number;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
