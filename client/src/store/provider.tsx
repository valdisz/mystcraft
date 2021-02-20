import * as React from 'react'
import { MainStore } from './main-store'

const storeContext = React.createContext<MainStore | null>(null)

export const StoreProvider = ({ children }: React.PropsWithChildren<{}>) => {
    const store = React.useState(() => new MainStore())[0]

    return <storeContext.Provider value={store}>{children}</storeContext.Provider>
}

export function useStore() {
    const store = React.useContext(storeContext)
    if (!store) {
        throw new Error('useStore must be used within a StoreProvider.')
    }

    return store
}
