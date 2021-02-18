import * as React from 'react'
import styled, { createGlobalStyle } from 'styled-components'
import { createMuiTheme, ThemeProvider, CssBaseline } from '@material-ui/core'
import { Switch, Route } from 'react-router-dom'
import { HomePage } from './pages/home-page'
import { GamePage } from './pages/game-page'

const GlobalStyles = createGlobalStyle`
html, body, #app-host {
    height: 100%;
}
`

const theme = createMuiTheme({})

export function App() {
    return <ThemeProvider theme={theme}>
        <CssBaseline />
        <GlobalStyles />
        <Switch>
            <Route path='/game/:gameId'>
                <GamePage />
            </Route>
            <Route path='/'>
                <HomePage />
            </Route>
        </Switch>
    </ThemeProvider>
}
