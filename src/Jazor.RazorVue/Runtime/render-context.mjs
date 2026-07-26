import { Fragment, createStaticVNode } from "vue";

import {
    RENDER_CONTEXT_PROTOCOL_VERSION,
    createRenderContextCore
} from "./render-context-core.mjs";

export { RENDER_CONTEXT_PROTOCOL_VERSION };

export function createRenderContext(h) {
    return createRenderContextCore(h, Fragment, createStaticVNode);
}
