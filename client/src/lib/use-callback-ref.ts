import * as React from 'react';


export function useCallbackRef<T>(initialValue: null | T | (() => T) = null): [T | null, (value: any) => any] {
    const [ref, setRef] = React.useState<T | null>(initialValue);
    const callback = React.useCallback(value => setRef(value), []);

    return [ref, callback];
}
