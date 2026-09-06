<script lang="ts">
	import { page } from '$app/state';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
</script>

<SeoHead
	title="Privacy Policy"
	description="How Digital Dust Library collects, uses, and protects visitor and contributor information."
	url={page.url.href}
/>

<div class="article-body prose prose-neutral max-w-none">
	<h1 class="font-display">Privacy Policy — Digital Dust Library</h1>
	<p class="text-sm text-ink/65">Last updated: September 2, 2026</p>

	<h2>What this covers</h2>
	<p>
		This policy explains what information Digital Dust Library (digitaldustlibrary.com, the
		"Site") collects and how it's used. It applies to visitors, and separately to
		contributor/author accounts.
	</p>

	<h2>Information we collect</h2>
	<p>
		<strong>If you apply to become an author:</strong> name, email address, and whatever you write
		in your pitch. Stored to process the application; if approved, your name and email become part
		of your author account.
	</p>
	<p>
		<strong>If you're a reader:</strong> we don't require an account or collect personal information
		just to read the Site. Basic, aggregate analytics are collected via Cloudflare Web Analytics
		(see below).
	</p>
	<p>
		<strong>If you're a logged-in contributor:</strong> your name, email, and the content you author,
		stored to run the editorial workflow (drafts, review, and publishing).
	</p>

	<h2>Analytics and tracking</h2>
	<p>
		The Site uses <strong>Cloudflare Web Analytics</strong>, deployed via <strong
			>Google Tag Manager (GTM)</strong
		>. Cloudflare Web Analytics is cookieless by design — it doesn't use tracking cookies or
		collect personally identifiable information, and it doesn't track you across other websites.
		It measures aggregate traffic (page views, load performance) only.
	</p>
	<p>
		GTM itself is a tag container — right now it only loads the Cloudflare Analytics beacon
		described above. If additional tags are ever added through GTM in the future that do use
		cookies or collect personal data (e.g., advertising or more detailed analytics tools), this
		policy needs to be updated first, and a cookie-consent mechanism may become necessary at that
		point since the cookieless status described here would no longer hold.
	</p>

	<h2>Cookies</h2>
	<p>
		Cookies are small text files stored on your device. Cloudflare Web Analytics, as deployed on
		this Site, does not use tracking cookies — see "Analytics and tracking" above. If that changes
		in the future, this section will be updated to explain what's set and how to control it
		through your browser settings.
	</p>

	<h2>Third-party services</h2>
	<ul>
		<li><strong>Cloudflare</strong> — hosting, DNS, and the analytics described above.</li>
		<li>
			<strong>Brevo</strong> — sends transactional email (e.g., author-application notifications,
			invite emails). Recipient email addresses are shared with Brevo solely to send these emails.
		</li>
	</ul>

	<h2>International data transfers</h2>
	<p>
		Some of the service providers above may process data on servers located outside Armenia,
		including in the United States and the European Union.
	</p>

	<h2>How we use information</h2>
	<ul>
		<li>To process author applications and run contributor accounts.</li>
		<li>To understand aggregate site traffic and improve the Site.</li>
		<li>We don't sell personal information, and we don't use it for advertising.</li>
	</ul>

	<h2>Data retention</h2>
	<p>
		Author application data is retained for as long as your application is under review. If your
		application isn't approved, or if an approved contributor account later becomes inactive,
		related personal data is deleted or anonymized within 3 years of your last interaction with
		the Site — sooner on request (see "Your rights" below). Active contributor accounts retain the
		data needed to run the account for as long as the account stays active.
	</p>

	<h2>Your rights</h2>
	<p>
		If you're in the EU/EEA (or another jurisdiction with similar protections), you may have
		rights to access, correct, or delete your personal data, and to object to certain processing.
		Contact us using the details below to make a request.
	</p>

	<h2>Changes to this policy</h2>
	<p>
		This policy may be updated as the Site's tracking or data practices change. Material changes
		will be reflected here with an updated date.
	</p>

	<h2>Contact</h2>
	<p>
		Questions about this policy or a data request: <a href="mailto:haykbaroyan@yahoo.com"
			>haykbaroyan@yahoo.com</a
		>.
	</p>
</div>
