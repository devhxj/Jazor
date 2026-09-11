import { PropType } from 'vue';
import { ImageInfo } from '../type';
declare const _default: import("vue").DefineComponent<import("vue").ExtractPropTypes<{
    zIndex: NumberConstructor;
    scale: NumberConstructor;
    onRotate: PropType<() => void>;
    onZoomIn: PropType<() => void>;
    onZoomOut: PropType<() => void>;
    onMirror: PropType<() => void>;
    onReset: PropType<() => void>;
    onDownload: PropType<(url: string) => void>;
    currentImage: {
        type: PropType<ImageInfo>;
        default(): {};
    };
}>, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<import("vue").ExtractPropTypes<{
    zIndex: NumberConstructor;
    scale: NumberConstructor;
    onRotate: PropType<() => void>;
    onZoomIn: PropType<() => void>;
    onZoomOut: PropType<() => void>;
    onMirror: PropType<() => void>;
    onReset: PropType<() => void>;
    onDownload: PropType<(url: string) => void>;
    currentImage: {
        type: PropType<ImageInfo>;
        default(): {};
    };
}>> & Readonly<{}>, {
    currentImage: ImageInfo;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
