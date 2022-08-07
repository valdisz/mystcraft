import * as React from 'react'
import { Routes, Route } from 'react-router-dom'
import { StoreProvider } from './store'
import * as Pages from './pages'

export function App() {
    return <StoreProvider>
        <Routes>
            <Route path='/game/:gameId' element={<Pages.GamePage />}>
                <Route path='stats' element={<Pages.StatsPage />}>
                    <Route path={`income`} element={<Pages.IncomeTab />} />
                    <Route path={`production`} element={<Pages.ProductionTab />} />
                    <Route index element={<Pages.SkillsTab />} />
                </Route>
                <Route path='university' element={<Pages.UniversityPage />} />
                <Route index element={<Pages.MapTab />} />
            </Route>
            <Route path='/' element={<Pages.Layout />}>
                <Route index element={<Pages.HomePage />} />
                <Route path='/engines' element={<Pages.GameEnginesPage />} />
                <Route path='/users' element={<Pages.UsersPage />} />
            </Route>
        </Routes>
    </StoreProvider>
}
