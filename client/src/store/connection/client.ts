import { createClient, defaultExchanges, Exchange } from 'urql'
import { multipartFetchExchange } from '@urql/exchange-multipart-fetch'
import { devtoolsExchange } from '@urql/devtools'
import { env } from 'process'

const exchanges: Exchange[] = [ ...defaultExchanges, multipartFetchExchange as any ]
if (env.NODE_ENV !== 'production') {
    console.debug('URQL Dev Tools added to the exchanges list')
    exchanges.unshift(devtoolsExchange as any)
}

const client = createClient({
    url: '/graphql',
    fetchOptions: {
        credentials: 'include'
    },
    exchanges
})

export default client
