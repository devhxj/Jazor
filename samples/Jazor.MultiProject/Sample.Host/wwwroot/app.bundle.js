// shared/greetings.mjs
function prefix() {
  return "Hello";
}
function compose(name) {
  return `${prefix()}, ${name}`;
}

// features/greeter.mjs
function greet(name) {
  return compose(name);
}

// host/app.mjs
function boot() {
  return greet("Jazor");
}
export {
  boot
};
//# sourceMappingURL=app.bundle.js.map
