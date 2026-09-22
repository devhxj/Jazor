import { Fragment, createCommentVNode, createStaticVNode, h } from "vue";

/**
 * Creates Vue framing for a dynamic Razor MarkupString.
 * 动态 payload 只求值一次；browser 计算真实 sibling count，SSR 仅保留 raw HTML。
 */
export function createRawMarkup(markup) {
    if (markup === null || markup === undefined || markup.length === 0) {
        return null;
    }

    const content = String(markup);
    const commentPrefix = splitLeadingComments(content);
    if (commentPrefix.comments.length == 0) {
        return createKeyedStaticVNode(content, countStaticNodes(content));
    }

    // Vue Static hydration accepts element/text as its first node, not an HTML comment. Keep
    // the rare comment-first shape in a keyed Fragment; the lexical split is identical in SSR
    // and browser so hydration sees the same VNode topology. leading comment 使用 Fragment。
    const children = commentPrefix.comments.map(comment => createCommentVNode(comment));
    if (commentPrefix.rest.length != 0) {
        children.push(createKeyedStaticVNode(commentPrefix.rest, countStaticNodes(commentPrefix.rest)));
    }

    return h(Fragment, { key: content }, children);
}

function createKeyedStaticVNode(content, count) {
    const vnode = createStaticVNode(content, count);
    // Compiler hoists have no key because their content is immutable. MarkupString may change,
    // so bind its payload to the vnode identity. A changed payload replaces exactly this raw DOM
    // range while unchanged content keeps the Static fast path and no wrapper element is added.
    // key 只影响 VNode diff，不写入 DOM，也不改变 Razor raw HTML 的可见结构。
    vnode.key = content;
    return vnode;
}

function countStaticNodes(content) {
    if (typeof document === "undefined") {
        // Vue SSR emits Static children directly and does not consume staticCount.
        return 1;
    }

    const template = document.createElement("template");
    template.innerHTML = content;
    return template.content.childNodes.length;
}

function splitLeadingComments(content) {
    const comments = [];
    let offset = 0;
    while (content.startsWith("<!--", offset)) {
        const end = findCommentEnd(content, offset + 4);
        if (end === null) {
            comments.push(content.slice(offset + 4));
            offset = content.length;
            break;
        }

        comments.push(content.slice(offset + 4, end.index));
        offset = end.index + end.length;
    }

    return { comments, rest: content.slice(offset) };
}

function findCommentEnd(content, start) {
    const standard = content.indexOf("-->", start);
    const legacy = content.indexOf("--!>", start);
    if (standard < 0 && legacy < 0) {
        return null;
    }

    return standard >= 0 && (legacy < 0 || standard < legacy)
        ? { index: standard, length: 3 }
        : { index: legacy, length: 4 };
}
