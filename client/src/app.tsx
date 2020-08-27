import * as React from 'react'
import { Switch, Route } from 'react-router-dom'
import { HomePage } from './pages/home-page'
import { GamePage } from './pages/game-page'

export function App() {
    return <Switch>
        <Route path='/game/:gameId'>
            <GamePage />
        </Route>
        <Route path='/'>
            <HomePage />
        </Route>
    </Switch>
}
