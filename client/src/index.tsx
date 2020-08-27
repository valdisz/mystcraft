import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { BrowserRouter} from 'react-router-dom';
import { App } from './app';

const RoutedApp = () => <BrowserRouter>
    <App />
</BrowserRouter>;

const host = document.getElementById('app-host');

ReactDOM.render(<RoutedApp />, host);

// Hot Module Replacement API
declare const module: { hot: any };
if (module && module.hot) {
    module.hot.accept();
}
