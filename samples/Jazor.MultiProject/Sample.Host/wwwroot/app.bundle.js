// shared/greetings.mjs
function Prefix() {
  return "Hello";
}
function Compose(name) {
  return `${Prefix()}, ${name}`;
}

// features/greeter.mjs
function Greet(name) {
  return Compose(name);
}

// host/app.mjs
function Boot() {
  return Greet("Jazor");
}
export {
  Boot
};
