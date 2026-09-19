import { CompareCore } from "System/Collections/Generic/ComparerT1Module.js";
import { Create_9a1218e69f90a6ca } from "System/Collections/Generic/DictionaryT2Module.js";
import { EqualsCore, GetHashCodeCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
import { CreateFrom } from "System/Collections/Generic/HashSetT1Module.js";
import { _0289dcf579b8a65e } from "System/Collections/Generic/IComparerT1Module.js";
import { _dae184550b995be1, _f53ff8f6435182d7 } from "System/Collections/Generic/IEqualityComparerT1Module.js";
import { _91a2436283a24315, _c11e0aef6b5ccf1e, _f5c1c0a2a040b000, _f73258f14e05c790 } from "System/DecimalModule.js";
import { Create, GetKey } from "System/Linq/GroupingT2Module.js";
let OrderedStates = new WeakMap;
let LookupComparers = new WeakMap;
function Materialize(source) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let result = new Array;
  for (let item of source)
    result.push(item);
  return result;
}
function RangeCore(start, count) {
  if (count < 0 || count > 0 && start + count - 1 > 2147483647)
    throw new Error("ArgumentOutOfRangeException: count must produce Int32 values");
  let result = new Array;
  for (let index = 0; index < count; index++)
    result.push(start + index);
  return result;
}
function RepeatCore(element, count) {
  if (count < 0)
    throw new Error("ArgumentOutOfRangeException: count must be non-negative");
  let result = new Array;
  for (let index = 0; index < count; index++)
    result.push(element);
  return result;
}
function CreateOrderKeys(items, keySelector) {
  let keys = new Array;
  for (let index = 0; index < items.length; index++)
    keys.push(keySelector(items[index]));
  return keys;
}
function CompareWith(comparer, left, right) {
  return comparer === null ? CompareCore(left, right) : _0289dcf579b8a65e(comparer, left, right);
}
function EqualsWith(comparer, left, right) {
  return comparer === null ? EqualsCore(left, right) : _dae184550b995be1(comparer, left, right);
}
function HashWith(comparer, value) {
  return comparer === null ? GetHashCodeCore(value) : _f53ff8f6435182d7(comparer, value);
}
function CreateKeyComparison(keys, descending, comparer) {
  return (left, right) => {
    let comparison = CompareWith(comparer, keys[left], keys[right]);
    return descending ? -comparison : comparison;
  };
}
function MaterializeOrderedResult(items, comparison) {
  let order = new Array;
  for (let index = 0; index < items.length; index++)
    order.push(index);
  order.sort((left, right) => {
    let result = comparison(left, right);
    return result !== 0 ? result : left - right;
  });
  let result = new Array;
  for (let index = 0; index < order.length; index++)
    result.push(items[order[index]]);
  OrderedStates.set(result, { Items: items, Compare: comparison });
  return result;
}
function OrderByCore(source, keySelector, descending, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  let items = Materialize(source);
  return MaterializeOrderedResult(items, CreateKeyComparison(CreateOrderKeys(items, keySelector), descending, comparer));
}
function OrderCore(source, descending) {
  return OrderByCore(source, item => {
    return item;
  }, descending, null);
}
function ThenByCore(source, keySelector, descending, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  if (!OrderedStates.has(source))
    throw new Error("NotSupportedException: ThenBy requires an ordering produced by Jazor's Enumerable.OrderBy runtime.");
  let state = OrderedStates.get(source);
  let secondary = CreateKeyComparison(CreateOrderKeys(state.Items, keySelector), descending, comparer);
  let comparison = (left, right) => {
    let primary = state.Compare(left, right);
    return primary !== 0 ? primary : secondary(left, right);
  };
  return MaterializeOrderedResult(state.Items, comparison);
}
function SkipCore(source, count) {
  let items = Materialize(source);
  return count <= 0 ? items : items.slice(count);
}
function TakeCore(source, count) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (count <= 0)
    return new Array;
  return Materialize(source).slice(0, count);
}
function TakeRangeCore(source, range) {
  let items = Materialize(source);
  let layout = range.GetOffsetAndLength(items.length);
  return items.slice(layout.Offset, layout.Offset + layout.Length);
}
function SkipWhileCore(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  let skipping = true;
  let result = new Array;
  for (let item of source) {
    if (skipping) {
      if (predicate(item))
        continue;
      skipping = false;
    }
    result.push(item);
  }
  return result;
}
function SkipWhileAtCore(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  let skipping = true;
  let index = 0;
  let result = new Array;
  for (let item of source) {
    if (skipping) {
      let shouldSkip = predicate(item, index);
      index++;
      if (shouldSkip)
        continue;
      skipping = false;
    }
    result.push(item);
  }
  return result;
}
function TakeWhileCore(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  let result = new Array;
  for (let item of source) {
    if (!predicate(item))
      break;
    result.push(item);
  }
  return result;
}
function TakeWhileAtCore(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  let index = 0;
  let result = new Array;
  for (let item of source) {
    let shouldTake = predicate(item, index);
    index++;
    if (!shouldTake)
      break;
    result.push(item);
  }
  return result;
}
function SkipLastCore(source, count) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (count <= 0)
    return Materialize(source);
  let result = new Array;
  let tail = new Array;
  let tailIndex = 0;
  for (let item of source) {
    if (tail.length < count) {
      tail.push(item);
      continue;
    }
    result.push(tail[tailIndex]);
    tail[tailIndex] = item;
    tailIndex = (tailIndex + 1) % count;
  }
  return result;
}
function TakeLastCore(source, count) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (count <= 0)
    return new Array;
  let tail = new Array;
  let tailIndex = 0;
  for (let item of source) {
    if (tail.length < count) {
      tail.push(item);
      continue;
    }
    tail[tailIndex] = item;
    tailIndex = (tailIndex + 1) % count;
  }
  if (tail.length < count)
    return tail;
  let result = new Array;
  for (let offset = 0; offset < tail.length; offset++) {
    result.push(tail[tailIndex]);
    tailIndex++;
    if (tailIndex === tail.length)
      tailIndex = 0;
  }
  return result;
}
function DefaultIfEmptyCore(source, defaultValue) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let result = Materialize(source);
  if (result.length === 0)
    result.push(defaultValue);
  return result;
}
function ChunkCore(source, size) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (size < 1)
    throw new Error("ArgumentOutOfRangeException: size must be greater than zero");
  let result = new Array;
  let chunk = new Array;
  for (let item of source) {
    chunk.push(item);
    if (chunk.length !== size)
      continue;
    result.push(chunk);
    chunk = new Array;
  }
  if (chunk.length > 0)
    result.push(chunk);
  return result;
}
function ReverseCore(items) {
  let result = new Array;
  for (let index = items.length; index > 0; index--)
    result.push(items[index - 1]);
  return result;
}
function ConcatCore(first, second) {
  if (first === null)
    throw new Error("ArgumentNullException: first is null");
  if (second === null)
    throw new Error("ArgumentNullException: second is null");
  let result = new Array;
  for (let item of first)
    result.push(item);
  for (let item of second)
    result.push(item);
  return result;
}
function AppendCore(source, element) {
  let result = Materialize(source);
  result.push(element);
  return result;
}
function PrependCore(source, element) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let result = new Array;
  result.push(element);
  for (let item of source)
    result.push(item);
  return result;
}
function ElementAtCore(source, index) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (index < 0)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  let currentIndex = 0;
  for (let item of source) {
    if (currentIndex === index)
      return item;
    currentIndex++;
  }
  throw new Error("ArgumentOutOfRangeException: index is out of range.");
}
function ElementAtIndexCore(source, index) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let indexValue = index.value;
  if (!index.fromEnd) {
    let currentIndex = 0;
    for (let item of source) {
      if (currentIndex === indexValue)
        return item;
      currentIndex++;
    }
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  }
  if (indexValue === 0)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  let tail = new Array;
  let tailIndex = 0;
  for (let item of source) {
    if (tail.length < indexValue) {
      tail.push(item);
      continue;
    }
    tail[tailIndex] = item;
    tailIndex = (tailIndex + 1) % indexValue;
  }
  if (tail.length < indexValue)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  return tail[tailIndex];
}
function FirstOrDefaultCore_9278215040059883(source, defaultValue) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  for (let item of source)
    return item;
  return defaultValue;
}
function FirstOrDefaultCore_efa3db75ab03e318(source, predicate, defaultValue) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  for (let item of source) {
    if (predicate(item))
      return item;
  }
  return defaultValue;
}
function FirstCore_e7de6b39c0e8f8e4(source) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  for (let item of source)
    return item;
  throw new Error("InvalidOperationException: Sequence contains no elements");
}
function FirstCore_9b3292be58cccd23(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  for (let item of source) {
    if (predicate(item))
      return item;
  }
  throw new Error("InvalidOperationException: Sequence contains no matching element");
}
function LastCore_afad789b1d346691(source) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let result = new Array;
  for (let item of source)
    result[0] = item;
  if (result.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return result[0];
}
function LastCore_887b1514a7d24be3(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  let result = new Array;
  for (let item of source) {
    if (predicate(item))
      result[0] = item;
  }
  if (result.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no matching element");
  return result[0];
}
function LastOrDefaultCore_865488c4c9aa94e6(source, defaultValue) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let result = defaultValue;
  let found = false;
  for (let item of source) {
    result = item;
    found = true;
  }
  return found ? result : defaultValue;
}
function LastOrDefaultCore_31047ab6bc8bda45(source, predicate, defaultValue) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  let result = defaultValue;
  let found = false;
  for (let item of source) {
    if (!predicate(item))
      continue;
    result = item;
    found = true;
  }
  return found ? result : defaultValue;
}
function SingleCore_f793d072b7cbb865(source) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let result = new Array;
  for (let item of source) {
    if (result.length !== 0)
      throw new Error("InvalidOperationException: Sequence contains more than one element");
    result[0] = item;
  }
  if (result.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return result[0];
}
function SingleCore_cc4071f727aedf50(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  let result = new Array;
  for (let item of source) {
    if (!predicate(item))
      continue;
    if (result.length !== 0)
      throw new Error("InvalidOperationException: Sequence contains more than one matching element");
    result[0] = item;
  }
  if (result.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no matching element");
  return result[0];
}
function SingleOrDefaultCore_47aa4cf4e16484ae(source, defaultValue) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let result = defaultValue;
  let found = false;
  for (let item of source) {
    if (found)
      throw new Error("InvalidOperationException: Sequence contains more than one element");
    result = item;
    found = true;
  }
  return found ? result : defaultValue;
}
function SingleOrDefaultCore_4be4e158eab2f96c(source, predicate, defaultValue) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  let result = defaultValue;
  let found = false;
  for (let item of source) {
    if (!predicate(item))
      continue;
    if (found)
      throw new Error("InvalidOperationException: Sequence contains more than one matching element");
    result = item;
    found = true;
  }
  return found ? result : defaultValue;
}
function SequenceEqualCore(first, second, comparer) {
  if (first === null)
    throw new Error("ArgumentNullException: first is null");
  if (second === null)
    throw new Error("ArgumentNullException: second is null");
  if (first.length !== second.length)
    return false;
  for (let index = 0; index < first.length; index++) {
    if (!EqualsWith(comparer, first[index], second[index]))
      return false;
  }
  return true;
}
function AggregateCore_61439d391df6936b(source, func) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (func === null)
    throw new Error("ArgumentNullException: func is null");
  let result = new Array;
  for (let item of source) {
    if (result.length === 0) {
      result[0] = item;
      continue;
    }
    result[0] = func(result[0], item);
  }
  if (result.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return result[0];
}
function AggregateCore_cc07e02b6996bc9b(source, seed, func) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (func === null)
    throw new Error("ArgumentNullException: func is null");
  let result = seed;
  for (let item of source)
    result = func(result, item);
  return result;
}
function AggregateCore_6b0502b3353ad08d(source, seed, func, resultSelector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (func === null)
    throw new Error("ArgumentNullException: func is null");
  if (resultSelector === null)
    throw new Error("ArgumentNullException: resultSelector is null");
  let result = seed;
  for (let item of source)
    result = func(result, item);
  return resultSelector(result);
}
function SelectManyCore_33a9042160d0fb8a(source, collectionSelector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (collectionSelector === null)
    throw new Error("ArgumentNullException: collectionSelector is null");
  let result = new Array;
  for (let sourceItem of source) {
    let collection = collectionSelector(sourceItem);
    AppendSelectedItems(result, collection);
  }
  return result;
}
function SelectManyAtCore(source, collectionSelector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (collectionSelector === null)
    throw new Error("ArgumentNullException: collectionSelector is null");
  let result = new Array;
  let index = 0;
  for (let sourceItem of source) {
    let collection = collectionSelector(sourceItem, index);
    index++;
    AppendSelectedItems(result, collection);
  }
  return result;
}
function SelectManyCore_9231369cd20082c1(source, collectionSelector, resultSelector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (collectionSelector === null)
    throw new Error("ArgumentNullException: collectionSelector is null");
  if (resultSelector === null)
    throw new Error("ArgumentNullException: resultSelector is null");
  let result = new Array;
  for (let sourceItem of source) {
    let collection = collectionSelector(sourceItem);
    if (collection === null)
      throw new Error("NullReferenceException: collection selector returned null");
    for (let collectionItem of collection)
      result.push(resultSelector(sourceItem, collectionItem));
  }
  return result;
}
function AppendSelectedItems(result, collection) {
  if (collection === null)
    throw new Error("NullReferenceException: collection selector returned null");
  for (let collectionItem of collection)
    result.push(collectionItem);
}
function AnyCore_969376a3feafe971(source) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  for (let _ of source)
    return true;
  return false;
}
function AnyCore_e7f01bde71fd0175(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  for (let item of source) {
    if (predicate(item))
      return true;
  }
  return false;
}
function AllCore(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  for (let item of source) {
    if (!predicate(item))
      return false;
  }
  return true;
}
function CountCore_ed43f15f591128f6(source) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let count = 0;
  for (let _ of source) {
    if (count === 2147483647)
      throw new Error("OverflowException: Count exceeds Int32.MaxValue.");
    count++;
  }
  return count;
}
function CountCore_3aeee7c2d8b395c4(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  let count = 0;
  for (let item of source) {
    if (predicate(item)) {
      if (count === 2147483647)
        throw new Error("OverflowException: Count exceeds Int32.MaxValue.");
      count++;
    }
  }
  return count;
}
function CountByCore(source, keySelector, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  let groups = new Array;
  let groupsByHash = new Map;
  for (let item of source) {
    let key = keySelector(item);
    let accumulator = GetGrouping(groupsByHash, groups, key, comparer);
    if (accumulator.length === 0) {
      accumulator.push(1);
      continue;
    }
    let count = accumulator[0];
    if (count === 2147483647)
      throw new Error("OverflowException: CountBy count exceeds Int32.MaxValue.");
    accumulator[0] = count + 1;
  }
  return MaterializeAccumulations(groups);
}
function AggregateByCore_70eb468965a27dd6(source, keySelector, seed, func, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  if (func === null)
    throw new Error("ArgumentNullException: func is null");
  let groups = new Array;
  let groupsByHash = new Map;
  for (let item of source) {
    let key = keySelector(item);
    let accumulator = GetGrouping(groupsByHash, groups, key, comparer);
    if (accumulator.length === 0) {
      accumulator.push(func(seed, item));
      continue;
    }
    accumulator[0] = func(accumulator[0], item);
  }
  return MaterializeAccumulations(groups);
}
function AggregateByCore_dc0172a0fbb5d3ab(source, keySelector, seedSelector, func, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  if (seedSelector === null)
    throw new Error("ArgumentNullException: seedSelector is null");
  if (func === null)
    throw new Error("ArgumentNullException: func is null");
  let groups = new Array;
  let groupsByHash = new Map;
  for (let item of source) {
    let key = keySelector(item);
    let accumulator = GetGrouping(groupsByHash, groups, key, comparer);
    if (accumulator.length === 0) {
      accumulator.push(func(seedSelector(key), item));
      continue;
    }
    accumulator[0] = func(accumulator[0], item);
  }
  return MaterializeAccumulations(groups);
}
function LongCountCore_0fc4307a112f326f(source) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let maximum = BigInt("9223372036854775807");
  let count = 0n;
  for (let _ of source) {
    if (count === maximum)
      throw new Error("OverflowException: LongCount exceeds Int64.MaxValue.");
    count = count + 1n;
  }
  return count;
}
function LongCountCore_ecbae8d7d3092d9f(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  let maximum = BigInt("9223372036854775807");
  let count = 0n;
  for (let item of source) {
    if (!predicate(item))
      continue;
    if (count === maximum)
      throw new Error("OverflowException: LongCount exceeds Int64.MaxValue.");
    count = count + 1n;
  }
  return count;
}
function SumIntCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    EnsureInt32AdditionInRange(sum, item, "Sum");
    sum += item;
  }
  return sum;
}
function EnsureInt32AdditionInRange(total, value, operation) {
  if (value > 0 && total > 2147483647 - value || value < 0 && total < -2147483648 - value) {
    throw new Error(`OverflowException: ${operation ?? ""} exceeds Int32 bounds.`);
  }
}
function SumInt64Core(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0n;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    EnsureInt64AdditionInRange(sum, item, "Sum");
    sum += item;
  }
  return sum;
}
function EnsureInt64AdditionInRange(total, value, operation) {
  let maximum = BigInt("9223372036854775807");
  let minimum = BigInt("-9223372036854775808");
  if (value > 0n && total > maximum - value || value < 0n && total < minimum - value) {
    throw new Error(`OverflowException: ${operation ?? ""} exceeds Int64 bounds.`);
  }
}
function SumNumberCore(source, selector, singlePrecision) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    sum += item;
  }
  return singlePrecision ? Math.fround(sum) : sum;
}
function SumDecimalCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = _91a2436283a24315("0");
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    sum = _f73258f14e05c790(sum, item);
  }
  return sum;
}
function AverageIntCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0n;
  let count = 0n;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    let value = BigInt(item);
    EnsureInt64AdditionInRange(sum, value, "Average");
    EnsureInt64AdditionInRange(count, 1n, "Average count");
    sum += value;
    count += 1n;
  }
  if (count === 0n)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return Number(sum) / Number(count);
}
function AverageInt64Core(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0n;
  let count = 0n;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    EnsureInt64AdditionInRange(sum, item, "Average");
    EnsureInt64AdditionInRange(count, 1n, "Average count");
    sum += item;
    count += 1n;
  }
  if (count === 0n)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return Number(sum) / Number(count);
}
function AverageNumberCore(source, selector, singlePrecision) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0;
  let count = 0n;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    EnsureInt64AdditionInRange(count, 1n, "Average count");
    sum += item;
    count += 1n;
  }
  if (count === 0n)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  let average = sum / Number(count);
  return singlePrecision ? Math.fround(average) : average;
}
function AverageDecimalCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = _91a2436283a24315("0");
  let count = 0n;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    EnsureInt64AdditionInRange(count, 1n, "Average count");
    sum = _f73258f14e05c790(sum, item);
    count += 1n;
  }
  if (count === 0n)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return _f5c1c0a2a040b000(sum, _91a2436283a24315(count.toString()));
}
function SumNullableIntCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0;
  for (let sourceItem of source) {
    let selected = selector(sourceItem);
    if (!(selected !== null && selected !== undefined))
      continue;
    let item = selected ?? (() => {
      throw new Error("InvalidOperationException: Nullable object must have a value.");
    })();
    EnsureInt32AdditionInRange(sum, item, "Sum");
    sum += item;
  }
  return sum;
}
function SumNullableInt64Core(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0n;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    if (item === null)
      continue;
    EnsureInt64AdditionInRange(sum, item, "Sum");
    sum += item;
  }
  return sum;
}
function SumNullableNumberCore(source, selector, singlePrecision) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0;
  for (let sourceItem of source) {
    let selected = selector(sourceItem);
    if (selected !== null && selected !== undefined)
      sum += selected ?? (() => {
        throw new Error("InvalidOperationException: Nullable object must have a value.");
      })();
  }
  return singlePrecision ? Math.fround(sum) : sum;
}
function SumNullableDecimalCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = _91a2436283a24315("0");
  for (let sourceItem of source) {
    let selected = selector(sourceItem);
    if (selected !== null && selected !== undefined)
      sum = _f73258f14e05c790(sum, selected ?? (() => {
        throw new Error("InvalidOperationException: Nullable object must have a value.");
      })());
  }
  return sum;
}
function AverageNullableIntCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0n;
  let count = 0n;
  for (let sourceItem of source) {
    let selected = selector(sourceItem);
    if (!(selected !== null && selected !== undefined))
      continue;
    let item = BigInt(selected ?? (() => {
      throw new Error("InvalidOperationException: Nullable object must have a value.");
    })());
    EnsureInt64AdditionInRange(sum, item, "Average");
    EnsureInt64AdditionInRange(count, 1n, "Average count");
    sum += item;
    count += 1n;
  }
  return count === 0n ? null : Number(sum) / Number(count);
}
function AverageNullableInt64Core(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0n;
  let count = 0n;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    if (item === null)
      continue;
    EnsureInt64AdditionInRange(sum, item, "Average");
    EnsureInt64AdditionInRange(count, 1n, "Average count");
    sum += item;
    count += 1n;
  }
  return count === 0n ? null : Number(sum) / Number(count);
}
function AverageNullableNumberCore(source, selector, singlePrecision) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = 0;
  let count = 0n;
  for (let sourceItem of source) {
    let selected = selector(sourceItem);
    if (!(selected !== null && selected !== undefined))
      continue;
    sum += selected ?? (() => {
      throw new Error("InvalidOperationException: Nullable object must have a value.");
    })();
    EnsureInt64AdditionInRange(count, 1n, "Average count");
    count += 1n;
  }
  if (count === 0n)
    return null;
  let average = sum / Number(count);
  return singlePrecision ? Math.fround(average) : average;
}
function AverageNullableDecimalCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let sum = _91a2436283a24315("0");
  let count = 0n;
  for (let sourceItem of source) {
    let selected = selector(sourceItem);
    if (!(selected !== null && selected !== undefined))
      continue;
    sum = _f73258f14e05c790(sum, selected ?? (() => {
      throw new Error("InvalidOperationException: Nullable object must have a value.");
    })());
    EnsureInt64AdditionInRange(count, 1n, "Average count");
    count += 1n;
  }
  return count === 0n ? null : _f5c1c0a2a040b000(sum, _91a2436283a24315(count.toString()));
}
function IndexCore(source) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let result = new Array;
  let index = 0;
  for (let item of source) {
    if (index === 2147483647)
      throw new Error("OverflowException: Index exceeds Int32.MaxValue.");
    result.push({ Index: index, Item: item });
    index++;
  }
  return result;
}
function MinNumberCore(source, selector, floating) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    if (candidates.length === 0) {
      candidates.push(item);
      continue;
    }
    if (floating && isNaN(item))
      return item;
    if (item < candidates[0])
      candidates[0] = item;
  }
  if (candidates.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return candidates[0];
}
function MaxNumberCore(source, selector, floating) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    if (candidates.length === 0) {
      candidates.push(item);
      continue;
    }
    if (floating && isNaN(candidates[0])) {
      if (!isNaN(item))
        candidates[0] = item;
      continue;
    }
    if (item > candidates[0])
      candidates[0] = item;
  }
  if (candidates.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return candidates[0];
}
function MinBigIntCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    if (candidates.length === 0) {
      candidates.push(item);
      continue;
    }
    if (item < candidates[0])
      candidates[0] = item;
  }
  if (candidates.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return candidates[0];
}
function MaxBigIntCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    if (candidates.length === 0) {
      candidates.push(item);
      continue;
    }
    if (item > candidates[0])
      candidates[0] = item;
  }
  if (candidates.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return candidates[0];
}
function MinDecimalCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    if (candidates.length === 0) {
      candidates.push(item);
      continue;
    }
    if (_c11e0aef6b5ccf1e(item, candidates[0]) < 0)
      candidates[0] = item;
  }
  if (candidates.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return candidates[0];
}
function MaxDecimalCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    if (candidates.length === 0) {
      candidates.push(item);
      continue;
    }
    if (_c11e0aef6b5ccf1e(item, candidates[0]) > 0)
      candidates[0] = item;
  }
  if (candidates.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return candidates[0];
}
function MinNullableNumberCore(source, selector, floating) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let selected = selector(sourceItem);
    if (!(selected !== null && selected !== undefined))
      continue;
    let item = selected ?? (() => {
      throw new Error("InvalidOperationException: Nullable object must have a value.");
    })();
    if (candidates.length === 0) {
      candidates.push(item);
      continue;
    }
    if (floating && isNaN(item))
      return item;
    if (item < candidates[0])
      candidates[0] = item;
  }
  return candidates.length === 0 ? null : candidates[0];
}
function MaxNullableNumberCore(source, selector, floating) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let selected = selector(sourceItem);
    if (!(selected !== null && selected !== undefined))
      continue;
    let item = selected ?? (() => {
      throw new Error("InvalidOperationException: Nullable object must have a value.");
    })();
    if (candidates.length === 0) {
      candidates.push(item);
      continue;
    }
    if (floating && isNaN(candidates[0])) {
      if (!isNaN(item))
        candidates[0] = item;
      continue;
    }
    if (item > candidates[0])
      candidates[0] = item;
  }
  return candidates.length === 0 ? null : candidates[0];
}
function MinNullableBigIntCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    if (item === null)
      continue;
    if (candidates.length === 0) {
      candidates.push(item);
      continue;
    }
    if (item < candidates[0])
      candidates[0] = item;
  }
  return candidates.length === 0 ? null : candidates[0];
}
function MaxNullableBigIntCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let item = selector(sourceItem);
    if (item === null)
      continue;
    if (candidates.length === 0) {
      candidates.push(item);
      continue;
    }
    if (item > candidates[0])
      candidates[0] = item;
  }
  return candidates.length === 0 ? null : candidates[0];
}
function MinNullableDecimalCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let selected = selector(sourceItem);
    if (!(selected !== null && selected !== undefined))
      continue;
    let item = selected ?? (() => {
      throw new Error("InvalidOperationException: Nullable object must have a value.");
    })();
    if (candidates.length === 0 || _c11e0aef6b5ccf1e(item, candidates[0]) < 0)
      candidates[0] = item;
  }
  return candidates.length === 0 ? null : candidates[0];
}
function MaxNullableDecimalCore(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  let candidates = new Array;
  for (let sourceItem of source) {
    let selected = selector(sourceItem);
    if (!(selected !== null && selected !== undefined))
      continue;
    let item = selected ?? (() => {
      throw new Error("InvalidOperationException: Nullable object must have a value.");
    })();
    if (candidates.length === 0 || _c11e0aef6b5ccf1e(item, candidates[0]) > 0)
      candidates[0] = item;
  }
  return candidates.length === 0 ? null : candidates[0];
}
function ContainsByEquality(valuesByHash, value, comparer) {
  let hashCode = HashWith(comparer, value);
  if (!valuesByHash.has(hashCode))
    return false;
  let bucket = valuesByHash.get(hashCode);
  if (bucket === null)
    return false;
  for (let index = 0; index < bucket.length; index++) {
    if (EqualsWith(comparer, bucket[index], value))
      return true;
  }
  return false;
}
function AddByEquality(valuesByHash, value, comparer) {
  if (ContainsByEquality(valuesByHash, value, comparer))
    return false;
  let hashCode = HashWith(comparer, value);
  let bucket = new Array;
  if (valuesByHash.has(hashCode)) {
    let existingBucket = valuesByHash.get(hashCode);
    if (existingBucket !== null)
      bucket = existingBucket;
  }
  else {
    valuesByHash.set(hashCode, bucket);
  }
  bucket.push(value);
  return true;
}
function RemoveByEquality(valuesByHash, value, comparer) {
  let hashCode = HashWith(comparer, value);
  if (!valuesByHash.has(hashCode))
    return false;
  let bucket = valuesByHash.get(hashCode);
  if (bucket === null)
    return false;
  for (let index = 0; index < bucket.length; index++) {
    if (!EqualsWith(comparer, bucket[index], value))
      continue;
    bucket.splice(index, 1);
    return true;
  }
  return false;
}
function CreateEqualitySet(source, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let valuesByHash = new Map;
  for (let value of source)
    AddByEquality(valuesByHash, value, comparer);
  return valuesByHash;
}
function DistinctCore(source, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let seen = new Map;
  let result = new Array;
  for (let item of source) {
    if (AddByEquality(seen, item, comparer))
      result.push(item);
  }
  return result;
}
function DistinctByCore(source, keySelector, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  let seenKeys = new Map;
  let result = new Array;
  for (let item of source) {
    if (AddByEquality(seenKeys, keySelector(item), comparer))
      result.push(item);
  }
  return result;
}
function ExtremumByCore(source, keySelector, maximum, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  let candidates = new Array;
  let candidateKeys = new Array;
  for (let item of source) {
    let key = keySelector(item);
    if (candidates.length === 0) {
      candidates.push(item);
      candidateKeys.push(key);
      continue;
    }
    let comparison = CompareWith(comparer, key, candidateKeys[0]);
    if (maximum ? comparison > 0 : comparison < 0) {
      candidates[0] = item;
      candidateKeys[0] = key;
    }
  }
  if (candidates.length === 0)
    throw new Error("InvalidOperationException: Sequence contains no elements");
  return candidates[0];
}
function UnionCore(first, second, comparer) {
  if (first === null)
    throw new Error("ArgumentNullException: first is null");
  if (second === null)
    throw new Error("ArgumentNullException: second is null");
  let seen = new Map;
  let result = new Array;
  for (let item of first) {
    if (AddByEquality(seen, item, comparer))
      result.push(item);
  }
  for (let item of second) {
    if (AddByEquality(seen, item, comparer))
      result.push(item);
  }
  return result;
}
function ExceptCore(first, second, comparer) {
  if (first === null)
    throw new Error("ArgumentNullException: first is null");
  let excluded = CreateEqualitySet(second, comparer);
  let result = new Array;
  for (let item of first) {
    if (AddByEquality(excluded, item, comparer))
      result.push(item);
  }
  return result;
}
function IntersectCore(first, second, comparer) {
  if (first === null)
    throw new Error("ArgumentNullException: first is null");
  let remaining = CreateEqualitySet(second, comparer);
  let result = new Array;
  for (let item of first) {
    if (RemoveByEquality(remaining, item, comparer))
      result.push(item);
  }
  return result;
}
function UnionByCore(first, second, keySelector, comparer) {
  if (first === null)
    throw new Error("ArgumentNullException: first is null");
  if (second === null)
    throw new Error("ArgumentNullException: second is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  let seenKeys = new Map;
  let result = new Array;
  for (let item of first) {
    if (AddByEquality(seenKeys, keySelector(item), comparer))
      result.push(item);
  }
  for (let item of second) {
    if (AddByEquality(seenKeys, keySelector(item), comparer))
      result.push(item);
  }
  return result;
}
function ExceptByCore(first, second, keySelector, comparer) {
  if (first === null)
    throw new Error("ArgumentNullException: first is null");
  if (second === null)
    throw new Error("ArgumentNullException: second is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  let excludedKeys = CreateEqualitySet(second, comparer);
  let result = new Array;
  for (let item of first) {
    if (AddByEquality(excludedKeys, keySelector(item), comparer))
      result.push(item);
  }
  return result;
}
function IntersectByCore(first, second, keySelector, comparer) {
  if (first === null)
    throw new Error("ArgumentNullException: first is null");
  if (second === null)
    throw new Error("ArgumentNullException: second is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  let remainingKeys = CreateEqualitySet(second, comparer);
  let result = new Array;
  for (let item of first) {
    if (RemoveByEquality(remainingKeys, keySelector(item), comparer))
      result.push(item);
  }
  return result;
}
function ContainsCore(source, value, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  for (let item of source) {
    if (EqualsWith(comparer, item, value))
      return true;
  }
  return false;
}
function FindGrouping(groupsByHash, key, comparer) {
  let hashCode = HashWith(comparer, key);
  if (!groupsByHash.has(hashCode))
    return null;
  let bucket = groupsByHash.get(hashCode);
  if (bucket === null)
    return null;
  for (let index = 0; index < bucket.length; index++) {
    let grouping = bucket[index];
    if (EqualsWith(comparer, GetKey(grouping), key))
      return grouping;
  }
  return null;
}
function FindLookupGrouping(groups, key, comparer) {
  for (let index = 0; index < groups.length; index++) {
    let group = groups[index];
    if (EqualsWith(comparer, GetKey(group), key))
      return group;
  }
  return null;
}
function GetGrouping(groupsByHash, groups, key, comparer) {
  let existing = FindGrouping(groupsByHash, key, comparer);
  if (existing !== null)
    return existing;
  let hashCode = HashWith(comparer, key);
  let bucket = new Array;
  if (groupsByHash.has(hashCode)) {
    let existingBucket = groupsByHash.get(hashCode);
    if (existingBucket !== null)
      bucket = existingBucket;
  }
  else {
    groupsByHash.set(hashCode, bucket);
  }
  let created = Create(key);
  bucket.push(created);
  groups.push(created);
  return created;
}
function MaterializeAccumulations(groups) {
  let result = new Array;
  for (let index = 0; index < groups.length; index++) {
    let group = groups[index];
    result.push([GetKey(group), group[0]]);
  }
  return result;
}
function GroupByCore_7b94285f70aa6f73(source, keySelector, comparer) {
  return GroupByCore_b0f00ed0755e257d(source, keySelector, item => {
    return item;
  }, comparer);
}
function JoinCore(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer) {
  if (outer === null)
    throw new Error("ArgumentNullException: outer is null");
  if (inner === null)
    throw new Error("ArgumentNullException: inner is null");
  if (outerKeySelector === null)
    throw new Error("ArgumentNullException: outerKeySelector is null");
  if (innerKeySelector === null)
    throw new Error("ArgumentNullException: innerKeySelector is null");
  if (resultSelector === null)
    throw new Error("ArgumentNullException: resultSelector is null");
  let groups = new Array;
  let groupsByHash = new Map;
  for (let innerItem of inner) {
    let key = innerKeySelector(innerItem);
    GetGrouping(groupsByHash, groups, key, comparer).push(innerItem);
  }
  let result = new Array;
  for (let outerItem of outer) {
    let grouping = FindGrouping(groupsByHash, outerKeySelector(outerItem), comparer);
    if (grouping === null)
      continue;
    for (let index = 0; index < grouping.length; index++)
      result.push(resultSelector(outerItem, grouping[index]));
  }
  return result;
}
function GroupJoinCore(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer) {
  if (outer === null)
    throw new Error("ArgumentNullException: outer is null");
  if (inner === null)
    throw new Error("ArgumentNullException: inner is null");
  if (outerKeySelector === null)
    throw new Error("ArgumentNullException: outerKeySelector is null");
  if (innerKeySelector === null)
    throw new Error("ArgumentNullException: innerKeySelector is null");
  if (resultSelector === null)
    throw new Error("ArgumentNullException: resultSelector is null");
  let groups = new Array;
  let groupsByHash = new Map;
  for (let innerItem of inner) {
    let key = innerKeySelector(innerItem);
    GetGrouping(groupsByHash, groups, key, comparer).push(innerItem);
  }
  let result = new Array;
  for (let outerItem of outer) {
    let grouping = FindGrouping(groupsByHash, outerKeySelector(outerItem), comparer);
    result.push(resultSelector(outerItem, grouping ?? new Array));
  }
  return result;
}
function GroupByCore_b0f00ed0755e257d(source, keySelector, elementSelector, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  if (elementSelector === null)
    throw new Error("ArgumentNullException: elementSelector is null");
  return GroupByCoreUnchecked(source, keySelector, elementSelector, comparer);
}
function GroupByResultCore(source, keySelector, elementSelector, resultSelector, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  if (elementSelector === null)
    throw new Error("ArgumentNullException: elementSelector is null");
  if (resultSelector === null)
    throw new Error("ArgumentNullException: resultSelector is null");
  let groups = GroupByCoreUnchecked(source, keySelector, elementSelector, comparer);
  let result = new Array;
  for (let index = 0; index < groups.length; index++) {
    let group = groups[index];
    result.push(resultSelector(GetKey(group), group));
  }
  return result;
}
function GroupByCoreUnchecked(source, keySelector, elementSelector, comparer) {
  let groups = new Array;
  let groupsByHash = new Map;
  for (let sourceItem of source) {
    let key = keySelector(sourceItem);
    let element = elementSelector(sourceItem);
    GetGrouping(groupsByHash, groups, key, comparer).push(element);
  }
  return groups;
}
function ToLookupCore(source, keySelector, elementSelector, comparer) {
  let groups = GroupByCore_b0f00ed0755e257d(source, keySelector, elementSelector, comparer);
  LookupComparers.set(groups, comparer);
  return groups;
}
function ToHashSetCore(source, comparer) {
  return CreateFrom(source, comparer);
}
function ToDictionaryCore(source, keySelector, elementSelector, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (keySelector === null)
    throw new Error("ArgumentNullException: keySelector is null");
  if (elementSelector === null)
    throw new Error("ArgumentNullException: elementSelector is null");
  let result = Create_9a1218e69f90a6ca(comparer);
  for (let item of source) {
    let key = keySelector(item);
    if (result.has(key))
      throw new Error("ArgumentException: An item with the same key has already been added.");
    result.set(key, elementSelector(item));
  }
  return result;
}
/*jazor:clr-member static System.Linq.Enumerable.Empty<TResult>()*/
export function empty() {
  return new Array;
}
/*jazor:clr-member static System.Linq.Enumerable.Range(int, int)*/
function Range(start, count) {
  return RangeCore(start, count);
}
export { Range as range };
/*jazor:clr-member static System.Linq.Enumerable.Repeat<TResult>(TResult, int)*/
export function repeat(element, count) {
  return RepeatCore(element, count);
}
/*jazor:clr-member static System.Linq.Enumerable.AsEnumerable<TSource>(System.Collections.Generic.IEnumerable<TSource>)*/
export function asEnumerable(source) {
  return source;
}
/*jazor:clr-member static System.Linq.Enumerable.Sequence<T>(T, T, T)*/
export function sequence(first, second, third) {
  return [first, second, third];
}
/*jazor:clr-member static System.Linq.Enumerable.SelectMany<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, System.Collections.Generic.IEnumerable<TResult>>)*/
export function _edce1ee9a9c5c4cc(source, collectionSelector) {
  return SelectManyCore_33a9042160d0fb8a(source, collectionSelector);
}
/*jazor:clr-member static System.Linq.Enumerable.SelectMany<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, System.Collections.Generic.IEnumerable<TResult>>)*/
export function _de31ec2f4619ef07(source, collectionSelector) {
  return SelectManyAtCore(source, collectionSelector);
}
/*jazor:clr-member static System.Linq.Enumerable.SelectMany<TSource, TCollection, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, System.Collections.Generic.IEnumerable<TCollection>>, System.Func<TSource, TCollection, TResult>)*/
export function _aacc82f5a0d854d2(source, collectionSelector, resultSelector) {
  return SelectManyCore_9231369cd20082c1(source, collectionSelector, resultSelector);
}
/*jazor:clr-member static System.Linq.Enumerable.Skip<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)*/
export function _7a0726e65cb5b3a2(source, count) {
  return SkipCore(source, count);
}
/*jazor:clr-member static System.Linq.Enumerable.Take<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)*/
export function _4abc4f56a4100834(source, count) {
  return TakeCore(source, count);
}
/*jazor:clr-member static System.Linq.Enumerable.Take<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Range)*/
export function takeRange(source, range) {
  return TakeRangeCore(source, range);
}
/*jazor:clr-member static System.Linq.Enumerable.SkipWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)*/
export function skipWhile(source, predicate) {
  return SkipWhileCore(source, predicate);
}
/*jazor:clr-member static System.Linq.Enumerable.SkipWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)*/
export function skipWhileAt(source, predicate) {
  return SkipWhileAtCore(source, predicate);
}
/*jazor:clr-member static System.Linq.Enumerable.TakeWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)*/
export function takeWhile(source, predicate) {
  return TakeWhileCore(source, predicate);
}
/*jazor:clr-member static System.Linq.Enumerable.TakeWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)*/
export function takeWhileAt(source, predicate) {
  return TakeWhileAtCore(source, predicate);
}
/*jazor:clr-member static System.Linq.Enumerable.SkipLast<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)*/
export function skipLast(source, count) {
  return SkipLastCore(source, count);
}
/*jazor:clr-member static System.Linq.Enumerable.TakeLast<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)*/
export function takeLast(source, count) {
  return TakeLastCore(source, count);
}
/*jazor:clr-member static System.Linq.Enumerable.Chunk<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)*/
function Chunk(source, size) {
  return ChunkCore(source, size);
}
export { Chunk as chunk };
/*jazor:clr-member static System.Linq.Enumerable.Reverse<TSource>(System.Collections.Generic.IEnumerable<TSource>)*/
export function reverse(source) {
  return ReverseCore(Materialize(source));
}
/*jazor:clr-member static System.Linq.Enumerable.Reverse<TSource>(TSource[])*/
export function reverseArray(source) {
  return ReverseCore(source);
}
/*jazor:clr-member static System.Linq.Enumerable.Concat<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)*/
export function concat(first, second) {
  return ConcatCore(first, second);
}
/*jazor:clr-member static System.Linq.Enumerable.Append<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)*/
export function append(source, element) {
  return AppendCore(source, element);
}
/*jazor:clr-member static System.Linq.Enumerable.Prepend<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)*/
export function prepend(source, element) {
  return PrependCore(source, element);
}
/*jazor:clr-member static System.Linq.Enumerable.DefaultIfEmpty<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)*/
export function defaultIfEmpty(source, defaultValue) {
  return DefaultIfEmptyCore(source, defaultValue);
}
/*jazor:clr-member static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)*/
export function firstOrDefault(source, defaultValue) {
  return FirstOrDefaultCore_9278215040059883(source, defaultValue);
}
/*jazor:clr-member static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)*/
export function firstOrDefaultWhere(source, predicate, defaultValue) {
  return FirstOrDefaultCore_efa3db75ab03e318(source, predicate, defaultValue);
}
/*jazor:clr-member static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)*/
export function lastOrDefault(source, defaultValue) {
  return LastOrDefaultCore_865488c4c9aa94e6(source, defaultValue);
}
/*jazor:clr-member static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)*/
export function lastOrDefaultWhere(source, predicate, defaultValue) {
  return LastOrDefaultCore_31047ab6bc8bda45(source, predicate, defaultValue);
}
/*jazor:clr-member static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)*/
export function singleOrDefault(source, defaultValue) {
  return SingleOrDefaultCore_47aa4cf4e16484ae(source, defaultValue);
}
/*jazor:clr-member static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)*/
export function singleOrDefaultWhere(source, predicate, defaultValue) {
  return SingleOrDefaultCore_4be4e158eab2f96c(source, predicate, defaultValue);
}
/*jazor:clr-member static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)*/
export function elementAt(source, index) {
  return ElementAtCore(source, index);
}
/*jazor:clr-member static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Index)*/
export function elementAtIndex(source, index) {
  return ElementAtIndexCore(source, index);
}
/*jazor:clr-member static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>)*/
function First(source) {
  return FirstCore_e7de6b39c0e8f8e4(source);
}
export { First as first };
/*jazor:clr-member static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)*/
export function firstWhere(source, predicate) {
  return FirstCore_9b3292be58cccd23(source, predicate);
}
/*jazor:clr-member static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>)*/
export function last(source) {
  return LastCore_afad789b1d346691(source);
}
/*jazor:clr-member static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)*/
export function lastWhere(source, predicate) {
  return LastCore_887b1514a7d24be3(source, predicate);
}
/*jazor:clr-member static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>)*/
export function single(source) {
  return SingleCore_f793d072b7cbb865(source);
}
/*jazor:clr-member static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)*/
export function singleWhere(source, predicate) {
  return SingleCore_cc4071f727aedf50(source, predicate);
}
/*jazor:clr-member static System.Linq.Enumerable.SequenceEqual<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)*/
export function sequenceEqual(first, second) {
  return SequenceEqualCore(first, second, null);
}
/*jazor:clr-member static System.Linq.Enumerable.SequenceEqual<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)*/
export function sequenceEqualWithComparer(first, second, comparer) {
  return SequenceEqualCore(first, second, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.Aggregate<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TSource, TSource>)*/
export function aggregate(source, func) {
  return AggregateCore_61439d391df6936b(source, func);
}
/*jazor:clr-member static System.Linq.Enumerable.Aggregate<TSource, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>)*/
export function aggregateWithSeed(source, seed, func) {
  return AggregateCore_cc07e02b6996bc9b(source, seed, func);
}
/*jazor:clr-member static System.Linq.Enumerable.Aggregate<TSource, TAccumulate, TResult>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Func<TAccumulate, TResult>)*/
export function aggregateWithResult(source, seed, func, resultSelector) {
  return AggregateCore_6b0502b3353ad08d(source, seed, func, resultSelector);
}
/*jazor:clr-member static System.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>)*/
export function _9832a60d5939c887(source) {
  return AnyCore_969376a3feafe971(source);
}
/*jazor:clr-member static System.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)*/
export function _8995eebc6c105f1d(source, predicate) {
  return AnyCore_e7f01bde71fd0175(source, predicate);
}
/*jazor:clr-member static System.Linq.Enumerable.All<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)*/
export function _7e4a11c411867592(source, predicate) {
  return AllCore(source, predicate);
}
/*jazor:clr-member static System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>)*/
export function _1cb3ec9a7fb8aaab(source) {
  return CountCore_ed43f15f591128f6(source);
}
/*jazor:clr-member static System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)*/
export function _e19baea1a0d8c2c6(source, predicate) {
  return CountCore_3aeee7c2d8b395c4(source, predicate);
}
/*jazor:clr-member static System.Linq.Enumerable.CountBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function countBy(source, keySelector, comparer) {
  return CountByCore(source, keySelector, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function aggregateBy(source, keySelector, seed, func, comparer) {
  return AggregateByCore_70eb468965a27dd6(source, keySelector, seed, func, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, TAccumulate>, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function aggregateByWithSeedSelector(source, keySelector, seedSelector, func, comparer) {
  return AggregateByCore_dc0172a0fbb5d3ab(source, keySelector, seedSelector, func, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.LongCount<TSource>(System.Collections.Generic.IEnumerable<TSource>)*/
export function longCount(source) {
  return LongCountCore_0fc4307a112f326f(source);
}
/*jazor:clr-member static System.Linq.Enumerable.LongCount<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)*/
export function longCountWhere(source, predicate) {
  return LongCountCore_ecbae8d7d3092d9f(source, predicate);
}
/*jazor:clr-member static System.Linq.Enumerable.Index<TSource>(System.Collections.Generic.IEnumerable<TSource>)*/
function Index(source) {
  return IndexCore(source);
}
export { Index as index };
/*jazor:clr-member static System.Linq.Enumerable.TryGetNonEnumeratedCount<TSource>(System.Collections.Generic.IEnumerable<TSource>, out int)*/
export function tryGetNonEnumeratedCount(source, count) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  return [true, source.length];
}
/*jazor:clr-member static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int>)*/
export function sumInt(source) {
  return SumIntCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int?>)*/
export function sumNullableInt(source) {
  return SumNullableIntCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)*/
export function sumNullableIntBy(source, selector) {
  return SumNullableIntCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)*/
export function sumIntBy(source, selector) {
  return SumIntCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long>)*/
export function sumInt64(source) {
  return SumInt64Core(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long?>)*/
export function sumNullableInt64(source) {
  return SumNullableInt64Core(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)*/
export function sumNullableInt64By(source, selector) {
  return SumNullableInt64Core(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)*/
export function sumInt64By(source, selector) {
  return SumInt64Core(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float>)*/
export function sumSingle(source) {
  return SumNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float?>)*/
export function sumNullableSingle(source) {
  return SumNullableNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)*/
export function sumNullableSingleBy(source, selector) {
  return SumNullableNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)*/
export function sumSingleBy(source, selector) {
  return SumNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double>)*/
export function sumDouble(source) {
  return SumNumberCore(source, item => {
    return item;
  }, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double?>)*/
export function sumNullableDouble(source) {
  return SumNullableNumberCore(source, item => {
    return item;
  }, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)*/
export function sumNullableDoubleBy(source, selector) {
  return SumNullableNumberCore(source, selector, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)*/
export function sumDoubleBy(source, selector) {
  return SumNumberCore(source, selector, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal>)*/
export function sumDecimal(source) {
  return SumDecimalCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal?>)*/
export function sumNullableDecimal(source) {
  return SumNullableDecimalCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)*/
export function sumNullableDecimalBy(source, selector) {
  return SumNullableDecimalCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)*/
export function sumDecimalBy(source, selector) {
  return SumDecimalCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int>)*/
export function averageInt(source) {
  return AverageIntCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int?>)*/
export function averageNullableInt(source) {
  return AverageNullableIntCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)*/
export function averageNullableIntBy(source, selector) {
  return AverageNullableIntCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)*/
export function averageIntBy(source, selector) {
  return AverageIntCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long>)*/
export function averageInt64(source) {
  return AverageInt64Core(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long?>)*/
export function averageNullableInt64(source) {
  return AverageNullableInt64Core(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)*/
export function averageNullableInt64By(source, selector) {
  return AverageNullableInt64Core(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)*/
export function averageInt64By(source, selector) {
  return AverageInt64Core(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float>)*/
export function averageSingle(source) {
  return AverageNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float?>)*/
export function averageNullableSingle(source) {
  return AverageNullableNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)*/
export function averageNullableSingleBy(source, selector) {
  return AverageNullableNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)*/
export function averageSingleBy(source, selector) {
  return AverageNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double>)*/
export function averageDouble(source) {
  return AverageNumberCore(source, item => {
    return item;
  }, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double?>)*/
export function averageNullableDouble(source) {
  return AverageNullableNumberCore(source, item => {
    return item;
  }, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)*/
export function averageNullableDoubleBy(source, selector) {
  return AverageNullableNumberCore(source, selector, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)*/
export function averageDoubleBy(source, selector) {
  return AverageNumberCore(source, selector, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal>)*/
export function averageDecimal(source) {
  return AverageDecimalCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal?>)*/
export function averageNullableDecimal(source) {
  return AverageNullableDecimalCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)*/
export function averageNullableDecimalBy(source, selector) {
  return AverageNullableDecimalCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)*/
export function averageDecimalBy(source, selector) {
  return AverageDecimalCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int>)*/
export function minInt(source) {
  return MinNumberCore(source, item => {
    return item;
  }, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int?>)*/
export function minNullableInt(source) {
  return MinNullableNumberCore(source, item => {
    return item;
  }, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)*/
export function minNullableIntBy(source, selector) {
  return MinNullableNumberCore(source, selector, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)*/
export function minIntBy(source, selector) {
  return MinNumberCore(source, selector, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<int>)*/
export function maxInt(source) {
  return MaxNumberCore(source, item => {
    return item;
  }, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<int?>)*/
export function maxNullableInt(source) {
  return MaxNullableNumberCore(source, item => {
    return item;
  }, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)*/
export function maxNullableIntBy(source, selector) {
  return MaxNullableNumberCore(source, selector, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)*/
export function maxIntBy(source, selector) {
  return MaxNumberCore(source, selector, false);
}
/*jazor:clr-member static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<long>)*/
export function minInt64(source) {
  return MinBigIntCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<long?>)*/
export function minNullableInt64(source) {
  return MinNullableBigIntCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)*/
export function minNullableInt64By(source, selector) {
  return MinNullableBigIntCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)*/
export function minInt64By(source, selector) {
  return MinBigIntCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<long>)*/
export function maxInt64(source) {
  return MaxBigIntCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<long?>)*/
export function maxNullableInt64(source) {
  return MaxNullableBigIntCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)*/
export function maxNullableInt64By(source, selector) {
  return MaxNullableBigIntCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)*/
export function maxInt64By(source, selector) {
  return MaxBigIntCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<float>)*/
export function minSingle(source) {
  return MinNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<float?>)*/
export function minNullableSingle(source) {
  return MinNullableNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)*/
export function minNullableSingleBy(source, selector) {
  return MinNullableNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)*/
export function minSingleBy(source, selector) {
  return MinNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<float>)*/
export function maxSingle(source) {
  return MaxNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<float?>)*/
export function maxNullableSingle(source) {
  return MaxNullableNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)*/
export function maxNullableSingleBy(source, selector) {
  return MaxNullableNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)*/
export function maxSingleBy(source, selector) {
  return MaxNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<double>)*/
export function minDouble(source) {
  return MinNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<double?>)*/
export function minNullableDouble(source) {
  return MinNullableNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)*/
export function minNullableDoubleBy(source, selector) {
  return MinNullableNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)*/
export function minDoubleBy(source, selector) {
  return MinNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<double>)*/
export function maxDouble(source) {
  return MaxNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<double?>)*/
export function maxNullableDouble(source) {
  return MaxNullableNumberCore(source, item => {
    return item;
  }, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)*/
export function maxNullableDoubleBy(source, selector) {
  return MaxNullableNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)*/
export function maxDoubleBy(source, selector) {
  return MaxNumberCore(source, selector, true);
}
/*jazor:clr-member static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<decimal>)*/
export function minDecimal(source) {
  return MinDecimalCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<decimal?>)*/
export function minNullableDecimal(source) {
  return MinNullableDecimalCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)*/
export function minNullableDecimalBy(source, selector) {
  return MinNullableDecimalCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)*/
export function minDecimalBy(source, selector) {
  return MinDecimalCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal>)*/
export function maxDecimal(source) {
  return MaxDecimalCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal?>)*/
export function maxNullableDecimal(source) {
  return MaxNullableDecimalCore(source, item => {
    return item;
  });
}
/*jazor:clr-member static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)*/
export function maxNullableDecimalBy(source, selector) {
  return MaxNullableDecimalCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)*/
export function maxDecimalBy(source, selector) {
  return MaxDecimalCore(source, selector);
}
/*jazor:clr-member static System.Linq.Enumerable.Distinct<TSource>(System.Collections.Generic.IEnumerable<TSource>)*/
export function _a2bc38786226403e(source) {
  return DistinctCore(source, null);
}
/*jazor:clr-member static System.Linq.Enumerable.Distinct<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)*/
export function distinctWithComparer(source, comparer) {
  return DistinctCore(source, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.DistinctBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)*/
export function distinctBy(source, keySelector) {
  return DistinctByCore(source, keySelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.DistinctBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function distinctByWithComparer(source, keySelector, comparer) {
  return DistinctByCore(source, keySelector, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.MinBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)*/
export function minBy(source, keySelector) {
  return ExtremumByCore(source, keySelector, false, null);
}
/*jazor:clr-member static System.Linq.Enumerable.MinBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)*/
export function minByWithComparer(source, keySelector, comparer) {
  return ExtremumByCore(source, keySelector, false, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.MaxBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)*/
export function maxBy(source, keySelector) {
  return ExtremumByCore(source, keySelector, true, null);
}
/*jazor:clr-member static System.Linq.Enumerable.MaxBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)*/
export function maxByWithComparer(source, keySelector, comparer) {
  return ExtremumByCore(source, keySelector, true, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.Union<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)*/
export function _b5fae0c231974056(first, second) {
  return UnionCore(first, second, null);
}
/*jazor:clr-member static System.Linq.Enumerable.Union<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)*/
export function unionWithComparer(first, second, comparer) {
  return UnionCore(first, second, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.Except<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)*/
export function _c71d4ff9a863431d(first, second) {
  return ExceptCore(first, second, null);
}
/*jazor:clr-member static System.Linq.Enumerable.Except<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)*/
export function exceptWithComparer(first, second, comparer) {
  return ExceptCore(first, second, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.Intersect<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)*/
export function _d83c9e4a7bf747a8(first, second) {
  return IntersectCore(first, second, null);
}
/*jazor:clr-member static System.Linq.Enumerable.Intersect<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)*/
export function intersectWithComparer(first, second, comparer) {
  return IntersectCore(first, second, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.UnionBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)*/
export function unionBy(first, second, keySelector) {
  return UnionByCore(first, second, keySelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.UnionBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function unionByWithComparer(first, second, keySelector, comparer) {
  return UnionByCore(first, second, keySelector, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.ExceptBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)*/
export function exceptBy(first, second, keySelector) {
  return ExceptByCore(first, second, keySelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.ExceptBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function exceptByWithComparer(first, second, keySelector, comparer) {
  return ExceptByCore(first, second, keySelector, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.IntersectBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)*/
export function intersectBy(first, second, keySelector) {
  return IntersectByCore(first, second, keySelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.IntersectBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function intersectByWithComparer(first, second, keySelector, comparer) {
  return IntersectByCore(first, second, keySelector, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)*/
export function _e94a7db8306f4e71(source, value) {
  return ContainsCore(source, value, null);
}
/*jazor:clr-member static System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource, System.Collections.Generic.IEqualityComparer<TSource>)*/
export function containsWithComparer(source, value, comparer) {
  return ContainsCore(source, value, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.GroupBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)*/
export function _b7a70ff977974880(source, keySelector) {
  return GroupByCore_7b94285f70aa6f73(source, keySelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.GroupBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function groupByWithComparer(source, keySelector, comparer) {
  return GroupByCore_7b94285f70aa6f73(source, keySelector, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)*/
export function _e62121525c074f74(source, keySelector, elementSelector) {
  return GroupByCore_b0f00ed0755e257d(source, keySelector, elementSelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function groupByElementWithComparer(source, keySelector, elementSelector, comparer) {
  return GroupByCore_b0f00ed0755e257d(source, keySelector, elementSelector, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.GroupBy<TSource, TKey, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, System.Collections.Generic.IEnumerable<TSource>, TResult>)*/
export function groupByResult(source, keySelector, resultSelector) {
  return GroupByResultCore(source, keySelector, item => {
    return item;
  }, resultSelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.GroupBy<TSource, TKey, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, System.Collections.Generic.IEnumerable<TSource>, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function groupByResultWithComparer(source, keySelector, resultSelector, comparer) {
  return GroupByResultCore(source, keySelector, item => {
    return item;
  }, resultSelector, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Func<TKey, System.Collections.Generic.IEnumerable<TElement>, TResult>)*/
export function groupByElementResult(source, keySelector, elementSelector, resultSelector) {
  return GroupByResultCore(source, keySelector, elementSelector, resultSelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Func<TKey, System.Collections.Generic.IEnumerable<TElement>, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function groupByElementResultWithComparer(source, keySelector, elementSelector, resultSelector, comparer) {
  return GroupByResultCore(source, keySelector, elementSelector, resultSelector, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.ToLookup<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)*/
export function toLookup(source, keySelector) {
  return ToLookupCore(source, keySelector, item => {
    return item;
  }, null);
}
/*jazor:clr-member static System.Linq.Enumerable.ToLookup<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function toLookupWithComparer(source, keySelector, comparer) {
  return ToLookupCore(source, keySelector, item => {
    return item;
  }, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.ToLookup<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)*/
export function toLookupElement(source, keySelector, elementSelector) {
  return ToLookupCore(source, keySelector, elementSelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.ToLookup<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function toLookupElementWithComparer(source, keySelector, elementSelector, comparer) {
  return ToLookupCore(source, keySelector, elementSelector, comparer);
}
/*jazor:clr-member System.Linq.ILookup<TKey, TElement>.Count.get*/
export function lookupCount(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  return instance.length;
}
/*jazor:clr-member System.Linq.ILookup<TKey, TElement>.Contains(TKey)*/
export function lookupContains(instance, key) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  let comparer = LookupComparers.has(instance) ? LookupComparers.get(instance) : null;
  return FindLookupGrouping(instance, key, comparer) !== null;
}
/*jazor:clr-member System.Linq.ILookup<TKey, TElement>.this[TKey].get*/
export function lookupGet(instance, key) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  let comparer = LookupComparers.has(instance) ? LookupComparers.get(instance) : null;
  return FindLookupGrouping(instance, key, comparer) ?? new Array;
}
/*jazor:clr-member static System.Linq.Enumerable.Join<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, TInner, TResult>)*/
export function _f10104b4c52b4f96(outer, inner, outerKeySelector, innerKeySelector, resultSelector) {
  return JoinCore(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.Join<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, TInner, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function joinWithComparer(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer) {
  return JoinCore(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.GroupJoin<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, System.Collections.Generic.IEnumerable<TInner>, TResult>)*/
export function _b61f41d1ac124b69(outer, inner, outerKeySelector, innerKeySelector, resultSelector) {
  return GroupJoinCore(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.GroupJoin<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, System.Collections.Generic.IEnumerable<TInner>, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function groupJoinWithComparer(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer) {
  return GroupJoinCore(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.OrderBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)*/
export function _c8e0de6cfb4d0b1e(source, keySelector) {
  return OrderByCore(source, keySelector, false, null);
}
/*jazor:clr-member static System.Linq.Enumerable.OrderBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)*/
export function orderByWithComparer(source, keySelector, comparer) {
  return OrderByCore(source, keySelector, false, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.OrderByDescending<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)*/
export function _c955435630a10962(source, keySelector) {
  return OrderByCore(source, keySelector, true, null);
}
/*jazor:clr-member static System.Linq.Enumerable.OrderByDescending<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)*/
export function orderByDescendingWithComparer(source, keySelector, comparer) {
  return OrderByCore(source, keySelector, true, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.Order<T>(System.Collections.Generic.IEnumerable<T>)*/
function Order(source) {
  return OrderCore(source, false);
}
export { Order as order };
/*jazor:clr-member static System.Linq.Enumerable.Order<T>(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IComparer<T>)*/
export function orderWithComparer(source, comparer) {
  return OrderByCore(source, item => {
    return item;
  }, false, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.OrderDescending<T>(System.Collections.Generic.IEnumerable<T>)*/
export function orderDescending(source) {
  return OrderCore(source, true);
}
/*jazor:clr-member static System.Linq.Enumerable.OrderDescending<T>(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IComparer<T>)*/
export function orderDescendingWithComparer(source, comparer) {
  return OrderByCore(source, item => {
    return item;
  }, true, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.ThenBy<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>)*/
export function _b9eeb5472648105d(source, keySelector) {
  return ThenByCore(source, keySelector, false, null);
}
/*jazor:clr-member static System.Linq.Enumerable.ThenBy<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)*/
export function thenByWithComparer(source, keySelector, comparer) {
  return ThenByCore(source, keySelector, false, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.ThenByDescending<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>)*/
export function _c08a571c42e14ee7(source, keySelector) {
  return ThenByCore(source, keySelector, true, null);
}
/*jazor:clr-member static System.Linq.Enumerable.ThenByDescending<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)*/
export function thenByDescendingWithComparer(source, keySelector, comparer) {
  return ThenByCore(source, keySelector, true, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.ToHashSet<TSource>(System.Collections.Generic.IEnumerable<TSource>)*/
export function toHashSet(source) {
  return ToHashSetCore(source, null);
}
/*jazor:clr-member static System.Linq.Enumerable.ToHashSet<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)*/
export function toHashSetWithComparer(source, comparer) {
  return ToHashSetCore(source, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.ToDictionary<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)*/
export function toDictionary(source, keySelector) {
  return ToDictionaryCore(source, keySelector, item => {
    return item;
  }, null);
}
/*jazor:clr-member static System.Linq.Enumerable.ToDictionary<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function toDictionaryWithComparer(source, keySelector, comparer) {
  return ToDictionaryCore(source, keySelector, item => {
    return item;
  }, comparer);
}
/*jazor:clr-member static System.Linq.Enumerable.ToDictionary<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)*/
export function toDictionaryElement(source, keySelector, elementSelector) {
  return ToDictionaryCore(source, keySelector, elementSelector, null);
}
/*jazor:clr-member static System.Linq.Enumerable.ToDictionary<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function toDictionaryElementWithComparer(source, keySelector, elementSelector, comparer) {
  return ToDictionaryCore(source, keySelector, elementSelector, comparer);
}
