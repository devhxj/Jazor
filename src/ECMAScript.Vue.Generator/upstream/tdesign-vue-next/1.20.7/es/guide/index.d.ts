import { TdGuideProps, GuideStep } from './type';
import './style';
export type TdGuideStepProps = GuideStep;
export * from './type';
export type GuideProps = TdGuideProps;
export declare const Guide: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
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
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        mode: "dialog" | "popup";
        modelValue: number;
        current: number;
        zIndex: number;
        showOverlay: boolean;
        highlightPadding: number;
        hideCounter: boolean;
        hidePrev: boolean;
        hideSkip: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
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
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        mode: "dialog" | "popup";
        modelValue: number;
        current: number;
        zIndex: number;
        showOverlay: boolean;
        highlightPadding: number;
        hideCounter: boolean;
        hidePrev: boolean;
        hideSkip: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
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
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    mode: "dialog" | "popup";
    modelValue: number;
    current: number;
    zIndex: number;
    showOverlay: boolean;
    highlightPadding: number;
    hideCounter: boolean;
    hidePrev: boolean;
    hideSkip: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Guide;
