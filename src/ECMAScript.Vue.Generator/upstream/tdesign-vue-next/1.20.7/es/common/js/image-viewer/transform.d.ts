import type { ZoomOptions, ZoomResult, TranslateOffset, ImageScale } from './types';
export type { ZoomOptions, ZoomResult, TranslateOffset, ImageScale };
export declare const DEFAULT_IMAGE_SCALE: ImageScale;
export declare const isImageExceedsViewport: (container: HTMLElement, modalBox: HTMLElement) => boolean;
export declare const MIRROR_DEFAULT = 1;
export declare const toggleMirror: (current: number) => number;
export declare const ROTATE_DEG = -90;
export declare function calcResetRotation(currentDeg: number): number;
export declare function clampScale(value: number, min: number, max: number): number;
export declare function calcZoomInScale(oldScale: number, step: number, min: number, max: number): number;
export declare function calcZoomOutScale(oldScale: number, step: number, min: number, max: number): number;
export declare function calculateTranslateOffset(oldScale: number, newScale: number, options?: ZoomOptions): TranslateOffset | undefined;
export declare function zoomIn(oldScale: number, step: number, min: number, max: number, options?: ZoomOptions): {
    newScale: number;
    zoomResult: ZoomResult;
};
export declare function zoomOut(oldScale: number, step: number, min: number, max: number, options?: ZoomOptions): {
    newScale: number;
    zoomResult: ZoomResult;
};
