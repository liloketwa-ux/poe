# CampusConnect — Interaction Design POE
## Part 2: Prototype Implementation and Evaluation Update

**Student name:** [Enter first name]  
**Surname:** [Enter surname]  
**Student number:** [Enter student number]  
**Module code:** [Enter module code]  
**Module:** Interaction Design  
**Submission component:** Part 2 — Prototype Implementation and Evaluation Update  
**Date:** 16 October 2026

## Contents
1. Part 2 purpose and scope
2. Review of Part 1 and design refinement
3. Prototype overview and implementation
4. Interaction design and accessibility
5. Prototype evidence
6. Functional testing and results
7. Limitations and further development
8. Conclusion
9. Reference list

## 1. Part 2 purpose and scope

Part 2 develops the CampusConnect concept from Part 1 into an interactive website prototype. Its purpose is to demonstrate that identified user needs and design requirements can be translated into usable interface behaviour, supporting Module Outcome 3 (MO3). CampusConnect combines five student-focused activities: discovering study groups, browsing campus events, searching support resources, maintaining a short personal task list and completing a brief, non-clinical emotional check-in.

The prototype uses semantic HTML, CSS and browser-based JavaScript. It can be opened locally in a modern browser and does not require an account, database, internet connection or package installation. This keeps the demonstration focused on interface behaviour rather than backend engineering.

The report documents design refinement, implemented features, accessibility decisions and scripted functional checks. The test evidence is limited to functional smoke tests; no participant usability study or formal WCAG conformance audit is claimed.

## 2. Review of Part 1 and design refinement

Part 1 requirements emphasised direct navigation, low cognitive load, clear feedback, reversible actions, responsive layouts and accessibility. These requirements informed the Part 2 interaction choices. The interface was considered against usability principles such as visibility of system status, consistency, recognition rather than recall and user control (Nielsen, 2024; Norman, 2013).

### 2.1 Feedback record and integrity note

The materials available for preparing this report did not include specific lecturer or peer comments from Part 1. To avoid inventing feedback, the changes below are described as self-review refinements against the design requirements, not as external reviewer comments. Add any authentic feedback separately received and explain the corresponding response before submission if required.

| Review point | Design need | Part 2 refinement |
|---|---|---|
| Navigation and orientation | Main student tasks should be easy to find. | Added top-level Home, Study groups, Events, Resources and My plan routes, plus shortcut cards on the home page. |
| Visible interaction feedback | Users should understand whether an action worked. | Join and RSVP labels change after activation; task progress updates. |
| User control | Users should be able to reverse low-risk actions. | Joining/leaving and RSVP/cancellation are reversible; tasks can be unchecked or deleted. |
| Accessibility and focus | Keyboard use and readable presentation preferences should be supported. | Added skip link, focus styling, labelled controls and text-size, contrast and reduced-motion preferences. |
| Responsive presentation | Common tasks should remain usable on phones. | Responsive grid rules collapse columns and a labelled Menu button reveals mobile navigation. |
| External feedback | No authentic reviewer comments were supplied with the project materials. | No external feedback is attributed. Insert genuine comments and actions here if applicable. |

### 2.2 Result of the review

The interface is shallow and task-led. Consistent cards, buttons and headings support scanning and orientation. Self-review identified a keyboard-focus problem on initial load; the implementation was adjusted so the skip link remains the first keyboard stop, while in-app navigation can move focus to the main content. The scripted keyboard check then passed. This is an implementation refinement, not a substitute for real lecturer or peer feedback.

## 3. Prototype overview and implementation

CampusConnect is a single-page web prototype with hash-based navigation. Its visual language uses a light background, dark text, blue primary actions, generous spacing and rounded cards. A persistent header and repeated navigation structure help users understand where they are and move between tasks.

### 3.1 Implemented features

| Feature | Implementation | Observed behaviour |
|---|---|---|
| Home and navigation | Quick links, weekly example and emotional check-in. | Routes open from the header and home-page shortcuts. |
| Study groups | Course, format and keyword filters; group cards; Join group action. | Filters update visible cards; joining and leaving are reversible in the current session. |
| Events | Category filter and event cards with time, location and descriptions. | RSVP is confirmed visibly and can be cancelled. |
| Resources | Keyword search across resource titles, categories and descriptions. | Matching cards appear; action buttons show demonstration alerts rather than opening live support pages. |
| My plan | Add tasks, complete/undo completion, delete tasks and view progress. | Task list and progress are recalculated after each action. |
| Accessibility settings | Dialog with larger text, higher contrast and reduced-motion controls. | Presentation changes while the page remains open; preferences are not saved to a backend. |

### 3.2 Technology and operation

The prototype comprises index.html for semantic structure, style.css for layout and responsive presentation, and app.js for navigation, filtering and state changes. No framework or build step is required. Open prototype/index.html in a modern browser. All sample groups, events and resources are illustrative, not a live campus directory or official event calendar.

Interaction state is held in JavaScript for the current open page and resets on reload. Authentication, server-side storage and institutional integration are not implemented. This is appropriate for a demonstrator but not for production use.

## 4. Interaction design and accessibility

Interaction design should make an action, its result and the next available step understandable. Human-centred design recommends considering users, tasks and context throughout development (ISO, 2019). CampusConnect applies this through direct labels, immediate feedback, scannable cards and reversible actions.

| Principle | Prototype example | Usability benefit |
|---|---|---|
| Visibility of system status | Join/RSVP labels change; task count and check-in status update. | Users see the result of an action without a page reload. |
| User control and freedom | Join/leave, RSVP/cancel, task undo and deletion. | Users can reverse common low-risk choices. |
| Recognition rather than recall | Cards show category, schedule, format, location or description. | Users can compare choices without remembering details across routes. |
| Consistency | Repeated button styling, navigation and page headings. | Similar controls have predictable meaning. |
| Error prevention | Required task field, 80-character limit and labelled filters. | Basic invalid task submission is prevented through native validation. |
| Responsive use | Grid columns collapse at narrower widths and Menu reveals mobile navigation. | The layout remains usable at the tested 390-pixel viewport width. |

### 4.1 Accessibility implementation

WCAG 2.2 organises accessibility guidance around perceivable, operable, understandable and robust content (W3C, 2024). The prototype implements selected observable practices; these are design intentions and preliminary checks, not a claim of full WCAG 2.2 Level AA conformance.

| Accessibility area | Implementation and current evidence |
|---|---|
| Keyboard access | Skip link, native links and buttons, and visible focus indicators. The skip link is reachable with the first Tab on initial load. |
| Labels and semantics | Landmarks, headings, labelled selection/search fields and a labelled task input. |
| Dynamic feedback | The emotional check-in uses a status region; Join and RSVP controls update their text to reflect state. |
| Presentation options | Larger text, higher contrast and reduced non-essential motion for the active session. |
| Reflow | Responsive CSS rearranges columns. The mobile smoke test found no horizontal overflow at 390 pixels. |

Further accessibility work should include keyboard-only and assistive-technology testing, contrast measurements for all states, zoom and reflow checks at several widths, and review against all applicable WCAG 2.2 success criteria. Automated checks alone cannot establish that the needs of all users are met (W3C, 2024).

## 5. Prototype evidence

The original submission package contains screenshots of the home page, study groups, My Plan, accessibility settings, mobile home and mobile events views. They document the visual implementation and the test viewport. The home page introduces the service in plain language and offers primary routes. Study-group filters appear before the comparable cards, and joined-state text gives action feedback without relying on colour alone. My Plan shows task completion controls and progress. The mobile view demonstrates a single-column reflow at 390 × 844 pixels.

Visual consistency in spacing, borders, typography and button placement supports the consistency and minimalist-design heuristics discussed by Nielsen (2024) and Krug (2014). Screenshots show specific views, not complete compatibility across every device, browser or zoom setting.

## 6. Functional testing and results

A scripted functional smoke-test pass was recorded using Playwright-controlled Chromium on 16 October 2026. Desktop checks used a 1440 × 1000 viewport; mobile checks used a 390 × 844 viewport. Tests exercised key controls and inspected visible outcomes.

| ID | Test action | Expected/observed outcome | Result |
|---|---|---|---|
| T01 | Open Home. | Correct title and home content are visible. | PASS |
| T02 | Filter study groups by Interaction Design. | Matching course groups remain visible. | PASS |
| T03 | Join a group, then leave it. | The action label changes and returns to its original state. | PASS |
| T04 | Filter Careers events; RSVP and cancel. | Matching event cards remain and RSVP can be reversed. | PASS |
| T05 | Search Resources for “careers”. | A careers-related resource is returned. | PASS |
| T06 | Complete a task, add a task and delete it. | Progress updates and the added task is removed. | PASS |
| T07 | Open accessibility preferences and toggle settings. | The dialog opens/closes and presentation classes change. | PASS |
| T08 | Choose an emotional check-in option; use first Tab on a fresh load. | Status text updates and the skip link is first and visible. | PASS |
| T09 | Use mobile menu and open Events. | Menu opens, Events appears and horizontal overflow is absent. | PASS |
| T10 | Monitor browser errors during checks. | No uncaught JavaScript page errors are reported. | PASS |

**Result:** 10 of 10 scripted functional checks passed in the recorded test environment, with no uncaught JavaScript errors detected. This is evidence that the tested interactions responded as expected under the test conditions.

**Testing boundary:** these checks are not observed sessions with representative student participants. No completion-time averages, satisfaction scores or participant quotations are reported because no user study was conducted. The checks are not a comprehensive WCAG conformance audit.

## 7. Limitations and further development

The prototype demonstrates interface behaviour but remains an early front-end model.

- No sign-in, student verification, backend, database or server-side persistence is implemented. Task, join and RSVP state resets when the page reloads.
- Group, event and resource content is illustrative. Events do not synchronise with an institutional calendar, and resource buttons do not open official support pages.
- The emotional check-in provides a simple acknowledgement and next-step prompt. It is not a health assessment, does not diagnose and does not replace professional support.
- Accessibility behaviours were checked selectively. The prototype has not been tested with screen readers, speech input, switch controls or a diverse group of users.
- No formal accessibility certification is claimed.
- External Part 1 feedback was not included in the supplied material. Add any authentic lecturer or peer comments if they were received and are required.

### 7.1 Recommended next iteration

1. Conduct task-based sessions with student volunteers and record completion, points of hesitation, errors and feedback with consent.
2. Perform keyboard-only and assistive-technology testing, measure contrast, test zoom/reflow and map issues to WCAG 2.2 criteria.
3. Replace demonstration resource actions with verified institution-approved links and connect event content to a maintained source.
4. Before storing student information, define consent, data minimisation, access controls, retention, security and persistence requirements.

## 8. Conclusion

Part 2 demonstrates an implemented CampusConnect prototype that translates Part 1 requirements into navigable routes and functional interactions. Users can filter and join study groups, filter events and RSVP/cancel, search resources, manage a small task list, complete an emotional check-in and change presentation preferences. All ten recorded scripted checks passed in the desktop/mobile test environment. The evidence supports progress towards MO3 while also identifying the need for authentic user feedback, wider accessibility evaluation and real service integration.

Before handing in, complete the personal information fields and incorporate any genuine lecturer/peer feedback received from Part 1. These details cannot be inferred and have not been invented.

## 9. Reference list

International Organization for Standardization (ISO) (2019) *ISO 9241-210:2019 Ergonomics of human-system interaction — Part 210: Human-centred design for interactive systems*. Available at: https://www.iso.org/standard/77520.html (Accessed: 16 October 2026).

Krug, S. (2014) *Don’t Make Me Think, Revisited: A Common Sense Approach to Web Usability*. 3rd edn. Berkeley, CA: New Riders.

Nielsen, J. (2024) ‘10 usability heuristics for user interface design’. *Nielsen Norman Group*, 30 January. Available at: https://www.nngroup.com/articles/ten-usability-heuristics/ (Accessed: 16 October 2026).

Norman, D.A. (2013) *The Design of Everyday Things: Revised and Expanded Edition*. New York: Basic Books.

Rogers, Y., Sharp, H. and Preece, J. (2019) *Interaction Design: Beyond Human-Computer Interaction*. 5th edn. Indianapolis: Wiley.

World Wide Web Consortium (W3C) (2024) *Web Content Accessibility Guidelines (WCAG) 2.2*. W3C Recommendation, 12 December. Available at: https://www.w3.org/TR/WCAG22/ (Accessed: 16 October 2026).

World Wide Web Consortium (W3C) (2024) *How People with Disabilities Use the Web*. Web Accessibility Initiative. Updated 25 June. Available at: https://www.w3.org/WAI/people-use-web/ (Accessed: 16 October 2026).

*Referencing style: Harvard Anglia author-date style.*
