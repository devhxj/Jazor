import { TdTagInputProps } from '../type';
export declare function useTagScroll(props: TdTagInputProps): {
    tagInputRef: import("vue").Ref<any, any>;
    scrollElement: import("vue").Ref<HTMLElement, HTMLElement>;
    scrollDistance: import("vue").Ref<number, number>;
    scrollTo: (distance: number) => void;
    scrollToRight: () => void;
    scrollToLeft: () => void;
    updateScrollElement: (element: HTMLElement) => void;
    updateScrollDistance: () => void;
    onWheel: ({ e }: {
        e: WheelEvent;
    }) => void;
    scrollToRightOnEnter: () => void;
    scrollToLeftOnLeave: () => void;
    isScrollable: import("vue").Ref<boolean, boolean>;
};
