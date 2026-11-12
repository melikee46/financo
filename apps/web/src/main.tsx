import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './styles.css'

function App() {
  return (
    <main className="shell">
      <p className="eyebrow">Akıllı Finans Asistanı</p>
      <h1>Finansal görünümünüzü anlamaya hazırlanın.</h1>
      <p className="intro">
        Bu ilk iskelet sürümünde veri modeli ve PostgreSQL bağlantısı hazırlandı.
        Ekstre yükleme ve analiz akışları sonraki adımlarda eklenecek.
      </p>
      <div className="status-card" role="status">
        <span className="status-dot" aria-hidden="true" />
        Proje iskeleti çalışıyor
      </div>
    </main>
  )
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)

