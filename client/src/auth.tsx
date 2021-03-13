import * as React from 'react'
import gql from 'graphql-tag'
import { CLIENT } from './client'
import { SignIn } from './components'

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
            }, err => {
                setNeedsSignIn(!!err.graphQLErrors.find(x => x.extensions['code'] === 'AUTH_NOT_AUTHORIZED'))
            })
            .finally(() => {
                setLoading(false)
            })
    }, [])

    if (loading) return <div>Loading...</div>

    if (needsSignIn) return <SignIn />

    return <>{children}</>
}
