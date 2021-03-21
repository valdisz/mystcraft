import * as React from 'react'
import gql from 'graphql-tag'
import { CLIENT } from './client'
import { SignIn } from './components'
import { ApolloError } from 'apollo-client'

export interface AuthenticateProps {
}

export function Authenticate({ children }: React.PropsWithChildren<AuthenticateProps>) {
    const [ loading, setLoading ] = React.useState(true)
    const [ needsSignIn, setNeedsSignIn ] = React.useState(true)

    React.useEffect(() => {
        CLIENT.query({
            query: gql`query { me { id } }`,
            errorPolicy: 'none'
        })
            .then(result => {
                setNeedsSignIn(false)
            }, (err: ApolloError) => {
                const is401 = (err.networkError as any).statusCode === 401
                const isNotAuthorized = !!err.graphQLErrors.find(x => x.extensions['code'] === 'AUTH_NOT_AUTHORIZED')

                setNeedsSignIn(is401 || isNotAuthorized)
            })
            .finally(() => {
                setLoading(false)
            })
    }, [])

    if (loading) return <div>Loading...</div>

    if (needsSignIn) return <SignIn onSuccess={() => setNeedsSignIn(false)}  />

    return <>{children}</>
}
