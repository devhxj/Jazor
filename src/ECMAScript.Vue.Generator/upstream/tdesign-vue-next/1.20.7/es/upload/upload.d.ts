declare const _default: import("vue").DefineComponent<{
    abridgeName?: Array<number>;
    accept?: string;
    action?: string;
    allowUploadDuplicateFile?: boolean;
    autoUpload?: boolean;
    beforeAllFilesUpload?: (file: import("./type").UploadFile[]) => boolean | Promise<boolean>;
    beforeUpload?: (file: import("./type").UploadFile) => boolean | Promise<boolean>;
    cancelUploadButton?: null | import("..").ButtonProps | import("..").TNode<{
        disabled: boolean;
        cancelUploadText: string;
        cancelUpload: (ctx: {
            e: MouseEvent;
        }) => void;
    }>;
    data?: Record<string, any> | ((files: import("./type").UploadFile[]) => Record<string, any>);
    default?: string | import("..").TNode;
    disabled?: boolean;
    dragContent?: (h: typeof import("vue").h, props: import("./type").TriggerContext) => import("..").TNodeReturnValue;
    draggable?: boolean;
    fileListDisplay?: import("..").TNode<{
        files: import("./type").UploadFile[];
        dragEvents?: import("..").UploadDisplayDragEvents;
    }>;
    files?: import("./type").UploadFile[];
    defaultFiles?: import("./type").UploadFile[];
    format?: (file: File) => import("./type").UploadFile;
    formatRequest?: (requestData: {
        [key: string]: any;
    }) => {
        [key: string]: any;
    };
    formatResponse?: (response: any, context: import("./type").FormatResponseContext) => import("./type").ResponseType;
    headers?: {
        [key: string]: string;
    };
    imageViewerProps?: import("..").ImageViewerProps;
    inputAttributes?: object;
    isBatchUpload?: boolean;
    locale?: import("..").UploadConfig;
    max?: number;
    method?: "POST" | "GET" | "PUT" | "OPTIONS" | "PATCH" | "post" | "get" | "put" | "options" | "patch";
    mockProgressDuration?: number;
    multiple?: boolean;
    name?: string;
    placeholder?: string;
    requestMethod?: (files: import("./type").UploadFile | import("./type").UploadFile[]) => Promise<import("./type").RequestMethodResponse>;
    showImageFileName?: boolean;
    showThumbnail?: boolean;
    showUploadProgress?: boolean;
    sizeLimit?: number | import("./type").SizeLimitObj;
    status?: "default" | "success" | "warning" | "error";
    theme?: "custom" | "file" | "file-input" | "file-flow" | "image" | "image-flow";
    tips?: string | import("..").TNode;
    trigger?: import("..").TNode<import("./type").TriggerContext>;
    triggerButtonProps?: import("..").ButtonProps;
    uploadAllFilesInOneRequest?: boolean;
    uploadButton?: null | import("..").ButtonProps | import("..").TNode<{
        disabled: boolean;
        uploading: boolean;
        uploadFiles: () => void;
        uploadText: string;
    }>;
    uploadPastedFiles?: boolean;
    useMockProgress?: boolean;
    value?: import("./type").UploadFile[];
    defaultValue?: import("./type").UploadFile[];
    modelValue?: import("./type").UploadFile[];
    withCredentials?: boolean;
    onCancelUpload?: () => void;
    onChange?: (value: import("./type").UploadFile[], context: import("./type").UploadChangeContext) => void;
    onDragenter?: (context: {
        e: DragEvent;
    }) => void;
    onDragleave?: (context: {
        e: DragEvent;
    }) => void;
    onDrop?: (context: {
        e: DragEvent;
    }) => void;
    onFail?: (options: import("./type").UploadFailContext) => void;
    onOneFileFail?: (options: import("./type").UploadFailContext) => void;
    onOneFileSuccess?: (context: Pick<import("./type").SuccessContext, "e" | "file" | "response" | "XMLHttpRequest">) => void;
    onPreview?: (options: {
        file: import("./type").UploadFile;
        index: number;
        e: MouseEvent;
    }) => void;
    onProgress?: (options: import("./type").ProgressContext) => void;
    onRemove?: (context: import("./type").UploadRemoveContext) => void;
    onSelectChange?: (files: File[], context: import("./type").UploadSelectChangeContext) => void;
    onSuccess?: (context: import("./type").SuccessContext) => void;
    onValidate?: (context: {
        type: import("./type").UploadValidateType;
        files: import("./type").UploadFile[];
    }) => void;
    onWaitingUploadFilesChange?: (context: {
        files: Array<import("./type").UploadFile>;
        trigger: "validate" | "remove" | "uploaded";
    }) => void;
}, () => import("vue/jsx-runtime").JSX.Element, {}, {}, {}, import("vue").ComponentOptionsMixin, import("vue").ComponentOptionsMixin, {}, string, import("vue").PublicProps, Readonly<{
    abridgeName?: Array<number>;
    accept?: string;
    action?: string;
    allowUploadDuplicateFile?: boolean;
    autoUpload?: boolean;
    beforeAllFilesUpload?: (file: import("./type").UploadFile[]) => boolean | Promise<boolean>;
    beforeUpload?: (file: import("./type").UploadFile) => boolean | Promise<boolean>;
    cancelUploadButton?: null | import("..").ButtonProps | import("..").TNode<{
        disabled: boolean;
        cancelUploadText: string;
        cancelUpload: (ctx: {
            e: MouseEvent;
        }) => void;
    }>;
    data?: Record<string, any> | ((files: import("./type").UploadFile[]) => Record<string, any>);
    default?: string | import("..").TNode;
    disabled?: boolean;
    dragContent?: (h: typeof import("vue").h, props: import("./type").TriggerContext) => import("..").TNodeReturnValue;
    draggable?: boolean;
    fileListDisplay?: import("..").TNode<{
        files: import("./type").UploadFile[];
        dragEvents?: import("..").UploadDisplayDragEvents;
    }>;
    files?: import("./type").UploadFile[];
    defaultFiles?: import("./type").UploadFile[];
    format?: (file: File) => import("./type").UploadFile;
    formatRequest?: (requestData: {
        [key: string]: any;
    }) => {
        [key: string]: any;
    };
    formatResponse?: (response: any, context: import("./type").FormatResponseContext) => import("./type").ResponseType;
    headers?: {
        [key: string]: string;
    };
    imageViewerProps?: import("..").ImageViewerProps;
    inputAttributes?: object;
    isBatchUpload?: boolean;
    locale?: import("..").UploadConfig;
    max?: number;
    method?: "POST" | "GET" | "PUT" | "OPTIONS" | "PATCH" | "post" | "get" | "put" | "options" | "patch";
    mockProgressDuration?: number;
    multiple?: boolean;
    name?: string;
    placeholder?: string;
    requestMethod?: (files: import("./type").UploadFile | import("./type").UploadFile[]) => Promise<import("./type").RequestMethodResponse>;
    showImageFileName?: boolean;
    showThumbnail?: boolean;
    showUploadProgress?: boolean;
    sizeLimit?: number | import("./type").SizeLimitObj;
    status?: "default" | "success" | "warning" | "error";
    theme?: "custom" | "file" | "file-input" | "file-flow" | "image" | "image-flow";
    tips?: string | import("..").TNode;
    trigger?: import("..").TNode<import("./type").TriggerContext>;
    triggerButtonProps?: import("..").ButtonProps;
    uploadAllFilesInOneRequest?: boolean;
    uploadButton?: null | import("..").ButtonProps | import("..").TNode<{
        disabled: boolean;
        uploading: boolean;
        uploadFiles: () => void;
        uploadText: string;
    }>;
    uploadPastedFiles?: boolean;
    useMockProgress?: boolean;
    value?: import("./type").UploadFile[];
    defaultValue?: import("./type").UploadFile[];
    modelValue?: import("./type").UploadFile[];
    withCredentials?: boolean;
    onCancelUpload?: () => void;
    onChange?: (value: import("./type").UploadFile[], context: import("./type").UploadChangeContext) => void;
    onDragenter?: (context: {
        e: DragEvent;
    }) => void;
    onDragleave?: (context: {
        e: DragEvent;
    }) => void;
    onDrop?: (context: {
        e: DragEvent;
    }) => void;
    onFail?: (options: import("./type").UploadFailContext) => void;
    onOneFileFail?: (options: import("./type").UploadFailContext) => void;
    onOneFileSuccess?: (context: Pick<import("./type").SuccessContext, "e" | "file" | "response" | "XMLHttpRequest">) => void;
    onPreview?: (options: {
        file: import("./type").UploadFile;
        index: number;
        e: MouseEvent;
    }) => void;
    onProgress?: (options: import("./type").ProgressContext) => void;
    onRemove?: (context: import("./type").UploadRemoveContext) => void;
    onSelectChange?: (files: File[], context: import("./type").UploadSelectChangeContext) => void;
    onSuccess?: (context: import("./type").SuccessContext) => void;
    onValidate?: (context: {
        type: import("./type").UploadValidateType;
        files: import("./type").UploadFile[];
    }) => void;
    onWaitingUploadFilesChange?: (context: {
        files: Array<import("./type").UploadFile>;
        trigger: "validate" | "remove" | "uploaded";
    }) => void;
}> & Readonly<{}>, {
    value: import("./type").UploadFile[];
    multiple: boolean;
    max: number;
    disabled: boolean;
    draggable: boolean;
    method: "POST" | "GET" | "PUT" | "OPTIONS" | "PATCH" | "post" | "get" | "put" | "options" | "patch";
    action: string;
    withCredentials: boolean;
    name: string;
    files: import("./type").UploadFile[];
    useMockProgress: boolean;
    uploadAllFilesInOneRequest: boolean;
    isBatchUpload: boolean;
    allowUploadDuplicateFile: boolean;
    autoUpload: boolean;
    defaultValue: import("./type").UploadFile[];
    placeholder: string;
    theme: "image" | "file" | "custom" | "file-input" | "file-flow" | "image-flow";
    modelValue: import("./type").UploadFile[];
    defaultFiles: import("./type").UploadFile[];
    accept: string;
    uploadPastedFiles: boolean;
    showUploadProgress: boolean;
    showImageFileName: boolean;
    showThumbnail: boolean;
}, {}, {}, {}, string, import("vue").ComponentProvideOptions, true, {}, any>;
export default _default;
