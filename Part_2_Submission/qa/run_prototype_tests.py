"""Run functional smoke tests for the CampusConnect prototype.
Requires Python package playwright and an installed Chromium browser.
"""
from pathlib import Path
import json
from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[1]
HTML = ROOT / "prototype" / "index.html"
EVIDENCE = ROOT / "evidence"
EVIDENCE.mkdir(exist_ok=True)

html = HTML.read_text(encoding="utf-8")
css = (ROOT / "prototype" / "style.css").read_text(encoding="utf-8")
js = (ROOT / "prototype" / "app.js").read_text(encoding="utf-8")
source = html.replace('<link rel="stylesheet" href="style.css">', f"<style>{css}</style>")
source = source.replace('<script src="app.js"></script>', f"<script>{js}</script>")
results, errors = [], []

def record(test_id, title, passed, details=""):
    results.append({"id": test_id, "test": title,
                    "result": "PASS" if passed else "FAIL", "details": details})

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True, args=["--no-sandbox"])
    page = browser.new_page(viewport={"width": 1440, "height": 1000})
    page.on("pageerror", lambda exc: errors.append(str(exc)))
    page.set_content(source)
    record("T01", "Load home page",
           page.title() == "CampusConnect - Student support hub" and page.locator("#homeRoute").is_visible(),
           page.title())

    page.locator('[data-route="groups"]').first.click()
    page.locator("#groupCourse").select_option(label="Interaction Design")
    titles = page.locator("#groupList h3").all_text_contents()
    badges = page.locator("#groupList .badge").all_text_contents()
    record("T02", "Filter study groups by course",
           bool(titles) and all(badges[i * 2] == "Interaction Design" for i in range(len(titles))),
           f"{len(titles)} matching groups")

    join = page.locator(".join-btn").first
    join.click()
    joined = join.inner_text().strip() == "Joined - leave"
    join.click()
    left = join.inner_text().strip() == "Join group"
    record("T03", "Join and leave a study group", joined and left, "Reversible state")

    page.locator('[data-route="events"]').first.click()
    page.locator("#eventCategory").select_option("Careers")
    events = page.locator("#eventList h3").all_text_contents()
    event_btn = page.locator(".event-btn").first
    event_btn.click()
    confirmed = event_btn.inner_text().strip() == "RSVP confirmed - cancel"
    event_btn.click()
    cancelled = event_btn.inner_text().strip() == "RSVP"
    record("T04", "Filter events and RSVP/cancel",
           bool(events) and confirmed and cancelled, f"{len(events)} careers events")

    page.locator('[data-route="resources"]').first.click()
    page.locator("#resourceSearch").fill("careers")
    found = page.locator("#resourceList h3").all_text_contents()
    record("T05", "Search resources by keyword",
           any("career" in title.lower() for title in found), str(found))

    page.locator('[data-route="plan"]').first.click()
    page.locator("#taskList input[type=checkbox]").nth(1).check()
    progress = page.locator("#progressPill").inner_text()
    page.locator("#newTask").fill("QA temporary task")
    page.locator("#taskForm button[type=submit]").click()
    added = page.get_by_text("QA temporary task").count() == 1
    page.get_by_role("button", name="Delete QA temporary task").click()
    deleted = page.get_by_text("QA temporary task").count() == 0
    record("T06", "Complete, add and delete tasks",
           "2 of 3 complete" in progress and added and deleted, progress)

    page.locator("#accessibilityBtn").click()
    dialog = page.locator("#accessibilityDialog")
    opened = dialog.evaluate("(el) => el.open")
    for selector, css_class in [
        ("#largeTextToggle", "large-text"),
        ("#contrastToggle", "high-contrast"),
        ("#motionToggle", "reduced-motion")
    ]:
        page.locator(selector).check()
        if not page.locator("body").evaluate("(el, name) => el.classList.contains(name)", css_class):
            errors.append(f"Preference class did not toggle: {css_class}")
    page.locator("#closeAccessibility").click()
    record("T07", "Accessibility settings dialog and preferences",
           opened and not dialog.evaluate("(el) => el.open") and not errors,
           "Open/close and preference toggles")

    page.locator('[data-route="home"]').first.click()
    page.get_by_role("button", name="Overwhelmed").click()
    mood = "Overwhelmed" in page.locator("#moodStatus").inner_text()
    keyboard = browser.new_page()
    keyboard.set_content(source)
    keyboard.keyboard.press("Tab")
    skip = keyboard.evaluate("document.activeElement.classList.contains('skip-link') and getComputedStyle(document.activeElement).top === '12px'")
    record("T08", "Emotional check-in and keyboard skip link", mood and skip,
           f"mood={mood}; skip link={skip}")
    keyboard.close()

    mobile = browser.new_page(viewport={"width": 390, "height": 844}, is_mobile=True, has_touch=True)
    mobile.on("pageerror", lambda exc: errors.append(str(exc)))
    mobile.set_content(source)
    mobile.locator(".nav-toggle").click()
    menu = mobile.locator("#primary-nav").evaluate("(el) => el.classList.contains('open')")
    mobile.locator('[data-route="events"]').first.click()
    route = mobile.locator("#eventsRoute").is_visible()
    overflow = mobile.evaluate("document.documentElement.scrollWidth > window.innerWidth")
    record("T09", "Mobile navigation and reflow", menu and route and not overflow,
           f"menu={menu}; events={route}; horizontal overflow={overflow}")
    record("T10", "No uncaught JavaScript errors", not errors,
           "; ".join(errors) if errors else "No page errors detected")
    browser.close()

(EVIDENCE / "test_results.json").write_text(json.dumps({
    "environment": "Playwright-controlled Chromium, desktop 1440x1000; mobile 390x844",
    "note": "Functional smoke tests only; not a participant usability study or WCAG audit.",
    "results": results, "page_errors": errors
}, indent=2), encoding="utf-8")
(EVIDENCE / "test_results.txt").write_text(
    "CAMPUSCONNECT PROTOTYPE - FUNCTIONAL SMOKE TESTS\n" +
    "Scope: scripted functional verification only; not a participant usability study or formal WCAG conformance audit.\n\n" +
    "\n".join(f"{r['id']} | {r['result']} | {r['test']} | {r['details']}" for r in results) + "\n",
    encoding="utf-8"
)
print(json.dumps({"results": results, "page_errors": errors}, indent=2))
