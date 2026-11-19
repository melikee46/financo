import { useEffect, useRef, useState } from 'react'
import { FinfinAvatar } from './FinfinAvatar'

export type FinfinChatProps = {
  onNavigate?: (view: 'overview' | 'transactions' | 'import' | 'budgets' | 'payments' | 'cashflow' | 'settings') => void
}

export function FinfinChat({ onNavigate }: FinfinChatProps) {
  const [isOpen, setIsOpen] = useState(false)
  const closeButtonRef = useRef<HTMLButtonElement>(null)
  const launcherButtonRef = useRef<HTMLButtonElement>(null)

  // Escape tuşuna basıldığında paneli kapatma
  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape' && isOpen) {
        setIsOpen(false)
        launcherButtonRef.current?.focus()
      }
    }
    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [isOpen])

  // Panel açıldığında kapat düğmesine otomatik odaklanma
  useEffect(() => {
    if (isOpen) {
      closeButtonRef.current?.focus()
    }
  }, [isOpen])

  return (
    <div className="finfin-floating-root">
      {isOpen && (
        <section
          className="finfin-window"
          role="dialog"
          aria-label="Finfin Yardım Masası"
          aria-modal="false"
        >
          <header className="finfin-window-header">
            <div className="finfin-header-info">
              <FinfinAvatar size={32} />
              <div>
                <h3 className="finfin-title">Finfin</h3>
                <span className="finfin-subtitle">Financo Yardımcısı</span>
              </div>
            </div>
            <div className="finfin-header-actions">
              <span className="soft-chip light-chip" title="Gerçek AI/LLM bağlantısı yoktur">
                Yerel Demo
              </span>
              <button
                ref={closeButtonRef}
                type="button"
                className="finfin-close-btn"
                onClick={() => {
                  setIsOpen(false)
                  launcherButtonRef.current?.focus()
                }}
                aria-label="Sohbeti kapat"
              >
                ✕
              </button>
            </div>
          </header>

          <div className="finfin-window-body">
            <div className="finfin-disclaimer-box" role="status">
              <span className="finfin-info-icon" aria-hidden="true">
                ℹ
              </span>
              <p>
                <strong>Demo Modu:</strong> Finfin şu anda bir harici yapay zeka (LLM) modeline bağlı
                değildir. Financo’daki doğrulanmış verileri ve özellikleri tanıtan yerel bir yardım asistanıdır.
              </p>
            </div>

            {/* Maskot karşılama alanı */}
            <div className="finfin-welcome-card">
              <div className="finfin-welcome-avatar-wrap">
                <FinfinAvatar size={48} />
              </div>
              <div className="finfin-welcome-content">
                <h4>Merhaba, ben Finfin! 🌿</h4>
                <p>
                  Financo’yu kullanırken sana rehberlik etmek için buradayım. Ekstre yükleme, işlem
                  özetleri veya nakit akışı stres testleri hakkında merak ettiklerini sorabilirsin.
                </p>
              </div>
            </div>

            {/* Boş durum ve dürüst veri mesajı */}
            <div className="finfin-guide-box">
              <div className="finfin-guide-header">
                <span className="tiny-dot" aria-hidden="true" />
                <strong>Nasıl çalışırım?</strong>
              </div>
              <p>
                Ben finansal rakam veya bakiye uydurmam. Gördüğün tüm analizler doğrudan senin
                eklediğin kayıtlı verilere dayanır.
              </p>
            </div>
          </div>
        </section>
      )}

      <button
        ref={launcherButtonRef}
        type="button"
        className={`finfin-launcher-btn ${isOpen ? 'active' : ''}`}
        onClick={() => setIsOpen((prev) => !prev)}
        aria-label={isOpen ? 'Finfin yardım sohbetini kapat' : 'Finfin yardım sohbetini aç'}
        aria-expanded={isOpen}
      >
        <FinfinAvatar size={22} />
        <span className="finfin-launcher-text">Finfin</span>
      </button>
    </div>
  )
}
