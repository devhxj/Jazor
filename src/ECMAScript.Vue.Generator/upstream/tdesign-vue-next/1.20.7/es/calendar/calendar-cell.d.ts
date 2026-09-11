import { CalendarCell } from './type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    item: {
        type: ObjectConstructor;
        default: () => CalendarCell;
    };
    fillWithZero: {
        type: BooleanConstructor;
        default: any;
    };
    theme: {
        type: StringConstructor;
        default: () => string;
    };
    t: FunctionConstructor;
    global: ObjectConstructor;
    cell: (StringConstructor | FunctionConstructor)[];
    cellAppend: (StringConstructor | FunctionConstructor)[];
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, string[], string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    item: {
        type: ObjectConstructor;
        default: () => CalendarCell;
    };
    fillWithZero: {
        type: BooleanConstructor;
        default: any;
    };
    theme: {
        type: StringConstructor;
        default: () => string;
    };
    t: FunctionConstructor;
    global: ObjectConstructor;
    cell: (StringConstructor | FunctionConstructor)[];
    cellAppend: (StringConstructor | FunctionConstructor)[];
}>> & Readonly<{
    [x: `on${Capitalize<string>}`]: (...args: any[]) => any;
}>, {
    item: Record<string, any>;
    theme: string;
    fillWithZero: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
