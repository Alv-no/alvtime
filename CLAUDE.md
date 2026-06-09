#### Project Overview
This is a monorepo for various components for the Alvtime project. See README.md for quick intro to the various components and tech stack.

#### Workflow
- Never jump into implementation. Announce what you are about to work on and wait for explicit approval before writing code.
- Work through tasks one at a time. State which task you are ready to start, then wait for the go-ahead.
- Do not batch tasks or auto-start the next task after completing one.
- Always verify existing patterns before introducing new ones
- Follow TDD: write the failing test first, then implement to make it pass. Never write implementation before its test.
- Ask clarifying questions rather than assuming

#### Building on /c/ filesystem

The `/c/` filesystem does not update file modification timestamps on write. The .NET incremental build system uses mtimes to detect changes, so edits made to source files in `/c/` are NOT picked up by normal `dotnet build` or `dotnet test`.

**Always delete the `obj/` directory of any changed project before building:**

```bash
rm -rf /c/_dev/alvtime/packages/api/AlvTime.Business/obj/
rm -rf /c/_dev/alvtime/packages/api/Tests/obj/
dotnet test /c/_dev/alvtime/packages/api/Tests/ -p:OutputPath=/tmp/alvbuild/ -p:UseAppHost=false
```

The `-p:OutputPath=/tmp/alvbuild/ -p:UseAppHost=false` flags are still required because `/c/` also can't create memory-mapped files needed by the app host.

#### Plans
- Make the plan extremely concise. Sacrifice grammar for the sake of concision.
- At the end of each plan, give me a numbered list of unresolved questions to answer, if any.
- Lists of tasks and subtasks should always be numbered, and with checkboxes.
- Write plans to `.claude/plans/<plan-name>.md` (tracked in git).
- `.claude/plans/local/` is gitignored — use for scratch/personal plans.