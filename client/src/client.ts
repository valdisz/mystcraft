import { ApolloClient } from 'apollo-client'
import { InMemoryCache, NormalizedCacheObject } from 'apollo-cache-inmemory'
import { createHttpLink } from 'apollo-link-http'

const cache = new InMemoryCache()
const link = createHttpLink({
    uri: `/graphql`,
    credentials: 'include'
})

export const CLIENT: ApolloClient<NormalizedCacheObject> = new ApolloClient({
    cache,
    link,
    defaultOptions: {
        watchQuery: {
            fetchPolicy: 'no-cache'
        },
        query: {
            fetchPolicy: 'no-cache'
        },
        mutate: {
            fetchPolicy: 'no-cache'
        }
    }
})
