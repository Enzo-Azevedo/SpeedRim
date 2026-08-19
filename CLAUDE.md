# Working notes for Claude

## Refactoring: what to look for

The question is never "how do I make this prettier?" It is:

> What model was the author trying to express, where does the implementation waste
> work, hide state or widen effects that the model does not need, and what structure
> expresses the same intent with less of that?

A refactor preserves behaviour. Fewer lines is not, by itself, an improvement.
Working code can still be badly modelled — that is the target, and it is exactly the
code a cosmetic refactor makes worse.

### Five questions, in order

1. **Repetition** — is the same work done more than once? Look for a missing index
   (a key/value map instead of a nested scan), a missing set when the question is
   membership, and work inside a loop that does not depend on the current item.
2. **Dependency** — does this step really have to wait? Parallelise independence;
   keep the sequence where there is a causal dependency. Never parallelise a
   dependency to look fast.
3. **State** — who owns this information and who may change it? Prefer a result that
   travels with the operation over a global another function has to interpret. Watch
   for functions that mutate their input while looking like transformations.
4. **Boundaries** — where does data leave a trusted domain? Parameterise queries,
   validate against an allowlist rather than a list of known-bad strings, and use the
   real parser for a known format instead of writing one.
5. **Failure** — what should happen when this goes wrong? Classify it, handle it at
   the level that knows the decision, preserve the original cause, clean up in the
   equivalent of `finally`.

### Guard rails — these matter more than the rules

- Do not optimise before naming the dominant work. Turning a loop into `map`, or
  `map` into `reduce`, proves nothing on its own.
- Falsy is not the same as absent, and optional chaining is for absence that is
  expected — not for hiding an invariant that should fail loudly.
- Do not convert a rich error into a poor value (`false`, `null`, `[]`) to avoid an
  exception. Do not use exceptions for ordinary control flow either.
- **Caching trades repetition for state.** Whenever I add one, the cache key must
  include everything the cached value depends on. A stale cache is a modelling
  error, not a performance detail.
- Not all state is bad. Local state and domain state are legitimate; the target is
  state that is implicit and unnecessary.
- Splitting a function is about separating responsibilities that have different
  reasons to change, not about line count.

### How this applies here

- The tooltip cache in `TimeControls_DoTimeControlsGUI_Patch` is exactly the trade
  above. It is keyed on the tier's multiplier, its key binding and the active
  language; any new input to the tooltip has to join that key.
- `TickManager_ExposeData_Patch` mutates vanilla state and restores it. The restore
  is a Harmony finalizer rather than a postfix, because a postfix does not run when
  the original method throws.
- The degraded paths in `SpeedRimGameCompat` and `SpeedRimMod.Settings` fall back
  instead of crashing — killing a player's UI is worse than running on defaults —
  but every fallback reports itself in the log. Degrading is a decision to declare,
  never a silence.
- `Array.IndexOf` over the six-entry speed ladder stays as it is: six elements, once
  per keypress. Replacing it would be optimisation without a dominant cost.
