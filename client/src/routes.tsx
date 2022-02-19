import * as React from 'react'
import { Switch, Route } from 'react-router-dom'
import * as Pages from './pages'

export function Routes() {
    return <Switch>
        <Route path='/game/:gameId/university'>
            <Pages.UniversityPage />
        </Route>
        <Route path='/game/:gameId'>
            <Pages.GamePage />
        </Route>
        <Route path='/'>
            <Pages.HomePage />
        </Route>
    </Switch>
}
