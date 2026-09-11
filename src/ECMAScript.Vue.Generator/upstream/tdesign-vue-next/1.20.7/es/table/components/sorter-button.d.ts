import { PropType } from 'vue';
import { SortType } from '../type';
import { TooltipProps } from '../../tooltip';
import type { TNode } from '../../common';
import type { TableConfig } from '../../config-provider';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    sortType: {
        type: PropType<SortType>;
        default: string;
    };
    sortOrder: {
        type: StringConstructor;
        default: () => string;
    };
    locale: PropType<TableConfig>;
    sortIcon: PropType<TNode>;
    tooltipProps: PropType<TooltipProps>;
    hideSortTips: BooleanConstructor;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "sort-icon-click"[], "sort-icon-click", import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    sortType: {
        type: PropType<SortType>;
        default: string;
    };
    sortOrder: {
        type: StringConstructor;
        default: () => string;
    };
    locale: PropType<TableConfig>;
    sortIcon: PropType<TNode>;
    tooltipProps: PropType<TooltipProps>;
    hideSortTips: BooleanConstructor;
}>> & Readonly<{
    "onSort-icon-click"?: (...args: any[]) => any;
}>, {
    hideSortTips: boolean;
    sortType: SortType;
    sortOrder: string;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
