export function Logo({ className }: { className?: string }) {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 100 100"
      fill="none"
      role="img"
      aria-label="Digital Dust Library"
      className={className}
    >
      <path
        d="M 94 40 V 80 A 14 14 0 0 1 80 94 H 20 A 14 14 0 0 1 6 80 V 20 A 14 14 0 0 1 20 6 H 60"
        stroke="currentColor"
        strokeWidth="5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />

      <rect x="22" y="32" width="13" height="46" rx="4" stroke="currentColor" strokeWidth="5" strokeLinejoin="round" />
      <rect x="38" y="40" width="13" height="38" rx="4" stroke="currentColor" strokeWidth="5" strokeLinejoin="round" />
      <rect x="54" y="50" width="12" height="28" rx="4" stroke="currentColor" strokeWidth="5" strokeLinejoin="round" />

      <path d="M25 70 H32 M41 70 H48 M57 70 H63" stroke="currentColor" strokeWidth="4" strokeLinecap="round" />

      <rect x="59" y="40" width="6" height="6" rx="1.2" stroke="currentColor" strokeWidth="2.4" />
      <rect x="66" y="42" width="5" height="5" rx="1" fill="currentColor" />
      <rect x="64" y="31" width="5" height="5" rx="1" stroke="currentColor" strokeWidth="2.2" />
      <rect x="73" y="35" width="5" height="5" rx="1" fill="currentColor" />
      <rect x="70" y="25" width="4" height="4" rx="1" stroke="currentColor" strokeWidth="2" />
      <rect x="79" y="29" width="4" height="4" rx="1" fill="currentColor" />
      <rect x="76" y="19" width="4" height="4" rx="1" stroke="currentColor" strokeWidth="2" />
      <rect x="85" y="23" width="3.4" height="3.4" rx="0.9" fill="currentColor" />
      <rect x="82" y="14" width="3.4" height="3.4" rx="0.9" stroke="currentColor" strokeWidth="1.8" />
      <rect x="89" y="17" width="3" height="3" rx="0.8" fill="currentColor" />
      <rect x="74" y="12" width="3" height="3" rx="0.8" fill="currentColor" />
      <rect x="90" y="10" width="2.4" height="2.4" rx="0.7" stroke="currentColor" strokeWidth="1.6" />
      <rect x="80" y="8" width="2.4" height="2.4" rx="0.7" fill="currentColor" />
    </svg>
  )
}
