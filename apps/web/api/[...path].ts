type VercelRequest = {
  method?: string
  url?: string
  headers: Record<string, string | string[] | undefined>
  [Symbol.asyncIterator](): AsyncIterator<Uint8Array>
}

type VercelResponse = {
  status: (code: number) => VercelResponse
  setHeader: (name: string, value: string) => VercelResponse
  send: (body: string | Uint8Array) => void
}

export const config = {
  api: { bodyParser: false },
}

async function readBody(request: VercelRequest): Promise<Uint8Array | undefined> {
  const chunks: Uint8Array[] = []
  for await (const chunk of request) chunks.push(chunk)
  if (!chunks.length) return undefined

  const size = chunks.reduce((total, chunk) => total + chunk.byteLength, 0)
  const body = new Uint8Array(size)
  let offset = 0
  for (const chunk of chunks) {
    body.set(chunk, offset)
    offset += chunk.byteLength
  }
  return body
}

export default async function handler(request: VercelRequest, response: VercelResponse) {
  const backendUrl = process.env.BACKEND_API_URL?.trim().replace(/\/$/, '')
  if (!backendUrl) {
    response.status(500).setHeader('Content-Type', 'application/json').send(JSON.stringify({
      title: 'Backend yapılandırılmamış',
      detail: 'Vercel Environment Variables bölümüne BACKEND_API_URL ekleyin.',
    }))
    return
  }

  const requestUrl = new URL(request.url ?? '/api', 'https://vercel.invalid')
  const backendPath = requestUrl.pathname.replace(/^\/api(?=\/|$)/, '') || '/'
  const targetUrl = `${backendUrl}${backendPath}${requestUrl.search}`
  const headers = new Headers()

  for (const [name, value] of Object.entries(request.headers)) {
    if (value && !['host', 'connection', 'content-length'].includes(name.toLowerCase())) {
      headers.set(name, Array.isArray(value) ? value.join(',') : value)
    }
  }

  const body = ['GET', 'HEAD'].includes(request.method ?? 'GET') ? undefined : await readBody(request)
  const upstream = await fetch(targetUrl, {
    method: request.method ?? 'GET',
    headers,
    body,
    redirect: 'manual',
  })

  response.status(upstream.status)
  upstream.headers.forEach((value, name) => {
    if (!['content-encoding', 'transfer-encoding'].includes(name.toLowerCase())) response.setHeader(name, value)
  })
  response.send(new Uint8Array(await upstream.arrayBuffer()))
}
