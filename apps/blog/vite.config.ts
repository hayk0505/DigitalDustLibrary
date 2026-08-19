import adapter from '@sveltejs/adapter-cloudflare';
import { sveltekit } from '@sveltejs/kit/vite';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vite';

export default defineConfig({
	plugins: [
		tailwindcss(),
		sveltekit({
			compilerOptions: {
				// Force runes mode for the project, except for libraries. Can be removed in svelte 6.
				runes: ({ filename }) =>
					filename.split(/[/\\]/).includes('node_modules') ? undefined : true
			},

			// adapter-auto only supports some environments, see https://svelte.dev/docs/kit/adapter-auto for a list.
			// If your environment is not supported, or you settled on a specific environment, switch out the adapter.
			// See https://svelte.dev/docs/kit/adapters for more information about adapters.
			adapter: adapter()
		})
	],
	optimizeDeps: {
		// lucide-svelte is imported only from sidebar components, so Vite doesn't
		// see it during initial scanning and discovers it mid-session. That
		// triggers a re-optimization, which changes the dep browserHash and makes
		// already-requested `?v=<hash>` module URLs 504 — and because a failed
		// icon import takes the whole client bundle down, the page then silently
		// stops hydrating. Declaring it here pre-bundles it up front so the hash
		// stays stable for the life of the session.
		include: ['lucide-svelte']
	},
	server: {
		port: 5174
	}
});
