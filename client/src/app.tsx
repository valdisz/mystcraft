import * as React from 'react'
import { createGlobalStyle } from 'styled-components'
import { createMuiTheme, ThemeProvider, CssBaseline } from '@material-ui/core'
import { StoreProvider } from './store'
import { Routes } from './routes'

const GlobalStyles = createGlobalStyle`
html, body, #app-host {
    height: 100%;
    margin: 0;
    padding: 0;
}
`

const theme = createMuiTheme({})

export function App() {
    return <StoreProvider>
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <GlobalStyles />
            <Routes />
        </ThemeProvider>
    </StoreProvider>
}
