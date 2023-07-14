import { Exchange, createClient, cacheExchange, fetchExchange } from 'urql'
import { devtoolsExchange } from '@urql/devtools'
import { env } from 'process'

const exchanges: Exchange[] = [
    // cacheExchange,
    fetchExchange
]

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
