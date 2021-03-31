import ClipboardJS from 'clipboard';
import * as React from 'react'

export function joinClasses(...classes: string[]) {
    return classes.filter(x => !!x).join(' ');
}

export function useClasses(classes: { [name: string]: boolean}): string {
    return React.useMemo(() =>  Object
        .keys(classes)
        .map(className => classes[className] && className)
        .filter(className => !!className)
        .join(' ')
    , [ Object.values(classes) ])
}

export function useCallbackRef<T>(initialValue: null | T | (() => T) = null): [ T | null, (value: any) => any ] {
    const [ ref, setRef ] = React.useState<T | null>(initialValue)
    const callback = React.useCallback(value => setRef(value), [ ])

    return [ ref, callback ]
}

export function useCopy(content: boolean = false) {
    const [ref, setRef] = useCallbackRef<HTMLButtonElement>()

    React.useEffect(() => {
        if (!ref) return

        const clip = content
            ? new ClipboardJS(ref, {
                text: (elm) => elm.textContent
            })
            : new ClipboardJS(ref)


        return () => clip.destroy()
    }, [ ref ])

    return setRef
}
