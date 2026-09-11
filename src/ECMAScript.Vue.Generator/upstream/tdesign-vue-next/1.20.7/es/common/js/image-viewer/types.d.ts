export interface ImageInfo {
    mainImage: string | File;
    thumbnail?: string | File;
    download?: boolean;
    isSvg?: boolean;
}
export type Images = Array<string | File | ImageInfo>;
export interface TranslateOffset {
    translateX: number;
    translateY: number;
}
export interface ZoomOptions {
    mouseOffsetX?: number;
    mouseOffsetY?: number;
    currentTranslate?: TranslateOffset;
}
export interface ZoomResult {
    newTranslate?: TranslateOffset;
}
export interface ImageScale {
    max: number;
    min: number;
    step: number;
    defaultScale: number;
}
