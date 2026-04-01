export function Prefix() {
  return "Hello";
}
export function Compose(name) {
  return `${Prefix()}, ${name}`;
}
