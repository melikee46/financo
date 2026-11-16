import type { Transaction } from './api'

export type CategorySpend = { category: string; amount: number; currency: string }
export type RecurringCandidate = { key: string; description: string; amount: number; currency: string; dates: string[] }

export function categorySpending(transactions: Transaction[]): CategorySpend[] {
  const totals = new Map<string, CategorySpend>()
  for (const transaction of transactions) {
    if (transaction.type === 'Transfer' || transaction.type === 'CardPayment' || transaction.type !== 'Expense') continue
    const category = transaction.categoryName?.trim()
    if (!category) continue
    const current = totals.get(category) ?? { category, amount: 0, currency: transaction.currency }
    current.amount += transaction.amount
    totals.set(category, current)
  }
  return [...totals.values()].sort((a, b) => b.amount - a.amount)
}

function normalizedDescription(description: string) {
  return description.toLocaleLowerCase('tr-TR').replace(/[^a-z0-9çğıöşü]+/gi, ' ').trim()
}

function dateDifferenceInDays(first: string, second: string) {
  return Math.abs((new Date(`${first}T00:00:00`).getTime() - new Date(`${second}T00:00:00`).getTime()) / 86_400_000)
}

export function recurringPaymentCandidates(transactions: Transaction[]): RecurringCandidate[] {
  const groups = new Map<string, Transaction[]>()
  for (const transaction of transactions) {
    if (transaction.type !== 'Expense') continue
    const key = normalizedDescription(transaction.description)
    if (!key) continue
    groups.set(key, [...(groups.get(key) ?? []), transaction])
  }

  return [...groups.entries()].flatMap(([key, group]) => {
    const ordered = [...group].sort((a, b) => a.date.localeCompare(b.date))
    const monthlyPairs = ordered.slice(1).filter((transaction, index) => {
      const previous = ordered[index]
      const days = dateDifferenceInDays(previous.date, transaction.date)
      return days >= 25 && days <= 35
    })
    if (monthlyPairs.length === 0) return []
    const dates = [...new Set([ordered[0].date, ...monthlyPairs.map((transaction) => transaction.date)])]
    const latest = ordered[ordered.length - 1]
    return [{ key, description: latest.description, amount: latest.amount, currency: latest.currency, dates }]
  }).sort((a, b) => b.dates.length - a.dates.length)
}

export function monthRange(date = new Date()) {
  const year = date.getFullYear()
  const month = date.getMonth()
  const pad = (value: number) => String(value).padStart(2, '0')
  const lastDay = new Date(year, month + 1, 0).getDate()
  return { from: `${year}-${pad(month + 1)}-01`, to: `${year}-${pad(month + 1)}-${pad(lastDay)}` }
}
