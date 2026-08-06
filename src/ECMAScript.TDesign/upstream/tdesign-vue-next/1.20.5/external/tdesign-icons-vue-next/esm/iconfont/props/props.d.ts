import { PropType } from 'vue';
import { TdIconfontProps } from '../../utils/types';
declare const _default: {
    name: {
        type: StringConstructor;
        default: string;
        required: boolean;
    };
    size: {
        type: PropType<SizeEnum | string>;
        default: any;
    };
    tag: {
        type: StringConstructor;
        default: string;
    };
    url: {
        type: PropType<TdIconfontProps["url"]>;
        default: any;
    };
    loadDefaultIcons: {
        type: BooleanConstructor;
        default: boolean;
    };
    onClick: PropType<TdIconfontProps["onClick"]>;
};
export default _default;
