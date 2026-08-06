import { PropType } from 'vue';
import { TdIconSVGProps } from '../../utils/types';
declare const _default: {
    name: {
        type: StringConstructor;
        default: string;
    };
    size: {
        type: PropType<SizeEnum | string>;
        default: any;
    };
    url: {
        type: PropType<TdIconSVGProps["url"]>;
        default: any;
    };
    loadDefaultIcons: {
        type: BooleanConstructor;
        default: boolean;
    };
    onClick: PropType<TdIconSVGProps["onClick"]>;
};
export default _default;
