import { TdBaseTableProps } from '../type';
export default function useAffix(props: TdBaseTableProps): {
    showAffixHeader: import("vue").Ref<boolean, boolean>;
    showAffixFooter: import("vue").Ref<boolean, boolean>;
    showAffixPagination: import("vue").Ref<boolean, boolean>;
    affixHeaderRef: import("vue").Ref<HTMLDivElement, HTMLDivElement>;
    affixFooterRef: import("vue").Ref<HTMLDivElement, HTMLDivElement>;
    horizontalScrollbarRef: import("vue").Ref<HTMLDivElement, HTMLDivElement>;
    paginationRef: import("vue").Ref<HTMLDivElement, HTMLDivElement>;
    onHorizontalScroll: (scrollElement?: HTMLElement) => void;
    setTableContentRef: (tableContent: HTMLDivElement) => void;
    updateAffixHeaderOrFooter: () => void;
};
