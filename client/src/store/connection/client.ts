import { createClient, dedupExchange } from 'urql'
import { multipartFetchExchange } from '@urql/exchange-multipart-fetch'

const client = createClient({
    url: '/graphql',
    fetchOptions: {
        credentials: 'include'
    },
    exchanges: [
        dedupExchange,
        multipartFetchExchange as any
    ]
})

export default client
