import { TdSwiperProps } from './type';
import './style';
export * from './type';
export type SwiperProps = TdSwiperProps;
export declare const Swiper: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        animation: {
            type: import("vue").PropType<TdSwiperProps["animation"]>;
            default: TdSwiperProps["animation"];
            validator(val: TdSwiperProps["animation"]): boolean;
        };
        autoplay: {
            type: BooleanConstructor;
            default: boolean;
        };
        cardScale: {
            type: NumberConstructor;
            default: number;
        };
        current: {
            type: NumberConstructor;
            default: number;
        };
        defaultCurrent: {
            type: NumberConstructor;
            default: number;
        };
        direction: {
            type: import("vue").PropType<TdSwiperProps["direction"]>;
            default: TdSwiperProps["direction"];
            validator(val: TdSwiperProps["direction"]): boolean;
        };
        duration: {
            type: NumberConstructor;
            default: number;
        };
        height: {
            type: NumberConstructor;
        };
        interval: {
            type: NumberConstructor;
            default: number;
        };
        loop: {
            type: BooleanConstructor;
            default: boolean;
        };
        navigation: {
            type: import("vue").PropType<TdSwiperProps["navigation"]>;
        };
        stopOnHover: {
            type: BooleanConstructor;
            default: boolean;
        };
        trigger: {
            type: import("vue").PropType<TdSwiperProps["trigger"]>;
            default: TdSwiperProps["trigger"];
            validator(val: TdSwiperProps["trigger"]): boolean;
        };
        type: {
            type: import("vue").PropType<TdSwiperProps["type"]>;
            default: TdSwiperProps["type"];
            validator(val: TdSwiperProps["type"]): boolean;
        };
        onChange: import("vue").PropType<TdSwiperProps["onChange"]>;
    }>> & Readonly<{
        "onUpdate:current"?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "update:current"[], import("vue").PublicProps, {
        type: "card" | "default";
        direction: "vertical" | "horizontal";
        duration: number;
        animation: "fade" | "slide";
        current: number;
        trigger: "click" | "hover";
        defaultCurrent: number;
        autoplay: boolean;
        cardScale: number;
        interval: number;
        loop: boolean;
        stopOnHover: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        animation: {
            type: import("vue").PropType<TdSwiperProps["animation"]>;
            default: TdSwiperProps["animation"];
            validator(val: TdSwiperProps["animation"]): boolean;
        };
        autoplay: {
            type: BooleanConstructor;
            default: boolean;
        };
        cardScale: {
            type: NumberConstructor;
            default: number;
        };
        current: {
            type: NumberConstructor;
            default: number;
        };
        defaultCurrent: {
            type: NumberConstructor;
            default: number;
        };
        direction: {
            type: import("vue").PropType<TdSwiperProps["direction"]>;
            default: TdSwiperProps["direction"];
            validator(val: TdSwiperProps["direction"]): boolean;
        };
        duration: {
            type: NumberConstructor;
            default: number;
        };
        height: {
            type: NumberConstructor;
        };
        interval: {
            type: NumberConstructor;
            default: number;
        };
        loop: {
            type: BooleanConstructor;
            default: boolean;
        };
        navigation: {
            type: import("vue").PropType<TdSwiperProps["navigation"]>;
        };
        stopOnHover: {
            type: BooleanConstructor;
            default: boolean;
        };
        trigger: {
            type: import("vue").PropType<TdSwiperProps["trigger"]>;
            default: TdSwiperProps["trigger"];
            validator(val: TdSwiperProps["trigger"]): boolean;
        };
        type: {
            type: import("vue").PropType<TdSwiperProps["type"]>;
            default: TdSwiperProps["type"];
            validator(val: TdSwiperProps["type"]): boolean;
        };
        onChange: import("vue").PropType<TdSwiperProps["onChange"]>;
    }>> & Readonly<{
        "onUpdate:current"?: (...args: any[]) => any;
    }>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        type: "card" | "default";
        direction: "vertical" | "horizontal";
        duration: number;
        animation: "fade" | "slide";
        current: number;
        trigger: "click" | "hover";
        defaultCurrent: number;
        autoplay: boolean;
        cardScale: number;
        interval: number;
        loop: boolean;
        stopOnHover: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    animation: {
        type: import("vue").PropType<TdSwiperProps["animation"]>;
        default: TdSwiperProps["animation"];
        validator(val: TdSwiperProps["animation"]): boolean;
    };
    autoplay: {
        type: BooleanConstructor;
        default: boolean;
    };
    cardScale: {
        type: NumberConstructor;
        default: number;
    };
    current: {
        type: NumberConstructor;
        default: number;
    };
    defaultCurrent: {
        type: NumberConstructor;
        default: number;
    };
    direction: {
        type: import("vue").PropType<TdSwiperProps["direction"]>;
        default: TdSwiperProps["direction"];
        validator(val: TdSwiperProps["direction"]): boolean;
    };
    duration: {
        type: NumberConstructor;
        default: number;
    };
    height: {
        type: NumberConstructor;
    };
    interval: {
        type: NumberConstructor;
        default: number;
    };
    loop: {
        type: BooleanConstructor;
        default: boolean;
    };
    navigation: {
        type: import("vue").PropType<TdSwiperProps["navigation"]>;
    };
    stopOnHover: {
        type: BooleanConstructor;
        default: boolean;
    };
    trigger: {
        type: import("vue").PropType<TdSwiperProps["trigger"]>;
        default: TdSwiperProps["trigger"];
        validator(val: TdSwiperProps["trigger"]): boolean;
    };
    type: {
        type: import("vue").PropType<TdSwiperProps["type"]>;
        default: TdSwiperProps["type"];
        validator(val: TdSwiperProps["type"]): boolean;
    };
    onChange: import("vue").PropType<TdSwiperProps["onChange"]>;
}>> & Readonly<{
    "onUpdate:current"?: (...args: any[]) => any;
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, "update:current"[], "update:current", {
    type: "card" | "default";
    direction: "vertical" | "horizontal";
    duration: number;
    animation: "fade" | "slide";
    current: number;
    trigger: "click" | "hover";
    defaultCurrent: number;
    autoplay: boolean;
    cardScale: number;
    interval: number;
    loop: boolean;
    stopOnHover: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const SwiperItem: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        index: {
            type: NumberConstructor;
        };
        currentIndex: {
            type: NumberConstructor;
        };
        isSwitching: {
            type: BooleanConstructor;
            default: boolean;
        };
        swiperWidth: {
            type: NumberConstructor;
            default: number;
        };
        swiperItemLength: {
            type: NumberConstructor;
            default: number;
        };
        animation: {
            type: import("vue").PropType<TdSwiperProps["animation"]>;
            default: TdSwiperProps["animation"];
            validator(val: TdSwiperProps["animation"]): boolean;
        };
        autoplay: {
            type: BooleanConstructor;
            default: boolean;
        };
        cardScale: {
            type: NumberConstructor;
            default: number;
        };
        current: {
            type: NumberConstructor;
            default: number;
        };
        defaultCurrent: {
            type: NumberConstructor;
            default: number;
        };
        direction: {
            type: import("vue").PropType<TdSwiperProps["direction"]>;
            default: TdSwiperProps["direction"];
            validator(val: TdSwiperProps["direction"]): boolean;
        };
        duration: {
            type: NumberConstructor;
            default: number;
        };
        height: {
            type: NumberConstructor;
        };
        interval: {
            type: NumberConstructor;
            default: number;
        };
        loop: {
            type: BooleanConstructor;
            default: boolean;
        };
        navigation: {
            type: import("vue").PropType<TdSwiperProps["navigation"]>;
        };
        stopOnHover: {
            type: BooleanConstructor;
            default: boolean;
        };
        trigger: {
            type: import("vue").PropType<TdSwiperProps["trigger"]>;
            default: TdSwiperProps["trigger"];
            validator(val: TdSwiperProps["trigger"]): boolean;
        };
        type: {
            type: import("vue").PropType<TdSwiperProps["type"]>;
            default: TdSwiperProps["type"];
            validator(val: TdSwiperProps["type"]): boolean;
        };
        onChange: import("vue").PropType<TdSwiperProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        type: "card" | "default";
        direction: "vertical" | "horizontal";
        duration: number;
        animation: "fade" | "slide";
        current: number;
        trigger: "click" | "hover";
        defaultCurrent: number;
        autoplay: boolean;
        isSwitching: boolean;
        swiperWidth: number;
        swiperItemLength: number;
        cardScale: number;
        interval: number;
        loop: boolean;
        stopOnHover: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        index: {
            type: NumberConstructor;
        };
        currentIndex: {
            type: NumberConstructor;
        };
        isSwitching: {
            type: BooleanConstructor;
            default: boolean;
        };
        swiperWidth: {
            type: NumberConstructor;
            default: number;
        };
        swiperItemLength: {
            type: NumberConstructor;
            default: number;
        };
        animation: {
            type: import("vue").PropType<TdSwiperProps["animation"]>;
            default: TdSwiperProps["animation"];
            validator(val: TdSwiperProps["animation"]): boolean;
        };
        autoplay: {
            type: BooleanConstructor;
            default: boolean;
        };
        cardScale: {
            type: NumberConstructor;
            default: number;
        };
        current: {
            type: NumberConstructor;
            default: number;
        };
        defaultCurrent: {
            type: NumberConstructor;
            default: number;
        };
        direction: {
            type: import("vue").PropType<TdSwiperProps["direction"]>;
            default: TdSwiperProps["direction"];
            validator(val: TdSwiperProps["direction"]): boolean;
        };
        duration: {
            type: NumberConstructor;
            default: number;
        };
        height: {
            type: NumberConstructor;
        };
        interval: {
            type: NumberConstructor;
            default: number;
        };
        loop: {
            type: BooleanConstructor;
            default: boolean;
        };
        navigation: {
            type: import("vue").PropType<TdSwiperProps["navigation"]>;
        };
        stopOnHover: {
            type: BooleanConstructor;
            default: boolean;
        };
        trigger: {
            type: import("vue").PropType<TdSwiperProps["trigger"]>;
            default: TdSwiperProps["trigger"];
            validator(val: TdSwiperProps["trigger"]): boolean;
        };
        type: {
            type: import("vue").PropType<TdSwiperProps["type"]>;
            default: TdSwiperProps["type"];
            validator(val: TdSwiperProps["type"]): boolean;
        };
        onChange: import("vue").PropType<TdSwiperProps["onChange"]>;
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        type: "card" | "default";
        direction: "vertical" | "horizontal";
        duration: number;
        animation: "fade" | "slide";
        current: number;
        trigger: "click" | "hover";
        defaultCurrent: number;
        autoplay: boolean;
        isSwitching: boolean;
        swiperWidth: number;
        swiperItemLength: number;
        cardScale: number;
        interval: number;
        loop: boolean;
        stopOnHover: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    index: {
        type: NumberConstructor;
    };
    currentIndex: {
        type: NumberConstructor;
    };
    isSwitching: {
        type: BooleanConstructor;
        default: boolean;
    };
    swiperWidth: {
        type: NumberConstructor;
        default: number;
    };
    swiperItemLength: {
        type: NumberConstructor;
        default: number;
    };
    animation: {
        type: import("vue").PropType<TdSwiperProps["animation"]>;
        default: TdSwiperProps["animation"];
        validator(val: TdSwiperProps["animation"]): boolean;
    };
    autoplay: {
        type: BooleanConstructor;
        default: boolean;
    };
    cardScale: {
        type: NumberConstructor;
        default: number;
    };
    current: {
        type: NumberConstructor;
        default: number;
    };
    defaultCurrent: {
        type: NumberConstructor;
        default: number;
    };
    direction: {
        type: import("vue").PropType<TdSwiperProps["direction"]>;
        default: TdSwiperProps["direction"];
        validator(val: TdSwiperProps["direction"]): boolean;
    };
    duration: {
        type: NumberConstructor;
        default: number;
    };
    height: {
        type: NumberConstructor;
    };
    interval: {
        type: NumberConstructor;
        default: number;
    };
    loop: {
        type: BooleanConstructor;
        default: boolean;
    };
    navigation: {
        type: import("vue").PropType<TdSwiperProps["navigation"]>;
    };
    stopOnHover: {
        type: BooleanConstructor;
        default: boolean;
    };
    trigger: {
        type: import("vue").PropType<TdSwiperProps["trigger"]>;
        default: TdSwiperProps["trigger"];
        validator(val: TdSwiperProps["trigger"]): boolean;
    };
    type: {
        type: import("vue").PropType<TdSwiperProps["type"]>;
        default: TdSwiperProps["type"];
        validator(val: TdSwiperProps["type"]): boolean;
    };
    onChange: import("vue").PropType<TdSwiperProps["onChange"]>;
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    type: "card" | "default";
    direction: "vertical" | "horizontal";
    duration: number;
    animation: "fade" | "slide";
    current: number;
    trigger: "click" | "hover";
    defaultCurrent: number;
    autoplay: boolean;
    isSwitching: boolean;
    swiperWidth: number;
    swiperItemLength: number;
    cardScale: number;
    interval: number;
    loop: boolean;
    stopOnHover: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Swiper;
