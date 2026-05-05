import { JQueue as i$988c2c3cb4523609, JStack as i$b2d4a0c380a9f152, RuntimeModule } from "System/RuntimeModule.js";
function ensureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
    throw new Error(message);
}
function ensureYearAndMonth(year, month) {
  ensureWholeNumber(year, "ArgumentOutOfRangeException: Year must be a whole number between 1 and 9999.");
  ensureWholeNumber(month, "ArgumentOutOfRangeException: Month must be a whole number between 1 and 12.");
  if (year < 1 || year > 9999)
    throw new Error("ArgumentOutOfRangeException: Year must be between 1 and 9999.");
  if (month < 1 || month > 12)
    throw new Error("ArgumentOutOfRangeException: Month must be between 1 and 12.");
}
function materializeArray(collection, nullMessage) {
  if (collection === null)
    throw new Error(nullMessage);
  let result = [];
  for (let item of collection)
    result.push(item);
  return result;
}
export class JDateTime {
  #_15721c3f3a339cec;
  get date() {
    return this.#_15721c3f3a339cec;
  }
  #_f8a232df07e8c2a5;
  get kind() {
    return this.#_f8a232df07e8c2a5;
  }
  #_5569017e74bb950d;
  get subMillisecondTicks() {
    return this.#_5569017e74bb950d;
  }
  constructor() {
    let $args = arguments;
    if ($args.length === 1) {
      let date = $args[0];
      this.$ctor_5f7a68d76534e272(date);
      return;
    }
    if ($args.length === 2) {
      let date = $args[0], kind = $args[1];
      this.$ctor_31a0f1908d992f04(date, kind);
      return;
    }
    if ($args.length === 3) {
      let date = $args[0], kind = $args[1], subMillisecondTicks = $args[2];
      this.$ctor_9eb10cd821441a68(date, kind, subMillisecondTicks);
      return;
    }
    throw new Error("No matching constructor overload for JDateTime.");
  }
  $ctor_5f7a68d76534e272(date) {
    this.date = new Date(date.getTime());
    this.kind = 0;
    this.subMillisecondTicks = BigInt.zero;
    Object.defineProperty(this, Symbol.toPrimitive, (() => {
      let __creation$f697bf67c500f21b3820d25b = new PropertyDescriptor;
      __creation$f697bf67c500f21b3820d25b.value = this.toPrimitive.bind(this);
      __creation$f697bf67c500f21b3820d25b.configurable = true;
      return __creation$f697bf67c500f21b3820d25b;
    })());
  }
  $ctor_31a0f1908d992f04(date, kind) {
    this.date = new Date(date.getTime());
    this.kind = kind;
    this.subMillisecondTicks = BigInt.zero;
    Object.defineProperty(this, Symbol.toPrimitive, (() => {
      let __creation$fa33366cc1bff5bce2ed2a16 = new PropertyDescriptor;
      __creation$fa33366cc1bff5bce2ed2a16.value = this.toPrimitive.bind(this);
      __creation$fa33366cc1bff5bce2ed2a16.configurable = true;
      return __creation$fa33366cc1bff5bce2ed2a16;
    })());
  }
  $ctor_9eb10cd821441a68(date, kind, subMillisecondTicks) {
    this.date = new Date(date.getTime());
    this.kind = kind;
    this.subMillisecondTicks = subMillisecondTicks;
    Object.defineProperty(this, Symbol.toPrimitive, (() => {
      let __creation$e235af7a4f169c89c0a5239a = new PropertyDescriptor;
      __creation$e235af7a4f169c89c0a5239a.value = this.toPrimitive.bind(this);
      __creation$e235af7a4f169c89c0a5239a.configurable = true;
      return __creation$e235af7a4f169c89c0a5239a;
    })());
  }
  toString() {
    return RuntimeModule.formatDateOnlyText(this.date.getFullYear(), this.date.getMonth() + 1, this.date.getDate()) + "T" + RuntimeModule.pad2(this.date.getHours()) + ":" + RuntimeModule.pad2(this.date.getMinutes()) + ":" + RuntimeModule.pad2(this.date.getSeconds()) + "." + RuntimeModule.pad7(BigInt(this.date.getMilliseconds()) * BigInt(10000) + this.subMillisecondTicks);
  }
  valueOf() {
    return Date.UTC(this.date.getFullYear(), this.date.getMonth(), this.date.getDate(), this.date.getHours(), this.date.getMinutes(), this.date.getSeconds(), this.date.getMilliseconds());
  }
  toPrimitive(hint) {
    if (hint === "number")
      return this.valueOf();
    return this.toString();
  }
}
export class JDateTimeOffset {
  #_bc4fa8c89ef2305e;
  get utcDateTime() {
    return this.#_bc4fa8c89ef2305e;
  }
  #_86a52a6efda99d07;
  get offsetTicks() {
    return this.#_86a52a6efda99d07;
  }
  #_ebbe9c31b38f8016;
  get utcSubMillisecondTicks() {
    return this.#_ebbe9c31b38f8016;
  }
  constructor() {
    let $args = arguments;
    if ($args.length === 2) {
      let utcDateTime = $args[0], offsetTicks = $args[1];
      this.$ctor_ec78e151ec26d931(utcDateTime, offsetTicks);
      return;
    }
    if ($args.length === 3) {
      let utcDateTime = $args[0], offsetTicks = $args[1], utcSubMillisecondTicks = $args[2];
      this.$ctor_edd22711399c52b9(utcDateTime, offsetTicks, utcSubMillisecondTicks);
      return;
    }
    throw new Error("No matching constructor overload for JDateTimeOffset.");
  }
  $ctor_ec78e151ec26d931(utcDateTime, offsetTicks) {
    this.utcDateTime = new Date(utcDateTime.getTime());
    this.offsetTicks = offsetTicks;
    this.utcSubMillisecondTicks = BigInt.zero;
    Object.defineProperty(this, Symbol.toPrimitive, (() => {
      let __creation$834e8b80c46b4e9f8ca25f59 = new PropertyDescriptor;
      __creation$834e8b80c46b4e9f8ca25f59.value = this.toPrimitive.bind(this);
      __creation$834e8b80c46b4e9f8ca25f59.configurable = true;
      return __creation$834e8b80c46b4e9f8ca25f59;
    })());
  }
  $ctor_edd22711399c52b9(utcDateTime, offsetTicks, utcSubMillisecondTicks) {
    this.utcDateTime = new Date(utcDateTime.getTime());
    this.offsetTicks = offsetTicks;
    this.utcSubMillisecondTicks = utcSubMillisecondTicks;
    Object.defineProperty(this, Symbol.toPrimitive, (() => {
      let __creation$401ff2259366375b5f06b78e = new PropertyDescriptor;
      __creation$401ff2259366375b5f06b78e.value = this.toPrimitive.bind(this);
      __creation$401ff2259366375b5f06b78e.configurable = true;
      return __creation$401ff2259366375b5f06b78e;
    })());
  }
  toString() {
    let local = new Date(this.utcDateTime.getTime() + Number(this.offsetTicks) / 10000);
    let negative = this.offsetTicks < BigInt.zero;
    let absolute = negative ? -this.offsetTicks : this.offsetTicks;
    let totalMinutes = absolute / BigInt(600000000);
    let hours = Number(totalMinutes / BigInt(60));
    let minutes = Number(totalMinutes % BigInt(60));
    let offset = (negative ? "-" : "+") + RuntimeModule.pad2(hours) + ":" + RuntimeModule.pad2(minutes);
    return RuntimeModule.formatDateOnlyText(local.getUTCFullYear(), local.getUTCMonth() + 1, local.getUTCDate()) + "T" + RuntimeModule.pad2(local.getUTCHours()) + ":" + RuntimeModule.pad2(local.getUTCMinutes()) + ":" + RuntimeModule.pad2(local.getUTCSeconds()) + "." + RuntimeModule.pad7(BigInt(local.getUTCMilliseconds()) * BigInt(10000) + this.utcSubMillisecondTicks) + offset;
  }
  valueOf() {
    return this.utcDateTime.getTime();
  }
  toPrimitive(hint) {
    if (hint === "number")
      return this.valueOf();
    return this.toString();
  }
}
export class JDateOnly {
  #_fd4171c16f12a2b7;
  get year() {
    return this.#_fd4171c16f12a2b7;
  }
  #_d85c4eb2681ea5ce;
  get month() {
    return this.#_d85c4eb2681ea5ce;
  }
  #_6aa17e37ff1fe5f5;
  get day() {
    return this.#_6aa17e37ff1fe5f5;
  }
  #_eb9606c3ab5fb5fa;
  get dayNumber() {
    return this.#_eb9606c3ab5fb5fa;
  }
  constructor(year, month, day) {
    this.year = year;
    this.month = month;
    this.day = day;
    let utcDate = RuntimeModule.createUtcDate(year, month, day);
    let start = RuntimeModule.createUtcDate(1, 1, 1);
    this.dayNumber = Math.floor((utcDate.getTime() - start.getTime()) / 86400000);
    Object.defineProperty(this, Symbol.toPrimitive, (() => {
      let __creation$b8269c9f48e7e6d9ea72c130 = new PropertyDescriptor;
      __creation$b8269c9f48e7e6d9ea72c130.value = this.toPrimitive.bind(this);
      __creation$b8269c9f48e7e6d9ea72c130.configurable = true;
      return __creation$b8269c9f48e7e6d9ea72c130;
    })());
  }
  toString() {
    return RuntimeModule.formatDateOnlyText(this.year, this.month, this.day);
  }
  valueOf() {
    return this.dayNumber;
  }
  toPrimitive(hint) {
    if (hint === "number")
      return this.valueOf();
    return this.toString();
  }
}
export class JQueue {
  #_83136e431e35aa99;
  get kind() {
    return this.#_83136e431e35aa99;
  }
  #_9d330afd204a2622;
  get items() {
    return this.#_9d330afd204a2622;
  }
  #_842d95ac7acabbf6;
  get head() {
    return this.#_842d95ac7acabbf6;
  }
  constructor() {
    let $args = arguments;
    if ($args.length === 0) {
      this.$ctor_83a6b5a077092c33();
      return;
    }
    if ($args.length === 1) {
      let collection = $args[0];
      this.$ctor_a172437de92c387f(collection);
      return;
    }
    throw new Error("No matching constructor overload for JQueue.");
  }
  $ctor_83a6b5a077092c33() {
    this.kind = "queue";
    this.items = [];
    this.head = 0;
  }
  $ctor_a172437de92c387f(collection) {
    this.kind = "queue";
    this.items = RuntimeModule.materializeArray(collection, "ArgumentNullException: collection cannot be null.");
    this.head = 0;
  }
  static withCapacity(capacity) {
    RuntimeModule.ensureWholeNumber(capacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
    if (capacity < 0)
      throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");
    return new i$988c2c3cb4523609;
  }
}
export class JStack {
  #_09bbabda9c3b0db2;
  get kind() {
    return this.#_09bbabda9c3b0db2;
  }
  #_f073d7f3274165b9;
  get items() {
    return this.#_f073d7f3274165b9;
  }
  constructor() {
    let $args = arguments;
    if ($args.length === 0) {
      this.$ctor_0be02918366cee67();
      return;
    }
    if ($args.length === 1) {
      let collection = $args[0];
      this.$ctor_a657e829623938c5(collection);
      return;
    }
    throw new Error("No matching constructor overload for JStack.");
  }
  $ctor_0be02918366cee67() {
    this.kind = "stack";
    this.items = [];
  }
  $ctor_a657e829623938c5(collection) {
    this.kind = "stack";
    this.items = RuntimeModule.materializeArray(collection, "ArgumentNullException: collection cannot be null.");
  }
  static withCapacity(capacity) {
    RuntimeModule.ensureWholeNumber(capacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
    if (capacity < 0)
      throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");
    return new i$b2d4a0c380a9f152;
  }
}
export class JTimeOnly {
  #_6eb0185bf87a7385;
  get ticks() {
    return this.#_6eb0185bf87a7385;
  }
  constructor(ticks) {
    let normalized = ticks % BigInt("864000000000");
    this.ticks = normalized < BigInt.zero ? normalized + BigInt("864000000000") : normalized;
    Object.defineProperty(this, Symbol.toPrimitive, (() => {
      let __creation$709efa840ebee99940da937e = new PropertyDescriptor;
      __creation$709efa840ebee99940da937e.value = this.toPrimitive.bind(this);
      __creation$709efa840ebee99940da937e.configurable = true;
      return __creation$709efa840ebee99940da937e;
    })());
  }
  toString() {
    let hour = Number(this.ticks / BigInt("36000000000"));
    let minute = Number(this.ticks / BigInt(600000000) % BigInt(60));
    let second = Number(this.ticks / BigInt(10000000) % BigInt(60));
    let fraction = this.ticks % BigInt(10000000);
    return RuntimeModule.pad2(hour) + ":" + RuntimeModule.pad2(minute) + ":" + RuntimeModule.pad2(second) + "." + RuntimeModule.pad7(fraction);
  }
  valueOf() {
    return this.ticks;
  }
  toPrimitive(hint) {
    if (hint === "number")
      return this.valueOf();
    return this.toString();
  }
}
export class JTimeSpan {
  #_c33e73b59c7c7576;
  get ticks() {
    return this.#_c33e73b59c7c7576;
  }
  constructor(ticks) {
    if (ticks < BigInt("-9223372036854775808") || ticks > BigInt("9223372036854775807"))
      throw new Error("OverflowException: TimeSpan is too long or too short.");
    this.ticks = ticks;
    Object.defineProperty(this, Symbol.toPrimitive, (() => {
      let __creation$1828998812383f30afbc43ff = new PropertyDescriptor;
      __creation$1828998812383f30afbc43ff.value = this.toPrimitive.bind(this);
      __creation$1828998812383f30afbc43ff.configurable = true;
      return __creation$1828998812383f30afbc43ff;
    })());
  }
  toString() {
    let negative = this.ticks < BigInt.zero;
    let absolute = negative ? -this.ticks : this.ticks;
    let days = absolute / BigInt("864000000000");
    let hours = Number(absolute / BigInt("36000000000") % BigInt(24));
    let minutes = Number(absolute / BigInt(600000000) % BigInt(60));
    let seconds = Number(absolute / BigInt(10000000) % BigInt(60));
    let fraction = absolute % BigInt(10000000);
    let text = (negative ? "-" : "") + (days > BigInt.zero ? days.toString() + "." : "") + RuntimeModule.pad2(hours) + ":" + RuntimeModule.pad2(minutes) + ":" + RuntimeModule.pad2(seconds);
    if (fraction !== BigInt.zero)
      text += "." + RuntimeModule.pad7(fraction);
    return text;
  }
  valueOf() {
    return this.ticks;
  }
  toPrimitive(hint) {
    if (hint === "number")
      return this.valueOf();
    return this.toString();
  }
}
export class JGregorianCalendar {
  #_3b95fe9e796f383a;
  get calendarType() {
    return this.#_3b95fe9e796f383a;
  }
  set calendarType(value) {
    this.#_3b95fe9e796f383a = value;
  }
  #_7b8f888d667850a9;
  get twoDigitYearMax() {
    return this.#_7b8f888d667850a9;
  }
  set twoDigitYearMax(value) {
    this.#_7b8f888d667850a9 = value;
  }
  constructor(calendarType, twoDigitYearMax) {
    this.calendarType = calendarType;
    this.twoDigitYearMax = twoDigitYearMax;
    Object.defineProperty(this, Symbol.toPrimitive, (() => {
      let __creation$40799ed5ca87d3216d90891d = new PropertyDescriptor;
      __creation$40799ed5ca87d3216d90891d.value = this.toPrimitive.bind(this);
      __creation$40799ed5ca87d3216d90891d.configurable = true;
      return __creation$40799ed5ca87d3216d90891d;
    })());
  }
  toString() {
    return "System.Globalization.GregorianCalendar";
  }
  valueOf() {
    return this.toString();
  }
  toPrimitive(hint) {
    return this.toString();
  }
}
export function getDaysInMonth(year, month) {
  ensureYearAndMonth(year, month);
  let probe = new Date(0);
  probe.setUTCHours(0, 0, 0, 0);
  probe.setUTCFullYear(year, month, 0);
  return probe.getUTCDate();
}
export function getInt64HashCode(value) {
  let low = Number(BigInt.asIntN(32, value));
  let high = Number(BigInt.asIntN(32, value >> BigInt(32)));
  return low ^ high;
}
function ensureValidDateParts(year, month, day) {
  ensureYearAndMonth(year, month);
  ensureWholeNumber(day, "ArgumentOutOfRangeException: Day must be a whole number.");
  if (day < 1 || day > getDaysInMonth(year, month))
    throw new Error("ArgumentOutOfRangeException: The supplied year, month, or day is out of range.");
}
function ensureValidDateTimeParts(year, month, day, hour, minute, second, millisecond) {
  ensureValidDateParts(year, month, day);
  ensureWholeNumber(hour, "ArgumentOutOfRangeException: Hour must be a whole number.");
  ensureWholeNumber(minute, "ArgumentOutOfRangeException: Minute must be a whole number.");
  ensureWholeNumber(second, "ArgumentOutOfRangeException: Second must be a whole number.");
  ensureWholeNumber(millisecond, "ArgumentOutOfRangeException: Millisecond must be a whole number.");
  if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59 || millisecond < 0 || millisecond > 999)
    throw new Error("ArgumentOutOfRangeException: The supplied date or time component is out of range.");
}
export function createUtcDate(year, month, day) {
  ensureValidDateParts(year, month, day);
  let result = new Date(0);
  result.setUTCHours(0, 0, 0, 0);
  result.setUTCFullYear(year, month - 1, day);
  return result;
}
export function createLocalDate(year, month, day) {
  ensureValidDateParts(year, month, day);
  let result = new Date(0);
  result.setHours(0, 0, 0, 0);
  result.setFullYear(year, month - 1, day);
  return result;
}
export function createLocalDateTime(year, month, day, hour, minute, second, millisecond) {
  ensureValidDateTimeParts(year, month, day, hour, minute, second, millisecond);
  let result = createLocalDate(year, month, day);
  result.setHours(hour, minute, second, millisecond);
  return result;
}
export function formatDateOnlyText(year, month, day) {
  return padLeft(year.toString(), 4) + "-" + pad2(month) + "-" + pad2(day);
}
export function pad2(value) {
  return padLeft(value.toString(), 2);
}
export function pad7(value) {
  return padLeft(value.toString(), 7);
}
export function padLeft(text, width) {
  while (text.length < width)
    text = "0" + text;
  return text;
}
export const RuntimeModule = {
  ensureWholeNumber,
  ensureYearAndMonth,
  materializeArray,
  JDateTime,
  JDateTimeOffset,
  JDateOnly,
  JQueue,
  JStack,
  JTimeOnly,
  JTimeSpan,
  JGregorianCalendar,
  getDaysInMonth,
  getInt64HashCode,
  ensureValidDateParts,
  ensureValidDateTimeParts,
  createUtcDate,
  createLocalDate,
  createLocalDateTime,
  formatDateOnlyText,
  pad2,
  pad7,
  padLeft
};
