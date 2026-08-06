import { TdPrimaryTableProps } from '../type';
export default function useAsyncLoading(props: TdPrimaryTableProps): {
    renderAsyncLoading: () => import("vue/jsx-runtime").JSX.Element;
};
