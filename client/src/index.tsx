import * as React from 'react'
import * as ReactDOM from 'react-dom'
import { BrowserRouter} from 'react-router-dom'
import { CssBaseline, GlobalStyles } from '@mui/material'
import { ThemeProvider } from '@mui/material/styles'
import 'regenerator-runtime/runtime.js'
import 'simplebar/dist/simplebar.min.css'

import { App } from './app'
import { Authenticate } from './auth'
import THEME from './theme'

const globalStyles = <GlobalStyles styles={`
    html, body, #app-host {
        height: 100%;
        margin: 0;
        padding: 0;
    }
`} />

const RoutedApp = () => (
    <ThemeProvider theme={THEME}>
        <CssBaseline />
        {globalStyles}
        <Authenticate>
            <BrowserRouter>
                <App />
            </BrowserRouter>
        </Authenticate>
    </ThemeProvider>
)

const host = document.getElementById('app-host');

ReactDOM.render(<RoutedApp />, host);

// Hot Module Replacement API
declare const module: { hot: any };
if (module && module.hot) {
    module.hot.accept();
}
