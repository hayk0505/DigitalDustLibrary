# Digital Dust Library — Project Memory

This file gives context for anyone (including a future Claude session) picking up this
repo cold. Read this before making structural changes.

## What this is

Digital Dust Library is Hayk Baroyan's long-form blog. Domain: `digitaldustlibrary.com`
(confirmed available, not yet live). It's one piece of a wider content ecosystem — this
repo holds only the blog itself, not the other pieces.

Three content pillars, all deep-dive length:
- Tech
- Social / psychological
- Software development

Role in the pipeline: source material for Glitch (YouTube) video scripts. Short posts on
LinkedIn drive traffic in; long-form lives here; Glitch pulls research/writing from here
rather than starting from scratch per video.

## Identity separation — read this before adding content

This is the most important structural rule in the whole ecosystem, so it's worth stating
plainly: **Digital Dust Library / Hayk Baroyan is deliberately kept separate from the
Ryan Kobary / Glitch fiction universe.**

- Hayk Baroyan = real name, engineer identity, CV/projects site (`haykbaroyan.com`),
  LinkedIn, and this blog.
- Ryan Kobary = pen name for the Glitch fiction/world-building universe (telepathic
  races, retro-digital-mystery setting). Own domain (`ryankobary.com` or similar, TBD).
  Lives in the parent `Digital Dust Library` folder under `Лор`, `Char`, and related
  material — **not** in this repo.

Whether the two identities are ever publicly linked is an open decision (see below), not
a structural one — keeping them on separate domains/repos preserves the option either
way. **Default: do not mix Ryan Kobary / Glitch fiction material into this repo.**

## What lives in this repo vs. the parent folder

This repo (`DigitalDustLibrary`) = blog only:
- `Content_Ecosystem_Structure.md` — the source-of-truth doc for how all the pieces
  (LinkedIn, blog, Glitch, Ryan Kobary, haykbaroyan.com) fit together.
- `Chapters/` — blog article drafts, one subfolder per article/topic (e.g.
  `Childfree`, `History Of AI`, `Lying flat`, `Year 1984`), each containing a `.docx`
  draft plus any reference images.

Stays in the parent `Digital Dust Library` folder, not this repo (Glitch/Ryan Kobary
material and unrelated assets):
- `Лор` — Glitch lore/world-building docs.
- `Char` — character reference images for Glitch.
- `Logo`, `Glitch_web` — Glitch branding/site assets.
- `games.txt`, `headings.txt`, `structure.txt`, `Фразы-Глитча.txt` — Glitch video
  production notes (background game footage, video format lengths, script phrases).

If new content is added to the parent folder, ask which side of the identity split it
belongs to before deciding whether it should be copied into this repo.

## Conventions

- Article drafts are Word docs (`.docx`) inside their own subfolder per topic.
- Office lock/temp files (`~$*.docx`, `~WRL*.tmp`) are gitignored — never commit these.
- No fixed naming scheme enforced yet beyond "one folder per article topic" — adopt this
  loosely until/unless it becomes a problem.

## Hosting / deployment

- Self-hosted on the existing DigitalOcean droplet (see the `haykbaroyan-hosting`
  project memory for full droplet/Caddy/Docker Compose conventions).
- Runs as its own Caddy site block/container alongside `haykbaroyan.com` and other
  projects on the same droplet — consistent pattern, not a one-off.
- Cloudflare in front for DNS, same as the other domains.
- Not yet deployed as of this writing — domain is reserved, site isn't live.

## Open decisions (deliberately deferred, don't resolve unilaterally)

- Whether Ryan Kobary is ever mentioned on the Hayk Baroyan CV/site.
- Whether the Glitch YouTube channel publicly attributes itself to Ryan Kobary.
- Final domain for the Ryan Kobary author site (availability check still needed).
- Whether `ryankobary.com` shares the droplet with the other sites or gets hosted
  separately — shared infra is a minor fingerprinting/leak risk given the deliberate
  identity separation, worth weighing if separation matters a lot.

## GitHub account note

Repo is intended to live under a single consolidated GitHub account (in the process of
merging `hayk0505` and `hayk-baroyan` into one — see chat history / commit author
config for current state). Don't be surprised if commit authorship looks inconsistent
across early commits while that's being sorted out.
