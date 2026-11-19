export type FinfinAvatarProps = {
  size?: number
  className?: string
}

export function FinfinAvatar({ size = 40, className = '' }: FinfinAvatarProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 48 48"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={`finfin-avatar-svg ${className}`}
      aria-hidden="true"
    >
      {/* Gövde / Yüz arka planı - yumuşak Financo yeşili dairesel form */}
      <circle cx="24" cy="24" r="22" fill="#E8F5F0" stroke="#17765D" strokeWidth="2.5" />

      {/* Maskotun küçük yaprak / anten tepesi */}
      <path
        d="M24 6C24 6 26 2 30 3C30 7 26 8 24 8"
        stroke="#17765D"
        strokeWidth="2.2"
        strokeLinecap="round"
        fill="#37A77F"
      />

      {/* Sevimli gözler */}
      <ellipse cx="18" cy="23" rx="2.5" ry="3.2" fill="#173A35" />
      <ellipse cx="30" cy="23" rx="2.5" ry="3.2" fill="#173A35" />

      {/* Göz ışıltıları */}
      <circle cx="17.2" cy="21.8" r="1" fill="#FFFFFF" />
      <circle cx="29.2" cy="21.8" r="1" fill="#FFFFFF" />

      {/* Sıcak yanak allıkları */}
      <circle cx="14" cy="27" r="2.4" fill="#FAD2D8" opacity="0.85" />
      <circle cx="34" cy="27" r="2.4" fill="#FAD2D8" opacity="0.85" />

      {/* Samimi, sakin gülümseme */}
      <path
        d="M20 28C21.5 30.5 26.5 30.5 28 28"
        stroke="#173A35"
        strokeWidth="2.2"
        strokeLinecap="round"
      />
    </svg>
  )
}
