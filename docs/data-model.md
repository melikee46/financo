# Veri modeli

İlk adım, CSV veya banka entegrasyonu olmadan finansal kayıtların temel şemasını kurar.

## Entity'ler

- **Account:** Banka hesabı, nakit hesabı veya kredi kartı. `CurrencyCode` ve isteğe bağlı başlangıç bakiyesi hesapta tutulur.
- **Category:** Gelir veya gider kategorisi. Silme yerine `IsArchived` ile arşivleme desteklenir.
- **Transaction:** Pozitif `Amount` ve hareketin anlamını taşıyan `Type` birlikte kullanılır. Özgün açıklama korunur; normalize edilmiş açıklama ayrı alandır.
- **ImportJob / ImportRow:** Gelecekteki CSV önizleme ve onay akışının kalıcı şema altyapısıdır. Bu adımda upload veya parse davranışı yoktur.

## Enum'lar

- `AccountType`: `Bank`, `Cash`, `CreditCard`
- `CategoryKind`: `Income`, `Expense`
- `TransactionType`: `Income`, `Expense`, `Transfer`, `Refund`, `CardPayment`
- `ImportStatus`: `Pending`, `Confirmed`, `Failed`

## Modelleme kararları

- Tutarlar C# tarafında `decimal`, PostgreSQL tarafında `numeric(18,2)` olarak saklanır. `Amount` pozitif tutulur; borç/alacak anlamı işlem türünden okunur.
- `Transfer` ve `CardPayment` değerleri tanımlıdır; finansal toplam hesapları bu adımda uygulanmaz.
- Başlangıç bakiyesi nullable'dır; eksik değer `0` olarak yazılmaz.
- Hesap ve kategori ilişkileri foreign key ile korunur. Kategori silindiğinde işlemdeki `CategoryId` null olur; normal ürün akışında arşivleme tercih edilir.
- Kullanıcı/auth modeli henüz yoktur. Bu nedenle mevcut şema tek kullanıcı/sentetik demo sınırında düşünülmelidir.

