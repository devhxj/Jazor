import { TypePropType, TypeCreateElement } from './utils/adapt';
import { TypeTreeItemProps } from './types';
export declare const treeItemProps: {
    stateId: {
        type: TypePropType<TypeTreeItemProps["stateId"]>;
    };
    itemKey: {
        type: TypePropType<TypeTreeItemProps["itemKey"]>;
    };
    rowIndex: {
        type: TypePropType<TypeTreeItemProps["rowIndex"]>;
    };
    treeScope: {
        type: TypePropType<TypeTreeItemProps["treeScope"]>;
    };
};
declare const _default: import("vue").DefineComponent<{
    stateId: string;
    itemKey: string;
    treeScope: import("./types").TypeTreeScope;
    rowIndex: number;
}, {
    treeItemRef: import("./utils/adapt").TypeRef<HTMLDivElement>;
    renderItemNode: (h: TypeCreateElement) => import("vue/jsx-runtime").JSX.Element;
}, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    stateId: string;
    itemKey: string;
    treeScope: import("./types").TypeTreeScope;
    rowIndex: number;
}> & Readonly<{}>, {}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
