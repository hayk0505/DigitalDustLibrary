/** @type {import("prettier").Config} */
const config = {
	useTabs: true,
	singleQuote: true,
	trailingComma: 'none',
	printWidth: 100,
	// Windows checkouts with core.autocrlf=true write CRLF to disk; without this,
	// Prettier's LF-only default flags every file as a formatting violation.
	endOfLine: 'auto',
	plugins: ['prettier-plugin-svelte'],
	overrides: [{ files: '*.svelte', options: { parser: 'svelte' } }]
};

export default config;
