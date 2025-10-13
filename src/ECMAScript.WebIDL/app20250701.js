import * as fs from 'fs';
import * as path from 'path';
import * as webidl2 from "webidl2";
import { getWebidls } from "./webref/idl.js";
const N = '\n', NN = '\n\n', P = '    ', PP = '        ';
const typeMap = {
    'bigint': 'BigInteger',
    //'DOMString': 'DOMString',
    //'USVString': 'USVString',
    //'CSSOMString': 'CSSOMString',
    //'HTMLString': 'HTMLString',
    //'ScriptString': 'ScriptString',
    //'ScriptURLString': 'ScriptURLString',
    'DOMString': 'string',
    'USVString': 'string',
    'CSSOMString': 'string',
    'HTMLString': 'string',
    'ScriptString': 'string',
    'ScriptURLString': 'string',
    'ByteString': 'byte[]',
    'boolean': 'bool',
    'byte': 'sbyte',
    'octet': 'byte',
    'short': 'short',
    'unsigned short': 'ushort',
    'long': 'int',
    'unsigned long': 'uint',
    'long long': 'long',
    'unsigned long long': 'ulong',
    'float': 'float',
    'unrestricted float': 'float',
    'double': 'double',
    'unrestricted double': 'double',
    'DOMHighResTimeStamp': 'double',
    'any': 'object',
    'object': 'object',
    'void': 'void',
    'DOMTokenList': 'List<string>',
    'BufferSource': 'IBufferSource',
    'ArrayBufferView': 'IArrayBufferView',
    'Function': 'Delegate',
    'VoidFunction': 'Action',
    'AllowSharedBufferSource': 'IAllowSharedBufferSource'
};
const usingMap = {
    'Delegate': 'System.Delegate',
    'BigInteger': 'System.Numerics.BigInteger',
    //'DOMString': 'Jazor.ECMAScript.DOMString',
    //'USVString': 'Jazor.ECMAScript.USVString',
    //'CSSOMString': 'Jazor.ECMAScript.CSSOMString',
    //'HTMLString': 'Jazor.ECMAScript.HTMLString',
    //'ScriptString': 'Jazor.ECMAScript.ScriptString',
    //'ScriptURLString': 'Jazor.ECMAScript.ScriptURLString',
    'ArrayBuffer': 'Jazor.ECMAScript.ArrayBuffer',
    'IBufferSource': 'Jazor.ECMAScript.IBufferSource',
    'IArrayBufferView': 'Jazor.ECMAScript.IArrayBufferView',
    'ReadableStream': 'Jazor.ECMAScript.ReadableStream',
    'ImageBitmap': 'Jazor.ECMAScript.ImageBitmap',
    'ImageData': 'Jazor.ECMAScript.ImageData',
    'HTMLImageElement': 'Jazor.ECMAScript.HTMLImageElement',
    'HTMLVideoElement': 'Jazor.ECMAScript.HTMLVideoElement',
    'VideoFrame': 'Jazor.ECMAScript.VideoFrame',
    'HTMLCanvasElement': 'Jazor.ECMAScript.HTMLCanvasElement',
    'OffscreenCanvas': 'Jazor.ECMAScript.OffscreenCanvas',
    'GPUCanvasContext': 'Jazor.ECMAScript.GPUBufferUsage.GPUCanvasContext',
    'IAllowSharedBufferSource': 'Jazor.ECMAScript.IAllowSharedBufferSource',
};
const keywords = new Set([
    'event', 'namespace', 'default', 'this', 'params', 'base', 'lock', 'async', 'await', 'static', 'virtual',
    'class', 'interface', 'enum', 'record', 'struct', 'using', 'public', 'private', 'protected', 'internal',
    'string', 'int', 'float', 'double', 'bool', 'long', 'short', 'ushort', 'uint', 'ulong', 'byte', 'sbyte'
]);
const commonUsings = [
    'global using System;',
    'global using System.ComponentModel;',
    'global using System.Collections.Generic;',
    'global using System.Collections.ObjectModel;',
    'global using System.Numerics;',
    'global using System.Collections.Frozen;',
    'global using System.Threading.Tasks;',
    'global using OneOf;',
    'global using Jazor;',
    'global using Jazor.ECMAScript;',
    'global using Jazor.ECMAScript.CSS;',
    'global using Jazor.ECMAScript.GPUBufferUsage;',
    'global using Jazor.ECMAScript.WebAssembly;'
];
const usingCaches = new Map();
const enumCaches = new Set();
const classCaches = new Map();
const toPascalCase = function (input) {
    if (!input)
        return '';
    // Convert to PascalCase first
    let pascalCase = input
        // Handle existing camelCase or ALLCAPS abbreviations (e.g., CSSRule �� CSS Rule)
        .replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2') // ALLCAPS + camelCase �� split
        .replace(/([a-z])([A-Z])/g, '$1 $2') // lowercase + uppercase �� split
        // Handle other separators (underscores, hyphens, etc.)
        .replace(/[^a-zA-Z0-9]+/g, ' ')
        .split(' ')
        .filter(word => word.length > 0)
        // Capitalize first letter, keep rest as-is
        .map(word => word === word.toUpperCase()
        ? word // Keep ALLCAPS abbreviations as-is (e.g., CSS)
        : word[0].toUpperCase() + word.slice(1))
        .join('');
    // If the result starts with a number, add underscore prefix
    if (/^[0-9]/.test(pascalCase)) {
        pascalCase = '_' + pascalCase;
    }
    return pascalCase;
};
const toCamelCase = function (input) {
    if (!input)
        return ''; // �������ַ���
    return input
        // �滻���з���ĸ�����ַ�Ϊ�ո�
        .replace(/[^a-zA-Z0-9]+/g, ' ')
        // �ָ�ʲ����˿�ֵ
        .split(' ')
        .filter(word => word.length > 0)
        // ����ÿ�����ʣ�����ĸ��д����һ�����ʳ��⣩
        .map((word, index) => index === 0
        ? word.toLowerCase() // ��һ������ȫСд
        : word[0].toUpperCase() + word.slice(1).toLowerCase())
        // ƴ�ӳ������ַ���
        .join('');
};
const toSharpType = function (idlType, namespace = undefined, using = false, defaultValue = 'object') {
    let sharpType = defaultValue;
    switch (idlType.generic) {
        case "":
            {
                if (idlType.union) {
                    const type = idlType;
                    let types = [];
                    let hasUndefined = 0;
                    for (const typeName of type.idlType) {
                        const v = toSharpType(typeName, namespace, using);
                        if (v == 'undefined')
                            hasUndefined++;
                        else
                            types.push(v);
                    }
                    if (types.length == 1 && hasUndefined > 0) {
                        sharpType = `${types[0]}?`;
                    }
                    else {
                        if (using)
                            sharpType = `OneOf.OneOf<${types.join(', ')}>`;
                        else
                            sharpType = `OneOf<${types.join(', ')}>`;
                    }
                }
                else {
                    const type = idlType;
                    const defType = typeMap[type.idlType] || type.idlType;
                    if (using) {
                        if (usingCaches.has(defType))
                            sharpType = usingCaches.get(defType) + `/*${defType}*/`;
                        else {
                            const v1 = typeMap[type.idlType];
                            sharpType = usingMap[defType]
                                ?? v1
                                ?? `Jazor.ECMAScript${namespace ? `.${namespace}` : ''}.${type.idlType}`;
                        }
                    }
                    else
                        sharpType = defType;
                }
                break;
            }
        case "sequence":
            {
                const type = idlType;
                const subType = toSharpType(type.idlType[0], namespace, using);
                sharpType = `${subType}[]`;
                break;
            }
        case "Promise":
            {
                const type = idlType;
                const subType = toSharpType(type.idlType[0], namespace, using);
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
                const type = idlType;
                const keyType = toSharpType(type.idlType[0], namespace, using);
                const valueType = toSharpType(type.idlType[1], namespace, using);
                if (using)
                    sharpType = `System.Collections.Generic.Dictionary<${keyType}, ${valueType}>`;
                else
                    sharpType = `Dictionary<${keyType}, ${valueType}>`;
                break;
            }
        case "FrozenArray":
            {
                const type = idlType;
                const subType = toSharpType(type.idlType[0], namespace, using);
                sharpType = `FrozenSet<${subType}>`;
                break;
            }
        case "ObservableArray":
            {
                const type = idlType;
                const subType = toSharpType(type.idlType[0], namespace, using);
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
        sharpType = defaultValue;
    return sharpType;
};
const getValue = function (val) {
    if (!val)
        return 'null';
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
};
const generateTypedef = function (item, namespace = undefined) {
    const name = toPascalCase(item.name);
    const type = toSharpType(item.idlType, namespace, true);
    const exists = usingCaches.has(type);
    let value = exists ? usingCaches.get(type) || type : type;
    if (value.endsWith('?'))
        value = value.slice(0, -1);
    const mark = exists ? `/*${type}*/` : '';
    if (!exists)
        usingCaches.set(name, value);
    return `global using ${name} = ${value};${mark}`;
};
const generateEnum = function (item) {
    const name = toPascalCase(item.name);
    const code = `/// <summary>${N}` +
        `/// ${item.name}${N}` +
        `/// </summary>${N}` +
        `[Description("${item.name}")]${N}` +
        `[ECMAScript(true)]${N}` +
        `public enum ${toPascalCase(item.name)}${N}{${N}` +
        item.values.map((value, index) => {
            let enumValue = value.value;
            if (typeof enumValue === "string" && enumValue.includes('"'))
                enumValue = enumValue.replace(/"/g, '');
            enumValue = toPascalCase(enumValue || 'Empty');
            return `${P}[Description("${enumValue}")]${N}${P}${enumValue} = ${index}`;
        }).join(",\n\n") +
        `${N}}`;
    enumCaches.add(name);
    return code;
};
const generateCallbackInterface = function (item, namespace = undefined) {
    const name = toPascalCase(item.name);
    const tab = namespace ? P : '';
    let code = '';
    for (const member of item.members) {
        switch (member.type) {
            case "operation": // ����
                {
                    const methodName = toPascalCase(member.name || "");
                    const paramsList = member.arguments.map(arg => {
                        let argType = toSharpType(arg.idlType);
                        let argName = toCamelCase(arg.name);
                        if (keywords.has(argName))
                            argName = `@${argName}`;
                        return `${argType} ${argName}`;
                    }).join(", ");
                    const returnType = member.idlType ? toSharpType(member.idlType, namespace, false, 'void') : "void";
                    code +=
                        `${N}/// <summary>${N}` +
                            `/// ${item.name}${N}` +
                            `/// </summary>${N}` +
                            `[ECMAScript(true)]${N}` +
                            `[Description("${item.name}")]${N}` +
                            `public delegate ${returnType} ${name}(${paramsList});/*${methodName}*/`;
                }
                break;
            case "const": // ����
                {
                    const constName = toPascalCase(member.name);
                    let constValue = getValue(member.value);
                    if (constValue == 'null')
                        constValue = 'default';
                    if (keywords.has(constName))
                        code += `${N}//public const ${toSharpType(member.idlType, namespace)} @${constName} = ${constValue};`;
                    else
                        code += `${N}//public const ${toSharpType(member.idlType, namespace)} ${constName} = ${constValue};`;
                }
                break;
        }
    }
    return code;
};
const generateCallback = function (item, namespace = undefined) {
    const name = toPascalCase(item.name);
    if (name == "DecodeErrorCallback") {
        let a = 10;
    }
    const paramsList = item.arguments.map(arg => {
        let argType = toSharpType(arg.idlType);
        let argName = toCamelCase(arg.name);
        if (keywords.has(argName))
            argName = `@${argName}`;
        return `${argType} ${argName}`;
    }).join(", ");
    const returnType = item.idlType ? toSharpType(item.idlType, namespace, false, 'void') : "void";
    const code = `/// <summary>${N}` +
        `/// ${item.name}${N}` +
        `/// </summary>${N}` +
        `[ECMAScript(true)]${N}` +
        `[Description("${item.name}")]${N}` +
        `public delegate ${returnType} ${name}(${paramsList});`;
    return code;
};
const generateInterface = function (className, items, namespace = undefined) {
    if (['DOMStringMap'].includes(className))
        return '';
    const name = toPascalCase(className);
    const inheritances = items.filter(x => x.inheritance).map(x => toPascalCase(x.inheritance));
    // Ŀǰ��֧�ֶ��ؼ̳�
    // ����Ƕ��ؼ̳У�����Ҫ�������룬�ĳɽӿ�
    if (inheritances.length > 1)
        throw new Error("Does not support multiple inheritance.");
    let inherit = '';
    if (inheritances.length > 0) {
        const parentName = toPascalCase(inheritances[0]);
        const cache = classCaches.get(parentName);
        const noParamCtors = cache.ctors.filter(x => x.item1 == 0);
        // ���������ڹ��캯�����Ҳ����޲εģ����������ʵ�ֺ���
        if (cache.ctors.length > 0 && noParamCtors.length == 0) {
            let hasSameParamCtors = false;
            breakCtor: for (const item of items) {
                for (const member of item.members) {
                    if (member.type != "constructor")
                        continue;
                    // ����Ҫ�жϵ�ǰ�Ĺ��캯��ǩ���Ƿ���ں͸��๹�캯��ǩ����ͬ�����
                    const key1 = member.arguments.map(p => toSharpType(p.idlType, namespace)).join('@');
                    const key2 = member.arguments.map(p => p.name + ':' + toSharpType(p.idlType, namespace)).join('@');
                    const sameParamCtors = cache.ctors.filter(x => x.item3 == key1);
                    if (sameParamCtors.length > 0) {
                        hasSameParamCtors = true;
                        break breakCtor;
                    }
                }
            }
            if (hasSameParamCtors)
                inherit = ` : ${parentName}`;
            else {
                const ctor = cache.ctors[0].item2;
                const baseParams = ctor.arguments.map(arg => toCamelCase(arg.name)).join(", ");
                const thisParams = ctor.arguments.map(arg => {
                    let argType = toSharpType(arg.idlType, namespace);
                    let argName = toCamelCase(arg.name);
                    if (keywords.has(argName))
                        argName = `@${argName}`;
                    return `${argType} ${argName}`;
                }).join(", ");
                inherit = `(${thisParams}) : ${parentName}(${baseParams})`;
            }
        }
        else
            inherit = ` : ${parentName}`;
    }
    //����Ψһ��������ȥ��
    const members = [];
    for (const x of items) {
        for (const member of x.members) {
            let key = member.type;
            switch (member.type) {
                case "constructor": // ���캯��
                    key = key + '$'
                        + member.arguments.map(p => p.name + ':' + toSharpType(p.idlType, namespace)).join('@');
                    break;
                case "operation": // ����
                    key = key + '$'
                        + member.name + '$'
                        + member.arguments.map(p => p.name + ':' + toSharpType(p.idlType, namespace)).join('@') + '$'
                        + (member.idlType ? toSharpType(member.idlType, namespace, false, 'void') : 'void');
                    break;
                case "attribute": // ����
                    key = key + '$'
                        + member.name + '$'
                        + toSharpType(member.idlType, namespace);
                    break;
                case "const": // ����
                    key = key + '$'
                        + member.name + '$'
                        + toSharpType(member.idlType, namespace);
                    break;
                case "iterable": // �ɵ�������
                    key = key + '$'
                        + member.idlType.map(x => toSharpType(x, namespace)).join(':');
                    break;
                case "maplike": // �ɵ�������
                    key = key + '$'
                        + member.idlType.map(x => toSharpType(x, namespace)).join(':');
                    break;
                case "setlike": // �ɵ�������
                    key = key + '$'
                        + member.idlType.map(x => toSharpType(x, namespace)).join(':');
                    break;
            }
            members.push({ item1: key, item2: member });
        }
    }
    const code = `/// <summary>${N}` +
        `/// ${className}${N}` +
        `/// </summary>${N}` +
        `[ECMAScript(true)]${N}` +
        `[Description("${className}")]${N}` +
        `public class ${name}${inherit}${N}{${N}` +
        distinct(members).map(item => {
            switch (item.item2.type) {
                case "constructor": // ���캯��
                    return generateConstructor(item.item2, namespace);
                case "operation": // ����
                    return generateMethod(item.item2, namespace);
                case "attribute": // ����
                    return generateProperty(item.item2, namespace);
                case "const": // ����
                    return generateConst(item.item2, namespace);
                case "iterable": // �ɵ�������
                    return generateIterable(item.item2, namespace);
                default:
                    return '';
            }
        }).filter(x => x).join('\n\n') +
        `${N}}`;
    return code;
};
const generateProperty = function (attribute, namespace = undefined) {
    const csharpType = toSharpType(attribute.idlType, namespace);
    const className = toPascalCase(attribute.parent.name);
    const staticVal = attribute.special == 'static' ? 'static ' : '';
    let propName = toPascalCase(attribute.name);
    if (propName == className)
        propName = `${propName}_`;
    if (className == "CSSFontFaceDescriptors" ||
        className == "CSSStyleDeclaration " ||
        className == "CSSPageDescriptors" ||
        className == "CSSPositionTryDescriptors") {
        if (attribute.name.includes("-")) {
            propName = attribute.name
                .split('-')
                .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
                .join('_');
        }
    }
    // ����Ƿ���д�����ֶ�
    let override = '';
    if (attribute.parent.inheritance) {
        const parentName = toPascalCase(attribute.parent.inheritance);
        const cache = classCaches.get(parentName);
        const a = cache.props.filter(x => x.item1 == propName);
        const b = a.filter(x => x.item2 == csharpType);
        if (b.length > 0)
            return ''; //����͸���������ȫһ��������������
        if (a.length > 0)
            override = 'new ';
    }
    const code = `${P}/// <summary>${N}` +
        `${P}/// ${attribute.name}${N}` +
        `${P}/// </summary>${N}` +
        `${P}[Description("${attribute.name}")]${N}` +
        `${P}public extern ${staticVal}${override}${csharpType} ${propName} { get;${attribute.readonly ? '' : 'set;'} }`;
    return code;
};
const generateConstructor = function (constructor, namespace = undefined) {
    let params = "", comments = "";
    if (constructor.arguments && constructor.arguments.length > 0) {
        params = constructor.arguments.map(arg => {
            let argType = toSharpType(arg.idlType, namespace);
            let argName = toCamelCase(arg.name);
            if (keywords.has(argName))
                argName = `@${argName}`;
            return `${argType} ${argName}`;
        }).join(", ");
        comments = constructor.arguments.map(arg => {
            return `${P}/// <param name="${toCamelCase(arg.name)}">${arg.name}</param>`;
        }).join("\n");
    }
    const code = `${P}/// <summary>${N}` +
        `${P}/// Constructor ${N}` +
        `${P}/// </summary>${N}` +
        (comments ? comments + N : '') +
        `${P}public extern ${constructor.parent.name}(${params});`;
    return code;
};
const generateMethod = function (item, namespace = undefined) {
    if (!item.name)
        return '//nullable operation';
    const name = toPascalCase(item.name);
    const staticVal = item.special == 'static' ? 'static ' : '';
    const returnType = item.idlType ? toSharpType(item.idlType, namespace, false, 'void') : 'void';
    let params = "", comments = "";
    if (item.arguments && item.arguments.length > 0) {
        params = item.arguments.map(arg => {
            let argType = toSharpType(arg.idlType, namespace);
            let argName = toCamelCase(arg.name);
            if (keywords.has(argName))
                argName = `@${argName}`;
            return `${argType} ${argName}`;
        }).join(", ");
        comments = item.arguments.map(arg => {
            return `${P}/// <param name="${toCamelCase(arg.name)}">${arg.name} para</param>`;
        }).join("\n");
    }
    // ����Ƿ���д�����ֶ�
    let override = '';
    if (item.parent.inheritance) {
        const parentName = toPascalCase(item.parent.inheritance);
        const item2 = item.arguments.map(p => p.name + ':' + toSharpType(p.idlType, namespace)).join('@');
        const item3 = item.idlType ? toSharpType(item.idlType, namespace) : 'void';
        const cache = classCaches.get(parentName);
        const a = cache.methods.filter(x => x.item1 == name && x.item2 == item2);
        const b = cache.methods.filter(x => x.item3 == item3);
        if (b.length > 0)
            return ''; //����͸���������ȫһ���������ɷ���
        if (a.length > 0)
            override = 'new ';
    }
    const code = `${P}/// <summary>${N}` +
        `${P}/// ${item.name}${N}` +
        `${P}/// </summary>${N}` +
        (comments ? comments + N : '') +
        `${P}[Description("${item.name}")]${N}` +
        `${P}public extern ${staticVal}${override}${returnType} ${name}(${params});`;
    return code;
};
const generateConst = function (constDef, namespace = undefined) {
    const csharpType = toSharpType(constDef.idlType, namespace);
    const propName = toPascalCase(constDef.name);
    const escapedName = constDef.name;
    const value = getValue(constDef.value);
    return `    /// <summary>
    /// ${constDef.name}
    /// </summary>
    [Description("${escapedName}")]
    public const ${csharpType} ${propName} = ${value};`;
};
const generateIterable = function (iterableDef, namespace = undefined) {
    const keyType = iterableDef.idlType ? toSharpType(iterableDef.idlType[0], namespace) : "string";
    const valueType = iterableDef.arguments.length > 1
        ? toSharpType(iterableDef.arguments[1].idlType)
        : "object";
    return `    // iterableDef: ${iterableDef}
    public extern IEnumerable<KeyValuePair<${keyType}, ${valueType}>> GetEnumerator();`;
};
const generateDictionary = function (recordName, items, namespace = undefined) {
    const name = toPascalCase(recordName);
    const inheritances = items.filter(x => x.inheritance).map(x => toPascalCase(x.inheritance));
    const inherit = inheritances.length > 0 ? `: ${inheritances.join(', ')}` : '';
    const params = items.map(item => item.members.map(field => {
        const name = toPascalCase(field.name);
        const required = field.required ? "required " : '';
        let type = toSharpType(field.idlType);
        const typeKey = type.endsWith('?') ? type.slice(0, -1) : type;
        const optionalType = new Set([
            'int', 'float', 'double', 'bool', 'long',
            'short', 'ushort', 'uint', 'ulong', 'byte', 'sbyte'
        ]);
        if (field.default) {
            const isEnumType = enumCaches.has(typeKey);
            let value = getValue(field.default);
            if (isEnumType && (!field.idlType.nullable || (value && value != "default" && value != "null")))
                value = `${typeKey}.${value == '""' ? 'Empty' : toPascalCase(value)}`;
            if (field.idlType.union)
                value = field.idlType.nullable ? 'default' : `new()/*${value}*/`;
            switch (type) {
                case "double":
                    value = `${value}d`;
                    break;
                case "float":
                    value = `${value}f`;
                    break;
            }
            if (!field.required && !field.idlType.nullable) {
                if (!optionalType.has(type) && !isEnumType) {
                    type = `${type}?`;
                    value = 'default';
                }
            }
            return {
                code: `[property: Description("${field.name}")]${type} ${name} = ${value}`,
                arg: `${N}${PP}[Description("${field.name}")]${type} ${toCamelCase(field.name)} = ${value}`,
                optional: true,
                name: toPascalCase(field.name)
            };
        }
        else {
            return {
                code: `[property: Description("${field.name}")]${typeKey}${optionalType.has(typeKey) ? '' : '?'} ${name} = default`,
                arg: `${N}${PP}[Description("${field.name}")]${typeKey}${optionalType.has(typeKey) ? '' : '?'} ${name} = default`,
                optional: false,
                name: toPascalCase(field.name)
            };
        }
    }));
    if (params.length == 1 && params[0].length == 0)
        return `/// <summary>${N}` +
            `/// ${recordName}${N}` +
            `/// </summary>${N}` +
            `[ECMAScript(true)]${N}` +
            `[Description("${(recordName)}")]${N}` +
            `public abstract record ${name}();`;
    let code = `/// <summary>${N}` +
        `/// ${recordName}${N}` +
        `/// </summary>${N}` +
        `[ECMAScript(true)]${N}` +
        `[Description("${(recordName)}")]${N}` +
        `public record ${name}(${N}${P}` +
        params.flat()
            .map(x => x.code)
            .join(',\n	') +
        `)${inherit}`;
    if (items.length > 1)
        code += `${N}{${N}${params.map(p => {
            return `${P}[Category("Optional")]${N}${P}public extern static ${name} Optional${p.map(x => x.name).join('')}(${p.map(x => x.arg).join(',')});`;
        }).join(NN)}${N}}`;
    else
        code += ';';
    return code;
};
function groupBy(arr, keySelector) {
    return arr.reduce((acc, item) => {
        const key = keySelector(item);
        if (!acc[key])
            acc[key] = [];
        acc[key].push(item);
        return acc;
    }, {});
}
function distinct(items) {
    const keySet = new Set();
    return items.filter(item => {
        if (keySet.has(item.item1)) {
            return false; // �ظ���ֵ�����˵�
        }
        keySet.add(item.item1);
        return true;
    });
}
export function collect(source, fileName) {
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
        const ast = webidl2.parse(source);
        const res = {
            Source: source,
            FileName: fileName,
            Namespace: ast.find(x => x.type === 'namespace')?.name,
            RootTypes: ast.sort((a, b) => priority[a.type] - priority[b.type])
        };
        return res;
    }
    catch (error) {
        console.error("ת������:", error);
        throw error;
    }
}
const dir = "C:/Users/hanxj/source/repos/Jazor2/Jazor/Jazor.ECMAScript";
const files = await getWebidls();
const collects = [];
// 1������webidl������ast
files.forEach((source, fileName) => collects.push(collect(source, fileName)));
// 2������ȫ��using
fs.writeFileSync(`${dir}/GlobalUsings.cs`, commonUsings
    .concat(collects.map(item => item.RootTypes
    .filter(x => x.type == 'typedef')
    .map(x => generateTypedef(x, item.Namespace))).flat())
    .join(N), { encoding: 'utf8' });
// 3������ö��
const enumGroups = groupBy(collects.map(item => item.RootTypes
    .filter(x => x.type == 'enum')
    .map(x => {
    return {
        namespace: item.Namespace || '',
        code: generateEnum(x)
    };
})).flat(), p => p.namespace);
for (const [namespace, enums] of Object.entries(enumGroups)) {
    const codePath = namespace == ''
        ? `${dir}/Enums/All.cs`
        : `${dir}/Enums/${toPascalCase(namespace)}.cs`;
    const codeNamespace = path.dirname(codePath);
    if (!fs.existsSync(codeNamespace))
        fs.mkdirSync(codeNamespace, { recursive: true });
    const code = 'namespace Jazor.ECMAScript' +
        (namespace == '' ? `;` : `.${toPascalCase(namespace)};`) +
        NN +
        enums.map(x => x.code).join(NN);
    fs.writeFileSync(codePath, code, 'utf-8');
}
// 4������ί��
const callbackGroups = groupBy(collects.map(item => item.RootTypes
    .filter(x => x.type == 'callback' || x.type == 'callback interface')
    .map(x => {
    return {
        namespace: item.Namespace || '',
        code: x.type == 'callback' ? generateCallback(x, item.Namespace) : generateCallbackInterface(x, item.Namespace)
    };
})).flat(), p => p.namespace);
for (const [namespace, callbacks] of Object.entries(callbackGroups)) {
    const codePath = namespace == ''
        ? `${dir}/Callbacks/All.cs`
        : `${dir}/Callbacks/${toPascalCase(namespace)}.cs`;
    const codeNamespace = path.dirname(codePath);
    if (!fs.existsSync(codeNamespace))
        fs.mkdirSync(codeNamespace, { recursive: true });
    const code = 'namespace Jazor.ECMAScript' +
        (namespace == '' ? `;` : `.${toPascalCase(namespace)};`) +
        NN +
        callbacks.map(x => x.code).join(NN);
    fs.writeFileSync(codePath, code, 'utf-8');
}
// 5�����ɽӿں���
// 5.1 ��ˢ�»���
collects.forEach(item => item.RootTypes
    .filter(x => x.type == 'interface')
    .forEach(x => {
    const className = toPascalCase(x.name);
    const desc = classCaches.has(className)
        ? classCaches.get(className)
        : { props: [], methods: [], ctors: [] };
    for (const member of x.members) {
        let key = member.type;
        switch (member.type) {
            case "constructor": // ���캯��
                desc.ctors.push({
                    item1: member.arguments.length,
                    item2: member,
                    item3: member.arguments.map(p => toSharpType(p.idlType, item.Namespace)).join('@'),
                    item4: member.arguments.map(p => p.name + ':' + toSharpType(p.idlType, item.Namespace)).join('@'),
                });
                break;
            case "operation": // ����
                desc.methods.push({
                    item1: toPascalCase(member.name || ""),
                    item2: member.arguments.map(p => p.name + ':' + toSharpType(p.idlType, item.Namespace)).join('@'),
                    item3: member.idlType ? toSharpType(member.idlType, item.Namespace) : 'void'
                });
                break;
            case "attribute": // ����
                desc.props.push({
                    item1: toPascalCase(member.name),
                    item2: toSharpType(member.idlType, item.Namespace)
                });
                break;
        }
    }
    classCaches.set(className, desc);
}));
// 5.2 �ϲ��ֲ���
const classGroups = groupBy(collects.map(item => item.RootTypes
    .filter(x => x.type == 'interface')
    .map(x => {
    return {
        key: `${item.Namespace}.${x.name}`,
        namespace: item.Namespace || '',
        fileName: item.FileName,
        itemName: x.name,
        itemType: x
    };
})).flat(), p => p.key);
const classCodes = groupBy(Object.entries(classGroups).map(([key, classes]) => {
    const fileName = classes[0].fileName;
    const namespace = classes[0].namespace;
    const className = classes[0].itemName;
    const classTypes = classes.map(x => x.itemType);
    return {
        fileName: fileName,
        namespace: namespace,
        className: className,
        classCode: generateInterface(className, classTypes, namespace)
    };
}), x => x.fileName);
for (const [key, codes] of Object.entries(classCodes)) {
    const fileName = codes[0].fileName;
    const namespace = codes[0].namespace;
    const codePath = namespace == ''
        ? `${dir}/Classes/${toPascalCase(fileName)}.cs`
        : `${dir}/Classes/${toPascalCase(namespace)}/${toPascalCase(fileName)}.cs`;
    const codeNamespace = path.dirname(codePath);
    if (!fs.existsSync(codeNamespace))
        fs.mkdirSync(codeNamespace, { recursive: true });
    const code = 'namespace Jazor.ECMAScript' +
        (namespace == '' ? `;` : `.${toPascalCase(namespace)};`) +
        NN +
        codes.map(x => x.classCode).join(NN);
    fs.writeFileSync(codePath, code, 'utf-8');
}
// 6�������ֵ�
const recordGroups = groupBy(collects.map(item => item.RootTypes
    .filter(x => x.type == 'dictionary')
    .map(x => {
    return {
        key: `${item.Namespace}.${x.name}`,
        namespace: item.Namespace || '',
        fileName: item.FileName,
        itemName: x.name,
        itemType: x
    };
})).flat(), p => p.key);
const recordCodes = groupBy(Object.entries(recordGroups).map(([key, records]) => {
    const fileName = records[0].fileName;
    const namespace = records[0].namespace;
    const recordName = records[0].itemName;
    const recordTypes = records.map(x => x.itemType);
    return {
        fileName: fileName,
        namespace: namespace,
        className: recordName,
        classCode: generateDictionary(recordName, recordTypes, namespace)
    };
}), x => x.fileName);
for (const [key, codes] of Object.entries(recordCodes)) {
    const fileName = codes[0].fileName;
    const namespace = codes[0].namespace;
    const codePath = namespace == ''
        ? `${dir}/Records/${toPascalCase(fileName)}.cs`
        : `${dir}/Records/${toPascalCase(namespace)}/${toPascalCase(fileName)}.cs`;
    const codeNamespace = path.dirname(codePath);
    if (!fs.existsSync(codeNamespace))
        fs.mkdirSync(codeNamespace, { recursive: true });
    const code = 'namespace Jazor.ECMAScript' +
        (namespace == '' ? `;` : `.${toPascalCase(namespace)};`) +
        NN +
        codes.map(x => x.classCode).join(NN);
    fs.writeFileSync(codePath, code, 'utf-8');
}
console.log('Generate completed!');
//# sourceMappingURL=app20250701.js.map