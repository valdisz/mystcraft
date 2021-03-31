import * as React from 'react'
import { createGlobalStyle, ThemeProvider as StyledThemeProvider } from 'styled-components'
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

const heading = {
    fontFamily: 'Almendra, serif'
}

const theme = createMuiTheme({
    typography: {
        fontFamily: 'Fira Code, monospace',
        h1: heading,
        h2: heading,
        h3: heading,
        h4: heading,
        h5: heading,
        h6: heading
    }
})

export function App() {
    return <StoreProvider>
        <ThemeProvider theme={theme}>
            <StyledThemeProvider theme={theme}>
                <CssBaseline />
                <GlobalStyles />
                <Routes />
            </StyledThemeProvider>
        </ThemeProvider>
    </StoreProvider>
}
