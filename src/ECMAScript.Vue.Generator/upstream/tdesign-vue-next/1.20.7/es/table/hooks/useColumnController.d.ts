import { SetupContext } from 'vue';
import { CheckboxGroupValue } from '../../checkbox';
import { PrimaryTableCol, TdPrimaryTableProps } from '../type';
export declare function getColumnKeys(columns: PrimaryTableCol[], keys?: Set<string>): Set<string>;
export default function useColumnController(props: TdPrimaryTableProps, context: SetupContext): {
    tDisplayColumns: import("vue").Ref<CheckboxGroupValue, CheckboxGroupValue>;
    columnCheckboxKeys: import("vue").Ref<(string | number | boolean)[], (string | number | boolean)[] | CheckboxGroupValue>;
    renderColumnController: () => import("vue/jsx-runtime").JSX.Element;
};
