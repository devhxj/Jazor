import { AvatarProps } from '../avatar';
import type { TNode } from '../common';
export interface TdCommentProps {
    actions?: Array<TNode> | TNode;
    author?: string | TNode;
    avatar?: string | AvatarProps | TNode;
    content?: string | TNode;
    datetime?: string | TNode;
    quote?: string | TNode;
    reply?: string | TNode;
}
