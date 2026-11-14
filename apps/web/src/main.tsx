import { StrictMode, useMemo, useState } from 'react'
import { createRoot } from 'react-dom/client'
import './styles.css'

type View = 'overview' | 'import' | 'transactions'

type PreviewRow = {
  rowNumber: number
  isValid: boolean
  transactionDate: string | null
  description: string | null
  amount: number | null
  currencyCode: string
  validationErrors: string[]
}

type PreviewResponse = {
  importJobId: string
  fileName: string
  totalRows: number
  validRows: number
  invalidRows: number
  rows: PreviewRow[]
}

type ConfirmResponse = {
  importJobId: string
  status: string
  totalRows: number
  validRows: number
  invalidRows: number
  createdTransactions: number
  alreadyConfirmed: boolean
}

const navItems: { id: View; label: string; icon: string }[] = [
  { id: 'overview', label: 'Genel Bakış', icon: '◼' },
  { id: 'import', label: 'Ekstre ekle', icon: '＋' },
  { id: 'transactions', label: 'İşlemler', icon: '≡' },
]

function App() {
  const [view, setView] = useState<View>('overview')
  const [preview, setPreview] = useState<PreviewResponse | null>(null)
  const [confirmation, setConfirmation] = useState<ConfirmResponse | null>(null)

  return (
    <div className="app-shell">
      <Sidebar activeView={view} onNavigate={setView} />
      <div className="main-column">
        <header className="topbar">
          <div><p className="topbar-kicker">Kişisel finans görünümü</p><h1>Denge</h1></div>
          <div className="topbar-actions"><span className="period-label">Kasım 2026</span><button className="primary-button compact" type="button" onClick={() => setView('import')}>＋ Ekstre ekle</button></div>
        </header>
        <main className="content">
          {view === 'overview' && <Overview preview={preview} onImport={() => setView('import')} />}
          {view === 'import' && <ImportView preview={preview} confirmation={confirmation} onPreview={(nextPreview) => { setPreview(nextPreview); setConfirmation(null) }} onConfirmed={setConfirmation} />}
          {view === 'transactions' && <TransactionsView preview={preview} confirmation={confirmation} onImport={() => setView('import')} />}
        </main>
      </div>
      <nav className="mobile-nav" aria-label="Ana menü">{navItems.map((item) => <NavButton key={item.id} item={item} active={view === item.id} onClick={() => setView(item.id)} />)}</nav>
    </div>
  )
}

function Sidebar({ activeView, onNavigate }: { activeView: View; onNavigate: (view: View) => void }) {
  return <aside className="sidebar">
    <div className="brand"><span className="brand-mark">D</span><span>Denge</span></div>
    <p className="sidebar-label">Çalışma alanı</p>
    <nav aria-label="Ana menü" className="desktop-nav">{navItems.map((item) => <NavButton key={item.id} item={item} active={activeView === item.id} onClick={() => onNavigate(item.id)} />)}</nav>
    <div className="sidebar-footer"><span className="tiny-dot" aria-hidden="true" /> <span>Yerel demo alanı</span></div>
  </aside>
}

function NavButton({ item, active, onClick }: { item: (typeof navItems)[number]; active: boolean; onClick: () => void }) {
  return <button className={`nav-button ${active ? 'active' : ''}`} type="button" onClick={onClick} aria-current={active ? 'page' : undefined}><span className="nav-icon" aria-hidden="true">{item.icon}</span><span>{item.label}</span></button>
}

function Overview({ preview, onImport }: { preview: PreviewResponse | null; onImport: () => void }) {
  const hasData = Boolean(preview?.validRows)
  return <>
    <div className="page-heading"><div><p className="section-kicker">Genel bakış</p><h2>Paranla aran nasıl?</h2><p className="muted">Verine dayalı sakin ve anlaşılır bir finans görünümü.</p></div><span className="data-badge"><span className="tiny-dot" /> {hasData ? 'Önizleme verisi' : 'Veri bekleniyor'}</span></div>
    <section className="hero-grid">
      <article className="hero-card"><div className="card-heading"><span className="section-kicker">Bugünkü görünüm</span><span className="info-mark" title="Başlangıç bakiyesi ve işlem verisi eklendiğinde hesaplanır.">i</span></div><p className="empty-value">—</p><p className="value-caption">Bakiye bilgisi eklenmedi</p><div className="card-divider" /><p className="muted small">Hesap ve başlangıç bakiyesi eklendiğinde güncel durum burada görünecek.</p></article>
      <article className="forecast-card"><div className="card-heading"><span className="section-kicker light">Maaş gününe kadar</span><span className="soft-chip">Tahmin kapalı</span></div><p className="empty-value light-value">—</p><p className="value-caption light-caption">Henüz yeterli veri yok</p><p className="forecast-note">Başlangıç bakiyesi, maaş tarihi ve tutarı tanımlandığında tahmin aralığı açıklamasıyla birlikte gösterilecek.</p></article>
    </section>
    <section className="dashboard-grid">
      <article className="panel chart-panel"><div className="panel-header"><div><p className="section-kicker">Nakit akışı</p><h3>Bugün → maaş günü</h3></div><span className="panel-action">Nasıl hesaplanır?</span></div><EmptyChart /></article>
      <article className="panel attention-panel"><div className="panel-header"><div><p className="section-kicker">Dikkat çekenler</p><h3>Henüz içgörü yok</h3></div></div><div className="empty-panel"><span className="empty-icon">✦</span><p>Ekstre yüklendiğinde yaklaşan ödemeler ve önemli değişimler burada açıklanacak.</p><button className="text-button" type="button" onClick={onImport}>İlk ekstreni ekle →</button></div></article>
    </section>
    <section className="panel activity-panel"><div className="panel-header"><div><p className="section-kicker">Son hareketler</p><h3>{preview ? `${preview.validRows} geçerli satır önizlendi` : 'İşlem geçmişi bekleniyor'}</h3></div><span className="panel-action">Tümünü gör</span></div>{preview ? <div className="mini-list">{preview.rows.slice(0, 3).map((row) => <div className="mini-row" key={row.rowNumber}><span className={`status-dot ${row.isValid ? 'valid' : 'invalid'}`} /><span>{row.description || 'Açıklamasız işlem'}</span><span className="mini-date">{row.transactionDate || 'Tarih yok'}</span></div>)}</div> : <div className="empty-inline">Henüz işlem bulunmuyor. Başlamak için bir CSV ekstre yükleyin.</div>}</section>
    <p className="privacy-note">Verilerinin kontrolü sende. Bu prototipte gerçek banka bağlantısı yoktur.</p>
  </>
}

function EmptyChart() { return <div className="empty-chart"><div className="chart-lines"><span /><span /><span /><span /></div><div className="chart-axis"><span>Bugün</span><span>Maaş günü</span></div><div className="chart-legend"><span><i className="legend-line" /> Gerçekleşen</span><span><i className="legend-dashed" /> Tahmin</span></div></div> }

function ImportView({ preview, confirmation, onPreview, onConfirmed }: { preview: PreviewResponse | null; confirmation: ConfirmResponse | null; onPreview: (preview: PreviewResponse) => void; onConfirmed: (confirmation: ConfirmResponse) => void }) {
  const [file, setFile] = useState<File | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [confirming, setConfirming] = useState(false)
  async function previewFile() {
    if (!file) return
    setLoading(true); setError(null)
    const formData = new FormData(); formData.append('file', file)
    try { const response = await fetch('http://localhost:5080/api/imports/preview', { method: 'POST', body: formData }); const body = await response.json(); if (!response.ok) throw new Error(body.detail ?? 'CSV önizlemesi oluşturulamadı.'); onPreview(body as PreviewResponse) }
    catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'Beklenmeyen bir hata oluştu.') }
    finally { setLoading(false) }
  }
  async function confirmImport() {
    if (!preview) return
    setConfirming(true); setError(null)
    try {
      const response = await fetch(`http://localhost:5080/api/imports/${preview.importJobId}/confirm`, { method: 'POST' })
      const body = await response.json()
      if (!response.ok) throw new Error(body.detail ?? 'İçe aktarma onaylanamadı.')
      onConfirmed(body as ConfirmResponse)
    } catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'Beklenmeyen bir hata oluştu.') }
    finally { setConfirming(false) }
  }
  return <>
    <div className="page-heading"><div><p className="section-kicker">Veri ekle</p><h2>Ekstreni içe aktar</h2><p className="muted">Önce kontrol ederiz, sen onaylamadan finansal kayıt oluşturmayız.</p></div></div>
    <section className="import-layout"><article className="panel upload-panel"><div className="dropzone"><div className="upload-symbol">↑</div><h3>CSV dosyanı buraya bırak</h3><p className="muted">veya bilgisayarından seç. En fazla 5 MB.</p><label className="primary-button file-picker">Dosya seç<input type="file" accept=".csv,text/csv" onChange={(event) => setFile(event.target.files?.[0] ?? null)} /></label>{file && <p className="selected-file">Seçilen dosya: <strong>{file.name}</strong></p>}</div><div className="format-note"><strong>Beklenen sütunlar</strong><span>tarih / açıklama / tutar</span><span>Para birimi isteğe bağlıdır, varsayılan TRY.</span></div>{error && <div className="alert error" role="alert">{error}</div>}<button className="primary-button full-button" type="button" onClick={previewFile} disabled={!file || loading}>{loading ? 'Kontrol ediliyor…' : 'Önizlemeyi başlat'}</button></article><aside className="explanation-card"><span className="explanation-icon">✓</span><h3>Kontrol sende</h3><p>Dosyayı önce satır satır doğrularız. Hatalı tarih veya tutarları görmeden hiçbir veri kalıcı işleme dönüşmez.</p><span className="privacy-label">Gizlilik · Açıklanabilirlik</span></aside></section>
    {preview && <PreviewResult preview={preview} confirmation={confirmation} confirming={confirming} onConfirm={confirmImport} />}
  </>
}

function PreviewResult({ preview, confirmation, confirming, onConfirm }: { preview: PreviewResponse; confirmation: ConfirmResponse | null; confirming: boolean; onConfirm: () => void }) { return <section className="panel preview-panel"><div className="panel-header"><div><p className="section-kicker">Önizleme sonucu</p><h3>{preview.fileName}</h3></div><span className="data-badge">{preview.importJobId.slice(0, 8)}</span></div><div className="summary-row"><Summary label="Toplam satır" value={preview.totalRows.toString()} /><Summary label="Geçerli" value={preview.validRows.toString()} tone="positive" /><Summary label="Hatalı" value={preview.invalidRows.toString()} tone="negative" /></div>{preview.validRows > 0 && !confirmation && <button className="primary-button" type="button" onClick={onConfirm} disabled={confirming}>{confirming ? 'Kaydediliyor…' : 'Onayla ve kaydet'}</button>}{confirmation && <div className="confirmation-result" role="status"><strong>{confirmation.alreadyConfirmed ? 'Bu import daha önce onaylandı.' : 'Import başarıyla onaylandı.'}</strong><span>{confirmation.createdTransactions} işlem kaydedildi. {confirmation.invalidRows} hatalı satır kaydedilmedi.</span></div>}<PreviewTable rows={preview.rows} /></section> }
function Summary({ label, value, tone = '' }: { label: string; value: string; tone?: string }) { return <div className="summary-item"><span>{label}</span><strong className={tone}>{value}</strong></div> }
function PreviewTable({ rows }: { rows: PreviewRow[] }) { return <div className="table-wrap"><table><thead><tr><th>Satır</th><th>Durum</th><th>Tarih</th><th>Açıklama</th><th>Tutar</th><th>Not</th></tr></thead><tbody>{rows.map((row) => <tr key={row.rowNumber}><td>{row.rowNumber}</td><td><span className={`tag ${row.isValid ? 'valid' : 'invalid'}`}>{row.isValid ? 'Geçerli' : 'Hatalı'}</span></td><td>{row.transactionDate ?? '—'}</td><td>{row.description || '—'}</td><td>{row.amount == null ? '—' : `${row.amount.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ${row.currencyCode}`}</td><td>{row.validationErrors.join(' ') || '—'}</td></tr>)}</tbody></table></div> }

function TransactionsView({ preview, confirmation, onImport }: { preview: PreviewResponse | null; confirmation: ConfirmResponse | null; onImport: () => void }) {
  const validRows = useMemo(() => preview?.rows.filter((row) => row.isValid) ?? [], [preview])
  return <><div className="page-heading"><div><p className="section-kicker">İşlem inceleme</p><h2>İşlemler</h2><p className="muted">İçe aktarılan satırları onaylamadan önce gözden geçir.</p></div><button className="secondary-button" type="button" onClick={onImport}>＋ Yeni ekstre</button></div>{!preview ? <section className="panel large-empty"><span className="empty-icon">≡</span><h3>Henüz işlem yok</h3><p className="muted">İşlemleri görmek için önce bir CSV ekstre yükle.</p><button className="primary-button" type="button" onClick={onImport}>Ekstre ekle</button></section> : <section className="panel"><div className="panel-header"><div><p className="section-kicker">İncelenecek satırlar</p><h3>{confirmation ? `${confirmation.createdTransactions} işlem kaydedildi` : `${validRows.length} geçerli işlem`}</h3></div><span className="soft-chip">{confirmation ? 'Onaylandı' : 'Onay bekliyor'}</span></div><PreviewTable rows={preview.rows} /></section>}</>
}

createRoot(document.getElementById('root')!).render(<StrictMode><App /></StrictMode>)

