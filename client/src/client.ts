import { ApolloClient, InMemoryCache, NormalizedCacheObject } from '@apollo/client'
import { createUploadLink } from 'apollo-upload-client'

const cache = new InMemoryCache()
const link = createUploadLink({
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
