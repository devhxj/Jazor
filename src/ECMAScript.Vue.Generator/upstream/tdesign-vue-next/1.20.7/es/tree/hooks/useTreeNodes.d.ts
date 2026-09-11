import { TypeCreateElement, TypeVNode } from '../utils/adapt';
import { TypeTreeState } from '../types';
export default function useTreeNodes(state: TypeTreeState): {
    nodesEmpty: import("vue").Ref<boolean, boolean>;
    renderTreeNodes: (h: TypeCreateElement) => TypeVNode[];
};
