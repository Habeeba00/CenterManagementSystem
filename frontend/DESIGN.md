---
name: Academic Core System
colors:
  surface: '#f8f9ff'
  surface-dim: '#cbdbf5'
  surface-bright: '#f8f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#eff4ff'
  surface-container: '#e5eeff'
  surface-container-high: '#dce9ff'
  surface-container-highest: '#d3e4fe'
  on-surface: '#0b1c30'
  on-surface-variant: '#434655'
  inverse-surface: '#213145'
  inverse-on-surface: '#eaf1ff'
  outline: '#737686'
  outline-variant: '#c3c6d7'
  surface-tint: '#0053db'
  primary: '#004ac6'
  on-primary: '#ffffff'
  primary-container: '#2563eb'
  on-primary-container: '#eeefff'
  inverse-primary: '#b4c5ff'
  secondary: '#545f73'
  on-secondary: '#ffffff'
  secondary-container: '#d5e0f8'
  on-secondary-container: '#586377'
  tertiary: '#943700'
  on-tertiary: '#ffffff'
  tertiary-container: '#bc4800'
  on-tertiary-container: '#ffede6'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dbe1ff'
  primary-fixed-dim: '#b4c5ff'
  on-primary-fixed: '#00174b'
  on-primary-fixed-variant: '#003ea8'
  secondary-fixed: '#d8e3fb'
  secondary-fixed-dim: '#bcc7de'
  on-secondary-fixed: '#111c2d'
  on-secondary-fixed-variant: '#3c475a'
  tertiary-fixed: '#ffdbcd'
  tertiary-fixed-dim: '#ffb596'
  on-tertiary-fixed: '#360f00'
  on-tertiary-fixed-variant: '#7d2d00'
  background: '#f8f9ff'
  on-background: '#0b1c30'
  surface-variant: '#d3e4fe'
typography:
  display:
    fontFamily: Hanken Grotesk
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Hanken Grotesk
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.01em
  headline-lg-mobile:
    fontFamily: Hanken Grotesk
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  headline-md:
    fontFamily: Hanken Grotesk
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  headline-sm:
    fontFamily: Hanken Grotesk
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  label-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 20px
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  sidebar-width: 280px
  container-max: 1440px
  gutter: 24px
  margin-mobile: 16px
  stack-sm: 8px
  stack-md: 16px
  stack-lg: 32px
---

## Brand & Style

This design system is engineered for high-stakes educational administration, where clarity and reliability are paramount. The aesthetic is **Corporate / Modern**, prioritizing a high-fidelity enterprise feel that balances a sophisticated dark navigation environment with a clinical, high-focus light workspace. 

The brand personality is **Reliable, Organized, and Efficient**. It avoids decorative flourishes in favor of "functional elegance"—using precise alignment, generous whitespace, and a structured information hierarchy to make complex data sets (like financial tracking and student records) feel manageable and intuitive.

**Target Audience:**
- **Administrators & Owners:** Seeking high-level insights and financial clarity.
- **Receptionists:** Requiring speed and accuracy in scheduling and student check-ins.
- **Teachers:** Needing a distraction-free environment for progress tracking.

## Colors

The palette utilizes a "Deep Navy Sidebar" strategy to anchor the interface and provide a professional, executive feel.

- **Primary Blue (#2563eb):** Used for primary actions, active states, and focus indicators. It represents intelligence and stability.
- **Surface Palette:** The content area uses a light-gray wash (#f8fafc) for the background to reduce eye strain, while cards and containers use pure white (#ffffff) to pop.
- **Sidebar (Secondary):** Deep Navy (#1e293b) provides high contrast against the content area, effectively separating navigation from execution.
- **Semantic Logic:** Success, Error, and Warning colors are strictly reserved for status indicators, badges, and feedback loops to ensure they carry maximum signal strength.

## Typography

The system uses a dual-font approach to distinguish between "Information Architecture" and "User Data."

- **Hanken Grotesk (Headlines):** Used for page titles, section headers, and dashboard metrics. Its sharp, contemporary geometry adds a premium "SaaS" feel.
- **Inter (Body & UI):** Used for all data-heavy elements, including tables, forms, and labels. Inter’s high x-height and neutral personality ensure maximum readability for long lists of student names and financial figures.

**Hierarchy Rules:**
- Use `label-sm` with 0.05em letter spacing and uppercase styling for sidebar category headers and table column headers.
- Use `body-sm` for secondary data in tables (e.g., student IDs, timestamps).

## Layout & Spacing

The layout follows a **Fixed-Fluid hybrid model**:
- **Sidebar:** Fixed at 280px. It remains persistent on desktop to allow quick switching between schedules, finances, and student records.
- **Main Content:** A fluid 12-column grid with a max-width of 1440px. 
- **Rhythm:** An 8px baseline grid governs all spacing. Vertical stacks within cards should use 16px (`stack-md`), while the gap between dashboard widgets should use 24px (`gutter`).

**Adaptation:**
- **Tablet:** The sidebar collapses into an icon-only rail (72px) or a hidden drawer.
- **Mobile:** Content stacks vertically. Margins reduce to 16px. Tables transition into "Data Cards" to prevent horizontal scrolling of critical student info.

## Elevation & Depth

Visual hierarchy is established through **Ambient Shadows** and **Tonal Layering**.

- **Level 0 (Background):** #f8fafc (Light blue-grey).
- **Level 1 (Cards/Tables):** Pure white background with a subtle shadow: `0px 1px 3px rgba(0,0,0,0.1), 0px 10px 15px -3px rgba(30,41,59,0.05)`. The shadow is slightly tinted with the secondary navy color to feel integrated.
- **Level 2 (Modals/Popovers):** Higher elevation with a more pronounced shadow to indicate temporary focus.
- **Level 3 (Sidebar):** Flat, dark surface (#1e293b). Depth is communicated here through "In-set" active states—active nav items use a subtle light blue glow or a left-side primary accent border.

## Shapes

The design system uses "Soft-Premium" rounding to balance approachability with professional rigor.

- **Base Components:** 0.5rem (8px) for buttons and input fields.
- **Containers & Cards:** 1rem (16px) for dashboard cards and data table wrappers. This larger radius creates a modern, high-fidelity look.
- **Status Badges:** Fully rounded (pill-shaped) to distinguish them from interactive buttons.
- **Interactive States:** Hovering over table rows or list items should reveal a 0.5rem rounded highlight background.

## Components

### Buttons & Inputs
- **Primary Button:** Solid #2563eb with white text. 8px corner radius. Subtle scale-down effect (0.98) on click.
- **Inputs:** 1px border (#e2e8f0). On focus, the border changes to Primary Blue with a 3px soft focus-ring.

### Data Tables
- **Header:** Light grey background (#f1f5f9) with `label-sm` text.
- **Rows:** White background with a 1px bottom border. Hover state: #f8fafc highlight.
- **Cell Content:** Use `body-sm` for density. Financial columns should be monospaced or right-aligned for easy comparison.

### Status Badges
- Small, pill-shaped components with a 10% opacity background of the semantic color and 100% opacity text of the same color (e.g., "Paid" has a light green background with dark green text).

### Dashboard Cards
- Must include a `headline-sm` title.
- Should utilize "Empty States" with muted illustrations when data is unavailable, maintaining the 16px corner radius.

### Sidebar Navigation
- Icons: 20px, stroke-based (2px weight), using #94a3b8 color when inactive.
- Active state: White icon and text, with a background highlight of rgba(255,255,255,0.1).