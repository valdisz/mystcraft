import React from 'react'
import { CLIENT } from './client'
import { SignIn } from './components'
import { ApolloError } from '@apollo/client'
import { GetMe, GetMeQuery, GetMeQueryVariables } from './schema'

export enum Role {
    Root = 'root',
    GameMaster = 'game-master',
    UserManager = 'user-manager'
}

export class Auth {
    constructor (private readonly roles: Role[]) {

    }

    hasRole(role: Role) {
        return this.roles.includes(role)
    }
}

const rolesContext = React.createContext<Auth>(null)

export function useAuth() {
    return React.useContext(rolesContext)
}

export interface AuthenticateProps {
}

export function Authenticate({ children }: React.PropsWithChildren<AuthenticateProps>) {
    const [ loading, setLoading ] = React.useState(true)
    const [ needsSignIn, setNeedsSignIn ] = React.useState(true)
    const [ auth, setAuth ] = React.useState<Auth>(null)

    React.useEffect(() => {
        CLIENT.query<GetMeQuery, GetMeQueryVariables>({
            query: GetMe,
            errorPolicy: 'none'
        })
            .then(result => {
                setAuth(new Auth((result.data.me.roles || []) as any))
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

    return <rolesContext.Provider value={auth}>{children}</rolesContext.Provider>
}

export interface ForRoleProps {
    role: Role | Role[]
    children: React.ReactNode
    forbidden?: React.ReactNode
}

export function ForRole({ role, children, forbidden }: ForRoleProps) {
    const auth = useAuth()
    if (!auth) {
        return <>{forbidden}</>
    }

    const isRoot = auth.hasRole(Role.Root)

    const isInRole = typeof role === 'string'
        ? auth.hasRole(role)
        : role.every(r => auth.hasRole(r))

    return <>{isRoot || isInRole ? children : (forbidden)}</>
}

export function forRole<P>(BaseComponent: React.ComponentType<P>, role: Role | Role[], forbidden?: React.ReactNode) {
    return (props) => <ForRole role={role} forbidden={forbidden}>
        <BaseComponent {...props} />
    </ForRole>
}
