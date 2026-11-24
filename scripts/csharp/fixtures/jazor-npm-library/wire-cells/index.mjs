const weights = { first: 2, second: 4 };
export const makeCell = value => ({ value, label: 'original' });
export const cellWeights = () => weights;
export const cellCount = () => Object.keys(weights).length;
export const readLabel = cell => cell.label;
export const describeSlots = (...values) => `${values.length}:${values.map(value => String(value)).join('|')}`;
