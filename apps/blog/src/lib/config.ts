import { PUBLIC_API_URL } from '$env/static/public';

// Points at the separate apps/admin app once it's deployed — the blog itself has no
// auth of its own (see CLAUDE.md hosting split).
export const ADMIN_URL = 'https://admin.digitaldustlibrary.com';

export const API_URL = PUBLIC_API_URL;
