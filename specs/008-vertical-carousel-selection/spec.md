# Feature Specification: Vertical Carousel Terminal Selection

**Feature Branch**: `008-vertical-carousel-selection`

**Created**: 2026-11-03

**Status**: Draft

**Input**: User description: "Na navegação no terminal, se a opção selecionada for a última e a seta apertada for pra baixo, a primeira opção deve ser selecionada. Se estiver na primeira e a seta pra cima for selecionada, a opção selecionada deve ser a última. Ou seja, a seleção deve ser em formato de carrossel vertical"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Forward Wrap-Around from End to Beginning (Priority: P1)

As a developer navigating the interactive terminal task menu, when I reach the last available option (including the exit option) and press the Down arrow key, I want the selection cursor to wrap around to the first option so that I do not get blocked at the bottom of the list and do not have to repeatedly press the Up arrow key to return to the top.

**Why this priority**: Eliminating navigation dead ends in interactive menus provides a seamless terminal experience and fulfills the primary user requirement for carousel navigation.

**Independent Test**: Can be tested by launching the interactive task selector, moving the selection cursor to the bottom-most item in the list, pressing Down arrow, and verifying that the first item in the list becomes highlighted.

**Acceptance Scenarios**:

1. **Given** the interactive terminal menu is open with multiple options and the active cursor is on the last option, **When** the user presses the Down arrow key, **Then** the cursor immediately wraps around and selects the first option at the top of the menu.
2. **Given** the interactive terminal menu displays configured tasks and an exit option at the end, **When** the cursor is focused on the exit option and the user presses the Down arrow key, **Then** the cursor moves to the very first task in the list.

---

### User Story 2 - Backward Wrap-Around from Beginning to End (Priority: P1)

As a developer navigating the interactive terminal task menu, when the selection cursor is on the first option and I press the Up arrow key, I want the selection cursor to wrap around to the last option so that I can quickly reach the bottom items or exit without scrolling down through all intervening choices.

**Why this priority**: Navigating upward from the start is an intuitive shortcut for users who want to reach the end of a list quickly (such as the exit option or the last configured task).

**Independent Test**: Can be tested by launching the interactive task selector (which defaults to the first option), pressing Up arrow once, and verifying that the cursor immediately jumps to the final option in the menu.

**Acceptance Scenarios**:

1. **Given** the interactive terminal menu is open and the active cursor is on the first option, **When** the user presses the Up arrow key, **Then** the cursor wraps around to the last option in the list.
2. **Given** the interactive terminal menu is open at initial launch (first item selected), **When** the user presses the Up arrow key once, **Then** the exit option at the bottom is selected.

---

### User Story 3 - Continuous Cyclical Navigation Across Long Lists (Priority: P2)

As a developer navigating a project with many configured tasks, I want continuous wrap-around navigation in both vertical directions across paginated views so that I can fluidly cycle through all tasks in a continuous vertical loop without encountering hard boundaries.

**Why this priority**: Ensures that pagination and list overflow do not break the continuous carousel loop behavior.

**Independent Test**: Can be tested with a configuration containing more items than fit on a single terminal page, verifying that continuous Up or Down navigation smoothly cycles through all pages and wraps from first to last and vice versa.

**Acceptance Scenarios**:

1. **Given** an interactive menu with more tasks than a single page display limit, **When** the user continuously navigates Down through all options, **Then** the menu scrolls through every page and smoothly wraps from the last item back to the first item on page one.
2. **Given** an interactive menu with multiple pages of tasks, **When** the user continuously navigates Up from the first item, **Then** the menu wraps to the final page and highlights the last option.

---

### Edge Cases

- **Single Option or Only Exit Option**: When the menu contains only a single option or only the exit option, pressing Up or Down must keep the single option selected without throwing errors or locking up the terminal interface.
- **Two Options in Menu**: When exactly two choices exist, pressing Down toggles between first and second, and pressing Up toggles between second and first smoothly.
- **Fast Continuous Keypresses**: Rapidly repeating Up or Down arrow keypresses must maintain consistent wrap-around state without skipping arbitrary elements or causing rendering artifacts.
- **Selection and Confirmation**: Wrapping around must only affect cursor highlight position; pressing the Enter key on any wrapped-to option must execute that option normally.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The interactive terminal selection interface MUST support circular (wrap-around) vertical navigation.
- **FR-002**: When the selection cursor is positioned on the last item of the list and the user inputs a Down navigation command, the system MUST move the cursor to the first item of the list.
- **FR-003**: When the selection cursor is positioned on the first item of the list and the user inputs an Up navigation command, the system MUST move the cursor to the last item of the list.
- **FR-004**: The wrap-around behavior MUST include all visible menu choices, including standard operational choices such as the exit option.
- **FR-005**: The interactive selection interface MUST preserve normal selection confirmation (e.g., Enter key) regardless of whether an option was reached via sequential traversal or wrap-around traversal.
- **FR-006**: The vertical carousel navigation MUST function consistently regardless of the number of available options (including lists larger than the display viewport).

### Key Entities

- **Interactive Menu Prompt**: The terminal component responsible for presenting a list of choices, handling keyboard navigation inputs, and rendering the active cursor highlight.
- **Menu Choice List**: The ordered collection of selectable items, consisting of configured tasks and default operational choices (such as the exit option).
- **Navigation Cursor**: The active pointer indicating which menu choice is currently highlighted for selection.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of vertical boundary transitions (first-to-last via Up arrow, and last-to-first via Down arrow) seamlessly wrap around without boundary stops or user input rejection.
- **SC-002**: Navigating to the last option from the initial state requires exactly 1 keypress (Up arrow) instead of N-1 keypresses.
- **SC-003**: User task selection success rate remains 100% with zero reported navigation lockups or cursor desynchronization.
- **SC-004**: Existing interactive menu features (filtering, task execution, exit handling, Portuguese prompt text) continue to function with zero regressions.

## Assumptions

- Terminal environments running interactive sessions support standard ANSI cursor key events (Up and Down arrows).
- Non-interactive and piped environments do not invoke the interactive selector and remain unaffected by menu navigation settings.
- The carousel navigation behavior applies to all interactive choice selection lists presented by the CLI.
