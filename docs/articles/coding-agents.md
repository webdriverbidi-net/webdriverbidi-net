# Using with Coding Agents

Coding agents, such as Claude Code, write better code against WebDriverBiDi.NET when they are given its documentation and its rules. Three things help.

## The Agent Skill

The repository publishes a skill, `webdriverbidi-net`: a short guide that an agent loads when it writes, reviews, or debugs code that uses the library. It covers connecting to each browser, commands, the two-step event subscription, where event handlers run, script results, and error handling, and ends with a checklist of the mistakes the library's users most often make. Its code samples are compiled with the documentation's, so they match the current API.

In Claude Code, add the repository's plugin marketplace and install the skill from it:

```bash
claude plugin marketplace add webdriverbidi-net/webdriverbidi-net
claude plugin install webdriverbidi-net@webdriverbidi-net
```

The skill is plain Markdown in the repository's [`skills/webdriverbidi-net`](https://github.com/webdriverbidi-net/webdriverbidi-net/tree/main/skills/webdriverbidi-net) folder, so it can also be copied into a project's `.claude/skills` directory, or given to another agent. It follows the repository's main branch, so it may describe an API newer than the latest release; the documentation describes the release.

## The Documentation as Text

The documentation site publishes two files for language models, at its root:

- [`llms.txt`](https://webdriverbidi-net.github.io/webdriverbidi-net/llms.txt): an index of every article, with a one-line description of each, from which an agent can choose what to read.
- [`llms-full.txt`](https://webdriverbidi-net.github.io/webdriverbidi-net/llms-full.txt): every article in one Markdown file, with its code samples written out.

Both are regenerated with each release.

## The Analyzers

The [WebDriverBiDi.Analyzers](advanced/analyzers.md) package reports many of the same mistakes at compile time, such as an observer whose event is never subscribed to, or a handler that blocks the transport. An agent that builds its code sees them as warnings and errors, and can fix them before anyone runs the code.
