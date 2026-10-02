# Using with Coding Agents

Coding agents, such as Claude Code, write more reliable browser tests with Dramaturge when they are given its documentation and its rules.

## The Agent Skill

The repository publishes a skill, `dramaturge`: a short guide that an agent loads when it writes, reviews, or debugs code that uses Dramaturge. It covers launching browsers and isolating tests, choosing locators in the order that makes tests last, why a test never sleeps and asserts with `Expect`, actions, routes, and the known browser gaps, and ends with a checklist of rules. Its code samples are compiled with the documentation's, so they match the current API.

In Claude Code, add the repository's plugin marketplace and install the skill from it:

```bash
claude plugin marketplace add webdriverbidi-net/dramaturge
claude plugin install dramaturge@dramaturge
```

The skill is plain Markdown in the repository's [`skills/dramaturge`](https://github.com/webdriverbidi-net/dramaturge/tree/main/skills/dramaturge) folder, so it can also be copied into a project's `.claude/skills` directory, or given to another agent. It follows the repository's main branch, so it may describe an API newer than the latest release; the documentation describes the release.

For code that works with the WebDriver BiDi protocol directly, WebDriverBiDi.NET publishes its own skill; see its [documentation](https://webdriverbidi-net.github.io/webdriverbidi-net/articles/coding-agents.html).

## The Documentation as Text

The documentation site publishes two files for language models, at its root:

- [`llms.txt`](https://webdriverbidi-net.github.io/dramaturge/llms.txt): an index of every article, with a one-line description of each.
- [`llms-full.txt`](https://webdriverbidi-net.github.io/dramaturge/llms-full.txt): every article in one Markdown file, with its code samples written out.

Both are regenerated with each release.
