import React from 'react'
import { createRoot } from 'react-dom/client'
import { CssBaseline, GlobalStyles } from '@mui/material'
import { ThemeProvider } from '@mui/material/styles'
import 'regenerator-runtime/runtime.js'
import 'simplebar/dist/simplebar.min.css'

import { App } from './app'
import THEME from './theme'

const globalStyles = <GlobalStyles styles={`
`} />

const RoutedApp = () => (
    <ThemeProvider theme={THEME}>
        <CssBaseline />
        {globalStyles}
        <App />
    </ThemeProvider>
)

const host = document.getElementById('app-host');

const root = createRoot(host)
root.render(<RoutedApp />);

// Hot Module Replacement API
declare const module: { hot: any };
if (module && module.hot) {
    module.hot.accept();
}
