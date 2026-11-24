export const apply = (value, select) => select(value);

export const label = Object.assign(
    value => typeof value === 'string' ? value : `${value.type}:${value.message}`,
    { loud: message => `loud:${message}`, prefix: 'loud' },
);
