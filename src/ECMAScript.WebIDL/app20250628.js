import * as fs from 'fs';
import * as path from 'path';
import * as webidl2 from "webidl2";
import { getWebidls } from "./webref/idl.js";
const typeMap = {
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
const usingMap = {
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
const keywords = new Set([
    'event', 'namespace', 'default', 'this', 'params', 'base', 'lock', 'async', 'await', 'static', 'virtual',
    'class', 'interface', 'enum', 'record', 'struct', 'using', 'public', 'private', 'protected', 'internal',
    'string', 'int', 'float', 'double', 'bool', 'long', 'short', 'ushort', 'uint', 'ulong', 'byte', 'sbyte'
]);
const defs = new Map();
const enums = new Set();
const classes = new Set();
const records = new Set();
const props = new Set();
// ��ת������
export function convert(idlContent) {
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
        const ast = webidl2.parse(idlContent).sort((a, b) => priority[a.type] - priority[b.type]);
        const res = {
            source: idlContent,
            namespace: ast.find(x => x.type === 'namespace')?.name,
            using: '',
            code: ''
        };
        res.code = `namespace ${res.namespace ? `Jazor.ECMAScript.${res.namespace}` : 'Jazor.ECMAScript'};\n`;
        const generateType = function (idlType, using = false) {
            let returnType = "object";
            switch (idlType.generic) {
                case "":
                    {
                        if (idlType.union) {
                            const type = idlType;
                            let types = [];
                            let hasUndefined = 0;
                            for (const typeName of type.idlType) {
                                const v = generateType(typeName, using);
                                if (v == 'undefined')
                                    hasUndefined++;
                                else
                                    types.push(v);
                            }
                            if (types.length == 1 && hasUndefined > 0) {
                                returnType = `${types[0]}?`;
                            }
                            else {
                                if (using)
                                    returnType = `OneOf.OneOf<${types.join(', ')}>`;
                                else
                                    returnType = `OneOf<${types.join(', ')}>`;
                            }
                        }
                        else {
                            const type = idlType;
                            const defType = typeMap[type.idlType] || type.idlType;
                            if (using) {
                                if (defs.has(defType))
                                    returnType = defs.get(defType) + `/*${defType}*/`;
                                else {
                                    const v1 = typeMap[type.idlType];
                                    returnType = usingMap[defType]
                                        ?? v1
                                        ?? `Jazor.ECMAScript${res.namespace ? `.${res.namespace}` : ''}.${type.idlType}`;
                                }
                            }
                            else
                                returnType = defType;
                        }
                        break;
                    }
                case "sequence":
                    {
                        const type = idlType;
                        const subType = generateType(type.idlType[0], using);
                        returnType = `${subType}[]`;
                        break;
                    }
                case "Promise":
                    {
                        const type = idlType;
                        const subType = generateType(type.idlType[0], using);
                        if (using)
                            returnType = subType == "undefined"
                                ? 'Jazor.ECMAScript.PromiseResult'
                                : `Jazor.ECMAScript.PromiseResult<${subType}>`;
                        else
                            returnType = subType == "undefined"
                                ? 'PromiseResult'
                                : `PromiseResult<${subType}>`;
                        break;
                    }
                case "record":
                    {
                        const type = idlType;
                        const keyType = generateType(type.idlType[0], using);
                        const valueType = generateType(type.idlType[1], using);
                        if (using)
                            returnType = `System.Collections.Generic.Dictionary<${keyType}, ${valueType}>`;
                        else
                            returnType = `Dictionary<${keyType}, ${valueType}>`;
                        break;
                    }
                case "FrozenArray":
                    {
                        const type = idlType;
                        const subType = generateType(type.idlType[0], using);
                        returnType = `FrozenSet<${subType}>`;
                        break;
                    }
                case "ObservableArray":
                    {
                        const type = idlType;
                        const subType = generateType(type.idlType[0], using);
                        returnType = `ObservableCollection<${subType}>`;
                        break;
                    }
                default:
                    returnType = "object";
                    break;
            }
            if (idlType.nullable)
                returnType = `${returnType}?`;
            return returnType;
        };
        const generateValue = function (val) {
            if (!val) {
                return 'null';
            }
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
        const generateEnum = function (enumDef) {
            if (enumDef.name === "XMLHttpRequestResponseType") {
                let a = 123;
            }
            const name = toPascalCase(enumDef.name);
            const exists = enums.has(name);
            if (!exists)
                enums.add(name);
            let code = `/// <summary>
/// ${enumDef.name}
/// </summary>
[Description("${escapeName(enumDef.name)}")]
public enum ${toPascalCase(enumDef.name)} 
{
`;
            code += enumDef.values.map((value, index) => {
                let enumValue = value.value;
                if (typeof enumValue === "string" && enumValue.includes('"')) {
                    enumValue = enumValue.replace(/"/g, '');
                }
                return `    [Description("${enumValue}")]
    ${toPascalCase(enumValue || 'Empty')} = ${index},`;
            }).join("\n\n");
            return code + "\n}";
        };
        const generateTypedef = function (typedef) {
            if (typedef.name === "EventHandler") {
                let a = 123;
            }
            const name = toPascalCase(typedef.name);
            const type = generateType(typedef.idlType, true);
            const exists = defs.has(type);
            let value = exists ? defs.get(type) || type : type;
            if (value.endsWith('?'))
                value = value.slice(0, -1);
            const mark = exists ? `/*${type}*/` : '';
            if (!exists)
                defs.set(name, value);
            return `global using ${name} = ${value};${mark}`;
        };
        const generateInterface = function (interfaceDef) {
            if (interfaceDef.name === "DOMStringMap") {
                let a = res.source;
            }
            if (['CDATASection', 'DOMStringMap'].includes(interfaceDef.name)) {
                return '';
            }
            const props = new Set();
            let name = toPascalCase(interfaceDef.name);
            const exists = classes.has(name);
            if (!exists)
                classes.add(name);
            let code = `/// <summary>
/// ${interfaceDef.name}
/// </summary>${exists ? '' : `\n[Description("${escapeName(interfaceDef.name)}")]`}
public partial class ${name} ${interfaceDef.inheritance ? `: ${toPascalCase(interfaceDef.inheritance)}` : ''} 
{`;
            let memberCodes = [];
            for (const member of interfaceDef.members) {
                switch (member.type) {
                    case "constructor": // ���캯��
                        memberCodes.push(generateConstructor(member));
                        break;
                    case "operation": // ����
                        memberCodes.push(generateMethod(member));
                        break;
                    case "attribute": // ����
                        memberCodes.push(generateProperty(member));
                        break;
                    case "const": // ����
                        memberCodes.push(generateConst(member));
                        break;
                    case "iterable": // �ɵ�������
                        memberCodes.push(generateIterable(member));
                        break;
                }
            }
            return code + '\n' + memberCodes.join('\n\n') + "\n}";
        };
        const generateDictionary = function (dictDef) {
            let name = toPascalCase(dictDef.name);
            let exists = records.has(name);
            if (exists) {
                if (dictDef.members.length == 1) {
                    if (name == 'AuthenticationExtensionsClientInputs' ||
                        name == 'AuthenticationExtensionsClientInputsJSON' ||
                        name == 'AuthenticationExtensionsClientOutputs' ||
                        name == 'AuthenticationExtensionsClientOutputsJSON' ||
                        name == 'MLOpSupportLimits' ||
                        name == 'RequestInit') {
                        name = `${name}${dictDef.members[0].name}`;
                        exists = false;
                    }
                }
            }
            else
                records.add(name);
            if (dictDef.name === "NavigationCurrentEntryChangeEventInit") {
                let a = res.source;
            }
            let code = `/// <summary>
/// ${dictDef.name}
/// </summary>${exists ? '' : `\n[Description("${escapeName(dictDef.name)}")]`}
public partial record ${name}${dictDef.inheritance ? `: ${toPascalCase(dictDef.inheritance)}` : ''}
{${exists ? '' : `\n    public extern ${name}();`}`;
            let params = [];
            let fieldCodes = [];
            for (const field of dictDef.members) {
                const desc = escapeName(field.name);
                const name = toPascalCase(field.name);
                const required = field.required ? "required " : '';
                let type = generateType(field.idlType);
                if (field.default) {
                    const typeKey = type.endsWith('?') ? type.slice(0, -1) : type;
                    const isEnumType = enums.has(typeKey);
                    let value = generateValue(field.default);
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
                        const optionalType = new Set([
                            'string', 'int', 'float', 'double', 'bool', 'long',
                            'short', 'ushort', 'uint', 'ulong', 'byte', 'sbyte'
                        ]);
                        if (!optionalType.has(type) && !isEnumType) {
                            type = `${type}?`;
                            value = 'null';
                        }
                    }
                    params.push({ code: `${type} ${name} = ${value}`, optional: true });
                }
                else {
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
        };
        const generateProperty = function (attribute) {
            const csharpType = generateType(attribute.idlType);
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
                        .split('-') // Split into words
                        .map(word => // Capitalize each word
                     word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
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
        };
        const generateConstructor = function (constructor) {
            let paramsList = "";
            let paramComments = "";
            if (constructor.arguments && constructor.arguments.length > 0) {
                paramsList = constructor.arguments.map(arg => {
                    let argType = generateType(arg.idlType);
                    let argName = toCamelCase(arg.name);
                    if (keywords.has(argName))
                        argName = `@${argName}`;
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
            if (paramComments)
                ctorCode += paramComments + "\n";
            ctorCode += `    public extern ${constructor.parent.name}(${paramsList});`;
            return ctorCode;
        };
        const generateMethod = function (operation) {
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
                    let argType = generateType(arg.idlType);
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
                returnType = generateType(operation.idlType);
            }
            if (returnType == "undefined")
                returnType = "void";
            let methodCode = `    /// <summary>
    /// ${operation.name || "call"} function
    /// </summary>
`;
            if (paramComments)
                methodCode += paramComments + "\n";
            const staticVal = operation.special == 'static' ? 'static ' : '';
            methodCode += `    [Description("${escapedName}")]
    public extern ${staticVal}${returnType} ${methodName}(${paramsList});`;
            return methodCode;
        };
        const generateConst = function (constDef) {
            const csharpType = generateType(constDef.idlType);
            const propName = toPascalCase(constDef.name);
            const escapedName = escapeName(constDef.name);
            const value = generateValue(constDef.value);
            return `    /// <summary>
    /// ${constDef.name}
    /// </summary>
    [Description("${escapedName}")]
    public const ${csharpType} ${propName} = ${value};`;
        };
        const generateIterable = function (iterableDef) {
            const keyType = iterableDef.idlType ? generateType(iterableDef.idlType[0]) : "string";
            const valueType = iterableDef.arguments.length > 1
                ? generateType(iterableDef.arguments[1].idlType)
                : "object";
            return `    // iterableDef: ${iterableDef}
    public extern IEnumerable<KeyValuePair<${keyType}, ${valueType}>> GetEnumerator();`;
        };
        const generateCallbackInterface = function (callbackInterfaceDef) {
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
                    case "operation": // ����
                        {
                            const methodName = toPascalCase(member.name || "");
                            const paramsList = member.arguments.map(arg => {
                                let argType = generateType(arg.idlType);
                                let argName = toCamelCase(arg.name);
                                if (keywords.has(argName))
                                    argName = `@${argName}`;
                                return `${argType} ${argName}`;
                            }).join(", ");
                            let returnType = "void";
                            if (member.idlType)
                                returnType = generateType(member.idlType);
                            if (returnType == "undefined")
                                returnType = "void";
                            code += `\npublic delegate ${returnType} ${name}(${paramsList});/*${methodName}*/`;
                        }
                        break;
                    //case "const": // ����
                    //    code += generateConst(member as webidl2.ConstantMemberType);
                }
            }
            //if (callbackInterfaceDef.members.length > 0) {
            //    code += `\n}`;
            //}
            return code;
        };
        const generateCallback = function (callbackDef) {
            const name = toPascalCase(callbackDef.name);
            let code = '';
            const paramsList = callbackDef.arguments.map(arg => {
                let argType = generateType(arg.idlType);
                let argName = toCamelCase(arg.name);
                if (keywords.has(argName))
                    argName = `@${argName}`;
                return `${argType} ${argName}`;
            }).join(", ");
            let returnType = "void";
            if (callbackDef.idlType)
                returnType = generateType(callbackDef.idlType);
            if (returnType == "undefined")
                returnType = "void";
            code += `\npublic delegate ${returnType} ${name}(${paramsList});`;
            return code;
        };
        for (const definition of ast) {
            switch (definition.type) {
                case 'typedef':
                    res.using += generateTypedef(definition) + "\n";
                    break;
                case 'enum':
                    res.code += "\n" + generateEnum(definition) + "\n";
                    break;
                case 'dictionary':
                    res.code += "\n" + generateDictionary(definition) + "\n";
                    break;
                case 'interface':
                    res.code += "\n" + generateInterface(definition) + "\n";
                    break;
                case 'callback interface':
                    res.code += "\n" + generateCallbackInterface(definition) + "\n";
                    break;
                case 'interface mixin':
                    break;
                case 'namespace':
                    break;
                case 'callback':
                    res.code += "\n" + generateCallback(definition) + "\n";
                    break;
                case 'includes':
                    break;
            }
        }
        return res;
    }
    catch (error) {
        console.error("ת������:", error);
        throw error;
    }
}
// ������ʽת��
function toPascalCase(input) {
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
}
function toCamelCase(input) {
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
}
// ����ת��
function escapeName(name) {
    if (!name)
        return '';
    return name; //.replace(/[^a-zA-Z0-9_]/g, "_");
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
//# sourceMappingURL=app20250628.js.map