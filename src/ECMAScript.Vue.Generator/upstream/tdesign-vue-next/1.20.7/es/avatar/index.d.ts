import { TdAvatarProps, TdAvatarGroupProps } from './type';
import './style';
export * from './type';
export type AvatarProps = TdAvatarProps;
export type AvatarGroupProps = TdAvatarGroupProps;
export declare const Avatar: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<{
        alt?: string;
        content?: string | import("..").TNode;
        default?: string | import("..").TNode;
        hideOnLoadFailed?: boolean;
        icon?: import("..").TNode;
        image?: string;
        imageProps?: import("..").ImageProps;
        shape?: import("..").ShapeEnum;
        size?: string;
        onError?: (context: {
            e: Event;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        image: string;
        size: string;
        shape: import("..").ShapeEnum;
        alt: string;
        hideOnLoadFailed: boolean;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<{
        alt?: string;
        content?: string | import("..").TNode;
        default?: string | import("..").TNode;
        hideOnLoadFailed?: boolean;
        icon?: import("..").TNode;
        image?: string;
        imageProps?: import("..").ImageProps;
        shape?: import("..").ShapeEnum;
        size?: string;
        onError?: (context: {
            e: Event;
        }) => void;
    }> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        image: string;
        size: string;
        shape: import("..").ShapeEnum;
        alt: string;
        hideOnLoadFailed: boolean;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<{
    alt?: string;
    content?: string | import("..").TNode;
    default?: string | import("..").TNode;
    hideOnLoadFailed?: boolean;
    icon?: import("..").TNode;
    image?: string;
    imageProps?: import("..").ImageProps;
    shape?: import("..").ShapeEnum;
    size?: string;
    onError?: (context: {
        e: Event;
    }) => void;
}> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    image: string;
    size: string;
    shape: import("..").ShapeEnum;
    alt: string;
    hideOnLoadFailed: boolean;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export declare const AvatarGroup: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        cascading: {
            type: import("vue").PropType<TdAvatarGroupProps["cascading"]>;
            default: TdAvatarGroupProps["cascading"];
            validator(val: TdAvatarGroupProps["cascading"]): boolean;
        };
        collapseAvatar: {
            type: import("vue").PropType<TdAvatarGroupProps["collapseAvatar"]>;
        };
        max: {
            type: NumberConstructor;
        };
        popupProps: {
            type: import("vue").PropType<TdAvatarGroupProps["popupProps"]>;
        };
        size: {
            type: StringConstructor;
            default: string;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {
        size: string;
        cascading: import("./type").CascadingValue;
    }, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        cascading: {
            type: import("vue").PropType<TdAvatarGroupProps["cascading"]>;
            default: TdAvatarGroupProps["cascading"];
            validator(val: TdAvatarGroupProps["cascading"]): boolean;
        };
        collapseAvatar: {
            type: import("vue").PropType<TdAvatarGroupProps["collapseAvatar"]>;
        };
        max: {
            type: NumberConstructor;
        };
        popupProps: {
            type: import("vue").PropType<TdAvatarGroupProps["popupProps"]>;
        };
        size: {
            type: StringConstructor;
            default: string;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {
        size: string;
        cascading: import("./type").CascadingValue;
    }>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    cascading: {
        type: import("vue").PropType<TdAvatarGroupProps["cascading"]>;
        default: TdAvatarGroupProps["cascading"];
        validator(val: TdAvatarGroupProps["cascading"]): boolean;
    };
    collapseAvatar: {
        type: import("vue").PropType<TdAvatarGroupProps["collapseAvatar"]>;
    };
    max: {
        type: NumberConstructor;
    };
    popupProps: {
        type: import("vue").PropType<TdAvatarGroupProps["popupProps"]>;
    };
    size: {
        type: StringConstructor;
        default: string;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {
    size: string;
    cascading: import("./type").CascadingValue;
}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Avatar;
