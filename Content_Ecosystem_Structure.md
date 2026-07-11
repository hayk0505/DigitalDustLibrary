# Content Ecosystem — Structure & Naming

Living plan for how LinkedIn, the blog, Glitch (YouTube), and the personal site fit together. Not locked — identities/attribution can be linked or separated later without rebuilding anything, as long as we keep them structurally independent now.

## The pieces

### 1. LinkedIn — Hayk Baroyan (personal brand)
- Short posts, high frequency, discovery layer.
- Three content pillars: tech, social/psychological, software development.
- Purpose: drives traffic into the blog; no long-form content lives here.

### 2. Digital Dust Library — the blog (long-form)
- Domain: `digitaldustlibrary.com` (confirmed available).
- Hosting: self-hosted on the existing DigitalOcean droplet, as its own Caddy site block/container — consistent with how EU Deepfake Toolkit and other projects are served (see haykbaroyan.com hosting project).
- Same three pillars as LinkedIn (tech / social-psych / software dev), just deep-dive length.
- Role in the pipeline: source material for Glitch video scripts ("did you know," "story of," "news" formats).

### 3. Glitch — YouTube channel
- Pulls research/writing from Digital Dust Library rather than starting from scratch per video.
- Attribution question open: whether the channel is presented as Ryan Kobary's project or kept unattributed. Not urgent — decide later.

### 4. Ryan Kobary — author identity (separate, own domain)
- Pen name for the Glitch fiction/world-building universe (telepathic races, retro-digital-mystery setting — currently in the `Лор` and `Chapters` folders).
- Domain: `ryankobary.com` or similar — needs an availability check, kept separate from `haykbaroyan.com`.
- Deliberately decoupled from the "Hayk Baroyan, engineer" identity. Whether to publicly link the two (e.g., mention on CV) is an open decision, not a structural one — keeping them on separate domains/sites preserves the option either way.

### 5. haykbaroyan.com — CV + projects hub
- Contains: CV, and a Projects section listing:
  - EU Deepfake Toolkit
  - Digital Dust Library (linked out)
  - *(Ryan Kobary / Glitch — added here only if/when the identity link is made public; omitted by default)*

## Hosting/routing summary
- One droplet, Docker Compose, Caddy as reverse proxy (per existing haykbaroyan.com hosting plan).
- Each project/domain gets its own Compose stack + Caddy site block: `haykbaroyan.com`, `digitaldustlibrary.com`, and (separately, possibly not on the same droplet given the identity separation) `ryankobary.com`.
- Cloudflare stays in front for DNS on all domains.

## Open questions (deliberately deferred)
- Whether Ryan Kobary is ever mentioned on the Hayk Baroyan CV/site.
- Whether the Glitch YouTube channel attributes itself to Ryan Kobary.
- Final domain for the Ryan Kobary author site (availability check needed).
- Whether `ryankobary.com` should share the droplet or be hosted separately, given the deliberate identity separation (shared infra is a minor leak risk — e.g., shared IP/hosting fingerprint — worth weighing if separation matters a lot).
