export type TransactionType = 'Income' | 'Expense' | 'Transfer' | 'Refund' | 'CardPayment'

export type Transaction = {
  date: string
  description: string
  amount: number
  currency: string
  type: TransactionType
  accountName: string
  categoryName: string | null
}

export type TransactionResponse = {
  items: Transaction[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type CashflowAccount = { id: string; name: string; currency: string; openingBalance: number | null; openingBalanceDate: string | null; nextIncomeDate: string | null; nextIncomeAmount: number | null }
export type CashflowPoint = { date: string; balance: number; isProjected: boolean }
export type CashflowEntry = { date: string; description: string; amount: number; currency: string; type: TransactionType }
export type CashflowResponse = { account: CashflowAccount; startDate: string; incomeDate: string | null; daysUntilIncome: number | null; lowestProjectedBalance: number | null; lowestBalanceDate: string | null; points: CashflowPoint[]; includedEntries: CashflowEntry[]; warnings: string[]; method: string; dataCutoffDate: string; isLimited: boolean; scenario: { description: string; incomeDate: string; points: CashflowPoint[]; lowestProjectedBalance: number | null; lowestBalanceDate: string | null } | null }
export type FinfinChatResponse = { mode: 'demo' | 'llm'; isDemo: boolean; answer: string; context: string[]; missingData: string[]; relatedPath: string; requiredEnvironmentVariables: string[] }

export type TransactionFilters = {
  from: string
  to: string
  type: '' | TransactionType
  accountId: string
  page: number
  pageSize: number
}

export type PreviewRow = {
  rowNumber: number
  isValid: boolean
  transactionDate: string | null
  description: string | null
  amount: number | null
  currencyCode: string
  validationErrors: string[]
}

export type PreviewResponse = {
  importJobId: string
  fileName: string
  totalRows: number
  validRows: number
  invalidRows: number
  rows: PreviewRow[]
}

export type ConfirmResponse = {
  importJobId: string
  status: string
  totalRows: number
  validRows: number
  invalidRows: number
  createdTransactions: number
  alreadyConfirmed: boolean
}

const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080'

export class ApiError extends Error {
  constructor(message: string, readonly status: number | null, readonly kind: 'network' | 'missing-data' | 'server' | 'client') {
    super(message)
    this.name = 'ApiError'
  }
}

async function requestJson<T>(path: string, options?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, options)
  } catch {
    throw new ApiError('Financo API’ye ulaşılamadı. Backend’in çalıştığını kontrol edip tekrar deneyin.', null, 'network')
  }

  const body = await response.json().catch(() => null) as { detail?: string; title?: string }
  if (!response.ok) {
    const isMissingData = response.status === 400 && (body?.title?.includes('Eksik') || body?.detail?.includes('eksik'))
    const kind = isMissingData ? 'missing-data' : response.status >= 500 ? 'server' : 'client'
    throw new ApiError(isMissingData ? (body?.detail ?? 'Tahmin için gerekli finansal bilgiler eksik.') : response.status >= 500 ? 'Sunucuda beklenmeyen bir hata oluştu. Lütfen biraz sonra tekrar deneyin.' : (body?.detail ?? body?.title ?? 'İstek geçersiz.') , response.status, kind)
  }
  return body as T
}

export async function listTransactions(filters: TransactionFilters): Promise<TransactionResponse> {
  const params = new URLSearchParams({ page: String(filters.page), pageSize: String(filters.pageSize) })
  if (filters.from) params.set('from', filters.from)
  if (filters.to) params.set('to', filters.to)
  if (filters.type) params.set('type', filters.type)
  if (filters.accountId.trim()) params.set('accountId', filters.accountId.trim())
  return requestJson<TransactionResponse>(`/api/transactions?${params.toString()}`)
}

export async function listAllTransactions(filters: Omit<TransactionFilters, 'page' | 'pageSize'>): Promise<Transaction[]> {
  const firstPage = await listTransactions({ ...filters, page: 1, pageSize: 100 })
  if (firstPage.totalPages <= 1) return firstPage.items

  const pages = await Promise.all(Array.from({ length: firstPage.totalPages - 1 }, (_, index) =>
    listTransactions({ ...filters, page: index + 2, pageSize: 100 })))
  return [firstPage, ...pages].flatMap((page) => page.items)
}

export async function previewImport(file: File): Promise<PreviewResponse> {
  const formData = new FormData()
  formData.append('file', file)
  return requestJson<PreviewResponse>('/api/imports/preview', { method: 'POST', body: formData })
}

export async function confirmImport(importJobId: string): Promise<ConfirmResponse> {
  return requestJson<ConfirmResponse>(`/api/imports/${importJobId}/confirm`, { method: 'POST' })
}

export async function listCashflowAccounts(): Promise<CashflowAccount[]> { return requestJson<CashflowAccount[]>('/api/cashflow/accounts') }
export async function saveCashflowSettings(accountId: string, settings: { openingBalance: number; openingBalanceDate: string; nextIncomeDate: string; nextIncomeAmount: number }): Promise<CashflowAccount> { return requestJson<CashflowAccount>(`/api/cashflow/accounts/${accountId}/settings`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(settings) }) }
export async function getCashflowProjection(accountId: string, scenario?: { unexpectedExpenseAmount?: number; unexpectedExpenseDate?: string; delayedIncomeDays?: number; additionalIncomeAmount?: number; additionalIncomeDate?: string }): Promise<CashflowResponse> { return requestJson<CashflowResponse>(`/api/cashflow/accounts/${accountId}/projection`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(scenario ?? {}) }) }
export async function chatWithFinfin(accountId: string, question: string): Promise<FinfinChatResponse> { return requestJson<FinfinChatResponse>('/api/finfin/chat', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ accountId, question }) }) }

export function formatMoney(amount: number, currency: string) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: currency || 'TRY' }).format(amount)
}

export function formatDate(value: string | null | undefined) {
  if (!value) return '—'
  const date = new Date(`${value}T00:00:00`)
  return Number.isNaN(date.getTime()) ? value : new Intl.DateTimeFormat('tr-TR').format(date)
}
