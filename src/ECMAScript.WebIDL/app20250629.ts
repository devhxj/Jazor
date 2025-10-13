import * as fs from 'fs';
import * as path from 'path';
import * as webidl2 from "webidl2";
import { getWebidls } from "./webref/idl.js";

const typeMap: Record<string, string> = {
    "bigint": "BigInteger",
    "DOMString": "string",
    "USVString": "string",
    "CSSOMString": "string",
    "HTMLString": "string",
    "ScriptString": "string",
    "ScriptURLString": "string",
    "ByteString": "byte[]",
    "boolean": "bool",
    "byte": "sbyte",
    "octet": "byte",
    "short": "short",
    "unsigned short": "ushort",
    "long": "int",
    "unsigned long": "uint",
    "long long": "long",
    "unsigned long long": "ulong",
    "float": "float",
    "unrestricted float": "float",
    "double": "double",
    "unrestricted double": "double",
    "DOMHighResTimeStamp": "double",
    "any": "object",
    "object": "object",
    "void": "void",
    "DOMException": "DomException",
    "DOMTokenList": "List<string>",
    "BufferSource": "IBufferSource",
    "ArrayBufferView": "IArrayBufferView",
    "Function": "Delegate",
    "VoidFunction": "Action",
};
const usingMap: Record<string, string> = {
    "Delegate": "System.Delegate",
    "BigInteger": "System.Numerics.BigInteger",
    "ArrayBuffer": "Jazor.ECMAScript.ArrayBuffer",
    "DOMException": "Jazor.ECMAScript.DomException",
    "IBufferSource": "Jazor.ECMAScript.IBufferSource",
    "IArrayBufferView": "Jazor.ECMAScript.IArrayBufferView",
    "ReadableStream": "Jazor.ECMAScript.ReadableStream",
    "ImageBitmap": "Jazor.ECMAScript.ImageBitmap",
    "ImageData": "Jazor.ECMAScript.ImageData",
    "HTMLImageElement": "Jazor.ECMAScript.HTMLImageElement",
    "HTMLVideoElement": "Jazor.ECMAScript.HTMLVideoElement",
    "VideoFrame": "Jazor.ECMAScript.VideoFrame",
    "HTMLCanvasElement": "Jazor.ECMAScript.HTMLCanvasElement",
    "OffscreenCanvas": "Jazor.ECMAScript.OffscreenCanvas",
    "GPUCanvasContext": "Jazor.ECMAScript.GPUBufferUsage.GPUCanvasContext",
};
const keywords: Set<string> = new Set<string>([
    'event', 'namespace', 'default', 'this', 'params', 'base', 'lock', 'async', 'await', 'static', 'virtual',
    'class', 'interface', 'enum', 'record', 'struct', 'using', 'public', 'private', 'protected', 'internal',
    'string', 'int', 'float', 'double', 'bool', 'long', 'short', 'ushort', 'uint', 'ulong', 'byte', 'sbyte'
]);
const usings: Map<string, string> = new Map<string, string>();
const enums: Set<string> = new Set<string>();
const classes: Map<string, webidl2.InterfaceType[]> = new Map<string, webidl2.InterfaceType[]>();
const records: Set<string> = new Set<string>();
const props: Set<string> = new Set<string>();
const toPascalCase = function (input: string): string {
    if (!input) return '';

    // Convert to PascalCase first
    let pascalCase = input
        // Handle existing camelCase or ALLCAPS abbreviations (e.g., CSSRule → CSS Rule)
        .replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2')  // ALLCAPS + camelCase → split
        .replace(/([a-z])([A-Z])/g, '$1 $2')       // lowercase + uppercase → split
        // Handle other separators (underscores, hyphens, etc.)
        .replace(/[^a-zA-Z0-9]+/g, ' ')
        .split(' ')
        .filter(word => word.length > 0)
        // Capitalize first letter, keep rest as-is
        .map(word =>
            word === word.toUpperCase()
                ? word // Keep ALLCAPS abbreviations as-is (e.g., CSS)
                : word[0].toUpperCase() + word.slice(1)
        )
        .join('');

    // If the result starts with a number, add underscore prefix
    if (/^[0-9]/.test(pascalCase)) {
        pascalCase = '_' + pascalCase;
    }

    return pascalCase;
}
const toCamelCase = function (input: string): string {
    if (!input) return ''; // 处理空字符串

    return input
        // 替换所有非字母数字字符为空格
        .replace(/[^a-zA-Z0-9]+/g, ' ')
        // 分割单词并过滤空值
        .split(' ')
        .filter(word => word.length > 0)
        // 处理每个单词：首字母大写（第一个单词除外）
        .map((word, index) =>
            index === 0
                ? word.toLowerCase() // 第一个单词全小写
                : word[0].toUpperCase() + word.slice(1).toLowerCase()
        )
        // 拼接成最终字符串
        .join('');
}
const N = '\n';
const P = '    ';

interface ConvertResult {
    source: string;
    using: string;
    namespace: string | undefined;
    code: string;
}

export function collect(source: string): ConvertResult {
    try {
        const priority = {
            'typedef': 0,
            'namespace': 1,
            'callback': 2,
            'callback interface': 3,
            'enum': 4,
            'interface mixin': 5,
            'dictionary': 6,
            'interface': 7,
            'includes': 8,
        };
        const ast = webidl2
            .parse(source)
            .sort((a, b) => priority[a.type] - priority[b.type]);
        const namespace = ast.find(x => x.type === 'namespace')?.name;
        const res: ConvertResult = { source: source, namespace: namespace, using: '', code: '' };
        res.code = `namespace ${res.namespace ? `Jazor.ECMAScript.${res.namespace}` : 'Jazor.ECMAScript'};\n`;

        const toSharpType = function (
            idlType: webidl2.IDLTypeDescription,
            using: boolean = false,
            defval: string = 'object'): string {

            let sharpType = defval;

            switch (idlType.generic) {
                case "":
                    {
                        if (idlType.union) {
                            const type = idlType as webidl2.UnionTypeDescription;
                            let types = [];
                            let hasUndefined = 0;
                            for (const typeName of type.idlType) {
                                const v = toSharpType(typeName, using);
                                if (v == 'undefined')
                                    hasUndefined++;
                                else
                                    types.push(v);
                            }
                            if (types.length == 1 && hasUndefined > 0) {
                                sharpType = `${types[0]}?`;
                            } else {
                                if (using)
                                    sharpType = `OneOf.OneOf<${types.join(', ')}>`;
                                else
                                    sharpType = `OneOf<${types.join(', ')}>`;
                            }
                        }
                        else {
                            const type = idlType as webidl2.SingleTypeDescription;
                            const defType = typeMap[type.idlType] || type.idlType;
                            if (using) {
                                if (usings.has(defType))
                                    sharpType = usings.get(defType) + `/*${defType}*/`;
                                else {
                                    const v1 = typeMap[type.idlType];
                                    sharpType = usingMap[defType]
                                        ?? v1
                                        ?? `Jazor.ECMAScript${res.namespace ? `.${res.namespace}` : ''}.${type.idlType}`;
                                }
                            }
                            else
                                sharpType = defType;
                        }
                        break;
                    }
                case "sequence":
                    {
                        const type = idlType as webidl2.SequenceTypeDescription;
                        const subType = toSharpType(type.idlType[0], using);
                        sharpType = `${subType}[]`;
                        break;
                    }
                case "Promise":
                    {
                        const type = idlType as webidl2.PromiseTypeDescription;
                        const subType = toSharpType(type.idlType[0], using);
                        if (using)
                            sharpType = subType == "undefined"
                                ? 'Jazor.ECMAScript.PromiseResult'
                                : `Jazor.ECMAScript.PromiseResult<${subType}>`;
                        else
                            sharpType = subType == "undefined"
                                ? 'PromiseResult'
                                : `PromiseResult<${subType}>`;
                        break;
                    }
                case "record":
                    {
                        const type = idlType as webidl2.RecordTypeDescription;
                        const keyType = toSharpType(type.idlType[0], using);
                        const valueType = toSharpType(type.idlType[1], using);
                        if (using)
                            sharpType = `System.Collections.Generic.Dictionary<${keyType}, ${valueType}>`;
                        else
                            sharpType = `Dictionary<${keyType}, ${valueType}>`;
                        break;
                    }
                case "FrozenArray":
                    {
                        const type = idlType as webidl2.FrozenArrayTypeDescription;
                        const subType = toSharpType(type.idlType[0], using);
                        sharpType = `FrozenSet<${subType}>`;
                        break;
                    }
                case "ObservableArray":
                    {
                        const type = idlType as webidl2.ObservableArrayTypeDescription;
                        const subType = toSharpType(type.idlType[0], using);
                        sharpType = `ObservableCollection<${subType}>`;
                        break;
                    }
                default:
                    sharpType = "object";
                    break;
            }

            if (idlType.nullable)
                sharpType = `${sharpType}?`;

            if (sharpType == "undefined")
                sharpType = defval;

            return sharpType;
        }
        const getValue = function (val: webidl2.ValueDescription): string {
            if (!val) return 'null';

            switch (val.type) {
                case 'string':
                    return `"${val.value}"`;
                case 'number':
                    return val.value.toString();
                case 'boolean':
                    return val.value.toString();
                case 'null':
                    return 'null';
                case 'Infinity':
                    return val.negative ? 'double.NegativeInfinity' : 'double.PositiveInfinity';
                case 'NaN':
                    return 'null';
                case 'sequence':
                    return '[]';
                case 'dictionary':
                    return 'new()';
                default:
                    return '';
            }
        }
        const generateEnum = function (item: webidl2.EnumType): string {
            const name = toPascalCase(item.name);
            const code =
                `/// <summary>${N}` +
                `/// ${item.name}${N}` +
                `/// </summary>${N}` +
                `[Description("${escapeName(item.name)}")]${N}` +
                `public enum ${toPascalCase(item.name)}${N}{${N}` +
                item.values.map((value, index) => {
                    let enumValue = value.value;
                    if (typeof enumValue === "string" && enumValue.includes('"'))
                        enumValue = enumValue.replace(/"/g, '');
                    enumValue = toPascalCase(enumValue || 'Empty');
                    return `${P}[Description("${enumValue}")]${N}${P}${enumValue} = ${index}`;
                }).join(",\n\n") +
                `${N}}`;

            enums.add(name);
            return code;
        }
        const generateTypedef = function (item: webidl2.TypedefType): string {
            const name = toPascalCase(item.name);
            const type = toSharpType(item.idlType, true);
            const exists = usings.has(type);
            let value = exists ? usings.get(type) || type : type;
            if (value.endsWith('?'))
                value = value.slice(0, -1);
            const mark = exists ? `/*${type}*/` : '';

            if (!exists) usings.set(name, value);

            return `global using ${name} = ${value};${mark}`;
        }
        const generateInterface = function (item: webidl2.InterfaceType): string {
            if (['CDATASection', 'DOMStringMap'].includes(item.name))
                return '';

            const name = toPascalCase(item.name);
            const exists = classes.has(name);
            const inherit = item.inheritance ? `: ${toPascalCase(item.inheritance)}` : '';
            const code =
                `/// <summary>${N}` +
                `/// ${item.name}${N}` +
                `/// </summary>${exists ? '' : `${N}[Description("${escapeName(item.name)}")]`}${N}` +
                `public partial class ${name} ${inherit}${N}{${N}` +
                item.members.map(member => {
                    switch (member.type) {
                        case "constructor": // 构造函数
                            return generateConstructor(member);
                        case "operation": // 方法
                            return generateMethod(member);
                        case "attribute": // 属性
                            return generateProperty(member);
                        case "const": // 常量
                            return generateConst(member);
                        case "iterable": // 可迭代类型
                            return generateIterable(member);
                        default:
                            return '';
                    }
                }).filter(x => x).join('\n\n') +
                `${N}}`;

            //if (!exists)
            //    classes.add(name);
            return code;
        }
        const generateDictionary = function (item: webidl2.DictionaryType): string {
            let name = toPascalCase(item.name);
            let exists = records.has(name);
            if (exists) {
                if (item.members.length == 1) {
                    if (['AuthenticationExtensionsClientInputs',
                        'AuthenticationExtensionsClientInputsJSON',
                        'AuthenticationExtensionsClientOutputs',
                        'AuthenticationExtensionsClientOutputsJSON',
                        'MLOpSupportLimits',
                        'RequestInit'].includes(name)) {
                        name = `${name}${item.members[0].name}`;
                        exists = false;
                    }
                }
            }
            else records.add(name);

            let code = `/// <summary>
/// ${item.name}
/// </summary>${exists ? '' : `\n[Description("${escapeName(item.name)}")]`}
public partial record ${name}${item.inheritance ? `: ${toPascalCase(item.inheritance)}` : ''}
{${exists ? '' : `\n    public extern ${name}();`}`;

            let params = [];
            let fieldCodes = [];
            for (const field of item.members) {
                const desc = escapeName(field.name);
                const name = toPascalCase(field.name);
                const required = field.required ? "required " : '';
                let type = toSharpType(field.idlType);
                if (field.default) {
                    const typeKey = type.endsWith('?') ? type.slice(0, -1) : type;
                    const isEnumType = enums.has(typeKey);
                    let value = getValue(field.default);
                    if (isEnumType && (!field.idlType.nullable || (value && value != "null")))
                        value = `${typeKey}.${value == '""' ? 'Empty' : toPascalCase(value)}`;
                    if (field.idlType.union)
                        value = field.idlType.nullable ? 'null' : `new()/*${value}*/`;

                    switch (type) {
                        case "double":
                            value = `${value}d`;
                            break;
                        case "float":
                            value = `${value}f`;
                            break;
                    }

                    if (!field.required && !field.idlType.nullable) {
                        const optionalType = new Set<string>([
                            'string', 'int', 'float', 'double', 'bool', 'long',
                            'short', 'ushort', 'uint', 'ulong', 'byte', 'sbyte'
                        ]);
                        if (!optionalType.has(type) && !isEnumType) {
                            type = `${type}?`;
                            value = 'null';
                        }
                    }

                    params.push({ code: `${type} ${name} = ${value}`, optional: true });
                } else {
                    params.push({ code: `${type} ${name}`, optional: false });
                }
                fieldCodes.push(`\n    [Description("${desc}")]\n    public ${required}${type} ${name} { get; init; }`);
            }

            if (params.length > 0) {
                code += `\n    public extern ${name}(
        ${params
                        .sort((a, b) => (a.optional === b.optional) ? a.code.localeCompare(b.code) : a.optional ? 1 : -1)
                        .map(x => x.code)
                        .join(',\n        ')});`;
            }

            return code + '\n' + fieldCodes.join('\n') + `\n}`;
        }
        const generateProperty = function (attribute: webidl2.AttributeMemberType): string {
            const csharpType = toSharpType(attribute.idlType);
            let propName = toPascalCase(attribute.name);
            const escapedName = escapeName(attribute.name);
            const className = toPascalCase(attribute.parent.name);
            const staticVal = attribute.special == 'static' ? 'static ' : '';
            if (propName == className)
                propName = `${propName}_`;

            if (className == "CSSFontFaceDescriptors" ||
                className == "CSSStyleDeclaration " ||
                className == "CSSPageDescriptors" ||
                className == "CSSPositionTryDescriptors") {
                if (attribute.name.includes("-")) {
                    propName = attribute.name
                        .split('-')               // Split into words
                        .map(word =>              // Capitalize each word
                            word.charAt(0).toUpperCase() + word.slice(1).toLowerCase()
                        )
                        .join('_');
                }
            }

            const propKey = `${className}.${propName}@${csharpType}`;
            if (props.has(propKey))
                return '';
            else
                props.add(propKey);

            const comment = `    /// <summary>
    /// ${attribute.name}
    /// </summary>`;

            if (attribute.readonly) {
                return `${comment}
    [Description("${escapedName}")]
    public extern ${staticVal}${csharpType} ${propName} { get; }`;
            }

            return `${comment}
    [Description("${escapedName}")]
    public extern ${staticVal}${csharpType} ${propName} { get; set; }`;
        }
        const generateConstructor = function (constructor: webidl2.ConstructorMemberType): string {
            let paramsList = "";
            let paramComments = "";

            if (constructor.arguments && constructor.arguments.length > 0) {
                paramsList = constructor.arguments.map(arg => {
                    let argType = toSharpType(arg.idlType);
                    let argName = toCamelCase(arg.name);
                    if (keywords.has(argName)) argName = `@${argName}`;
                    return `${argType} ${argName}`;
                }).join(", ");

                paramComments = constructor.arguments.map(arg => {
                    return `    /// <param name="${toCamelCase(arg.name)}">${arg.name}</param>`;
                }).join("\n");
            }

            let ctorCode = `    /// <summary>
    /// constructor
    /// </summary>
`;

            if (paramComments) ctorCode += paramComments + "\n";

            ctorCode += `    public extern ${constructor.parent.name}(${paramsList});`;
            return ctorCode;
        }
        const generateMethod = function (operation: webidl2.OperationMemberType): string {

            const methodName = operation.name === ""
                ? "CallOperation"
                : toPascalCase(operation.name || "");
            const escapedName = escapeName(operation.name || "call");

            if (operation.parent.name == "ContentIndex" && methodName == "Add") {
                let b = 22;
            }

            let paramsList = "";
            let paramComments = "";
            if (operation.arguments && operation.arguments.length > 0) {
                paramsList = operation.arguments.map(arg => {
                    let argType = toSharpType(arg.idlType);
                    let argName = toCamelCase(arg.name);
                    if (keywords.has(argName))
                        argName = `@${argName}`;
                    return `${argType} ${argName}`;
                }).join(", ");

                paramComments = operation.arguments.map(arg => {
                    return `    /// <param name="${toCamelCase(arg.name)}">${arg.name} para</param>`;
                }).join("\n");
            }

            let returnType = "void";
            if (operation.idlType) {
                returnType = toSharpType(operation.idlType);
            }
            if (returnType == "undefined")
                returnType = "void";


            let methodCode = `    /// <summary>
    /// ${operation.name || "call"} function
    /// </summary>
`;
            if (paramComments) methodCode += paramComments + "\n";


            const staticVal = operation.special == 'static' ? 'static ' : '';

            methodCode += `    [Description("${escapedName}")]
    public extern ${staticVal}${returnType} ${methodName}(${paramsList});`;

            return methodCode;
        }
        const generateConst = function (constDef: webidl2.ConstantMemberType): string {
            const csharpType = toSharpType(constDef.idlType);
            const propName = toPascalCase(constDef.name);
            const escapedName = escapeName(constDef.name);
            const value = getValue(constDef.value);

            return `    /// <summary>
    /// ${constDef.name}
    /// </summary>
    [Description("${escapedName}")]
    public const ${csharpType} ${propName} = ${value};`;
        }
        const generateIterable = function (iterableDef: webidl2.IterableDeclarationMemberType): string {
            const keyType = iterableDef.idlType ? toSharpType(iterableDef.idlType[0]) : "string";
            const valueType = iterableDef.arguments.length > 1
                ? toSharpType(iterableDef.arguments[1].idlType)
                : "object";

            return `    // iterableDef: ${iterableDef}
    public extern IEnumerable<KeyValuePair<${keyType}, ${valueType}>> GetEnumerator();`;
        }
        const generateCallbackInterface = function (callbackInterfaceDef: webidl2.CallbackInterfaceType): string {

            const name = toPascalCase(callbackInterfaceDef.name);
            let code = '';

            if (name == 'EventListener') {
                let b = 3;
            }

            //if (callbackInterfaceDef.members.length > 0) {
            //    code += `    public static class ${name}\n{`;
            //}

            for (const member of callbackInterfaceDef.members) {
                switch (member.type) {
                    case "operation": // 方法
                        {
                            const methodName = toPascalCase(member.name || "");
                            const paramsList = member.arguments.map(arg => {
                                let argType = toSharpType(arg.idlType);
                                let argName = toCamelCase(arg.name);
                                if (keywords.has(argName))
                                    argName = `@${argName}`;
                                return `${argType} ${argName}`;
                            }).join(", ");

                            let returnType = "void";
                            if (member.idlType)
                                returnType = toSharpType(member.idlType);

                            if (returnType == "undefined")
                                returnType = "void";

                            code += `\npublic delegate ${returnType} ${name}(${paramsList});/*${methodName}*/`;
                        }
                        break;
                    //case "const": // 常量
                    //    code += generateConst(member as webidl2.ConstantMemberType);
                }
            }

            //if (callbackInterfaceDef.members.length > 0) {
            //    code += `\n}`;
            //}

            return code;
        }
        const generateCallback = function (callbackDef: webidl2.CallbackType): string {
            const name = toPascalCase(callbackDef.name);
            let code = '';
            const paramsList = callbackDef.arguments.map(arg => {
                let argType = toSharpType(arg.idlType);
                let argName = toCamelCase(arg.name);
                if (keywords.has(argName))
                    argName = `@${argName}`;
                return `${argType} ${argName}`;
            }).join(", ");

            let returnType = "void";
            if (callbackDef.idlType)
                returnType = toSharpType(callbackDef.idlType);

            if (returnType == "undefined")
                returnType = "void";

            code += `\npublic delegate ${returnType} ${name}(${paramsList});`;

            return code;
        }

        for (const item of ast) {
            switch (item.type) {
                case 'typedef':
                    res.using += generateTypedef(item) + "\n";
                    break;
                case 'enum':
                    res.code += "\n" + generateEnum(item) + "\n";
                    break;
                case 'dictionary':
                    res.code += "\n" + generateDictionary(item) + "\n";
                    break;
                case 'interface':
                    {
                        const name = toPascalCase(item.name);
                        if (classes.has(item.name)) {
                            classes.get(item.name)?.push(item as webidl2.InterfaceType);
                        } else {
                            classes.set(name, [item as webidl2.InterfaceType]);
                        }
                    }
                    break;
                case 'callback interface':
                    res.code += "\n" + generateCallbackInterface(item) + "\n";
                    break;
                case 'interface mixin':
                    break;
                case 'namespace':
                    break;
                case 'callback':
                    res.code += "\n" + generateCallback(item) + "\n";
                    break;
                case 'includes':
                    break;
            }
        }

        return res;
    } catch (error) {
        console.error("转换错误:", error);
        throw error;
    }
}

// 主转换函数
export function convert(source: string): ConvertResult {
    try {
        const priority = {
            'typedef': 0,
            'namespace': 1,
            'callback': 2,
            'callback interface': 3,
            'enum': 4,
            'interface mixin': 5,
            'dictionary': 6,
            'interface': 7,
            'includes': 8,
        };
        const ast = webidl2
            .parse(source)
            .sort((a, b) => priority[a.type] - priority[b.type]);
        const namespace = ast.find(x => x.type === 'namespace')?.name;
        const res: ConvertResult = { source: source, namespace: namespace, using: '', code: '' };
        res.code = `namespace ${res.namespace ? `Jazor.ECMAScript.${res.namespace}` : 'Jazor.ECMAScript'};\n`;

        const toSharpType = function (
            idlType: webidl2.IDLTypeDescription,
            using: boolean = false,
            defval: string = 'object'): string {

            let sharpType = defval;

            switch (idlType.generic) {
                case "":
                    {
                        if (idlType.union) {
                            const type = idlType as webidl2.UnionTypeDescription;
                            let types = [];
                            let hasUndefined = 0;
                            for (const typeName of type.idlType) {
                                const v = toSharpType(typeName, using);
                                if (v == 'undefined')
                                    hasUndefined++;
                                else
                                    types.push(v);
                            }
                            if (types.length == 1 && hasUndefined > 0) {
                                sharpType = `${types[0]}?`;
                            } else {
                                if (using)
                                    sharpType = `OneOf.OneOf<${types.join(', ')}>`;
                                else
                                    sharpType = `OneOf<${types.join(', ')}>`;
                            }
                        }
                        else {
                            const type = idlType as webidl2.SingleTypeDescription;
                            const defType = typeMap[type.idlType] || type.idlType;
                            if (using) {
                                if (usings.has(defType))
                                    sharpType = usings.get(defType) + `/*${defType}*/`;
                                else {
                                    const v1 = typeMap[type.idlType];
                                    sharpType = usingMap[defType]
                                        ?? v1
                                        ?? `Jazor.ECMAScript${res.namespace ? `.${res.namespace}` : ''}.${type.idlType}`;
                                }
                            }
                            else
                                sharpType = defType;
                        }
                        break;
                    }
                case "sequence":
                    {
                        const type = idlType as webidl2.SequenceTypeDescription;
                        const subType = toSharpType(type.idlType[0], using);
                        sharpType = `${subType}[]`;
                        break;
                    }
                case "Promise":
                    {
                        const type = idlType as webidl2.PromiseTypeDescription;
                        const subType = toSharpType(type.idlType[0], using);
                        if (using)
                            sharpType = subType == "undefined"
                                ? 'Jazor.ECMAScript.PromiseResult'
                                : `Jazor.ECMAScript.PromiseResult<${subType}>`;
                        else
                            sharpType = subType == "undefined"
                                ? 'PromiseResult'
                                : `PromiseResult<${subType}>`;
                        break;
                    }
                case "record":
                    {
                        const type = idlType as webidl2.RecordTypeDescription;
                        const keyType = toSharpType(type.idlType[0], using);
                        const valueType = toSharpType(type.idlType[1], using);
                        if (using)
                            sharpType = `System.Collections.Generic.Dictionary<${keyType}, ${valueType}>`;
                        else
                            sharpType = `Dictionary<${keyType}, ${valueType}>`;
                        break;
                    }
                case "FrozenArray":
                    {
                        const type = idlType as webidl2.FrozenArrayTypeDescription;
                        const subType = toSharpType(type.idlType[0], using);
                        sharpType = `FrozenSet<${subType}>`;
                        break;
                    }
                case "ObservableArray":
                    {
                        const type = idlType as webidl2.ObservableArrayTypeDescription;
                        const subType = toSharpType(type.idlType[0], using);
                        sharpType = `ObservableCollection<${subType}>`;
                        break;
                    }
                default:
                    sharpType = "object";
                    break;
            }

            if (idlType.nullable)
                sharpType = `${sharpType}?`;

            if (sharpType == "undefined")
                sharpType = defval;

            return sharpType;
        }
        const getValue = function (val: webidl2.ValueDescription): string {
            if (!val) return 'null';

            switch (val.type) {
                case 'string':
                    return `"${val.value}"`;
                case 'number':
                    return val.value.toString();
                case 'boolean':
                    return val.value.toString();
                case 'null':
                    return 'null';
                case 'Infinity':
                    return val.negative ? 'double.NegativeInfinity' : 'double.PositiveInfinity';
                case 'NaN':
                    return 'null';
                case 'sequence':
                    return '[]';
                case 'dictionary':
                    return 'new()';
                default:
                    return '';
            }
        }
        const generateEnum = function (item: webidl2.EnumType): string {
            const name = toPascalCase(item.name);
            const code =
                `/// <summary>${N}` +
                `/// ${item.name}${N}` +
                `/// </summary>${N}` +
                `[Description("${escapeName(item.name)}")]${N}` +
                `public enum ${toPascalCase(item.name)}${N}{${N}` +
                item.values.map((value, index) => {
                    let enumValue = value.value;
                    if (typeof enumValue === "string" && enumValue.includes('"'))
                        enumValue = enumValue.replace(/"/g, '');
                    enumValue = toPascalCase(enumValue || 'Empty');
                    return `${P}[Description("${enumValue}")]${N}${P}${enumValue} = ${index}`;
                }).join(",\n\n") +
                `${N}}`;

            enums.add(name);
            return code;
        }
        const generateTypedef = function (item: webidl2.TypedefType): string {
            const name = toPascalCase(item.name);
            const type = toSharpType(item.idlType, true);
            const exists = usings.has(type);
            let value = exists ? usings.get(type) || type : type;
            if (value.endsWith('?'))
                value = value.slice(0, -1);
            const mark = exists ? `/*${type}*/` : '';

            if (!exists) usings.set(name, value);

            return `global using ${name} = ${value};${mark}`;
        }
        const generateInterface = function (item: webidl2.InterfaceType): string {
            if (['CDATASection', 'DOMStringMap'].includes(item.name))
                return '';

            const name = toPascalCase(item.name);
            const exists = classes.has(name);
            const inherit = item.inheritance ? `: ${toPascalCase(item.inheritance)}` : '';
            const code =
                `/// <summary>${N}` +
                `/// ${item.name}${N}` +
                `/// </summary>${exists ? '' : `${N}[Description("${escapeName(item.name)}")]`}${N}` +
                `public partial class ${name} ${inherit}${N}{${N}` +
                item.members.map(member => {
                    switch (member.type) {
                        case "constructor": // 构造函数
                            return generateConstructor(member);
                        case "operation": // 方法
                            return generateMethod(member);
                        case "attribute": // 属性
                            return generateProperty(member);
                        case "const": // 常量
                            return generateConst(member);
                        case "iterable": // 可迭代类型
                            return generateIterable(member);
                        default:
                            return '';
                    }
                }).filter(x => x).join('\n\n') +
                `${N}}`;

            //if (!exists)
            //    classes.add(name);
            return code;
        }
        const generateDictionary = function (item: webidl2.DictionaryType): string {
            let name = toPascalCase(item.name);
            let exists = records.has(name);
            if (exists) {
                if (item.members.length == 1) {
                    if (['AuthenticationExtensionsClientInputs',
                        'AuthenticationExtensionsClientInputsJSON',
                        'AuthenticationExtensionsClientOutputs',
                        'AuthenticationExtensionsClientOutputsJSON',
                        'MLOpSupportLimits',
                        'RequestInit'].includes(name)) {
                        name = `${name}${item.members[0].name}`;
                        exists = false;
                    }
                }
            }
            else records.add(name);

            let code = `/// <summary>
/// ${item.name}
/// </summary>${exists ? '' : `\n[Description("${escapeName(item.name)}")]`}
public partial record ${name}${item.inheritance ? `: ${toPascalCase(item.inheritance)}` : ''}
{${exists ? '' : `\n    public extern ${name}();`}`;

            let params = [];
            let fieldCodes = [];
            for (const field of item.members) {
                const desc = escapeName(field.name);
                const name = toPascalCase(field.name);
                const required = field.required ? "required " : '';
                let type = toSharpType(field.idlType);
                if (field.default) {
                    const typeKey = type.endsWith('?') ? type.slice(0, -1) : type;
                    const isEnumType = enums.has(typeKey);
                    let value = getValue(field.default);
                    if (isEnumType && (!field.idlType.nullable || (value && value != "null")))
                        value = `${typeKey}.${value == '""' ? 'Empty' : toPascalCase(value)}`;
                    if (field.idlType.union)
                        value = field.idlType.nullable ? 'null' : `new()/*${value}*/`;

                    switch (type) {
                        case "double":
                            value = `${value}d`;
                            break;
                        case "float":
                            value = `${value}f`;
                            break;
                    }

                    if (!field.required && !field.idlType.nullable) {
                        const optionalType = new Set<string>([
                            'string', 'int', 'float', 'double', 'bool', 'long',
                            'short', 'ushort', 'uint', 'ulong', 'byte', 'sbyte'
                        ]);
                        if (!optionalType.has(type) && !isEnumType) {
                            type = `${type}?`;
                            value = 'null';
                        }
                    }

                    params.push({ code: `${type} ${name} = ${value}`, optional: true });
                } else {
                    params.push({ code: `${type} ${name}`, optional: false });
                }
                fieldCodes.push(`\n    [Description("${desc}")]\n    public ${required}${type} ${name} { get; init; }`);
            }

            if (params.length > 0) {
                code += `\n    public extern ${name}(
        ${params
                        .sort((a, b) => (a.optional === b.optional) ? a.code.localeCompare(b.code) : a.optional ? 1 : -1)
                        .map(x => x.code)
                        .join(',\n        ')});`;
            }

            return code + '\n' + fieldCodes.join('\n') + `\n}`;
        }
        const generateProperty = function (attribute: webidl2.AttributeMemberType): string {
            const csharpType = toSharpType(attribute.idlType);
            let propName = toPascalCase(attribute.name);
            const escapedName = escapeName(attribute.name);
            const className = toPascalCase(attribute.parent.name);
            const staticVal = attribute.special == 'static' ? 'static ' : '';
            if (propName == className)
                propName = `${propName}_`;

            if (className == "CSSFontFaceDescriptors" ||
                className == "CSSStyleDeclaration " ||
                className == "CSSPageDescriptors" ||
                className == "CSSPositionTryDescriptors") {
                if (attribute.name.includes("-")) {
                    propName = attribute.name
                        .split('-')               // Split into words
                        .map(word =>              // Capitalize each word
                            word.charAt(0).toUpperCase() + word.slice(1).toLowerCase()
                        )
                        .join('_');
                }
            }

            const propKey = `${className}.${propName}@${csharpType}`;
            if (props.has(propKey))
                return '';
            else
                props.add(propKey);

            const comment = `    /// <summary>
    /// ${attribute.name}
    /// </summary>`;

            if (attribute.readonly) {
                return `${comment}
    [Description("${escapedName}")]
    public extern ${staticVal}${csharpType} ${propName} { get; }`;
            }

            return `${comment}
    [Description("${escapedName}")]
    public extern ${staticVal}${csharpType} ${propName} { get; set; }`;
        }
        const generateConstructor = function (constructor: webidl2.ConstructorMemberType): string {
            let paramsList = "";
            let paramComments = "";

            if (constructor.arguments && constructor.arguments.length > 0) {
                paramsList = constructor.arguments.map(arg => {
                    let argType = toSharpType(arg.idlType);
                    let argName = toCamelCase(arg.name);
                    if (keywords.has(argName)) argName = `@${argName}`;
                    return `${argType} ${argName}`;
                }).join(", ");

                paramComments = constructor.arguments.map(arg => {
                    return `    /// <param name="${toCamelCase(arg.name)}">${arg.name}</param>`;
                }).join("\n");
            }

            let ctorCode = `    /// <summary>
    /// constructor
    /// </summary>
`;

            if (paramComments) ctorCode += paramComments + "\n";

            ctorCode += `    public extern ${constructor.parent.name}(${paramsList});`;
            return ctorCode;
        }
        const generateMethod = function (operation: webidl2.OperationMemberType): string {

            const methodName = operation.name === ""
                ? "CallOperation"
                : toPascalCase(operation.name || "");
            const escapedName = escapeName(operation.name || "call");

            if (operation.parent.name == "ContentIndex" && methodName == "Add") {
                let b = 22;
            }

            let paramsList = "";
            let paramComments = "";
            if (operation.arguments && operation.arguments.length > 0) {
                paramsList = operation.arguments.map(arg => {
                    let argType = toSharpType(arg.idlType);
                    let argName = toCamelCase(arg.name);
                    if (keywords.has(argName))
                        argName = `@${argName}`;
                    return `${argType} ${argName}`;
                }).join(", ");

                paramComments = operation.arguments.map(arg => {
                    return `    /// <param name="${toCamelCase(arg.name)}">${arg.name} para</param>`;
                }).join("\n");
            }

            let returnType = "void";
            if (operation.idlType) {
                returnType = toSharpType(operation.idlType);
            }
            if (returnType == "undefined")
                returnType = "void";


            let methodCode = `    /// <summary>
    /// ${operation.name || "call"} function
    /// </summary>
`;
            if (paramComments) methodCode += paramComments + "\n";


            const staticVal = operation.special == 'static' ? 'static ' : '';

            methodCode += `    [Description("${escapedName}")]
    public extern ${staticVal}${returnType} ${methodName}(${paramsList});`;

            return methodCode;
        }
        const generateConst = function (constDef: webidl2.ConstantMemberType): string {
            const csharpType = toSharpType(constDef.idlType);
            const propName = toPascalCase(constDef.name);
            const escapedName = escapeName(constDef.name);
            const value = getValue(constDef.value);

            return `    /// <summary>
    /// ${constDef.name}
    /// </summary>
    [Description("${escapedName}")]
    public const ${csharpType} ${propName} = ${value};`;
        }
        const generateIterable = function (iterableDef: webidl2.IterableDeclarationMemberType): string {
            const keyType = iterableDef.idlType ? toSharpType(iterableDef.idlType[0]) : "string";
            const valueType = iterableDef.arguments.length > 1
                ? toSharpType(iterableDef.arguments[1].idlType)
                : "object";

            return `    // iterableDef: ${iterableDef}
    public extern IEnumerable<KeyValuePair<${keyType}, ${valueType}>> GetEnumerator();`;
        }
        const generateCallbackInterface = function (callbackInterfaceDef: webidl2.CallbackInterfaceType): string {

            const name = toPascalCase(callbackInterfaceDef.name);
            let code = '';

            if (name == 'EventListener') {
                let b = 3;
            }

            //if (callbackInterfaceDef.members.length > 0) {
            //    code += `    public static class ${name}\n{`;
            //}

            for (const member of callbackInterfaceDef.members) {
                switch (member.type) {
                    case "operation": // 方法
                        {
                            const methodName = toPascalCase(member.name || "");
                            const paramsList = member.arguments.map(arg => {
                                let argType = toSharpType(arg.idlType);
                                let argName = toCamelCase(arg.name);
                                if (keywords.has(argName))
                                    argName = `@${argName}`;
                                return `${argType} ${argName}`;
                            }).join(", ");

                            let returnType = "void";
                            if (member.idlType)
                                returnType = toSharpType(member.idlType);

                            if (returnType == "undefined")
                                returnType = "void";

                            code += `\npublic delegate ${returnType} ${name}(${paramsList});/*${methodName}*/`;
                        }
                        break;
                    //case "const": // 常量
                    //    code += generateConst(member as webidl2.ConstantMemberType);
                }
            }

            //if (callbackInterfaceDef.members.length > 0) {
            //    code += `\n}`;
            //}

            return code;
        }
        const generateCallback = function (callbackDef: webidl2.CallbackType): string {
            const name = toPascalCase(callbackDef.name);
            let code = '';
            const paramsList = callbackDef.arguments.map(arg => {
                let argType = toSharpType(arg.idlType);
                let argName = toCamelCase(arg.name);
                if (keywords.has(argName))
                    argName = `@${argName}`;
                return `${argType} ${argName}`;
            }).join(", ");

            let returnType = "void";
            if (callbackDef.idlType)
                returnType = toSharpType(callbackDef.idlType);

            if (returnType == "undefined")
                returnType = "void";

            code += `\npublic delegate ${returnType} ${name}(${paramsList});`;

            return code;
        }

        for (const item of ast) {
            switch (item.type) {
                case 'typedef':
                    res.using += generateTypedef(item) + "\n";
                    break;
                case 'enum':
                    res.code += "\n" + generateEnum(item) + "\n";
                    break;
                case 'dictionary':
                    res.code += "\n" + generateDictionary(item) + "\n";
                    break;
                case 'interface':
                    {
                        const name = toPascalCase(item.name);
                        if (classes.has(item.name)) {
                            classes.get(item.name)?.push(item as webidl2.InterfaceType);
                        } else {
                            classes.set(name, [item as webidl2.InterfaceType]);
                        }
                    }
                    res.code += "\n" + generateInterface(item) + "\n";
                    break;
                case 'callback interface':
                    res.code += "\n" + generateCallbackInterface(item) + "\n";
                    break;
                case 'interface mixin':
                    break;
                case 'namespace':
                    break;
                case 'callback':
                    res.code += "\n" + generateCallback(item) + "\n";
                    break;
                case 'includes':
                    break;
            }
        }

        return res;
    } catch (error) {
        console.error("转换错误:", error);
        throw error;
    }
}


// 名称转义
function escapeName(name: string): string {
    if (!name) return '';
    return name;//.replace(/[^a-zA-Z0-9_]/g, "_");
}

const dir = "C:/Users/hanxj/source/repos/Jazor2/Jazor/Jazor.ECMAScript";
const files = await getWebidls();
const globalUsingsPath = `${dir}/GlobalUsings.cs`;

fs.writeFileSync(globalUsingsPath, `
global using System;
global using System.ComponentModel;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Numerics;
global using System.Collections.Frozen;
global using System.Threading.Tasks;
global using OneOf;
global using Jazor;
global using Jazor.ECMAScript;
global using Jazor.ECMAScript.CSS;
global using Jazor.ECMAScript.GPUBufferUsage;
global using Jazor.ECMAScript.WebAssembly;
`, { encoding: 'utf8' });

files.forEach((file, shortname) => {
    const res = convert(file);
});

files.forEach((file, shortname) => {
    const res = convert(file);
    const codePath = res.namespace
        ? `${dir}/${res.namespace}/${toPascalCase(escapeName(shortname))}.cs`
        : `${dir}/${toPascalCase(escapeName(shortname))}.cs`;

    const namespace = path.dirname(codePath);
    if (!fs.existsSync(namespace))
        fs.mkdirSync(namespace, { recursive: true });

    fs.writeFileSync(codePath, res.code, 'utf-8');
    fs.appendFileSync(globalUsingsPath, res.using, 'utf-8');
});

console.log('Hello world');