import * as React from 'react'
import * as ReactDOM from 'react-dom'
import { BrowserRouter} from 'react-router-dom'
import 'regenerator-runtime/runtime.js'

import { App } from './app'
import { Authenticate } from './auth'

const RoutedApp = () => (
    <Authenticate>
        <BrowserRouter>
            <App />
        </BrowserRouter>
    </Authenticate>
)

const host = document.getElementById('app-host');

ReactDOM.render(<RoutedApp />, host);

// Hot Module Replacement API
declare const module: { hot: any };
if (module && module.hot) {
    module.hot.accept();
}
