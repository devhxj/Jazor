import { Ref } from 'vue';
export declare function useCheckboxLazyLoad(labelRef: Ref<HTMLElement>, lazyLoad: Ref<boolean>): {
    showCheckbox: Ref<boolean, boolean>;
};
export default useCheckboxLazyLoad;
