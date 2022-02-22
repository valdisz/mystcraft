import * as React from 'react'
import { Routes, Route } from 'react-router-dom'
import * as Pages from './pages'

export function AppRoutes() {
    return <Routes>
        <Route path='/game/:gameId' element={<Pages.GamePage />}>
            <Route path='stats' element={<Pages.StatsPage />}>
                <Route path={`income`} element={<Pages.IncomeTab />} />
                <Route path={`production`} element={<Pages.ProductionTab />} />
                <Route index element={<Pages.SkillsTab />} />
            </Route>
            <Route path='university' element={<Pages.UniversityPage />} />
            <Route index element={<Pages.MapTab />} />
        </Route>
        <Route path='/' element={<Pages.HomePage />} />
    </Routes>
}
