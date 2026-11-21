# Financo

Financo, finansal işlemleri tek bir çalışma alanında görünür hale getiren yerel bir finans yönetimi uygulamasıdır. Kullanıcı; ekstre verilerini içe aktarabilir, işlemlerini filtreleyebilir, harcama içgörülerini inceleyebilir ve mevcut veriye dayanarak nakit akışı projeksiyonu oluşturabilir.

Proje, ürünün ilk doğrulanabilir kapsamını temsil eden **MVP 1** sürümüdür. Öncelik; gerçek finansal verinin güvenilir biçimde içe aktarılması, anlaşılır şekilde sunulması ve karar destekleyici temel projeksiyonların oluşturulmasıdır.

## MVP 1 kapsamı

- CSV ekstre yükleme, satır bazlı önizleme ve kullanıcı onayı sonrası içe aktarma
- Tarih, işlem türü ve hesap bazlı işlem listeleme
- Harcama kategorileri ve dönemsel finansal içgörüler
- Hesap bakiyesi ve beklenen gelir bilgileriyle nakit akışı projeksiyonu
- Gecikmiş gelir ve beklenmedik gider gibi senaryo analizleri
- Finfin isimli açıklama katmanı için demo sohbet deneyimi

Bu sürüm tek kullanıcı ve yerel çalışma alanı senaryosuna odaklanır. Kimlik doğrulama, gerçek banka entegrasyonları ve üretim ölçeğinde çok kullanıcılı yapı sonraki ürün aşamalarının konusudur.

## Teknik yapı

- **Frontend:** React, TypeScript, Vite
- **Backend:** ASP.NET Core Minimal API, .NET 10
- **Veri erişimi:** Entity Framework Core
- **Veritabanı:** PostgreSQL
- **Test:** xUnit
- **Yerel altyapı:** Docker Compose

Frontend, backend API üzerinden işlemleri ve hesap verilerini tüketir. Backend; CSV ayrıştırma, veri doğrulama, işlem sorgulama ve nakit akışı hesaplama sorumluluklarını üstlenir. PostgreSQL ise hesap, işlem, kategori ve içe aktarma kayıtlarını kalıcı olarak saklar.


