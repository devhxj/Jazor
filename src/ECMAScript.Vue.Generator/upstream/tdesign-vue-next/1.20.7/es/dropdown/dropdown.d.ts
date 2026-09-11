import { DropdownOption } from './type';
declare const _default: import("vue").DefineComponent<{
    direction?: "left" | "right";
    disabled?: boolean;
    hideAfterItemClick?: boolean;
    maxColumnWidth?: string | number;
    maxHeight?: number;
    minColumnWidth?: string | number;
    options?: Array<DropdownOption>;
    panelBottomContent?: string | import("..").TNode;
    panelTopContent?: string | import("..").TNode;
    placement?: "top" | "left" | "right" | "bottom" | "top-left" | "top-right" | "bottom-left" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
    popupProps?: import("..").PopupProps;
    trigger?: "hover" | "click" | "focus" | "context-menu";
    onClick?: (dropdownItem: import("./type").TdDropdownItemProps["value"], context: {
        e: MouseEvent;
    }) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    direction?: "left" | "right";
    disabled?: boolean;
    hideAfterItemClick?: boolean;
    maxColumnWidth?: string | number;
    maxHeight?: number;
    minColumnWidth?: string | number;
    options?: Array<DropdownOption>;
    panelBottomContent?: string | import("..").TNode;
    panelTopContent?: string | import("..").TNode;
    placement?: "top" | "left" | "right" | "bottom" | "top-left" | "top-right" | "bottom-left" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
    popupProps?: import("..").PopupProps;
    trigger?: "hover" | "click" | "focus" | "context-menu";
    onClick?: (dropdownItem: import("./type").TdDropdownItemProps["value"], context: {
        e: MouseEvent;
    }) => void;
}> & Readonly<{}>, {
    maxHeight: number;
    direction: "left" | "right";
    disabled: boolean;
    options: DropdownOption[];
    placement: "left" | "right" | "top" | "bottom" | "top-left" | "bottom-left" | "top-right" | "bottom-right" | "left-top" | "left-bottom" | "right-top" | "right-bottom";
    trigger: "click" | "focus" | "hover" | "context-menu";
    maxColumnWidth: string | number;
    minColumnWidth: string | number;
    hideAfterItemClick: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
