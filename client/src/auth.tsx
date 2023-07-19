import React from 'react'
import { CLIENT } from './client'
import { SignIn } from './components'
import { ApolloError } from '@apollo/client'
import { GetMe, GetMeQuery, GetMeQueryVariables } from './schema'
import { canUsePasskey } from './lib'

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
    const [ state, setState ] = React.useState<'loading' | 'sign-in' | 'sign-in-passkey' | 'error' | 'normal'>('loading')
    const [ auth, setAuth ] = React.useState<Auth>(null)

    React.useEffect(() => {
        Promise.all([
            CLIENT.query<GetMeQuery, GetMeQueryVariables>({
                query: GetMe,
                errorPolicy: 'none'
            }),
            canUsePasskey()
        ])
            .then(([ me, passkey ]) => {
                setAuth(new Auth((me.data.me.roles || []) as any))
                setState('normal')
            }, (err: ApolloError) => {
                const is401 = (err.networkError as any)?.statusCode === 401
                const isNotAuthorized = err.graphQLErrors?.some(x => x.extensions['code'] === 'AUTH_NOT_AUTHORIZED')

                setState(is401 || isNotAuthorized ? 'sign-in' : 'error')
            })
    }, [])

    if (state === 'loading') return <div>Loading...</div>
    if (state.startsWith('sign-in')) return <SignIn withPasskey={state === 'sign-in-passkey'} onSuccess={() => setState('normal')} />
    if (state === 'error') return <div>Cannot reach server.</div>

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
