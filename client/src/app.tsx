import * as React from 'react'
import { StoreProvider } from './store'
import { AppRoutes } from './routes'
import { CssBaseline, GlobalStyles } from '@mui/material'
import { ThemeProvider, createTheme } from '@mui/material/styles'

const globalStyles = <GlobalStyles styles={`
    html, body, #app-host {
        height: 100%;
        margin: 0;
        padding: 0;
    }
`} />

const heading = {
    fontFamily: 'Almendra, serif'
}

const theme = createTheme({
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
            <CssBaseline />
            {globalStyles}
            <AppRoutes />
        </ThemeProvider>
    </StoreProvider>
}
