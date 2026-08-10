# Deployment

How `apps/api` + `apps/admin` get onto the DigitalOcean droplet, and how
`apps/blog` gets onto Cloudflare. Read this once before the first real
deploy — after initial setup, shipping changes is just `git push`.

No droplet exists yet as of this writing. This doc is both the one-time
setup checklist and the ongoing reference for how the pipeline works.

## Topology

```
digitaldustlibrary.com          -> Cloudflare Workers (apps/blog, SvelteKit SSR)
api.digitaldustlibrary.com      -> droplet, Caddy -> api container
admin.digitaldustlibrary.com    -> droplet, Caddy -> static admin build (/)
                                                    -> api container (/api/*)
```

Root domain never touches the droplet — that's Cloudflare's job, deployed
separately (`apps/blog`, `wrangler deploy`, not covered here).

`admin` and `api` sit behind the *same* Caddy host deliberately — the
admin panel's auth uses an httpOnly refresh cookie
(`Admin_Panel_Build_Spec.md`), which needs same-origin requests to avoid
cross-site cookie restrictions. The public blog calls `api.*` cross-origin
instead, which is why `Program.cs` has real CORS config
(`Cors:AllowedOrigins`) rather than `AllowAnyOrigin`.

Caddy itself is **not** part of this repo. It's shared, always-on infra for
the droplet as a whole rather than something owned by this project — kept
that way so it stays reusable if anything else ever ends up on this box,
without that being a current plan or dependency. It lives in its own
directory on the droplet, outside any single project's repo.

## One-time droplet setup

### 1. Create the droplet

**2 GB RAM / 1 vCPU** (~$12/mo) — deliberately staying at the baseline rather
than sizing up front for the database-backed workload, even though that
cuts against the existing droplet-sizing guidance ("size up before adding a
DB-backed app, not after hitting problems"). Worth knowing the actual risk
this accepts: not gradual slowdown, but an OOM kill if Postgres + the API +
Caddy all spike memory at once. DigitalOcean resizes are non-destructive
(resize + reboot, no rebuild), so this isn't a permanent bet — just start
cheap and upgrade if `docker stats` / `free -h` show real pressure, not
before. Ubuntu 24.04 LTS, region your call (closest to you or your expected
readers).

This droplet is dedicated to digitaldustlibrary right now — no other project
is confirmed to land on it, haykbaroyan.com included; that was floated as a
possible future direction elsewhere but isn't a plan this doc should assume.
If something else ever does join later, it'd follow the same pattern (its
own isolated Compose stack, its own Caddy site block), and that's the point
to revisit the 2 GB sizing — not before.

A cheap mitigation worth doing regardless of size — a swap file gives the
kernel somewhere to fall back to under a brief memory spike instead of
immediately OOM-killing a container. Doesn't fix sustained memory pressure,
but softens short bursts:

```bash
fallocate -l 2G /swapfile
chmod 600 /swapfile
mkswap /swapfile
swapon /swapfile
echo '/swapfile none swap sw 0 0' >> /etc/fstab
```

### 2. Basic hardening

SSH in as root once, then:

```bash
adduser deploy
usermod -aG sudo deploy
# copy your own SSH public key into /home/deploy/.ssh/authorized_keys so you
# can still get in as `deploy` afterward

# then, in /etc/ssh/sshd_config:
#   PermitRootLogin no
#   PasswordAuthentication no
systemctl restart sshd

ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw enable
```

### 3. Install Docker

```bash
curl -fsSL https://get.docker.com | sh
usermod -aG docker deploy
```

Worth being honest about a tradeoff here: adding `deploy` to the `docker`
group makes it root-equivalent (anyone who can talk to the Docker socket can
mount the host filesystem into a container and read/write anything). For a
solo-maintained personal project this is a reasonable simplification —
tightening it later (rootless Docker, or sudo rules scoped to just
`docker compose` on specific directories) is a good follow-up once more than
one person has any access to this box, per the co-hosting scenario already
flagged as a future possibility.

### 4. Shared Caddy stack

On the droplet, outside this repo (e.g. `~/caddy/`):

```yaml
# ~/caddy/docker-compose.yml
services:
  caddy:
    image: caddy:2-alpine
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./Caddyfile:/etc/caddyfile:ro
      - caddy_data:/data
      - caddy_config:/config
      - /home/deploy/digitaldustlibrary/admin-static:/srv/admin-static:ro
      # Absolute path, not `~` — Compose doesn't expand `~` inside a YAML
      # volume path (no shell involved to do that expansion), unlike the
      # shell commands elsewhere in this doc where `~` works fine.
    command: caddy run --config /etc/caddyfile
    networks:
      - caddy_net

networks:
  caddy_net:
    name: caddy_net

volumes:
  caddy_data:
  caddy_config:
```

```
# ~/caddy/Caddyfile
api.digitaldustlibrary.com {
    reverse_proxy api:8080
}

admin.digitaldustlibrary.com {
    handle /api/* {
        reverse_proxy api:8080
    }
    handle {
        root * /srv/admin-static
        try_files {path} /index.html
        # SPA fallback — client-side routing (TanStack Router) needs unknown
        # paths to still resolve to index.html, not a Caddy 404. Caddy applies
        # its directives in a fixed internal order regardless of how they're
        # written here, but try_files-before-file_server is the conventional
        # ordering and matches Caddy's own docs.
        file_server
    }
}
```

`api:8080` resolves because both this Caddy container and the
`digitaldustlibrary` project's `api` container join the same external
`caddy_net` network (see `docker-compose.prod.yml` in this repo). Caddy
issues and renews its own Let's Encrypt certs automatically for both
hostnames — no manual cert handling needed.

Bring it up:

```bash
cd ~/caddy && docker network create caddy_net && docker compose up -d
```

If another project ever joins this droplet, its site block gets added to
this same Caddyfile — that'd be the one place that grows over time,
everything else staying isolated per project. Not a current plan, just how
it'd slot in if it happened.

### 5. Deploy-only SSH key

From your own machine, not the droplet:

```bash
ssh-keygen -t ed25519 -f digitaldustlibrary_deploy -C "digitaldustlibrary-ci" -N ""
```

Append the `.pub` file's contents to `/home/deploy/.ssh/authorized_keys` on
the droplet. Keep the private key off the droplet entirely — it only ever
lives in GitHub's encrypted secrets, and GitHub Actions is the only thing
that ever uses it. This is what "no standing SSH" means in practice: nobody
holds an interactive session open to this box just to ship a change; the
credential only exists for the seconds a deploy takes, invoked by CI.

### 6. `.env` on the droplet

```bash
mkdir -p ~/digitaldustlibrary && cd ~/digitaldustlibrary
nano .env
```

```
POSTGRES_PASSWORD=<generate a real one, e.g. openssl rand -base64 24>
CONNECTIONSTRINGS__DEFAULT=Host=postgres;Port=5432;Database=digitaldustlibrary;Username=digitaldustlibrary;Password=<same password as above>
JWT__SIGNINGKEY=<generate one, e.g. openssl rand -base64 48>
RESEND__APIKEY=<from your Resend dashboard>
CORS__ALLOWEDORIGINS__0=https://digitaldustlibrary.com
CORS__ALLOWEDORIGINS__1=https://admin.digitaldustlibrary.com
GHCR_OWNER=<your github username/org, lowercase>
```

This file is never committed and never touches CI — it's the one thing
that's set up by hand, once, directly on the droplet. Everything else is
automated.

### 7. GitHub repo secrets

Settings → Secrets and variables → Actions, add:

| Secret | Value |
|---|---|
| `DROPLET_HOST` | the droplet's public IP |
| `DEPLOY_USER` | `deploy` |
| `DEPLOY_SSH_KEY` | contents of `digitaldustlibrary_deploy` (the private key, not `.pub`) |

`GITHUB_TOKEN` for pushing to ghcr.io is automatic — nothing to add there.

### 8. DNS on Cloudflare

Add two A records, both **DNS only** (grey cloud, not proxied):

```
api.digitaldustlibrary.com    A    <droplet IP>
admin.digitaldustlibrary.com  A    <droplet IP>
```

DNS-only matters here specifically: Caddy issues its own TLS certs via
Let's Encrypt's HTTP-01 challenge, which needs to reach the droplet
directly. If Cloudflare's proxy (orange cloud) sits in front, it terminates
TLS at Cloudflare's edge instead and the challenge breaks — fixable with a
DNS-01 challenge and a Cloudflare API token, but that's extra setup this
doesn't need yet. The root domain (`digitaldustlibrary.com`, pointed at
Cloudflare Workers for the blog) is unaffected by this — that's handled
separately when `apps/blog` is actually deployed.

## Ongoing: how a deploy actually works

Push to `main` (or trigger manually from the Actions tab) and
`.github/workflows/deploy.yml` does, in order:

1. Builds `apps/api`'s production image (`Dockerfile.prod`) and pushes it to
   `ghcr.io` — tagged both `:latest` and `:<commit-sha>`.
2. Builds `apps/admin`'s static bundle.
3. Copies the compose file and the new admin build to the droplet.
4. Swaps the admin build into place, pulls the new API image, runs the EF
   Core migrations bundle (baked into the image itself, reads its connection
   string from the same env the API container gets), then restarts.

First deploy will fail if `Migrations/` doesn't exist in the repo yet or if
`apps/api` doesn't build — those are prerequisites independent of this
pipeline (see `apps/api/README.md`).

## Rollback

If a deploy breaks something, on the droplet:

```bash
cd ~/digitaldustlibrary
docker compose -f docker-compose.prod.yml pull  # no-op, just confirms current state
docker tag ghcr.io/<owner>/digitaldustlibrary-api:<known-good-sha> ghcr.io/<owner>/digitaldustlibrary-api:latest
docker compose -f docker-compose.prod.yml up -d
```

Admin's previous static build isn't kept automatically — if a bad admin
build ships, the fix is reverting the commit and letting CI redeploy, not a
manual swap. The deploy script updates `admin-static/` in place (clears and
repopulates the same directory) rather than swapping directories, since
Caddy has it bind-mounted and a Linux bind mount doesn't follow a renamed-
away directory — an earlier version of this script did an mv-based swap and
silently broke admin's routing this way (files existed on disk, Caddy kept
serving the stale pre-swap directory regardless). Worth knowing if `admin.*`
ever 404s despite `admin-static/` looking correct on disk: check whether
this in-place-update approach got changed back to a swap.
