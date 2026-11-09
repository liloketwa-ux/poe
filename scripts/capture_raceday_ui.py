from pathlib import Path
import os
import re
from playwright.sync_api import sync_playwright, expect

BASE = os.getenv("RACEDAY_BASE_URL", "http://127.0.0.1:5101").rstrip("/")
API = os.getenv("RACEDAY_API_URL", "http://127.0.0.1:5100").rstrip("/")
OUT = Path("docs/evidence/ui")
OUT.mkdir(parents=True, exist_ok=True)

def shot(page, filename):
    page.screenshot(path=str(OUT / filename), full_page=True, animations="disabled")
    print(f"Saved {OUT / filename}")

def open_page(page, url):
    response = page.goto(url, wait_until="domcontentloaded", timeout=60000)
    if response is not None and response.status >= 400:
        raise RuntimeError(f"{url} returned HTTP {response.status}")
    page.wait_for_timeout(600)

def login(page, email, password):
    open_page(page, f"{BASE}/Account/Login")
    page.locator('input[name="Email"]').fill(email)
    page.locator('input[name="Password"]').fill(password)
    page.locator('form button[type="submit"]').click()
    page.wait_for_url(re.compile(r".*/Dashboard(?:/Index)?(?:\?.*)?$"), timeout=30000)
    expect(page.locator("h1")).to_contain_text("dashboard", ignore_case=True)

with sync_playwright() as p:
    browser = p.chromium.launch()
    page = browser.new_page(viewport={"width": 1600, "height": 1050}, device_scale_factor=1)
    page.set_default_timeout(15000)

    # Public event catalogue and a seeded event.
    open_page(page, BASE)
    expect(page.locator("article.event-card").first).to_be_visible(timeout=30000)
    shot(page, "01-public-event-catalogue.png")
    first_event_link = page.locator('a[href^="/Events/Details/"]').first
    event_href = first_event_link.get_attribute("href")
    if not event_href:
        raise RuntimeError("No event details link was rendered on the public catalogue.")
    event_path = event_href if event_href.startswith("/") else f"/{event_href}"
    event_match = re.search(r"/Events/Details/(\d+)", event_path)
    if not event_match:
        raise RuntimeError(f"Could not read the event ID from {event_path}")
    event_id = event_match.group(1)

    open_page(page, f"{BASE}{event_path}")
    expect(page.locator("h1")).to_be_visible()
    shot(page, "02-public-event-details.png")

    # Participant pages: seeded account, entry, profile and published result.
    login(page, "participant@raceday.demo", "RaceDayDemo!2026")
    shot(page, "03-participant-dashboard-and-results.png")
    open_page(page, f"{BASE}/Profile")
    expect(page.locator("h1")).to_contain_text("profile", ignore_case=True)
    shot(page, "04-participant-profile.png")
    open_page(page, f"{BASE}{event_path}")
    expect(page.get_by_role("button", name=re.compile("Enter event", re.I)).first).to_be_visible()
    shot(page, "05-participant-event-enrolment.png")

    # Organiser pages: dashboard, event management, categories, enrolments and results.
    login(page, "organiser@raceday.demo", "RaceDayDemo!2026")
    expect(page.locator("h1")).to_contain_text("organiser dashboard", ignore_case=True)
    shot(page, "06-organiser-dashboard.png")
    open_page(page, f"{BASE}{event_path}")
    expect(page.get_by_text("ORGANISER TOOLS")).to_be_visible()
    shot(page, "07-organiser-event-and-category-management.png")
    open_page(page, f"{BASE}/Events/Create")
    expect(page.locator("form")).to_be_visible()
    shot(page, "08-organiser-create-event.png")
    open_page(page, f"{BASE}/Categories/Create?eventId={event_id}")
    expect(page.locator("form")).to_be_visible()
    shot(page, "09-organiser-create-category.png")
    open_page(page, f"{BASE}/Events/Enrolments/{event_id}")
    expect(page.locator("h1")).to_contain_text("enrolments", ignore_case=True)
    shot(page, "10-organiser-event-enrolments.png")
    open_page(page, f"{BASE}/Results/Entry?eventId={event_id}")
    expect(page.locator("h1")).to_contain_text("results", ignore_case=True)
    shot(page, "11-organiser-result-entry.png")

    # Live Swagger UI served by the running API.
    open_page(page, f"{API}/swagger")
    expect(page.locator("body")).to_contain_text("RaceDay REST API", timeout=30000)
    shot(page, "12-live-swagger-api.png")

    browser.close()
print("RaceDay live UI evidence capture completed.")
