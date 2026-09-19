import { GetStringRepresentation, MaterializeReadOnlyCharSpan } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
let DefaultCapacity = 16;
let DefaultMaxCapacity = 2147483647;
let CapacityStates = new WeakMap;
function EnsureInstance(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
}
function EnsureWholeNumber(value, parameterName) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName ?? ""} must be a whole number.`);
}
function EnsureNonNegative(value, parameterName) {
  EnsureWholeNumber(value, parameterName);
  if (value < 0)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName ?? ""} cannot be negative.`);
}
function GetCapacityState(instance) {
  EnsureInstance(instance);
  if (CapacityStates.has(instance))
    return CapacityStates.get(instance);
  let capacity = instance.length > DefaultCapacity ? instance.length : DefaultCapacity;
  let state = new Array(capacity, DefaultMaxCapacity);
  CapacityStates.set(instance, state);
  return state;
}
function GetInitialCapacity(requestedCapacity, maxCapacity, textLength) {
  EnsureNonNegative(requestedCapacity, "capacity");
  EnsureWholeNumber(maxCapacity, "maxCapacity");
  if (maxCapacity <= 0 || requestedCapacity > maxCapacity || textLength > maxCapacity)
    throw new Error("ArgumentOutOfRangeException: Capacity exceeds maximum capacity.");
  let capacity = requestedCapacity === 0 ? maxCapacity < DefaultCapacity ? maxCapacity : DefaultCapacity : requestedCapacity;
  return textLength > capacity ? textLength : capacity;
}
function CreateBuilder(value, capacity, maxCapacity) {
  let text = value ?? "";
  let initialCapacity = GetInitialCapacity(capacity, maxCapacity, text.length);
  let instance = new Array;
  for (let index = 0; index < text.length; index++)
    instance.push(_5ad63706a889c294(text, index));
  CapacityStates.set(instance, new Array(initialCapacity, maxCapacity));
  return instance;
}
function EnsureTargetLength(instance, targetLength) {
  let state = GetCapacityState(instance);
  if (state[0] < instance.length)
    state[0] = instance.length;
  if (targetLength <= instance.length)
    return;
  if (targetLength <= state[0])
    return;
  if (targetLength > state[1])
    throw new Error("ArgumentOutOfRangeException: The length cannot be greater than the capacity.");
  let doubledCapacity = state[0] * 2;
  state[0] = targetLength > doubledCapacity ? targetLength : doubledCapacity;
}
function EnsureAdditionalCapacity(instance, additionalLength) {
  if (additionalLength === 0)
    return;
  EnsureTargetLength(instance, instance.length + additionalLength);
}
function GetCapacity(instance) {
  return GetCapacityState(instance)[0];
}
function GetMaxCapacity(instance) {
  return GetCapacityState(instance)[1];
}
function SetCapacity(instance, value) {
  EnsureInstance(instance);
  EnsureNonNegative(value, "value");
  let state = GetCapacityState(instance);
  if (value > state[1] || value < instance.length)
    throw new Error("ArgumentOutOfRangeException: Capacity is outside the supported range.");
  state[0] = value;
}
function EnsureCapacity(instance, value) {
  EnsureInstance(instance);
  EnsureNonNegative(value, "capacity");
  let state = GetCapacityState(instance);
  if (value > state[1])
    throw new Error("ArgumentOutOfRangeException: Capacity exceeds maximum capacity.");
  if (value > state[0])
    state[0] = value;
  return state[0];
}
function EnsureInsertIndex(instance, index) {
  EnsureInstance(instance);
  EnsureWholeNumber(index, "index");
  if (index < 0 || index > instance.length)
    throw new Error("ArgumentOutOfRangeException: index is outside the builder.");
}
function EnsureExistingIndex(instance, index) {
  EnsureInstance(instance);
  EnsureWholeNumber(index, "index");
  if (index < 0 || index >= instance.length)
    throw new Error("ArgumentOutOfRangeException: index is outside the builder.");
}
function EnsureRange(instance, startIndex, length) {
  EnsureInstance(instance);
  EnsureNonNegative(startIndex, "startIndex");
  EnsureNonNegative(length, "length");
  if (startIndex > instance.length - length)
    throw new Error("ArgumentOutOfRangeException: startIndex and length must identify a valid range.");
}
function JoinRange(value, startIndex, length) {
  let result = "";
  for (let offset = 0; offset < length; offset++)
    result += value[startIndex + offset];
  return result;
}
function EqualsBuilder(instance, other) {
  EnsureInstance(instance);
  if (other === null || instance.length !== other.length)
    return false;
  for (let index = 0; index < instance.length; index++) {
    if (instance[index] !== other[index])
      return false;
  }
  return true;
}
function AppendText(instance, value) {
  EnsureInstance(instance);
  if (value === null)
    return instance;
  EnsureAdditionalCapacity(instance, value.length);
  for (let index = 0; index < value.length; index++)
    instance.push(_5ad63706a889c294(value, index));
  return instance;
}
function AppendJoinedStrings(instance, separator, values) {
  EnsureInstance(instance);
  if (values === null)
    throw new Error("ArgumentNullException: values is null.");
  let text = "";
  for (let index = 0; index < values.length; index++) {
    if (index !== 0)
      text += separator ?? "";
    text += values[index] ?? "";
  }
  return AppendText(instance, text);
}
function AppendJoinedValues_573cb9d95540fca7(instance, separator, values) {
  EnsureInstance(instance);
  if (values === null)
    throw new Error("ArgumentNullException: values is null.");
  let text = "";
  let first = true;
  for (let value of values) {
    if (!first)
      text += separator ?? "";
    text += GetStringRepresentation(value);
    first = false;
  }
  return AppendText(instance, text);
}
function AppendJoinedValues_29aea02330e3cfdc(instance, separator, values) {
  EnsureInstance(instance);
  if (values === null)
    throw new Error("ArgumentNullException: values is null.");
  let text = "";
  for (let index = 0; index < values.length; index++) {
    if (index !== 0)
      text += separator ?? "";
    text += GetStringRepresentation(values[index]);
  }
  return AppendText(instance, text);
}
function AppendArrayRange(instance, value, startIndex, count) {
  EnsureInstance(instance);
  EnsureNonNegative(startIndex, "startIndex");
  EnsureNonNegative(count, "count");
  if (value === null) {
    if (startIndex === 0 && count === 0)
      return instance;
    throw new Error("ArgumentNullException: value is null.");
  }
  if (startIndex > value.length - count)
    throw new Error("ArgumentOutOfRangeException: startIndex and count must identify a valid range.");
  let snapshot = value.slice(startIndex, startIndex + count);
  EnsureAdditionalCapacity(instance, snapshot.length);
  for (let index = 0; index < snapshot.length; index++)
    instance.push(snapshot[index]);
  return instance;
}
function AppendStringRange(instance, value, startIndex, count) {
  EnsureInstance(instance);
  EnsureNonNegative(startIndex, "startIndex");
  EnsureNonNegative(count, "count");
  if (value === null) {
    if (startIndex === 0 && count === 0)
      return instance;
    throw new Error("ArgumentNullException: value is null.");
  }
  if (startIndex > value.length - count)
    throw new Error("ArgumentOutOfRangeException: startIndex and count must identify a valid range.");
  EnsureAdditionalCapacity(instance, count);
  for (let offset = 0; offset < count; offset++)
    instance.push(_5ad63706a889c294(value, startIndex + offset));
  return instance;
}
function InsertText(instance, index, value, count) {
  EnsureInsertIndex(instance, index);
  EnsureNonNegative(count, "count");
  if (value === null || value.length === 0 || count === 0)
    return instance;
  EnsureAdditionalCapacity(instance, value.length * count);
  for (let repeat = 0; repeat < count; repeat++) {
    for (let offset = value.length - 1; offset >= 0; offset--)
      instance.splice(index, 0, _5ad63706a889c294(value, offset));
  }
  return instance;
}
function InsertArrayRange(instance, index, value, startIndex, count) {
  EnsureInsertIndex(instance, index);
  EnsureNonNegative(startIndex, "startIndex");
  EnsureNonNegative(count, "charCount");
  if (value === null) {
    if (startIndex === 0 && count === 0)
      return instance;
    throw new Error("ArgumentNullException: value is null.");
  }
  if (startIndex > value.length - count)
    throw new Error("ArgumentOutOfRangeException: startIndex and charCount must identify a valid range.");
  return InsertText(instance, index, JoinRange(value, startIndex, count), 1);
}
function ReplaceTextRange(instance, oldValue, newValue, startIndex, count) {
  if (oldValue === null)
    throw new Error("ArgumentNullException: oldValue is null.");
  if (oldValue.length === 0)
    throw new Error("ArgumentException: oldValue cannot be empty.");
  EnsureRange(instance, startIndex, count);
  let replaced = JoinRange(instance, startIndex, count).replaceAll(oldValue, newValue ?? "");
  EnsureTargetLength(instance, instance.length - count + replaced.length);
  instance.splice(startIndex, count);
  return InsertText(instance, startIndex, replaced, 1);
}
/*jazor:clr-member System.Text.StringBuilder.StringBuilder()*/
export function _2154365d1f9a2abf() {
  return CreateBuilder(null, DefaultCapacity, DefaultMaxCapacity);
}
/*jazor:clr-member System.Text.StringBuilder.StringBuilder(int)*/
export function _404c94878c905b27(capacity) {
  return CreateBuilder(null, capacity, DefaultMaxCapacity);
}
/*jazor:clr-member System.Text.StringBuilder.StringBuilder(string)*/
export function _c2c8c4778873ccdc(value) {
  return CreateBuilder(value, DefaultCapacity, DefaultMaxCapacity);
}
/*jazor:clr-member System.Text.StringBuilder.StringBuilder(string, int)*/
export function _8ddc5378f62c27cc(value, capacity) {
  return CreateBuilder(value, capacity, DefaultMaxCapacity);
}
/*jazor:clr-member System.Text.StringBuilder.StringBuilder(string, int, int, int)*/
export function _70c61ab8ef3313c3(value, startIndex, length, capacity) {
  let text = value ?? "";
  EnsureNonNegative(startIndex, "startIndex");
  EnsureNonNegative(length, "length");
  if (startIndex > text.length - length)
    throw new Error("ArgumentOutOfRangeException: startIndex and length must identify a valid range.");
  return CreateBuilder(text.substring(startIndex, startIndex + length), capacity, DefaultMaxCapacity);
}
/*jazor:clr-member System.Text.StringBuilder.StringBuilder(int, int)*/
export function _f69cee28dea8bcdc(capacity, maxCapacity) {
  return CreateBuilder(null, capacity, maxCapacity);
}
/*jazor:clr-member System.Text.StringBuilder.Capacity.get*/
export function _20274b0eadfc0539(instance) {
  return GetCapacity(instance);
}
/*jazor:clr-member System.Text.StringBuilder.Capacity.set*/
export function _d58ab6215b243f4f(instance, value) {
  SetCapacity(instance, value);
}
/*jazor:clr-member System.Text.StringBuilder.MaxCapacity.get*/
export function _32a883f2233e3134(instance) {
  return GetMaxCapacity(instance);
}
/*jazor:clr-member System.Text.StringBuilder.EnsureCapacity(int)*/
export function _e957bcfaa166161c(instance, capacity) {
  return EnsureCapacity(instance, capacity);
}
/*jazor:clr-member System.Text.StringBuilder.ToString(int, int)*/
export function _4941946dde4f03f0(instance, startIndex, length) {
  EnsureRange(instance, startIndex, length);
  return JoinRange(instance, startIndex, length);
}
/*jazor:clr-member System.Text.StringBuilder.Length.set*/
export function _085925374c6d3abd(instance, value) {
  EnsureInstance(instance);
  EnsureNonNegative(value, "value");
  if (value < instance.length) {
    instance.splice(value, instance.length - value);
    return;
  }
  EnsureTargetLength(instance, value);
  while (instance.length < value)
    instance.push("\0");
}
/*jazor:clr-member System.Text.StringBuilder.this[int].get*/
export function _c59f10eccb1d75d4(instance, index) {
  EnsureExistingIndex(instance, index);
  return instance[index];
}
/*jazor:clr-member System.Text.StringBuilder.this[int].set*/
export function _a970d620cd814959(instance, index, value) {
  EnsureExistingIndex(instance, index);
  instance[index] = value;
}
/*jazor:clr-member System.Text.StringBuilder.Append(char, int)*/
export function _77869f53e4b4cf63(instance, value, repeatCount) {
  return InsertText(instance, instance.length, value, repeatCount);
}
/*jazor:clr-member System.Text.StringBuilder.Append(char[], int, int)*/
export function _76a6be47564b1442(instance, value, startIndex, charCount) {
  return AppendArrayRange(instance, value, startIndex, charCount);
}
/*jazor:clr-member System.Text.StringBuilder.Append(string)*/
export function _2879b76db56f25fb(instance, value) {
  return AppendText(instance, value);
}
/*jazor:clr-member System.Text.StringBuilder.Append(string, int, int)*/
export function _643a38ba616afd42(instance, value, startIndex, count) {
  return AppendStringRange(instance, value, startIndex, count);
}
/*jazor:clr-member System.Text.StringBuilder.Append(System.Text.StringBuilder)*/
export function _390481e4ef6d1b43(instance, value) {
  return AppendArrayRange(instance, value, 0, value?.length ?? 0);
}
/*jazor:clr-member System.Text.StringBuilder.Append(System.Text.StringBuilder, int, int)*/
export function _2a75c7a6bec12592(instance, value, startIndex, count) {
  return AppendArrayRange(instance, value, startIndex, count);
}
/*jazor:clr-member System.Text.StringBuilder.AppendLine()*/
export function _35fe8bcf463e879b(instance) {
  return AppendText(instance, "\n");
}
/*jazor:clr-member System.Text.StringBuilder.AppendLine(string)*/
export function _c06aaa44e213e405(instance, value) {
  AppendText(instance, value);
  return AppendText(instance, "\n");
}
/*jazor:clr-member System.Text.StringBuilder.CopyTo(int, char[], int, int)*/
export function _e7c76d547b84e1dd(instance, sourceIndex, destination, destinationIndex, count) {
  EnsureInstance(instance);
  EnsureNonNegative(sourceIndex, "sourceIndex");
  if (destination === null)
    throw new Error("ArgumentNullException: destination is null.");
  EnsureNonNegative(destinationIndex, "destinationIndex");
  EnsureNonNegative(count, "count");
  if (sourceIndex > instance.length - count)
    throw new Error("ArgumentException: source range exceeds the builder.");
  if (destinationIndex > destination.length - count)
    throw new Error("ArgumentException: destination array is too small.");
  for (let offset = 0; offset < count; offset++)
    destination[destinationIndex + offset] = instance[sourceIndex + offset];
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, string, int)*/
export function _da897479d9bd6139(instance, index, value, count) {
  return InsertText(instance, index, value, count);
}
/*jazor:clr-member System.Text.StringBuilder.Remove(int, int)*/
export function _152bf60dc35a5bb6(instance, startIndex, length) {
  EnsureRange(instance, startIndex, length);
  instance.splice(startIndex, length);
  return instance;
}
/*jazor:clr-member System.Text.StringBuilder.Append(bool)*/
export function _dded353c61620d12(instance, value) {
  return AppendText(instance, value ? "True" : "False");
}
/*jazor:clr-member System.Text.StringBuilder.Append(char)*/
export function _a2ce7c5adfc1553c(instance, value) {
  return AppendText(instance, value);
}
/*jazor:clr-member System.Text.StringBuilder.Append(sbyte)*/
export function _3ce4c9341fd5777f(instance, value) {
  return AppendText(instance, value.toString());
}
/*jazor:clr-member System.Text.StringBuilder.Append(byte)*/
export function _d530c416b64aac49(instance, value) {
  return AppendText(instance, value.toString());
}
/*jazor:clr-member System.Text.StringBuilder.Append(short)*/
export function _ea789609ea3aeeb0(instance, value) {
  return AppendText(instance, value.toString());
}
/*jazor:clr-member System.Text.StringBuilder.Append(int)*/
export function _212b9738d2ea3b2d(instance, value) {
  return AppendText(instance, value.toString());
}
/*jazor:clr-member System.Text.StringBuilder.Append(long)*/
export function _a20035534ee530dd(instance, value) {
  return AppendText(instance, value.toString());
}
/*jazor:clr-member System.Text.StringBuilder.Append(float)*/
export function _ec1b541b6a274b24(instance, value) {
  return AppendText(instance, value.toString());
}
/*jazor:clr-member System.Text.StringBuilder.Append(double)*/
export function _817e46ee3d60bf66(instance, value) {
  return AppendText(instance, value.toString());
}
/*jazor:clr-member System.Text.StringBuilder.Append(decimal)*/
export function _f07022820ca3881f(instance, value) {
  return AppendText(instance, value);
}
/*jazor:clr-member System.Text.StringBuilder.Append(ushort)*/
export function _37e94b64bce60492(instance, value) {
  return AppendText(instance, value.toString());
}
/*jazor:clr-member System.Text.StringBuilder.Append(uint)*/
export function _423a4a09f9fa54c4(instance, value) {
  return AppendText(instance, value.toString());
}
/*jazor:clr-member System.Text.StringBuilder.Append(ulong)*/
export function _f09314f07502e2a3(instance, value) {
  return AppendText(instance, value.toString());
}
/*jazor:clr-member System.Text.StringBuilder.Append(object)*/
export function _06379efa8addb10d(instance, value) {
  return AppendText(instance, GetStringRepresentation(value));
}
/*jazor:clr-member System.Text.StringBuilder.Append(char[])*/
export function _4ec74831297581ec(instance, value) {
  return AppendArrayRange(instance, value, 0, value?.length ?? 0);
}
/*jazor:clr-member System.Text.StringBuilder.Append(System.ReadOnlySpan<char>)*/
export function _8c68c811d3d42bcf(instance, value) {
  return AppendText(instance, MaterializeReadOnlyCharSpan(value));
}
/*jazor:clr-member System.Text.StringBuilder.AppendJoin(string, params object[])*/
export function _8bc8cc43c6d93195(instance, separator, values) {
  return AppendJoinedValues_29aea02330e3cfdc(instance, separator, values);
}
/*jazor:clr-member System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<object>)*/
export function _f4377679fddd51ad(instance, separator, values) {
  return AppendJoinedValues_29aea02330e3cfdc(instance, separator, values);
}
/*jazor:clr-member System.Text.StringBuilder.AppendJoin<T>(string, System.Collections.Generic.IEnumerable<T>)*/
export function _8d04089684a00c7b(instance, separator, values) {
  return AppendJoinedValues_573cb9d95540fca7(instance, separator, values);
}
/*jazor:clr-member System.Text.StringBuilder.AppendJoin(string, params string[])*/
export function _6ceea7a4bfd233b6(instance, separator, values) {
  return AppendJoinedStrings(instance, separator, values);
}
/*jazor:clr-member System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<string>)*/
export function _035c615b56218700(instance, separator, values) {
  return AppendJoinedStrings(instance, separator, values);
}
/*jazor:clr-member System.Text.StringBuilder.AppendJoin(char, params object[])*/
export function _a5aab658026ac255(instance, separator, values) {
  return AppendJoinedValues_29aea02330e3cfdc(instance, separator, values);
}
/*jazor:clr-member System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<object>)*/
export function _f9ca702aaa0e6322(instance, separator, values) {
  return AppendJoinedValues_29aea02330e3cfdc(instance, separator, values);
}
/*jazor:clr-member System.Text.StringBuilder.AppendJoin<T>(char, System.Collections.Generic.IEnumerable<T>)*/
export function _3510fcab582042e0(instance, separator, values) {
  return AppendJoinedValues_573cb9d95540fca7(instance, separator, values);
}
/*jazor:clr-member System.Text.StringBuilder.AppendJoin(char, params string[])*/
export function _02a3ec9f0e91877f(instance, separator, values) {
  return AppendJoinedStrings(instance, separator, values);
}
/*jazor:clr-member System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<string>)*/
export function _08c4f86d45c8b851(instance, separator, values) {
  return AppendJoinedStrings(instance, separator, values);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, string)*/
export function _40a305d0112c40d9(instance, index, value) {
  return InsertText(instance, index, value, 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, bool)*/
export function _2e7808d3cd4780e8(instance, index, value) {
  return InsertText(instance, index, value ? "True" : "False", 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, sbyte)*/
export function _5d866e86d8040d7d(instance, index, value) {
  return InsertText(instance, index, value.toString(), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, byte)*/
export function _a90cbae6c991fb88(instance, index, value) {
  return InsertText(instance, index, value.toString(), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, short)*/
export function _bf04d5cd34dd9bba(instance, index, value) {
  return InsertText(instance, index, value.toString(), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, char)*/
export function _d09b2a26b288fbd7(instance, index, value) {
  return InsertText(instance, index, value, 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, char[])*/
export function _a4c62411da366ab0(instance, index, value) {
  return InsertArrayRange(instance, index, value, 0, value?.length ?? 0);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, char[], int, int)*/
export function _f5ea58b7b0201715(instance, index, value, startIndex, charCount) {
  return InsertArrayRange(instance, index, value, startIndex, charCount);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, int)*/
export function _762de3335798fa24(instance, index, value) {
  return InsertText(instance, index, value.toString(), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, long)*/
export function _057e461451fbc2f6(instance, index, value) {
  return InsertText(instance, index, value.toString(), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, float)*/
export function _5fa422ae348735cc(instance, index, value) {
  return InsertText(instance, index, value.toString(), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, double)*/
export function _7e09aba586586854(instance, index, value) {
  return InsertText(instance, index, value.toString(), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, decimal)*/
export function _7244d40cd7bdaa7a(instance, index, value) {
  return InsertText(instance, index, value, 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, ushort)*/
export function _62b03548ac3a7f3c(instance, index, value) {
  return InsertText(instance, index, value.toString(), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, uint)*/
export function _865132ea357402b6(instance, index, value) {
  return InsertText(instance, index, value.toString(), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, ulong)*/
export function _e98da0d88b51734a(instance, index, value) {
  return InsertText(instance, index, value.toString(), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, object)*/
export function _463fe06f693b73f1(instance, index, value) {
  return InsertText(instance, index, GetStringRepresentation(value), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Insert(int, System.ReadOnlySpan<char>)*/
export function _ed1b69fd4bc25279(instance, index, value) {
  return InsertText(instance, index, MaterializeReadOnlyCharSpan(value), 1);
}
/*jazor:clr-member System.Text.StringBuilder.Replace(string, string)*/
export function _e11a2e954631c69a(instance, oldValue, newValue) {
  EnsureInstance(instance);
  return ReplaceTextRange(instance, oldValue, newValue, 0, instance.length);
}
/*jazor:clr-member System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)*/
export function _c7be232bff90ab62(instance, oldValue, newValue) {
  return ReplaceTextRange(instance, MaterializeReadOnlyCharSpan(oldValue), MaterializeReadOnlyCharSpan(newValue), 0, instance.length);
}
/*jazor:clr-member System.Text.StringBuilder.Equals(System.Text.StringBuilder)*/
export function _843038bb92e97c63(instance, sb) {
  return EqualsBuilder(instance, sb);
}
/*jazor:clr-member System.Text.StringBuilder.Equals(System.ReadOnlySpan<char>)*/
export function _251b340a59afa04d(instance, span) {
  EnsureInstance(instance);
  return JoinRange(instance, 0, instance.length) === MaterializeReadOnlyCharSpan(span);
}
/*jazor:clr-member System.Text.StringBuilder.Replace(string, string, int, int)*/
export function _34859fdec187084f(instance, oldValue, newValue, startIndex, count) {
  return ReplaceTextRange(instance, oldValue, newValue, startIndex, count);
}
/*jazor:clr-member System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, int, int)*/
export function _5681048ad18a4b3f(instance, oldValue, newValue, startIndex, count) {
  return ReplaceTextRange(instance, MaterializeReadOnlyCharSpan(oldValue), MaterializeReadOnlyCharSpan(newValue), startIndex, count);
}
/*jazor:clr-member System.Text.StringBuilder.Replace(char, char)*/
export function _618d386adc69ad32(instance, oldChar, newChar) {
  EnsureInstance(instance);
  return ReplaceTextRange(instance, oldChar, newChar, 0, instance.length);
}
/*jazor:clr-member System.Text.StringBuilder.Replace(char, char, int, int)*/
export function _b1fd321da487f718(instance, oldChar, newChar, startIndex, count) {
  return ReplaceTextRange(instance, oldChar, newChar, startIndex, count);
}
