# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| API endpoints, vertical slices, EF Core, database | Rhodey | Create endpoint, add entity, modify query logic |
| Blazor UI, Tailwind CSS, components, responsive design | Parker | Add page, style component, fix layout, accessibility |
| Aspire infrastructure, Azure deployment, containers, service config | Friday | Add resource, configure Redis/SQL, health checks, azd deploy, publish to Azure |
| Code review, architecture, scope decisions | Stark | Review PRs, design decisions, trade-offs |
| Testing, QA, edge cases | Banner | Write tests, verify fixes, coverage gaps |
| Session logging | Scribe | Automatic — never needs routing |

## Keyword Routing

| Keywords | Route To |
|----------|----------|
| endpoint, API, route, feature slice, EF Core, database, migration | Rhodey |
| page, component, UI, Blazor, Tailwind, responsive, CSS, layout, accessibility | Parker |
| test, coverage, edge case, QA, assert, xUnit | Banner |
| Aspire, AppHost, Redis, SQL Server, container, resource, health, deploy, Azure, publish, azd | Friday |
| architecture, design, review, scope, decision | Stark |

## Review Gates

| Work Type | Reviewer |
|-----------|----------|
| API endpoints (new or modified) | Banner (tests) → Stark (architecture) |
| Frontend components | Stark (design review) |
| Infrastructure changes | Stark (architecture review) |
| Test changes | Rhodey or Parker (depending on domain) |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Stark (Lead) |
| `squad:rhodey` | Pick up API/backend issue | Rhodey |
| `squad:parker` | Pick up frontend/UI issue | Parker |
| `squad:banner` | Pick up testing issue | Banner |
| `squad:friday` | Pick up infra/DevOps issue | Friday |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, **Stark** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Lead review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn the tester to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. Stark handles all `squad` (base label) triage.
8. **Skills** — Rhodey uses `api-creator` and `api-versioning` skills. Parker uses `frontend-dev` skill. Friday uses `aspire` skill.
