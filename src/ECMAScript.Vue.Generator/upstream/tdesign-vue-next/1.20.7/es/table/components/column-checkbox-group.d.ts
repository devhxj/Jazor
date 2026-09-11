import { CheckboxGroupChangeContext, CheckboxGroupProps } from '../../checkbox';
export type ColumnCheckboxGroupProps = Pick<CheckboxGroupProps, 'value' | 'onChange' | 'options'> & {
    checkboxProps: CheckboxGroupProps;
    label?: string;
    uniqueKey?: string;
};
declare const _default: import("vue").DefineComponent<{
    value?: import("../..").CheckboxGroupValue;
    options?: Array<import("../..").CheckboxOption>;
    onChange?: (value: import("../..").CheckboxGroupValue, context: CheckboxGroupChangeContext) => void;
    checkboxProps: CheckboxGroupProps;
    label?: string;
    uniqueKey?: string;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    value?: import("../..").CheckboxGroupValue;
    options?: Array<import("../..").CheckboxOption>;
    onChange?: (value: import("../..").CheckboxGroupValue, context: CheckboxGroupChangeContext) => void;
    checkboxProps: CheckboxGroupProps;
    label?: string;
    uniqueKey?: string;
}> & Readonly<{}>, {
    options: import("../..").CheckboxOption[];
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
