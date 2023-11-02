import { ApolloClient, InMemoryCache, NormalizedCacheObject, createHttpLink } from '@apollo/client'

const cache = new InMemoryCache()
const link = createHttpLink({
    uri: `/graphql`,
    credentials: 'include'
})

export const CLIENT: ApolloClient<NormalizedCacheObject> = new ApolloClient({
    cache,
    link,
    // defaultOptions: {
    //     watchQuery: {
    //         fetchPolicy: 'no-cache'
    //     },
    //     query: {
    //         fetchPolicy: 'no-cache'
    //     },
    //     mutate: {
    //         fetchPolicy: 'no-cache'
    //     }
    // }
})
