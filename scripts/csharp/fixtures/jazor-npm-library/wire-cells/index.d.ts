export interface Cell<T> { value: T; readonly label: string }
export declare function makeCell<T>(value: T): Cell<T>;
export declare function cellWeights(): Record<string, number>;
export declare function cellCount(): number;
export declare function readLabel<T>(cell: Cell<T>): string;
