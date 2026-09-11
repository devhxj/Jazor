import { PropType } from 'vue';
import type { AttachNode, TNode } from '../../common';
import { TooltipProps } from '../../tooltip';
export interface EllipsisProps {
    content: string | TNode;
    default: string | TNode;
    tooltipContent: string | number | TNode;
    placement: TooltipProps['placement'];
    attach?: AttachNode;
    tooltipProps: TooltipProps;
    zIndex: number;
}
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    content: {
        type: PropType<EllipsisProps["content"]>;
    };
    default: {
        type: PropType<EllipsisProps["default"]>;
    };
    tooltipContent: {
        type: PropType<EllipsisProps["tooltipContent"]>;
    };
    placement: PropType<EllipsisProps["placement"]>;
    attach: PropType<EllipsisProps["attach"]>;
    tooltipProps: PropType<EllipsisProps["tooltipProps"]>;
    zIndex: NumberConstructor;
    overlayClassName: StringConstructor;
    classPrefix: {
        type: StringConstructor;
        default: string;
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    content: {
        type: PropType<EllipsisProps["content"]>;
    };
    default: {
        type: PropType<EllipsisProps["default"]>;
    };
    tooltipContent: {
        type: PropType<EllipsisProps["tooltipContent"]>;
    };
    placement: PropType<EllipsisProps["placement"]>;
    attach: PropType<EllipsisProps["attach"]>;
    tooltipProps: PropType<EllipsisProps["tooltipProps"]>;
    zIndex: NumberConstructor;
    overlayClassName: StringConstructor;
    classPrefix: {
        type: StringConstructor;
        default: string;
    };
}>> & Readonly<{}>, {
    classPrefix: string;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
