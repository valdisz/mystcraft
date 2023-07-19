import * as React from 'react'
import { RouterProvider, createBrowserRouter } from 'react-router-dom'
import { MainStore, StoreProvider, useStore } from './store'
import { MapContext, MapProvider, useMapContext } from './map'
import { Authenticate } from './auth'
import * as Pages from './pages'

function gameLoader(store: MainStore, map: MapContext) {
    return ({ params }) => {
        const { gameId } = params

        const { game, loading } = store

        loading.clear()

        game.load(gameId, loading)
            .then(() => {
                loading.begin('Map graphics')
                return map.load()
            })
            .then(() => loading.end())
    }
}

function statsLoader(store: MainStore) {
    return () => store.stats.loadStats()
}

const router = (store: MainStore, map: MapContext) => createBrowserRouter([
    {
        path: '/play/:gameId', element: <Pages.GamePage />,
        // FIXME: loader function return value is not assignable to type 'RouteLoader'
        loader: gameLoader(store, map) as any,
        children: [
            { index: true, element: <Pages.MapTab /> },
            {
                path: 'stats', element: <Pages.StatsPage />,
                loader: statsLoader(store),
                children: [
                    { index: true, element: <Pages.TreasuryTab />},
                    { path: 'income', element: <Pages.IncomeTab /> },
                    { path: 'production', element: <Pages.ProductionTab />},
                    { path: 'skills', element: <Pages.SkillsTab />},
                ]
            },
            // { path: '', element: <Pages.UniversityPage /> }
        ]
    },
    {
        path: '/', element: <Pages.Layout />,
        children: [
            { index: true, element: <Pages.HomePage /> },
            { path: 'engines', element: <Pages.GameEnginesPage /> },
            { path: 'users', element: <Pages.UsersPage />},
            { path: 'games/:gameId', element: <Pages.GameDetailsPage />}
        ]
    }
])


export function App() {
    return <StoreProvider>
        <MapProvider>
            <Authenticate>
                <AppRoutes />
            </Authenticate>
        </MapProvider>
    </StoreProvider>
}

function AppRoutes() {
    const store = useStore()
    const map = useMapContext()

    return <RouterProvider router={router(store, map)} />
}
