function hyphenToCamelCase(name) {
    const camel = name
        .replace(/^-(\w)/, (_, c) => c)
        .replace(/-(\w)/g, (_, c) => c.toUpperCase());
    return camel === "float" ? "_float" : camel;
}
export function generateWebIdlFromCssProperties(properties) {
    return `partial interface CSSStyleDeclaration {${properties
        .map((property) => `\n  [CEReactions] attribute [LegacyNullToEmptyString] CSSOMString ${hyphenToCamelCase(property)};`)
        .join("")}\n};`;
}
//# sourceMappingURL=css.js.map