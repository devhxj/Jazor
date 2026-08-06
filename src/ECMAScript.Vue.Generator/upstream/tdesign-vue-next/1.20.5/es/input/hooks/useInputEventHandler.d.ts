import { Ref } from 'vue';
import { TdInputProps } from './../type';
export declare function useInputEventHandler(props: TdInputProps, isHover: Ref<boolean>, isComposition?: Ref<boolean>): {
    isHover: Ref<boolean>;
    handleKeydown: (e: KeyboardEvent) => void;
    handleKeyUp: (e: KeyboardEvent) => void;
    handleKeypress: (e: KeyboardEvent) => void;
    onHandlePaste: (e: ClipboardEvent) => void;
    onHandleMousewheel: (e: WheelEvent) => void;
    onInputMouseenter: (e: MouseEvent) => void;
    onInputMouseleave: (e: MouseEvent) => void;
};
