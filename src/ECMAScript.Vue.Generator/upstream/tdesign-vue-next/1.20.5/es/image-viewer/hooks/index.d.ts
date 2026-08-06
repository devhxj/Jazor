import type { ZoomOptions, ZoomResult, TranslateOffset } from 'tdesign-vue-next/es/common/js/image-viewer/transform';
import { ImageScale } from '../type';
export type { ZoomOptions, ZoomResult, TranslateOffset };
interface InitTransform {
    translateX: number;
    translateY: number;
}
export declare function useDrag(initTransform: InitTransform): {
    transform: import("vue").Ref<{
        translateX: number;
        translateY: number;
    }>;
    mouseDownHandler: (e: MouseEvent | TouchEvent) => void;
    pointerDownHandler: (e: MouseEvent | TouchEvent) => void;
    resetTransform: () => void;
};
export declare function useMirror(): {
    mirror: import("vue").Ref<number>;
    onMirror: () => void;
    resetMirror: () => void;
};
export declare function useScale(imageScale: Partial<ImageScale> | undefined): {
    scale: import("vue").Ref<number>;
    onZoomIn: (options?: ZoomOptions) => ZoomResult;
    onZoomOut: (options?: ZoomOptions) => ZoomResult;
    resetScale: () => void;
    onTouchStart: (e: TouchEvent) => void;
    onTouchMove: (e: TouchEvent) => void;
    onTouchEnd: () => void;
};
export declare function useRotate(): {
    rotate: import("vue").Ref<number>;
    onRotate: () => void;
    resetRotate: () => void;
};
