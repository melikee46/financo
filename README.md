# Akıllı Finans Asistanı

CSV ekstrelerini ileride normalize edip analiz edecek web uygulaması. Bu repository'deki ilk adım yalnızca çalışan proje iskeletini, PostgreSQL bağlantısını ve ilk finansal veri modelini içerir.

## Gereksinimler

- .NET 8 SDK
- Node.js 20+ ve npm
- Docker Desktop / Docker Compose

## Yapı

```text
apps/web       React + TypeScript + Vite
apps/api       ASP.NET Core Web API + EF Core + PostgreSQL
docs           Veri modeli ve proje notları
```

## Yerel çalıştırma

1. PostgreSQL'i başlatın:

   ```bash
   docker compose up -d postgres
   ```

2. API bağlantı değişkenini ayarlayın. Varsayılan yerel bağlantı `appsettings.json` içinde bulunur; farklı ortamlar için secret'ları kaynak koda koymadan environment variable kullanın:

   ```bash
   export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=finaco;Username=finaco;Password=finaco_local_only'
   ```

3. Migration'ı uygulayın. Uygulama başlangıçta otomatik migration çalıştırmaz:

   ```bash
   dotnet tool install --global dotnet-ef --version 8.*
   dotnet ef database update --project apps/api/Finaco.Api --startup-project apps/api/Finaco.Api
   ```

4. API'yi çalıştırın:

   ```bash
   dotnet run --project apps/api/Finaco.Api
   ```

   Health endpoint: `http://localhost:5080/health`

5. Frontend'i çalıştırın:

   ```bash
   cd apps/web
   npm install
   npm run dev
   ```

## Migration komutları

Yeni migration eklemek için:

```bash
dotnet ef migrations add MigrationName --project apps/api/Finaco.Api --startup-project apps/api/Finaco.Api
dotnet ef database update --project apps/api/Finaco.Api --startup-project apps/api/Finaco.Api
```

İlk migration `apps/api/Finaco.Api/Migrations/202611120001_InitialCreate.cs` altında bulunur.

## Environment değişkenleri

`.env.example` yalnızca yerel geliştirme için örnek değerler içerir. Gerçek parola, secret, banka bilgisi veya kişisel finans verisi repository'ye eklenmemelidir. Docker Compose şu değişkenleri destekler: `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`.

## Bu adımda bulunanlar

- React + TypeScript + Vite frontend iskeleti
- ASP.NET Core Web API
- PostgreSQL Docker Compose servisi
- EF Core DbContext ve ilk migration
- Account, Category, Transaction, ImportJob ve ImportRow modeli
- `/health` endpoint'i ve DB bağlantı health check'i
- Temel domain/configuration testleri

## Henüz bulunmayanlar

CSV yükleme/parsing/önizleme, işlem CRUD endpoint'leri, dashboard hesaplamaları, nakit akışı/tahmin, kimlik doğrulama, gerçek banka bağlantısı, ML/NLP, TÜFE ve anomali analizi sonraki adımlardır. Kullanıcı/auth modeli henüz kurulmadığı için veri erişimi tek kullanıcı/sentetik demo sınırında tutulmalıdır.

