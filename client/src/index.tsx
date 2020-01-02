import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { BrowserRouter} from 'react-router-dom';
import { App } from './app';

export interface AppOptions {
}

function getOptions() {
    const container = document.getElementById('app-options') as HTMLOrSVGScriptElement;
    if (!container) return;

    const json = JSON.parse(container.textContent);
    return json as AppOptions;
}

const options: AppOptions = getOptions();

declare const module: { hot: any };

const RoutedApp = () => <BrowserRouter>
    <App />
</BrowserRouter>;

const host = document.getElementById('app-host');

ReactDOM.render(<RoutedApp />, host);

// Hot Module Replacement API
if (module && module.hot) {
    module.hot.accept();
}
