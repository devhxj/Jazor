import { TdCommentProps } from './type';
import './style';
export * from './type';
export type CommentProps = TdCommentProps;
export declare const Comment: {
    new (...args: any[]): import("vue").CreateComponentPublicInstanceWithMixins<Readonly<import("vue").ExtractPropTypes<{
        actions: {
            type: import("vue").PropType<TdCommentProps["actions"]>;
        };
        author: {
            type: import("vue").PropType<TdCommentProps["author"]>;
        };
        avatar: {
            type: import("vue").PropType<TdCommentProps["avatar"]>;
        };
        content: {
            type: import("vue").PropType<TdCommentProps["content"]>;
        };
        datetime: {
            type: import("vue").PropType<TdCommentProps["datetime"]>;
        };
        quote: {
            type: import("vue").PropType<TdCommentProps["quote"]>;
        };
        reply: {
            type: import("vue").PropType<TdCommentProps["reply"]>;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, import("vue").PublicProps, {}, true, {}, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, {}, any, import("vue").ComponentProvideOptions, {
        P: {};
        B: {};
        D: {};
        C: {};
        M: {};
        Defaults: {};
    }, Readonly<import("vue").ExtractPropTypes<{
        actions: {
            type: import("vue").PropType<TdCommentProps["actions"]>;
        };
        author: {
            type: import("vue").PropType<TdCommentProps["author"]>;
        };
        avatar: {
            type: import("vue").PropType<TdCommentProps["avatar"]>;
        };
        content: {
            type: import("vue").PropType<TdCommentProps["content"]>;
        };
        datetime: {
            type: import("vue").PropType<TdCommentProps["datetime"]>;
        };
        quote: {
            type: import("vue").PropType<TdCommentProps["quote"]>;
        };
        reply: {
            type: import("vue").PropType<TdCommentProps["reply"]>;
        };
    }>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, {}>;
    __isFragment?: never;
    __isTeleport?: never;
    __isSuspense?: never;
} & import("vue").ComponentOptionsBase<Readonly<import("vue").ExtractPropTypes<{
    actions: {
        type: import("vue").PropType<TdCommentProps["actions"]>;
    };
    author: {
        type: import("vue").PropType<TdCommentProps["author"]>;
    };
    avatar: {
        type: import("vue").PropType<TdCommentProps["avatar"]>;
    };
    content: {
        type: import("vue").PropType<TdCommentProps["content"]>;
    };
    datetime: {
        type: import("vue").PropType<TdCommentProps["datetime"]>;
    };
    quote: {
        type: import("vue").PropType<TdCommentProps["quote"]>;
    };
    reply: {
        type: import("vue").PropType<TdCommentProps["reply"]>;
    };
}>> & Readonly<{}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, {}, {}, string, {}, import("vue").GlobalComponents, import("vue").GlobalDirectives, string, import("vue").ComponentProvideOptions> & import("vue").VNodeProps & import("vue").AllowedComponentProps & import("vue").ComponentCustomProps & import("vue").Plugin;
export default Comment;
