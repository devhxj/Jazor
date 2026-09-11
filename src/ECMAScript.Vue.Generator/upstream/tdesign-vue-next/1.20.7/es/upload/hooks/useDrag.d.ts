import { Ref } from 'vue';
import { TdUploadProps } from '../type';
export interface UploadDragEvents {
    onDragFileChange?: (files: File[]) => void;
    onDragenter?: TdUploadProps['onDragenter'];
    onDragleave?: TdUploadProps['onDragleave'];
    onDrop?: TdUploadProps['onDrop'];
}
export default function useDrag(props: UploadDragEvents, accept: Ref<string>): {
    target: Ref<any, any>;
    dragActive: Ref<boolean, boolean>;
    handleDrop: (event: DragEvent) => void;
    handleDragenter: (event: DragEvent) => void;
    handleDragleave: (event: DragEvent) => void;
    handleDragover: (event: DragEvent) => void;
};
