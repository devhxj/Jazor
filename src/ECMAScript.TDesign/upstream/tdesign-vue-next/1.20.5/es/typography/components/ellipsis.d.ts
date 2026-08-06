import type { TypographyEllipsis } from '../type';
declare const _default: import("vue").DefineComponent<{
    renderCopy: FunctionConstructor;
    content: {
        type: import("vue").PropType<import("..").TdParagraphProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("..").TdParagraphProps["default"]>;
    };
    ellipsis: {
        type: import("vue").PropType<import("..").TdParagraphProps["ellipsis"]>;
        default: import("..").TdParagraphProps["ellipsis"];
    };
}, () => import("vue/jsx-runtime").JSX.Element, unknown, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps, Readonly<import("vue").ExtractPropTypes<{
    renderCopy: FunctionConstructor;
    content: {
        type: import("vue").PropType<import("..").TdParagraphProps["content"]>;
    };
    default: {
        type: import("vue").PropType<import("..").TdParagraphProps["default"]>;
    };
    ellipsis: {
        type: import("vue").PropType<import("..").TdParagraphProps["ellipsis"]>;
        default: import("..").TdParagraphProps["ellipsis"];
    };
}>>, {
    ellipsis: boolean | TypographyEllipsis;
}, {}>;
export default _default;
