import { createRouterMatcher } from "vue-router";

export function assertHostRequirements(hostRequirements) {
  if (hostRequirements === null || typeof hostRequirements !== "object") {
    throw new Error("RazorVue host requirements were not provided to the Playground consumer runtime.");
  }

  if (!Array.isArray(hostRequirements.pluginRequirements)) {
    throw new Error("RazorVue host requirements must expose a pluginRequirements array.");
  }

  if (!Array.isArray(hostRequirements.styles)) {
    throw new Error("RazorVue host requirements must expose a styles array.");
  }

  if (!hostRequirements.pluginRequirements.includes("vuetify")) {
    throw new Error("RazorVue host requirements must declare the Vuetify plugin.");
  }

  if (!hostRequirements.styles.includes("vuetify/styles")) {
    throw new Error("RazorVue host requirements must declare Vuetify styles.");
  }
}

export function resolveRequiredComponentExport(components, exportName) {
  if (components === null || typeof components !== "object") {
    throw new Error("Playground consumer component exports must be provided as an object.");
  }

  const component = components[exportName];
  if (typeof component !== "object" && typeof component !== "function") {
    throw new Error(`Playground consumer expected a '${exportName}' component export.`);
  }

  return component;
}

export function resolveConsumerRoutes(routeDefinitions) {
  if (routeDefinitions == null) {
    throw new Error("Playground consumer routes must be provided by the generated RazorVue consumer entry.");
  }

  if (!Array.isArray(routeDefinitions)) {
    throw new Error("Playground consumer routes must be provided as an array.");
  }

  const normalizedRoutes = routeDefinitions.map((route, index) => normalizeRoute(route, index));
  if (normalizedRoutes.length === 0) {
    throw new Error("Playground consumer routes must contain at least one route.");
  }

  return Object.freeze(normalizedRoutes);
}

export function installShellNavigationInterception(router, routeDefinitions) {
  if (typeof document === "undefined") {
    return;
  }

  document.addEventListener("click", async (event) => {
    if (event.defaultPrevented || event.button !== 0) {
      return;
    }

    const anchor = event.target instanceof Element
      ? event.target.closest("a[href]")
      : null;
    if (!(anchor instanceof HTMLAnchorElement)) {
      return;
    }

    if (anchor.target && anchor.target !== "_self") {
      return;
    }

    if (anchor.hasAttribute("download")) {
      return;
    }

    if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
      return;
    }

    const url = new URL(anchor.href, window.location.href);
    if (url.origin !== window.location.origin) {
      return;
    }

    if (!shouldHandleClientRoute(url.pathname, routeDefinitions)) {
      return;
    }

    event.preventDefault();
    await router.push(`${url.pathname}${url.search}${url.hash}`);
  });
}

function shouldHandleClientRoute(pathname, routeDefinitions) {
  return routeDefinitions.some((route) => doesRouteMatchPath(route, pathname));
}

export function doesRouteMatchPath(routeDefinition, pathname) {
  const resolved = getRouteMatcher(routeDefinition).resolve(
    { path: pathname },
    createMatcherCurrentLocation()
  );
  return Array.isArray(resolved.matched) &&
    resolved.matched.length > 0 &&
    doRouteParametersSatisfyConstraints(routeDefinition, resolved.params);
}

function normalizeRoute(route, index) {
  if (route === null || typeof route !== "object") {
    throw new Error(`Playground consumer route at index ${index} is invalid.`);
  }

  const name = typeof route.name === "string" && route.name.length > 0
    ? route.name
    : null;
  const alias = typeof route.alias === "string" && route.alias.length > 0
    ? route.alias
    : null;
  const path = typeof route.path === "string" && route.path.startsWith("/")
    ? route.path
    : null;
  const parameterNames = Array.isArray(route.parameterNames)
    ? route.parameterNames.filter((item) => typeof item === "string" && item.length > 0)
    : [];
  const defaultParameterValues = normalizeDefaultParameterValues(route.defaultParameterValues, parameterNames);
  const elidableDefaultParameterNames = normalizeElidableDefaultParameterNames(
    route.elidableDefaultParameterNames,
    defaultParameterValues
  );
  const parameterConstraints = normalizeParameterConstraints(route.parameterConstraints, parameterNames);

  if (!name || !alias || !path) {
    throw new Error(`Playground consumer route at index ${index} is missing required metadata.`);
  }

  return Object.freeze({
    name,
    alias,
    componentId: typeof route.componentId === "string" ? route.componentId : "",
    componentName: typeof route.componentName === "string" ? route.componentName : alias,
    componentModel: typeof route.componentModel === "string" ? route.componentModel : "",
    routeTemplate: typeof route.routeTemplate === "string" ? route.routeTemplate : path,
    path,
    parameterNames: Object.freeze(parameterNames),
    defaultParameterValues: Object.freeze(defaultParameterValues),
    elidableDefaultParameterNames: Object.freeze(elidableDefaultParameterNames),
    parameterConstraints: Object.freeze(parameterConstraints)
  });
}

function normalizeDefaultParameterValues(values, parameterNames) {
  if (values === null || typeof values !== "object" || Array.isArray(values)) {
    return {};
  }

  const allowedNames = new Set(parameterNames);
  const normalized = {};
  for (const [key, value] of Object.entries(values)) {
    if (!allowedNames.has(key) || typeof value !== "string") {
      continue;
    }

    normalized[key] = value;
  }

  return normalized;
}

function normalizeElidableDefaultParameterNames(values, defaultParameterValues) {
  if (!Array.isArray(values)) {
    return [];
  }

  const normalized = [];
  const allowedNames = new Set(Object.keys(defaultParameterValues));
  for (const value of values) {
    if (typeof value !== "string" || value.length === 0 || !allowedNames.has(value)) {
      continue;
    }

    if (!normalized.includes(value)) {
      normalized.push(value);
    }
  }

  return normalized;
}

function normalizeParameterConstraints(values, parameterNames) {
  if (values === null || typeof values !== "object" || Array.isArray(values)) {
    return {};
  }

  const allowedNames = new Set(parameterNames);
  const normalized = {};
  for (const [key, constraints] of Object.entries(values)) {
    if (!allowedNames.has(key) || !Array.isArray(constraints)) {
      continue;
    }

    const normalizedConstraints = constraints
      .map(normalizeParameterConstraint)
      .filter((constraint) => constraint !== null);
    if (normalizedConstraints.length > 0) {
      normalized[key] = Object.freeze(normalizedConstraints);
    }
  }

  return normalized;
}

function normalizeParameterConstraint(constraint) {
  if (constraint === null || typeof constraint !== "object") {
    return null;
  }

  if (constraint.kind === "integerRange") {
    return normalizeRangeParameterConstraint("integerRange", constraint, isCanonicalIntegerConstraintBoundary);
  }

  if (constraint.kind === "lengthRange") {
    return normalizeRangeParameterConstraint("lengthRange", constraint, isCanonicalNonNegativeIntegerBoundary);
  }

  if (
    constraint.kind === "numberParse" &&
    (constraint.format === "decimal" || constraint.format === "double" || constraint.format === "float")
  ) {
    return Object.freeze({
      kind: "numberParse",
      format: constraint.format
    });
  }

  if (constraint.kind === "dateTimeParse") {
    return Object.freeze({
      kind: "dateTimeParse"
    });
  }

  return null;
}

function normalizeRangeParameterConstraint(kind, constraint, isValidBoundary) {
  const min = typeof constraint.min === "string" && isValidBoundary(constraint.min)
    ? constraint.min
    : null;
  const max = typeof constraint.max === "string" && isValidBoundary(constraint.max)
    ? constraint.max
    : null;
  if (min === null && max === null) {
    return null;
  }

  return Object.freeze({ kind, min, max });
}

function isCanonicalIntegerConstraintBoundary(value) {
  return /^-?(0|[1-9]\d*)$/.test(value);
}

function isCanonicalNonNegativeIntegerBoundary(value) {
  return /^(0|[1-9]\d*)$/.test(value);
}

export function applyRouteDefaultParameterValues(routeDefinition, routeParameters = {}) {
  const normalized = routeParameters !== null && typeof routeParameters === "object"
    ? { ...routeParameters }
    : {};

  const defaultValues = routeDefinition?.defaultParameterValues;
  if (defaultValues === null || typeof defaultValues !== "object") {
    return normalized;
  }

  for (const [key, defaultValue] of Object.entries(defaultValues)) {
    if (!Object.prototype.hasOwnProperty.call(normalized, key)) {
      normalized[key] = defaultValue;
      continue;
    }

    const currentValue = normalized[key];
    if (currentValue === "" || currentValue === null || typeof currentValue === "undefined") {
      normalized[key] = defaultValue;
    }
  }

  return normalized;
}

export function doRouteParametersSatisfyConstraints(routeDefinition, routeParameters = {}) {
  const parameterConstraints = routeDefinition?.parameterConstraints;
  if (parameterConstraints === null || typeof parameterConstraints !== "object") {
    return true;
  }

  const normalizedParameters = applyRouteDefaultParameterValues(routeDefinition, routeParameters);
  for (const [parameterName, constraints] of Object.entries(parameterConstraints)) {
    if (!Array.isArray(constraints) || constraints.length === 0) {
      continue;
    }

    const value = normalizedParameters[parameterName];
    if (value === "" || value === null || typeof value === "undefined") {
      continue;
    }

    const values = Array.isArray(value) ? value : [value];
    if (values.length === 0) {
      return false;
    }

    for (const item of values) {
      if (typeof item !== "string" || !doesRouteParameterValueSatisfyConstraints(item, constraints)) {
        return false;
      }
    }
  }

  return true;
}

function doesRouteParameterValueSatisfyConstraints(value, constraints) {
  return constraints.every((constraint) => {
    if (constraint?.kind === "integerRange") {
      return isIntegerInRange(value, constraint.min, constraint.max);
    }

    if (constraint?.kind === "lengthRange") {
      return isTextLengthInRange(value, constraint.min, constraint.max);
    }

    if (constraint?.kind === "numberParse") {
      return constraint.format === "decimal"
        ? isInvariantDecimalText(value)
        : isInvariantFloatingPointText(value);
    }

    if (constraint?.kind === "dateTimeParse") {
      return isInvariantDateTimeText(value);
    }

    return false;
  });
}

function isIntegerInRange(value, min, max) {
  if (!/^[+-]?\d+$/.test(value)) {
    return false;
  }

  let parsed;
  try {
    parsed = BigInt(value);
  } catch {
    return false;
  }

  if (typeof min === "string" && parsed < BigInt(min)) {
    return false;
  }

  if (typeof max === "string" && parsed > BigInt(max)) {
    return false;
  }

  return true;
}

function isTextLengthInRange(value, min, max) {
  const length = value.length;

  if (typeof min === "string" && length < Number.parseInt(min, 10)) {
    return false;
  }

  if (typeof max === "string" && length > Number.parseInt(max, 10)) {
    return false;
  }

  return true;
}

function isInvariantFloatingPointText(value) {
  const text = tryDecodeRouteParameter(value).trim();
  if (/^[+-]?(?:nan|infinity)$/i.test(text)) {
    return true;
  }

  return /^[+-]?(?:(?:\d[\d,]*(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)$/.test(text);
}

function isInvariantDecimalText(value) {
  const parsed = tryParseInvariantDecimal(value);
  if (parsed === null) {
    return false;
  }

  const magnitude = parsed.magnitude < 0n ? -parsed.magnitude : parsed.magnitude;
  const decimalMax = 79228162514264337593543950335n;
  return magnitude <= decimalMax;
}

function tryParseInvariantDecimal(value) {
  const text = tryDecodeRouteParameter(value).trim();
  const match = text.match(/^([+-])?((?:\d[\d,]*(?:\.\d*)?|\.\d+))([+-])?$/);
  if (!match) {
    return null;
  }

  if (match[1] && match[3]) {
    return null;
  }

  const isNegative = match[1] === "-" || match[3] === "-";
  const unsigned = match[2].replaceAll(",", "");
  const separatorIndex = unsigned.indexOf(".");
  const integerPart = separatorIndex === -1 ? unsigned : unsigned.slice(0, separatorIndex);
  const fractionPart = separatorIndex === -1 ? "" : unsigned.slice(separatorIndex + 1);

  if (integerPart.length === 0 && fractionPart.length === 0) {
    return null;
  }

  const digits = `${integerPart}${fractionPart}`;
  if (!/^\d+$/.test(digits)) {
    return null;
  }

  return roundDecimalDigitsToScale(digits, fractionPart.length, 28, isNegative);
}

function roundDecimalDigitsToScale(digits, scale, targetScale, isNegative) {
  let normalizedDigits = digits.replace(/^0+(?=\d)/, "");
  let normalizedScale = scale;
  const precisionTrimCount = Math.max(0, normalizedDigits.length - 29);
  const scaleTrimCount = Math.max(0, normalizedScale - targetScale);
  const trimCount = Math.max(precisionTrimCount, scaleTrimCount);
  if (trimCount > 0) {
    const keptLength = normalizedDigits.length - trimCount;
    const keptDigits = keptLength > 0 ? normalizedDigits.slice(0, keptLength) : "0";
    const discardedDigits = keptLength > 0 ? normalizedDigits.slice(keptLength) : normalizedDigits.padStart(trimCount, "0");
    const keptMagnitude = BigInt(keptDigits);
    normalizedDigits = shouldRoundDecimalToEven(keptMagnitude, discardedDigits)
      ? (keptMagnitude + 1n).toString()
      : keptDigits;
    normalizedScale = Math.max(0, normalizedScale - trimCount);
  }

  while (normalizedScale > 0 && normalizedDigits.endsWith("0")) {
    normalizedDigits = normalizedDigits.slice(0, -1);
    normalizedScale -= 1;
  }

  const magnitude = BigInt(normalizedDigits.length === 0 ? "0" : normalizedDigits);
  return Object.freeze({
    magnitude: isNegative ? -magnitude : magnitude,
    scale: normalizedScale
  });
}

function shouldRoundDecimalToEven(keptMagnitude, discardedDigits) {
  if (discardedDigits.length === 0) {
    return false;
  }

  const first = discardedDigits.charCodeAt(0) - 48;
  if (first > 5) {
    return true;
  }

  if (first < 5) {
    return false;
  }

  const restHasNonZero = discardedDigits.slice(1).includes("1") ||
    discardedDigits.slice(1).includes("2") ||
    discardedDigits.slice(1).includes("3") ||
    discardedDigits.slice(1).includes("4") ||
    discardedDigits.slice(1).includes("5") ||
    discardedDigits.slice(1).includes("6") ||
    discardedDigits.slice(1).includes("7") ||
    discardedDigits.slice(1).includes("8") ||
    discardedDigits.slice(1).includes("9");
  return restHasNonZero || (keptMagnitude % 2n !== 0n);
}

const invariantMonthNames = Object.freeze({
  jan: 1,
  january: 1,
  feb: 2,
  february: 2,
  mar: 3,
  march: 3,
  apr: 4,
  april: 4,
  may: 5,
  jun: 6,
  june: 6,
  jul: 7,
  july: 7,
  aug: 8,
  august: 8,
  sep: 9,
  sept: 9,
  september: 9,
  oct: 10,
  october: 10,
  nov: 11,
  november: 11,
  dec: 12,
  december: 12
});

const invariantDayNames = Object.freeze({
  sun: 0,
  sunday: 0,
  mon: 1,
  monday: 1,
  tue: 2,
  tuesday: 2,
  wed: 3,
  wednesday: 3,
  thu: 4,
  thursday: 4,
  fri: 5,
  friday: 5,
  sat: 6,
  saturday: 6
});

function isInvariantDateTimeText(value) {
  const text = tryDecodeRouteParameter(value).trim();
  if (text.length === 0) {
    return false;
  }

  const weekdayMatch = text.match(/^([A-Za-z]+),\s+(.+)$/);
  if (weekdayMatch) {
    const expectedDay = invariantDayNames[weekdayMatch[1].toLowerCase()];
    if (typeof expectedDay !== "number") {
      return false;
    }

    const parsed = tryParseInvariantDateTimeCore(weekdayMatch[2]);
    return parsed !== null &&
      parsed.date !== null &&
      getInvariantDayOfWeek(parsed.date.year, parsed.date.month, parsed.date.day) === expectedDay;
  }

  return tryParseInvariantDateTimeCore(text) !== null;
}

function tryParseInvariantDateTimeCore(text) {
  const zoneResult = tryReadInvariantTimeZoneSuffix(text);
  if (zoneResult === null) {
    return null;
  }

  const parsed = tryParseIsoLikeInvariantDateTime(zoneResult.text) ??
    tryParseNumericInvariantDateTime(zoneResult.text) ??
    tryParseMonthNameFirstInvariantDateTime(zoneResult.text) ??
    tryParseDayMonthNameInvariantDateTime(zoneResult.text) ??
    tryParseTimeOnlyInvariantDateTime(zoneResult.text);
  if (parsed?.date === null && zoneResult.suffix === "Z") {
    return null;
  }

  return parsed;
}

function tryReadInvariantTimeZoneSuffix(text) {
  const trimmed = text.trim();
  const match = trimmed.match(/^(.*?)(?:\s*(Z|GMT|[+-]\d{2}:?\d{2}))$/i);
  if (!match) {
    return Object.freeze({ text: trimmed, suffix: null });
  }

  const remainder = match[1].trimEnd();
  if (remainder.length === 0) {
    return null;
  }

  const suffix = match[2].toUpperCase();
  if (suffix === "Z" || suffix === "GMT") {
    return Object.freeze({ text: remainder, suffix });
  }

  const offsetMatch = suffix.match(/^([+-])(\d{2}):?(\d{2})$/);
  if (!offsetMatch) {
    return null;
  }

  const hours = Number.parseInt(offsetMatch[2], 10);
  const minutes = Number.parseInt(offsetMatch[3], 10);
  if (hours > 14 || minutes > 59 || (hours === 14 && minutes !== 0)) {
    return null;
  }

  return Object.freeze({ text: remainder, suffix: "offset" });
}

function tryParseIsoLikeInvariantDateTime(text) {
  const match = text.match(/^(\d{1,4})([-/.])(\d{1,2})(?:\2(\d{1,2}))?(?:[T\s]+(.+))?$/);
  if (!match) {
    return null;
  }

  const year = parseInvariantYear(match[1]);
  const month = Number.parseInt(match[3], 10);
  const day = typeof match[4] === "string" ? Number.parseInt(match[4], 10) : 1;
  return buildInvariantDateTimeParseResult(year, month, day, match[5] ?? "");
}

function tryParseNumericInvariantDateTime(text) {
  const match = text.match(/^(\d{1,2})([/.-])(\d{1,2})(?:\2(\d{1,4}))?(?:\s+(.+))?$/);
  if (!match) {
    return null;
  }

  const month = Number.parseInt(match[1], 10);
  const day = Number.parseInt(match[3], 10);
  const year = typeof match[4] === "string"
    ? parseInvariantYear(match[4])
    : new Date().getFullYear();
  return buildInvariantDateTimeParseResult(year, month, day, match[5] ?? "");
}

function tryParseMonthNameFirstInvariantDateTime(text) {
  const match = text.match(/^([A-Za-z]+)\s+(\d{1,2})(?:,\s*|\s+)?(.*)$/);
  if (!match) {
    return null;
  }

  const month = invariantMonthNames[match[1].toLowerCase()];
  if (typeof month !== "number") {
    return null;
  }

  const day = Number.parseInt(match[2], 10);
  const tail = match[3].trim();
  return buildInvariantDateTimeParseResultFromTail(month, day, tail);
}

function tryParseDayMonthNameInvariantDateTime(text) {
  const match = text.match(/^(\d{1,2})\s+([A-Za-z]+)(?:,\s*|\s+)?(.*)$/);
  if (!match) {
    return null;
  }

  const day = Number.parseInt(match[1], 10);
  const month = invariantMonthNames[match[2].toLowerCase()];
  if (typeof month !== "number") {
    return null;
  }

  const tail = match[3].trim();
  return buildInvariantDateTimeParseResultFromTail(month, day, tail);
}

function buildInvariantDateTimeParseResultFromTail(month, day, tail) {
  if (tail.length === 0) {
    return buildInvariantDateTimeParseResult(new Date().getFullYear(), month, day, "");
  }

  const yearMatch = tail.match(/^(\d{1,4})(?:\s+(.+))?$/);
  if (yearMatch) {
    return buildInvariantDateTimeParseResult(
      parseInvariantYear(yearMatch[1]),
      month,
      day,
      yearMatch[2] ?? "");
  }

  return buildInvariantDateTimeParseResult(new Date().getFullYear(), month, day, tail);
}

function tryParseTimeOnlyInvariantDateTime(text) {
  const time = tryParseInvariantTime(text);
  return time === null ? null : Object.freeze({ date: null, time });
}

function buildInvariantDateTimeParseResult(year, month, day, timeText) {
  if (!isValidInvariantDate(year, month, day)) {
    return null;
  }

  const time = timeText.length === 0 ? null : tryParseInvariantTime(timeText);
  if (timeText.length > 0 && time === null) {
    return null;
  }

  if (time?.rollsToNextDay === true && year === 9999 && month === 12 && day === 31) {
    return null;
  }

  return Object.freeze({
    date: Object.freeze({ year, month, day }),
    time
  });
}

function parseInvariantYear(value) {
  const year = Number.parseInt(value, 10);
  if (value.length === 2) {
    return year <= 49 ? 2000 + year : 1900 + year;
  }

  return year;
}

function tryParseInvariantTime(text) {
  const match = text.trim().match(/^(\d{1,2})(?::(\d{1,2})(?::(\d{1,2})(?:\.\d+)?)?)?(?:\s*([AaPp][Mm]))?$/);
  if (!match) {
    return null;
  }

  if (typeof match[2] !== "string" && typeof match[4] !== "string") {
    return null;
  }

  let hour = Number.parseInt(match[1], 10);
  const minute = typeof match[2] === "string" ? Number.parseInt(match[2], 10) : 0;
  const second = typeof match[3] === "string" ? Number.parseInt(match[3], 10) : 0;
  const meridiem = typeof match[4] === "string" ? match[4].toUpperCase() : null;

  if (minute > 59 || second > 59) {
    return null;
  }

  if (meridiem !== null) {
    if (hour < 1 || hour > 12) {
      return null;
    }

    if (meridiem === "PM" && hour !== 12) {
      hour += 12;
    } else if (meridiem === "AM" && hour === 12) {
      hour = 0;
    }
  }

  const rollsToNextDay = hour === 24 && minute === 0 && second === 0 && meridiem === null;
  if (!rollsToNextDay && hour > 23) {
    return null;
  }

  return Object.freeze({ rollsToNextDay });
}

function isValidInvariantDate(year, month, day) {
  return Number.isInteger(year) &&
    Number.isInteger(month) &&
    Number.isInteger(day) &&
    year >= 1 &&
    year <= 9999 &&
    month >= 1 &&
    month <= 12 &&
    day >= 1 &&
    day <= getDaysInInvariantMonth(year, month);
}

function getDaysInInvariantMonth(year, month) {
  if (month === 2) {
    return isInvariantLeapYear(year) ? 29 : 28;
  }

  return month === 4 || month === 6 || month === 9 || month === 11 ? 30 : 31;
}

function isInvariantLeapYear(year) {
  return year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0);
}

function getInvariantDayOfWeek(year, month, day) {
  const adjustedMonth = month < 3 ? month + 12 : month;
  const adjustedYear = month < 3 ? year - 1 : year;
  const yearInCentury = adjustedYear % 100;
  const zeroBasedCentury = Math.trunc(adjustedYear / 100);
  const zeller =
    (day +
      Math.trunc((13 * (adjustedMonth + 1)) / 5) +
      yearInCentury +
      Math.trunc(yearInCentury / 4) +
      Math.trunc(zeroBasedCentury / 4) +
      5 * zeroBasedCentury) % 7;
  return (zeller + 6) % 7;
}

function tryDecodeRouteParameter(value) {
  try {
    return decodeURIComponent(value);
  } catch {
    return value;
  }
}

const routeMatcherCache = new WeakMap();

function getRouteMatcher(routeDefinition) {
  let matcher = routeMatcherCache.get(routeDefinition);
  if (matcher) {
    return matcher;
  }

  matcher = createRouterMatcher(
    [
      {
        path: routeDefinition.path,
        name: routeDefinition.name,
        component: {}
      }
    ],
    {}
  );
  routeMatcherCache.set(routeDefinition, matcher);
  return matcher;
}

function createMatcherCurrentLocation() {
  return {
    path: "/",
    fullPath: "/",
    params: {},
    query: {},
    hash: "",
    matched: [],
    meta: {}
  };
}
