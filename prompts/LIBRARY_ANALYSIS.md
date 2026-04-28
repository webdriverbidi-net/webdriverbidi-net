# Analysis of WebDriverBiDi.NET

The code in this project is a .NET library written in C# intended to act as a
client library for the WebDriver BiDi protocol. The code for the library is
included in the `src` directory, tests are included in the `test` directory,
and library documentation is included in the `docs` directory. Analyze the code
and summarize its functionality. **IMPORTANT** You must discard all previous
analysis or memory of this project before performing your analysis in this pass.
This also means you **must** ignore any ANALYSIS.md or similar documents you
may find that already exist in the source tree. You must also ignore any items
in the `prompts` directory, as the documents contained therein are outside
the scope of your analysis.

After performing this analysis, make suggestions for how to make the library
better, and where defective issues may occur. Note carefully that the code in
the `src/WebDriverBiDi.Client` and `src/WebDriverBiDi.Demo` projects is
provided for demonstration to users, and is not shipping production code. Make
suggestions and a plan for addressing the issues you find, but do not make any
actual changes to the source code. Pay special attention to the public-facing
API design, and make suggestions for improvements there. Please also pay
attention to the documentation in the `docs` directory, as many of the design
decisions you may find questionable could be fully documented there. 

Be very thorough in your exploration of the code and documentation.

## Assumptions and design invariants

There are some design decisions of which you should be aware when performing
your analysis. Before forming any finding, read the design invariants below.
Any finding that touches a design invariant must be moved to the Non-Issues
section with evidence that you read the relevant documentation.
* An overarching principle of this library is that the main library does not
have dependencies on assemblies that are not shipped as part of a .NET platform
release. Suggesting enhancments or features that require additional
dependencies is absolutely forbidden for the main library.
* Null-forgiving operators used to express established .NET/TPL invariants
are acceptable and should not be flagged unless the invariant is demonstrably
violated by surrounding code. Likewise, null-forgiving operators where the
value is owned by this library and where the result will never be null are
acceptable and should not be flagged unless that invariant is also demonstrably
violated.
* Because this is a low-level library, convenience methods for module commands
are not supplied. It is expected that users will obscure the verbosity of the
library's syntax with their own wrapper methods. You may note this design
decision if you need to, but you **MUST NOT** flag it as a deficiency, nor
penalize the design for this choice.
* Standard .NET events are uniquely inappropriate for use with asynchronous
libraries. They require hacks like `await void`, which break down quickly in
the async/await pattern. Therefore, for the event-based mechanism, this library
uses a custom implementation of the Observer pattern. Feel free to make
suggestions on how to improve the existing pattern, including API changes, but
never recommend using standard .NET events.
* Event subscription requires two steps. Users must add an `EventObserver<T>` to
the class events, and send a message to the remote end by calling the
`session.subscribe` command. This is intentional, and will not be changing.
Note this limitation if you feel the need to, but **UNDER NO CIRCUMSTANCES**
suggest it as something that should be changed.
* The default error behavior is to ignore invalid protocol exceptions. This is
intentional. Note this as a limitation of the library if you feel the need to,
but do not list it as something that should be changed. The rationale for this
design decision is thoroughly documented in both XML documentation comments and
in the documentation in the `docs` directory. Under no circumstances should you
ever recommend revisiting this design decision.
* For many `CommandParameters` properties that expose lists, if the property is
marked as nullable, that means the protocol sees it as optional for the JSON
payload. It is important to distinguish between omitting the value altogether
(null) or an empty list, so these properties are deliberately initialized as
null. These properties should be documented in their XML documentation comments
for the rationale for the There is no need to recommend making changes to these
properties.
* Some module commands accept an optional `CommandParameters` object, and this
is an intentional API design decision. The rule for this pattern is that only
commands taking `CommandParameters` with no required properties omitting the
parameters **and** the `CommandParameters` class does **not** have a public
static property that provides a value to reset a value on the remote end; all
other commands require a command parameters object. `CommandParameters` classes
that **do** have a "reset" property **must** be used in their corresponding
command, because the name of the command would make the intent unclear if no
command parameters were passed For example, `SetSomeTemporaryCondition()` makes it
seem as though a condition is set in the driver vs.
`SetSomeTemporaryCondition(SetSomeTemporaryConditionCommandParameters.ResetOverridenCondition)`
expressly shows that the condition should be reset to its original condition.
Commands meeting this criteria include, but are not limited to, the following:

  * Browser.CloseAsync
  * Browser.CreateUserContextAsync
  * Browser.GetClientWindowsAsync
  * Browser.GetUserContextsAsync
  * BrowsingContext.GetTreeAsync
  * Script.GetRealmsAsync
  * Session.EndAsync
  * Session.StatusAsync
  * Storage.DeleteCookiesAsync
  * Storage.GetCookiesAsync
  * UserAgentClientHints.SetClientHintsOverrideAsync

As part of your evaluation, you should search for other commands that meet this
criteria.
* The Transport class uses an unbounded Channel for message processing. The use
of an unbounded channel was carefully considered, as the rate of incoming
messages should never cause memory issues. Moreover, this decision is
thoroughly documented in the XML documentation comments for the Transport
class. You may flag modifying this as a future enhancement, but it should
not affect your analysis of the production readiness of the library.
* The default command timeout is deliberately set to 60 seconds. This is to
allow very long running commands without modification to the timeout in the
common case. `DefaultCommandWaitTimeout` is a `public static readonly` field —
it is not a settable property. It can be overridden by users when constructing
a `BiDiDriver` object, or at the individual command level.
* The project is designed to be compatible with ahead-of-time (AOT) compilation
scenarios, and JSON serialization/deserialization is expected to work in those
environments, with one exception. The WebDriver BiDi protocol defines a free-form
`Extensible` construct, which allows for arbitrary extraneous data to be transmitted
in either direction as part of the JSON payload. One design invariant is that
we never want to expose `JsonElement` in any public API, which means that the
`AdditionalData` property of several objects is defined as `Dictionary<string, object?>`.
Normally, this is not compatible with `System.Text.Json`'s serialization and
deserialization for AOT environments. This is an acknowledged limitation of
the library, and must not be called out as a defect or area for modification.
* The `BiDiDriver` class does not implement a builder pattern for configuration.
Do not recommend one.
* This project is not a replacement for higher-level automation libraries like
Selenium, Puppeteer, or Playwright. Therefore, there is no need for a migration
guide In the project documentation. Please do not suggest one.
* This project includes Roslyn Analyzers that may help to alleviate some of the
issues you find in your analysis. Verify if any of your design issues are
covered by those analyzers, and if any new analyzers would help users avoid
what you consider design deficiencies.

## Verification rules

**Before suggesting any documentation improvement or gap**, search the `docs`
directory (including `docs/articles/`, `docs/articles/modules/`,
`docs/articles/advanced/`, etc.) to verify the topic is not already covered.
Any doc you claim has a gap, search the full docs tree first to confirm it is
not covered elsewhere (e.g., "all module docs reference timeout/
cancellation," "analyzers are referenced in X"). You must read the relevant
files and cite specific evidence. Do not recommend adding documentation for
concepts that are already documented elsewhere. If you are going to flag a
documentation code sample issue, you **must** validate the code sample against
the _actual_ code in the library.

**Before claiming that a documentation file does not exist**, perform an
explicit directory listing of the relevant directory (e.g., use Glob on
`docs/articles/modules/*.md`) and confirm the file is absent in the listing
output. Do not infer absence from a broader or filtered search that may have
missed the file. The directory listing result must be cited as the evidence.
Claiming a file is missing without a confirmed directory listing is a false
positive.

**Before claiming that a design decision lacks documentation**, perform an
explicit search of `docs/articles/` and its subdirectories and read any file
whose name plausibly covers the topic. Claiming a decision is undocumented
without a confirmed full-tree listing and a read of the candidate files is
a false positive of the same severity as recommending a prohibited change.

**Before claiming behavioral test coverage is missing or thin, inspect every test file that could plausibly cover the feature.**
In this project, per-module tests are conventionally split across multiple
files in the same directory: *ModuleTests.cs, *CommandParametersTests.cs,
*CommandResultTests.cs, and *EventArgsTests.cs. Before filing a coverage gap,
read all sibling test files in the module's test directory and grep the whole
test project for references to the specific matrix axes (enum values,optional-
property combinations, constructor forms) you claim are untested. File length,
test-method count, or line-count comparisons between directories are never by
themselves evidence of a gap — they are style observations at best. If the
authoritative check (reading sibling files and grepping for the specific axis)
does not reveal a real absence, withdraw the item.

**Before claiming that specific lines, branches, or code paths lack test coverage,**
run the project's coverage tool and cite the lcov output as evidence. Reading
test files, counting test methods, and grepping for feature names reveals what
tests exist, not what code they execute. The authoritative source for any
coverage-gap claim is:
```shell
dotnet test --project test/WebDriverBiDi.Tests --coverlet --coverlet-output-format lcov
```
with specific uncovered line numbers from the resulting .info file cited as evidence.
A coverage-gap finding filed without citing line numbers from an actual coverage
run is a false positive of the same severity as a documentation gap filed without
reading the docs tree.

**Before flagging any code as a defect, performance concern, or improvement opportunity**,
read the relevant implementation. Do not rely on assumptions about lock
duration, performance characteristics, or behavio; trace through the code and
verify your concern before including it in the analysis.

**Verify API facts before claiming gaps.** Before stating that a class of
methods lacks a feature (e.g., CancellationToken, timeout overrides), inspect
the actual method signatures in the module files. Do not rely on assumptions
or outdated documentation.

**Unverified suspicions must be omitted, not softened.** If you notice
something that might be incorrect but have not yet verified it against
source code, you have two options: read the source and verify it, or omit
it entirely. Writing hedged notes like "verify this", "ensure this is
accurate", or "keep this up to date" without first confirming there is
actually a discrepancy is prohibited. A finding must be either confirmed or
dropped, never filed as a vague suggestion.

**Before recommending that an exception type be changed in a guard clause**,
identify the operation the guard is protecting and determine what exception the
CLR itself would throw at that line if the guard were removed. If the current
exception type matches the CLR's own behavior for that operation (e.g.,
`InvalidCastException` protecting an explicit generic cast `(T)value`,
`ArgumentNullException` protecting a dereference), the choice is BCL-consistent
and must not be flagged. A recommendation to change it is a false positive.
Only recommend changing an exception type when the current type is
demonstrably inconsistent with what the CLR would throw for the same operation.

**When a claim concerns something a method or property does not do (lacks validation, lacks a check, lacks a feature), follow every delegation in the call chain to its concrete implementation before concluding the behavior is absent.**
If A.Foo delegates to B.Bar which calls C.Baz, the claim "Foo does not validate
X" requires reading C.Baz — not just A.Foo. Abstract methods, virtual methods,
and interface implementations must be traced to their concrete override.
Stopping at the first delegation point and inferring the absence of behavior is
a verification failure equivalent to not reading the implementation at all.

**Before recommending a manual verification step for any artifact (documentation snippets, sample code correctness, file existence)**,
check whether a CI job already validates that artifact continuously. Read
`.github/workflows/ci.yml` and any reusable workflow files it references.
If an existing job demonstrably covers the concern, the finding is not actionable;
move it to the Non-Issues section with the job name and script as evidence. Do not
file "verify X exists" or "run Y to confirm Z" action items for conditions that CI
already enforces on every PR.

**Proxy signals do not substitute for authoritative state.** Before recommending any "remove
/ clean up / re-add / re-register X" change, verify X's status in the authoritative source
for that change — not in a proxy. Examples: git tracking is authoritative for "is this in the
repo?" (not `ls`); the declaration is authoritative for "does this property exist?" (not `grep`
hits, which can be from docs or comments); `AnalyzerReleases.Shipped.md` is authoritative for
"is this analyzer shipped?" (not the presence of a `.cs` file). If you consulted only a proxy,
either read the authoritative source before recommending or omit the item.

**Before recommending a structural refactoring, identify the concrete benefit beyond aesthetics.**
Recommendations to split a file, extract a class, reduce the size of a method, introduce an
interface, or otherwise rearrange code must be grounded in at least one of:
* a **defect** the current structure makes hard to fix or hides (cite the defect);
* a **capability** the current structure prevents (cite the capability and a concrete caller who wants it);
* a **constraint** the project has adopted that the current structure violates (cite the constraint from
CLAUDE.md, the project invariants, or an explicit coding standard); or
* a **risk** the current structure materially increases, with evidence (not speculation).

A file being "large," a class having "many responsibilities," or a method being "long" is not by
itself a finding. Large, cohesive files are common in well-factored code. Before filing a structural
recommendation, ask: if I implemented this change today, what would be measurably better tomorrow?
If the answer is "the file would be smaller," withdraw the item. File-size concerns and similar
style observations may be noted as context but must not be deducted against the scorecard.

Specifically: do not recommend "extract responsibility X into its own class" when X has (a) exactly
one caller inside the current class, (b) 100% test coverage through the current class's public
surface, (c) no separate extension point that third-party code depends on, and (d) no demonstrated
defect. All four together indicate the refactor delivers indirection without benefit.

False positives (recommending changes for things already present, or without
verification) damage trust and waste maintainer time. They are worse than
missing a real improvement. When in doubt, omit the recommendation. Only
recommend after verification.

## Editing project code

If you are asked to make changes to this library, keep the following concepts
in mind:
* The unit test for the primary WebDriverBiDi library project currently provide
100% code coverage for lines, branches, and methods. If you are asked to make
changes to this library, you should maintain that level of code coverage. Note
that this level of code coverage is not provided for the Roslyn analyzers
project (`WebDriverBiDi.Analyzers`) nor for the structured logging project
(`WebDriverBiDi.Logging`). This is not a deficiency in either of those libraries,
and should not be flagged as such.
* The convention for code in this project is to avoid the use of the `var`
keyword in the C# language. You should adhere to this convention for any code
you generate. **IMPORTANT**: While this convention applies to code within the
project itself, it does _not_ apply to code in the documentation in the `docs`
directory.
* The tests in this project are written using the Microsoft Test Platform (MTP)
test runner. This means that the legacy VSTest runner command line arguments
used with `dotnet test` do not apply. When running tests for a specific project,
you will need to supply the project with the `--project` command line argument.
Code coverage is generated using the `--coverlet` command line argument, **not**
`--collect:"XPlat Code Coverage"`.

## Output

### Scoring

As the output of your analysis, present a scorecard for the production
readiness of the library. You should provide scores in the form of a numeric
score on a scale of 0-100 for the following categories:
* Code quality
* API design
* Concurrency and thread-safety
* Documentation
* Testing

Scores for individual categories should be accompanied by concrete suggestions
for how to raise them, if any exist. Apply rigorous thought to the suggestions
you make; invalid suggestions, or those already completed are treated as false
positives. If there are no suggestions to raise the score — either because the
project's stated design constraints preclude all recommendations or because no
verified gaps exist — that is a positive signal, not a neutral one. A category
with zero actionable improvements must receive a score of 90 or higher. Do not
deduct points to "leave room" for suggestions that the constraints forbid you
from making; phantom deductions for precluded suggestions are treated as false
positives with the same severity as recommending changes for things already
present.

**Style observations do not warrant score deductions.** If the only concern about
a category is aesthetic (file length, method length, naming-taste disagreements,
"this could be cleaner"), record the observation in the Non-Issues section rather
than deducting against the score. Deductions must be tied to concrete, actionable
recommendations that survive the structural-refactoring bar above.

### Action plan verification

For each action item you list, verify its current
state before recommending it. If an item would be "add X to Y," first read Y to
confirm X is not already present. Mark items as "Already complete" with
evidence (e.g., "browser.md lines 18–21 include Timeout and Cancellation section")
rather than listing them as work to be done.

Before recommending any change to a specific field, property, or method (e.g.,
"add validation to setter," "add overload," "change access modifier"), read its
declaration in the source file and confirm its kind, access modifiers, and
current signature. Do not assume a named member is a settable property, or
mutable, or missing a feature, without reading its declaration first. A
recommendation made without reading the declaration is a false positive.

For each action item in the action plan, include:
- **Recommendation:** [what to do]
- **Evidence:** [file path and line range, or grep output, showing current state]
- **Verification:** [one-sentence confirmation that you read the declaration before recommending]

Once your analysis is complete, write it to `ANALYSIS.md` in the root of the
project so that it can be referred to in future sessions. If the `ANALYSIS.md`
file already exists, overwrite it with your current findings. Before writing
ANALYSIS.md, for each planned recommendation, confirm:

[] I read the declaration/source for the affected symbol
[] I did not assume; I verified
[] This is not already implemented/documented (per my search results)

If any item is unchecked, perform the verification or remove the recommendation.
