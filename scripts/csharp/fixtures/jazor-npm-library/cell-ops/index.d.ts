export declare function apply<TValue, TResult>(value: TValue, select: (value: TValue) => TResult): TResult;

interface LabelOptions {
    message?: string | number | (() => string);
    type?: 'plain' | 'loud';
}

export declare const label: {
    (message: string): string;
    (options: LabelOptions): string;
    loud(message: string): string;
    readonly prefix: string;
};
