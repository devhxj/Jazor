import { isUsefulSpecificationProse } from "./collect.mjs";

function assert(value: boolean, message: string): void {
  if (!value) {
    throw new Error(message);
  }
}

export function specificationProseKeepsDirectApiGuidance(): void {
  const abortCandidate = {
    types: ["method"],
    names: ["abort", "abort()"],
    owners: ["AbortController/abort(reason)"],
  };
  assert(
    isUsefulSpecificationProse(
      "AbortController is meant to support abort semantics by providing an abort() method that toggles the corresponding AbortSignal state.",
      abortCandidate,
    ),
    "direct API prose should be kept",
  );
  assert(
    !isUsefulSpecificationProse(
      "The new AbortController() constructor steps are:",
      abortCandidate,
    ),
    "algorithm scaffolding should not become a summary",
  );
  assert(
    !isUsefulSpecificationProse(
      "options|'s signal, **and** the AbortSignal of an AbortController.",
      abortCandidate,
    ),
    "unresolved Bikeshed syntax should not reach generated XML docs",
  );
}

export function argumentProseIdentifiesItsOwningOperation(): void {
  const reasonCandidate = {
    types: ["argument"],
    names: ["reason"],
    owners: ["AbortController/abort(reason)"],
    requiresQualifiedOwner: true,
  };
  assert(
    isUsefulSpecificationProse(
      "Calling abort(reason) communicates the reason through the associated AbortSignal.",
      reasonCandidate,
    ),
    "an argument explanation tied to its operation should be kept",
  );
  assert(
    !isUsefulSpecificationProse(
      "A caller may pass an options object to subscribe().",
      reasonCandidate,
    ),
    "same-name or nearby argument text from another operation should be dropped",
  );
}
