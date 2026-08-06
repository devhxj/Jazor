import { TdSelectInputProps } from '../type';
import { PopupVisibleChangeContext } from '../../popup';
export type overlayInnerStyleProps = Pick<TdSelectInputProps, 'popupProps' | 'autoWidth' | 'readonly' | 'onPopupVisibleChange' | 'allowInput' | 'popupVisible'> & {
    disabled?: boolean | Array<boolean>;
};
export declare function useOverlayInnerStyle(props: overlayInnerStyleProps): {
    tOverlayInnerStyle: import("vue").ComputedRef<import("../..").Styles | ((triggerElement: HTMLElement, popupElement: HTMLElement) => import("../..").Styles)>;
    innerPopupVisible: import("vue").Ref<boolean>;
    onInnerPopupVisibleChange: (visible: boolean, ctx: PopupVisibleChangeContext) => void;
};
