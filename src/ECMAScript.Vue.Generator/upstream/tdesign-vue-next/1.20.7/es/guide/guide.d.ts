import { GuideStep } from './type';
declare const _default: import("vue").DefineComponent<{
    counter?: import("..").TNode<{
        current: number;
        total: number;
    }>;
    current?: number;
    defaultCurrent?: number;
    modelValue?: number;
    finishButtonProps?: import("..").ButtonProps;
    hideCounter?: boolean;
    hidePrev?: boolean;
    hideSkip?: boolean;
    highlightPadding?: number;
    mode?: "popup" | "dialog";
    nextButtonProps?: import("..").ButtonProps;
    prevButtonProps?: import("..").ButtonProps;
    showOverlay?: boolean;
    skipButtonProps?: import("..").ButtonProps;
    steps?: Array<GuideStep>;
    zIndex?: number;
    onChange?: (current: number, context?: {
        e: MouseEvent;
        total: number;
    }) => void;
    onFinish?: (context: {
        e: MouseEvent;
        current: number;
        total: number;
    }) => void;
    onNextStepClick?: (context: {
        e: MouseEvent;
        next: number;
        current: number;
        total: number;
    }) => void;
    onPrevStepClick?: (context: {
        e: MouseEvent;
        prev: number;
        current: number;
        total: number;
    }) => void;
    onSkip?: (context: {
        e: MouseEvent;
        current: number;
        total: number;
    }) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    counter?: import("..").TNode<{
        current: number;
        total: number;
    }>;
    current?: number;
    defaultCurrent?: number;
    modelValue?: number;
    finishButtonProps?: import("..").ButtonProps;
    hideCounter?: boolean;
    hidePrev?: boolean;
    hideSkip?: boolean;
    highlightPadding?: number;
    mode?: "popup" | "dialog";
    nextButtonProps?: import("..").ButtonProps;
    prevButtonProps?: import("..").ButtonProps;
    showOverlay?: boolean;
    skipButtonProps?: import("..").ButtonProps;
    steps?: Array<GuideStep>;
    zIndex?: number;
    onChange?: (current: number, context?: {
        e: MouseEvent;
        total: number;
    }) => void;
    onFinish?: (context: {
        e: MouseEvent;
        current: number;
        total: number;
    }) => void;
    onNextStepClick?: (context: {
        e: MouseEvent;
        next: number;
        current: number;
        total: number;
    }) => void;
    onPrevStepClick?: (context: {
        e: MouseEvent;
        prev: number;
        current: number;
        total: number;
    }) => void;
    onSkip?: (context: {
        e: MouseEvent;
        current: number;
        total: number;
    }) => void;
}> & Readonly<{}>, {
    mode: "dialog" | "popup";
    modelValue: number;
    current: number;
    zIndex: number;
    showOverlay: boolean;
    highlightPadding: number;
    hideCounter: boolean;
    hidePrev: boolean;
    hideSkip: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
