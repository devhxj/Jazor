import { TdBaseTableProps } from '../type';
import { RowAndColFixedPosition } from '../types';
export interface TFootProps {
    rowKey: string;
    isFixedHeader: boolean;
    rowAndColFixedPosition: RowAndColFixedPosition;
    footData: TdBaseTableProps['footData'];
    columns: TdBaseTableProps['columns'];
    rowAttributes: TdBaseTableProps['rowAttributes'];
    rowClassName: TdBaseTableProps['rowClassName'];
    thWidthList?: {
        [colKey: string]: number;
    };
    footerSummary?: TdBaseTableProps['footerSummary'];
    rowspanAndColspanInFooter: TdBaseTableProps['rowspanAndColspanInFooter'];
    virtualScroll?: boolean;
}
declare const _default: import("vue").DefineComponent<{
    rowKey: string;
    isFixedHeader: boolean;
    rowAndColFixedPosition: RowAndColFixedPosition;
    footData: TdBaseTableProps["footData"];
    columns: TdBaseTableProps["columns"];
    rowAttributes: TdBaseTableProps["rowAttributes"];
    rowClassName: TdBaseTableProps["rowClassName"];
    thWidthList?: {
        [colKey: string]: number;
    };
    footerSummary?: TdBaseTableProps["footerSummary"];
    rowspanAndColspanInFooter: TdBaseTableProps["rowspanAndColspanInFooter"];
    virtualScroll?: boolean;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    rowKey: string;
    isFixedHeader: boolean;
    rowAndColFixedPosition: RowAndColFixedPosition;
    footData: TdBaseTableProps["footData"];
    columns: TdBaseTableProps["columns"];
    rowAttributes: TdBaseTableProps["rowAttributes"];
    rowClassName: TdBaseTableProps["rowClassName"];
    thWidthList?: {
        [colKey: string]: number;
    };
    footerSummary?: TdBaseTableProps["footerSummary"];
    rowspanAndColspanInFooter: TdBaseTableProps["rowspanAndColspanInFooter"];
    virtualScroll?: boolean;
}> & Readonly<{}>, {
    isFixedHeader: boolean;
    virtualScroll: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
