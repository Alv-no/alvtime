# CustomerCard overflow fix

Fix: long customer name pushes `MudCardActions` button outside fixed `20rem` card.
Scope: `Shared/components/CustomerCard.razor` only. One usage (`Customers.razor`). `EmployeeCard` is a separate copy — untouched.

## Part 1 — Button can't escape (root cause)
Make card a flex column so actions pin to bottom regardless of content height.
- [ ] 1. `MudCard` Style: add `display:flex; flex-direction:column;` (keep `width:13rem; height:20rem;`).
- [ ] 2. `MudCardContent` Style: add `flex:1 1 auto; min-height:0; overflow:hidden;` (keep existing wrap/center).
- [ ] 3. `MudCardActions`: stays last child → sits at bottom. Keep `align-self:center`.

## Part 2 — Name readability
- [ ] 4. Clamp name `MudText` (line 41) to 2 lines w/ ellipsis via inline style:
  `display:-webkit-box; -webkit-line-clamp:2; -webkit-box-orient:vertical; overflow:hidden; font-size:1.5rem;`

## Verify
- [ ] 5. Short name: unchanged look.
- [ ] 6. Long name (e.g. "Handelsbanken Kapitalforvaltning AS"): clamps to 2 lines + ellipsis; button stays inside card.
- [ ] 7. Long name + ContactPerson present: button still inside (flex pins it).

## Out of scope (follow-up)
Whole-card-clickable UX enhancement — needs a11y (keyboard/focus/role) + affordance; separate PR.

## Decisions
1. Overflow: **clipped** (`overflow:hidden`), no inner scroll.
2. Name clamp: **2 lines**.
