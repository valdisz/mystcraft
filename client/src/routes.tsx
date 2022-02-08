import * as React from 'react'
import { Switch, Route } from 'react-router-dom'
// import * as Pages from './pages'
import { Hexmap2Page } from './hexmap2.page'

export function Routes() {
    return <Switch>
        {/* <Route path='/game/:gameId/university'>
            <Pages.UniversityPage />
        </Route>
        <Route path='/game/:gameId'>
            <Pages.GamePage />
        </Route>
        <Route path='/'>
            <Pages.HomePage />
        </Route> */}
        <Route path='/'>
            <Hexmap2Page/>
        </Route>
    </Switch>
}
