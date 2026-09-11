import { TreeNode } from './tree-node';
import type { TreeNodeValue, TypeTreeNodeModel } from './types';
export declare const PATH_SEPARATOR = "/";
export declare function pathToKey(path: TreeNodeValue[]): string;
export declare function createNodeModel(node: TreeNode): TypeTreeNodeModel;
export declare function updateNodeModel(model: TypeTreeNodeModel, node: TreeNode): void;
