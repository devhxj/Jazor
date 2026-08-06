import { TypeCreateElement } from '../utils/adapt';
import { TypeTreeItemState } from '../types';
export default function useRenderIcon(state: TypeTreeItemState): {
    renderIcon: (h: TypeCreateElement) => import("vue/jsx-runtime").JSX.Element;
};
