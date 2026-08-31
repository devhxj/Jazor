import { _5ad63706a889c294 } from "System/StringModule.js";
export function MaterializeReadOnlyCharSpan(value) {
  let raw = value;
  if (raw === null)
    return "";
  let text = typeof raw === "string" ? raw : null;
  if (text !== null)
    return text;
  let characters = raw;
  let parts = new Array;
  for (let index = 0; index < characters.length; index++)
    parts.push(characters[index]);
  return parts.join("");
}
export function TryDecodeUtf8(utf8Text) {
  if (utf8Text.length >= 3 && utf8Text[0] === 239 && utf8Text[1] === 187 && utf8Text[2] === 191)
    return null;
  try {
    return Utf8Decoder.decode(new Uint8Array(utf8Text));
  } catch {
    return null;
  }
}
let Utf8Decoder = new TextDecoder("utf-8", { fatal: true, ignoreBOM: true });
export function DecodeUtf8OrThrowFormat(utf8Text) {
  let text = TryDecodeUtf8(utf8Text);
  if (text === null)
    throw new Error("FormatException: The UTF-8 input was not in a correct format.");
  return text;
}
function EnsureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
    throw new Error(message);
}
let HashCapacityPrimes = [
  3,
  7,
  11,
  17,
  23,
  29,
  37,
  47,
  59,
  71,
  89,
  107,
  131,
  163,
  197,
  239,
  293,
  353,
  431,
  521,
  631,
  761,
  919,
  1103,
  1327,
  1597,
  1931,
  2333,
  2801,
  3371,
  4049,
  4861,
  5839,
  7013,
  8419,
  10103,
  12143,
  14591,
  17519,
  21023,
  25229,
  30293,
  36353,
  43627,
  52361,
  62851,
  75431,
  90523,
  108631,
  130363,
  156437,
  187751,
  225307,
  270371,
  324449,
  389357,
  467237,
  560689,
  672827,
  807403,
  968897,
  1162687,
  1395263,
  1674319,
  2009191,
  2411033,
  2893249,
  3471899,
  4166287,
  4999559,
  5999471,
  7199369
];
let MaxHashCapacity = 2146435069;
export function GetHashCollectionCapacity(minimum) {
  EnsureWholeNumber(minimum, "ArgumentOutOfRangeException: capacity must be a whole number.");
  if (minimum < 0)
    throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");
  if (minimum === 0)
    return 0;
  for (let index = 0; index < HashCapacityPrimes.length; index++) {
    if (HashCapacityPrimes[index] >= minimum)
      return HashCapacityPrimes[index];
  }
  let candidate = minimum % 2 === 0 ? minimum + 1 : minimum;
  while (candidate <= MaxHashCapacity) {
    if (IsHashCapacityPrime(candidate) && (candidate - 1) % 101 !== 0)
      return candidate;
    candidate += 2;
  }
  throw new Error("OutOfMemoryException: requested collection capacity is too large.");
}
export function ExpandHashCollectionCapacity(currentCapacity) {
  EnsureWholeNumber(currentCapacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
  if (currentCapacity < 0)
    throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");
  if (currentCapacity === 0)
    return GetHashCollectionCapacity(1);
  if (currentCapacity >= MaxHashCapacity)
    return MaxHashCapacity;
  let minimum = currentCapacity * 2;
  if (minimum > MaxHashCapacity)
    return MaxHashCapacity;
  return GetHashCollectionCapacity(minimum);
}
function IsHashCapacityPrime(candidate) {
  if (candidate === 2)
    return true;
  if (candidate < 2 || candidate % 2 === 0)
    return false;
  let limit = Math.floor(Math.sqrt(candidate));
  for (let divisor = 3; divisor <= limit; divisor += 2) {
    if (candidate % divisor === 0)
      return false;
  }
  return true;
}
function EnsureYearAndMonth(year, month) {
  EnsureWholeNumber(year, "ArgumentOutOfRangeException: Year must be a whole number between 1 and 9999.");
  EnsureWholeNumber(month, "ArgumentOutOfRangeException: Month must be a whole number between 1 and 12.");
  if (year < 1 || year > 9999)
    throw new Error("ArgumentOutOfRangeException: Year must be between 1 and 9999.");
  if (month < 1 || month > 12)
    throw new Error("ArgumentOutOfRangeException: Month must be between 1 and 12.");
}
function MaterializeArray(collection, nullMessage) {
  if (collection == null)
    throw new Error(nullMessage);
  let result = new Array;
  for (let item of collection)
    result.push(item);
  return result;
}
const ReadOnlyCarrierMutationMessage = "NotSupportedException: Collection is read-only.";
function ThrowReadOnlyArraySet(target, property, value, receiver) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function ThrowReadOnlyArrayDelete(target, property) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function ThrowReadOnlyArrayDefine(target, property, attributes) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function ThrowReadOnlyArrayMutation() {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function GetReadOnlyArrayProperty(target, property, receiver) {
  let propertyName = property;
  if (propertyName === "copyWithin" || propertyName === "fill" || propertyName === "pop" || propertyName === "push" || propertyName === "reverse" || propertyName === "shift" || propertyName === "sort" || propertyName === "splice" || propertyName === "unshift")
    return ThrowReadOnlyArrayMutation;
  return BindReadOnlyCollectionProperty(target, property);
}
export function CreateReadOnlyArrayView(source, nullMessage) {
  if (source == null)
    throw new Error(nullMessage);
  let handler = {
    get: GetReadOnlyArrayProperty,
    set: ThrowReadOnlyArraySet,
    deleteProperty: ThrowReadOnlyArrayDelete,
    defineProperty: ThrowReadOnlyArrayDefine
  };
  let view = new Proxy(source, handler);
  ReadOnlyCarriers.add(view);
  return view;
}
let ReadOnlyCarriers = new WeakSet;
let MutableListCarriers = new WeakSet;
export function MarkAsMutableListCarrier(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  MutableListCarriers.add(instance);
  return instance;
}
export function IsMutableListCarrier(instance) {
  return instance !== null && MutableListCarriers.has(instance);
}
export function RequireMutableListCarrier(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (MutableListCarriers.has(instance))
    return;
  if (ReadOnlyCarriers.has(instance))
    throw new Error("NotSupportedException: Collection is read-only.");
  throw new Error("NotSupportedException: Collection has a fixed size.");
}
let ReferenceHashCodes = new WeakMap;
let NextReferenceHashCode = 1;
export function GetStringHashCode(text, seed) {
  let hash = seed;
  for (let index = 0; index < text.length; index++)
    hash = hash * 31 + _5ad63706a889c294(text, index).charCodeAt(0) | 0;
  return hash;
}
export function GetHighestSetBit(value) {
  let bit = -1;
  while (value > 0) {
    value = Math.floor(value / 2);
    bit++;
  }
  return bit;
}
function GetReferenceHashCode(value) {
  if (ReferenceHashCodes.has(value))
    return ReferenceHashCodes.get(value);
  let hash = NextReferenceHashCode;
  NextReferenceHashCode = NextReferenceHashCode + 1 | 0;
  if (NextReferenceHashCode === 0)
    NextReferenceHashCode = 1;
  ReferenceHashCodes.set(value, hash);
  return hash;
}
export function GetObjectHashCode(value) {
  if (value === null)
    return 0;
  let type = typeof value;
  if (type === "boolean")
    return value ? 1 : 0;
  if (type === "number") {
    let number = value;
    if (isNaN(number) || number === 0)
      return 0;
    if (Math.floor(number) === number && number >= -2147483648 && number <= 2147483647)
      return number | 0;
    return GetStringHashCode(number.toString(), 17);
  }
  if (type === "string")
    return GetStringHashCode(value, 17);
  if (type === "bigint")
    return GetStringHashCode(value.toString(), 17);
  if (type === "object" || type === "function")
    return GetReferenceHashCode(value);
  return GetStringHashCode(value.toString() ?? "", 17);
}
export function GetStringRepresentation(value) {
  if (value === null)
    return "";
  if (typeof value === "boolean")
    return value ? "True" : "False";
  return value.toString() ?? "";
}
function BindReadOnlyCollectionProperty(target, property) {
  let value = Reflect.get(target, property, target);
  return typeof value === "function" ? value.bind(target) : value;
}
export function IsReadOnlySetCarrier(instance) {
  return instance !== null && ReadOnlyCarriers.has(instance);
}
function ThrowReadOnlySetAdd(item) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function ThrowReadOnlySetDelete(item) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function ThrowReadOnlySetClear() {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function GetReadOnlySetProperty(target, property, receiver) {
  let propertyName = property;
  if (propertyName === "add")
    return ThrowReadOnlySetAdd;
  if (propertyName === "delete")
    return ThrowReadOnlySetDelete;
  if (propertyName === "clear")
    return ThrowReadOnlySetClear;
  return BindReadOnlyCollectionProperty(target, property);
}
function ThrowReadOnlySetPropertySet(target, property, value, receiver) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function ThrowReadOnlySetPropertyDelete(target, property) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function ThrowReadOnlySetPropertyDefine(target, property, attributes) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
export function MarkAsReadOnlySetCarrier(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (IsReadOnlySetCarrier(instance))
    return instance;
  let handler = {
    get: GetReadOnlySetProperty,
    set: ThrowReadOnlySetPropertySet,
    deleteProperty: ThrowReadOnlySetPropertyDelete,
    defineProperty: ThrowReadOnlySetPropertyDefine
  };
  let view = new Proxy(instance, handler);
  ReadOnlyCarriers.add(view);
  return view;
}
export function IsReadOnlyDictionaryCarrier(instance) {
  return instance !== null && ReadOnlyCarriers.has(instance);
}
function ThrowReadOnlyDictionarySet(key, value) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function ThrowReadOnlyDictionaryDelete(key) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function ThrowReadOnlyDictionaryClear() {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function GetReadOnlyDictionaryProperty(target, property, receiver) {
  let propertyName = property;
  if (propertyName === "set")
    return ThrowReadOnlyDictionarySet;
  if (propertyName === "delete")
    return ThrowReadOnlyDictionaryDelete;
  if (propertyName === "clear")
    return ThrowReadOnlyDictionaryClear;
  return BindReadOnlyCollectionProperty(target, property);
}
function ThrowReadOnlyDictionaryPropertySet(target, property, value, receiver) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function ThrowReadOnlyDictionaryPropertyDelete(target, property) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
function ThrowReadOnlyDictionaryPropertyDefine(target, property, attributes) {
  throw new Error("NotSupportedException: Collection is read-only.");
}
export function MarkAsReadOnlyDictionaryCarrier(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (IsReadOnlyDictionaryCarrier(instance))
    return instance;
  let handler = {
    get: GetReadOnlyDictionaryProperty,
    set: ThrowReadOnlyDictionaryPropertySet,
    deleteProperty: ThrowReadOnlyDictionaryPropertyDelete,
    defineProperty: ThrowReadOnlyDictionaryPropertyDefine
  };
  let view = new Proxy(instance, handler);
  ReadOnlyCarriers.add(view);
  return view;
}
export class JDateTime {
  #_15721c3f3a339cec = null;
  get date() {
    return this.#_15721c3f3a339cec;
  }
  #_f8a232df07e8c2a5 = 0;
  get kind() {
    return this.#_f8a232df07e8c2a5;
  }
  #_5569017e74bb950d = null;
  get subMillisecondTicks() {
    return this.#_5569017e74bb950d;
  }
  constructor() {
    let $args = arguments;
    let $ctor = $args[0];
    if ($ctor === "$ctor_5f7a68d76534e272") {
      let date = $args[1];
      this.$ctor_5f7a68d76534e272(date);
      return;
    }
    if ($ctor === "$ctor_31a0f1908d992f04") {
      let date = $args[1], kind = $args[2];
      this.$ctor_31a0f1908d992f04(date, kind);
      return;
    }
    if ($ctor === "$ctor_9eb10cd821441a68") {
      let date = $args[1], kind = $args[2], subMillisecondTicks = $args[3];
      this.$ctor_9eb10cd821441a68(date, kind, subMillisecondTicks);
      return;
    }
    throw new Error("No matching constructor overload for JDateTime.");
  }
  $ctor_5f7a68d76534e272(date) {
    this.#_15721c3f3a339cec = new Date(date.getTime());
    this.#_f8a232df07e8c2a5 = 0;
    this.#_5569017e74bb950d = 0n;
    Object.defineProperty(this, Symbol.toPrimitive, { value: this.toPrimitive.bind(this), configurable: true });
  }
  $ctor_31a0f1908d992f04(date, kind) {
    this.#_15721c3f3a339cec = new Date(date.getTime());
    this.#_f8a232df07e8c2a5 = kind;
    this.#_5569017e74bb950d = 0n;
    Object.defineProperty(this, Symbol.toPrimitive, { value: this.toPrimitive.bind(this), configurable: true });
  }
  $ctor_9eb10cd821441a68(date, kind, subMillisecondTicks) {
    this.#_15721c3f3a339cec = new Date(date.getTime());
    this.#_f8a232df07e8c2a5 = kind;
    this.#_5569017e74bb950d = subMillisecondTicks;
    Object.defineProperty(this, Symbol.toPrimitive, { value: this.toPrimitive.bind(this), configurable: true });
  }
  toString() {
    return FormatDateOnlyText(this.date.getFullYear(), this.date.getMonth() + 1, this.date.getDate()) + "T" + Pad2(this.date.getHours()) + ":" + Pad2(this.date.getMinutes()) + ":" + Pad2(this.date.getSeconds()) + "." + Pad7(BigInt(this.date.getMilliseconds()) * BigInt(10000) + this.subMillisecondTicks);
  }
  valueOf() {
    if (this.kind !== 2)
      return Date.UTC(this.date.getFullYear(), this.date.getMonth(), this.date.getDate(), this.date.getHours(), this.date.getMinutes(), this.date.getSeconds(), this.date.getMilliseconds());
    return this.date.getTime();
  }
  toPrimitive(hint) {
    if (hint === "number")
      return this.valueOf();
    return this.toString();
  }
}
export class JDateTimeOffset {
  #_bc4fa8c89ef2305e = null;
  get utcDateTime() {
    return this.#_bc4fa8c89ef2305e;
  }
  #_86a52a6efda99d07 = null;
  get offsetTicks() {
    return this.#_86a52a6efda99d07;
  }
  #_ebbe9c31b38f8016 = null;
  get utcSubMillisecondTicks() {
    return this.#_ebbe9c31b38f8016;
  }
  constructor() {
    let $args = arguments;
    let $ctor = $args[0];
    if ($ctor === "$ctor_ec78e151ec26d931") {
      let utcDateTime = $args[1], offsetTicks = $args[2];
      this.$ctor_ec78e151ec26d931(utcDateTime, offsetTicks);
      return;
    }
    if ($ctor === "$ctor_edd22711399c52b9") {
      let utcDateTime = $args[1], offsetTicks = $args[2], utcSubMillisecondTicks = $args[3];
      this.$ctor_edd22711399c52b9(utcDateTime, offsetTicks, utcSubMillisecondTicks);
      return;
    }
    throw new Error("No matching constructor overload for JDateTimeOffset.");
  }
  $ctor_ec78e151ec26d931(utcDateTime, offsetTicks) {
    this.#_bc4fa8c89ef2305e = new Date(utcDateTime.getTime());
    this.#_86a52a6efda99d07 = offsetTicks;
    this.#_ebbe9c31b38f8016 = 0n;
    Object.defineProperty(this, Symbol.toPrimitive, { value: this.toPrimitive.bind(this), configurable: true });
  }
  $ctor_edd22711399c52b9(utcDateTime, offsetTicks, utcSubMillisecondTicks) {
    this.#_bc4fa8c89ef2305e = new Date(utcDateTime.getTime());
    this.#_86a52a6efda99d07 = offsetTicks;
    this.#_ebbe9c31b38f8016 = utcSubMillisecondTicks;
    Object.defineProperty(this, Symbol.toPrimitive, { value: this.toPrimitive.bind(this), configurable: true });
  }
  toString() {
    let local = new Date(this.utcDateTime.getTime() + Number(this.offsetTicks) / 10000);
    let negative = this.offsetTicks < 0n;
    let absolute = negative ? -this.offsetTicks : this.offsetTicks;
    let totalMinutes = absolute / BigInt(600000000);
    let hours = Number(totalMinutes / BigInt(60));
    let minutes = Number(totalMinutes % BigInt(60));
    let offset = (negative ? "-" : "+") + Pad2(hours) + ":" + Pad2(minutes);
    return FormatDateOnlyText(local.getUTCFullYear(), local.getUTCMonth() + 1, local.getUTCDate()) + "T" + Pad2(local.getUTCHours()) + ":" + Pad2(local.getUTCMinutes()) + ":" + Pad2(local.getUTCSeconds()) + "." + Pad7(BigInt(local.getUTCMilliseconds()) * BigInt(10000) + this.utcSubMillisecondTicks) + offset;
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
  #_fd4171c16f12a2b7 = 0;
  get year() {
    return this.#_fd4171c16f12a2b7;
  }
  #_d85c4eb2681ea5ce = 0;
  get month() {
    return this.#_d85c4eb2681ea5ce;
  }
  #_6aa17e37ff1fe5f5 = 0;
  get day() {
    return this.#_6aa17e37ff1fe5f5;
  }
  #_eb9606c3ab5fb5fa = 0;
  get dayNumber() {
    return this.#_eb9606c3ab5fb5fa;
  }
  constructor(year, month, day) {
    this.#_fd4171c16f12a2b7 = year;
    this.#_d85c4eb2681ea5ce = month;
    this.#_6aa17e37ff1fe5f5 = day;
    let utcDate = CreateUtcDate(year, month, day);
    let start = CreateUtcDate(1, 1, 1);
    this.#_eb9606c3ab5fb5fa = Math.floor((utcDate.getTime() - start.getTime()) / 86400000);
    Object.defineProperty(this, Symbol.toPrimitive, { value: this.toPrimitive.bind(this), configurable: true });
  }
  toString() {
    return FormatDateOnlyText(this.year, this.month, this.day);
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
export class JIndex {
  #_158b3c4001abbc39 = 0;
  get value() {
    return this.#_158b3c4001abbc39;
  }
  #_8f2ffc71a264c4c4 = false;
  get fromEnd() {
    return this.#_8f2ffc71a264c4c4;
  }
  constructor(value, fromEnd) {
    EnsureWholeNumber(value, "ArgumentOutOfRangeException: Index value must be a non-negative whole number.");
    if (value < 0)
      throw new Error("ArgumentOutOfRangeException: Index value must be non-negative.");
    this.#_158b3c4001abbc39 = value;
    this.#_8f2ffc71a264c4c4 = fromEnd;
  }
  GetOffset(length) {
    EnsureWholeNumber(length, "ArgumentOutOfRangeException: Length must be a non-negative whole number.");
    if (length < 0)
      throw new Error("ArgumentOutOfRangeException: Length must be non-negative.");
    return this.fromEnd ? length - this.value : this.value;
  }
}
export class JRange {
  #_7116bee208d0f484 = null;
  get start() {
    return this.#_7116bee208d0f484;
  }
  #_f362d29ac083e0db = null;
  get end() {
    return this.#_f362d29ac083e0db;
  }
  constructor(start, end) {
    this.#_7116bee208d0f484 = start;
    this.#_f362d29ac083e0db = end;
  }
  GetOffsetAndLength(length) {
    let start = this.start.GetOffset(length);
    let end = this.end.GetOffset(length);
    if (start < 0 || end < start || end > length)
      throw new Error("ArgumentOutOfRangeException: Range is outside the bounds of the collection.");
    return { Offset: start, Length: end - start };
  }
}
export class JQueue {
  #_9d330afd204a2622 = null;
  get items() {
    return this.#_9d330afd204a2622;
  }
  #_3538250d0e7d652a = 0;
  get head() {
    return this.#_3538250d0e7d652a;
  }
  set head(value) {
    this.#_3538250d0e7d652a = value;
  }
  constructor() {
    let $args = arguments;
    let $ctor = $args[0];
    if ($ctor === "$ctor_83a6b5a077092c33") {
      this.$ctor_83a6b5a077092c33();
      return;
    }
    if ($ctor === "$ctor_a172437de92c387f") {
      let collection = $args[1];
      this.$ctor_a172437de92c387f(collection);
      return;
    }
    throw new Error("No matching constructor overload for JQueue.");
  }
  $ctor_83a6b5a077092c33() {
    this.#_9d330afd204a2622 = new Array;
    this.head = 0;
  }
  $ctor_a172437de92c387f(collection) {
    this.#_9d330afd204a2622 = MaterializeArray(collection, "ArgumentNullException: collection cannot be null.");
    this.head = 0;
  }
  static WithCapacity(capacity) {
    EnsureWholeNumber(capacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
    if (capacity < 0)
      throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");
    return new JQueue("$ctor_83a6b5a077092c33");
  }
}
export class JStack {
  #_f073d7f3274165b9 = null;
  get items() {
    return this.#_f073d7f3274165b9;
  }
  constructor() {
    let $args = arguments;
    let $ctor = $args[0];
    if ($ctor === "$ctor_0be02918366cee67") {
      this.$ctor_0be02918366cee67();
      return;
    }
    if ($ctor === "$ctor_a657e829623938c5") {
      let collection = $args[1];
      this.$ctor_a657e829623938c5(collection);
      return;
    }
    throw new Error("No matching constructor overload for JStack.");
  }
  $ctor_0be02918366cee67() {
    this.#_f073d7f3274165b9 = [];
  }
  $ctor_a657e829623938c5(collection) {
    this.#_f073d7f3274165b9 = MaterializeArray(collection, "ArgumentNullException: collection cannot be null.");
  }
  static WithCapacity(capacity) {
    EnsureWholeNumber(capacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
    if (capacity < 0)
      throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");
    return new JStack("$ctor_0be02918366cee67");
  }
}
export class JTimeOnly {
  #_6eb0185bf87a7385 = null;
  get ticks() {
    return this.#_6eb0185bf87a7385;
  }
  constructor(ticks) {
    let normalized = ticks % BigInt("864000000000");
    this.#_6eb0185bf87a7385 = normalized < 0n ? normalized + BigInt("864000000000") : normalized;
    Object.defineProperty(this, Symbol.toPrimitive, { value: this.toPrimitive.bind(this), configurable: true });
  }
  toString() {
    let hour = Number(this.ticks / BigInt("36000000000"));
    let minute = Number(this.ticks / BigInt(600000000) % BigInt(60));
    let second = Number(this.ticks / BigInt(10000000) % BigInt(60));
    let fraction = this.ticks % BigInt(10000000);
    return Pad2(hour) + ":" + Pad2(minute) + ":" + Pad2(second) + "." + Pad7(fraction);
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
  #_c33e73b59c7c7576 = null;
  get ticks() {
    return this.#_c33e73b59c7c7576;
  }
  constructor(ticks) {
    if (ticks < BigInt("-9223372036854775808") || ticks > BigInt("9223372036854775807"))
      throw new Error("OverflowException: TimeSpan is too long or too short.");
    this.#_c33e73b59c7c7576 = ticks;
    Object.defineProperty(this, Symbol.toPrimitive, { value: this.toPrimitive.bind(this), configurable: true });
  }
  toString() {
    let negative = this.ticks < 0n;
    let absolute = negative ? -this.ticks : this.ticks;
    let days = absolute / BigInt("864000000000");
    let hours = Number(absolute / BigInt("36000000000") % BigInt(24));
    let minutes = Number(absolute / BigInt(600000000) % BigInt(60));
    let seconds = Number(absolute / BigInt(10000000) % BigInt(60));
    let fraction = absolute % BigInt(10000000);
    let text = (negative ? "-" : "") + (days > 0n ? days.toString() + "." : "") + Pad2(hours) + ":" + Pad2(minutes) + ":" + Pad2(seconds);
    if (fraction !== 0n)
      text += "." + Pad7(fraction);
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
  #_3b95fe9e796f383a = 0;
  get calendarType() {
    return this.#_3b95fe9e796f383a;
  }
  set calendarType(value) {
    this.#_3b95fe9e796f383a = value;
  }
  #_7b8f888d667850a9 = 0;
  get twoDigitYearMax() {
    return this.#_7b8f888d667850a9;
  }
  set twoDigitYearMax(value) {
    this.#_7b8f888d667850a9 = value;
  }
  constructor(calendarType, twoDigitYearMax) {
    this.calendarType = calendarType;
    this.twoDigitYearMax = twoDigitYearMax;
    Object.defineProperty(this, Symbol.toPrimitive, { value: this.toPrimitive.bind(this), configurable: true });
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
export function RequireGregorianCalendar(calendar) {
  if (calendar === null)
    throw new Error("ArgumentNullException: calendar is null.");
  return calendar;
}
export class JCancellationTokenRegistration {
  #_7d16df61ca45db25 = null;
  get signal() {
    return this.#_7d16df61ca45db25;
  }
  #_ff7336e67821d2f3 = null;
  get handler() {
    return this.#_ff7336e67821d2f3;
  }
  set handler(value) {
    this.#_ff7336e67821d2f3 = value;
  }
  constructor(signal, handler) {
    this.#_7d16df61ca45db25 = signal;
    this.handler = handler;
  }
}
export function RegisterCancellationCallback(signal, callback) {
  if (signal.aborted) {
    callback();
    return new JCancellationTokenRegistration(signal, null);
  }
  let handler = _ => {
    callback();
    return;
  };
  signal.addEventListener("abort", handler, false);
  return new JCancellationTokenRegistration(signal, handler);
}
export function UnregisterCancellationCallback(registration) {
  let handler = registration.handler;
  if (handler === null)
    return false;
  registration.signal.removeEventListener("abort", handler, false);
  registration.handler = null;
  return !registration.signal.aborted;
}
export function GetDaysInMonth(year, month) {
  EnsureYearAndMonth(year, month);
  let probe = new Date(0);
  probe.setUTCHours(0, 0, 0, 0);
  probe.setUTCFullYear(year, month, 0);
  return probe.getUTCDate();
}
export function GetInt64HashCode(value) {
  let low = Number(BigInt.asIntN(32, value));
  let high = Number(BigInt.asIntN(32, value >> BigInt(32)));
  return low ^ high;
}
export function GetInt128HashCode(value) {
  let low = BigInt.asIntN(64, value);
  let high = BigInt.asIntN(64, value >> BigInt(64));
  return GetInt64HashCode(low) ^ GetInt64HashCode(high);
}
function EnsureValidDateParts(year, month, day) {
  EnsureYearAndMonth(year, month);
  EnsureWholeNumber(day, "ArgumentOutOfRangeException: Day must be a whole number.");
  if (day < 1 || day > GetDaysInMonth(year, month))
    throw new Error("ArgumentOutOfRangeException: The supplied year, month, or day is out of range.");
}
function EnsureValidDateTimeParts(year, month, day, hour, minute, second, millisecond) {
  EnsureValidDateParts(year, month, day);
  EnsureWholeNumber(hour, "ArgumentOutOfRangeException: Hour must be a whole number.");
  EnsureWholeNumber(minute, "ArgumentOutOfRangeException: Minute must be a whole number.");
  EnsureWholeNumber(second, "ArgumentOutOfRangeException: Second must be a whole number.");
  EnsureWholeNumber(millisecond, "ArgumentOutOfRangeException: Millisecond must be a whole number.");
  if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59 || millisecond < 0 || millisecond > 999)
    throw new Error("ArgumentOutOfRangeException: The supplied date or time component is out of range.");
}
export function CreateUtcDate(year, month, day) {
  EnsureValidDateParts(year, month, day);
  let result = new Date(0);
  result.setUTCHours(0, 0, 0, 0);
  result.setUTCFullYear(year, month - 1, day);
  return result;
}
export function CreateLocalDate(year, month, day) {
  EnsureValidDateParts(year, month, day);
  let result = new Date(0);
  result.setHours(0, 0, 0, 0);
  result.setFullYear(year, month - 1, day);
  return result;
}
export function CreateLocalDateTime(year, month, day, hour, minute, second, millisecond) {
  EnsureValidDateTimeParts(year, month, day, hour, minute, second, millisecond);
  let result = CreateLocalDate(year, month, day);
  result.setHours(hour, minute, second, millisecond);
  return result;
}
export function FormatDateOnlyText(year, month, day) {
  return PadLeft(year.toString(), 4) + "-" + Pad2(month) + "-" + Pad2(day);
}
export function Pad2(value) {
  return PadLeft(value.toString(), 2);
}
export function Pad7(value) {
  return PadLeft(value.toString(), 7);
}
export function PadLeft(text, width) {
  let missing = width - text.length;
  if (missing <= 0)
    return text;
  let parts = new Array;
  for (let index = 0; index < missing; index++)
    parts.push("0");
  parts.push(text);
  return parts.join("");
}
