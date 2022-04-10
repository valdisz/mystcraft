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

export interface UseCopyOptions extends ClipboardJS.Options {
    onSuccess?: () => void
    onError?: () => void
}

// data-clipboard-text
export function useCopy(content: boolean = false, { onError, onSuccess, ...options }: UseCopyOptions = { }) {
    const [ref, setRef] = useCallbackRef<HTMLButtonElement>()

    React.useEffect(() => {
        if (!ref) return

        const clip = content
            ? new ClipboardJS(ref, {
                ...options,
                text: (elm) => elm.textContent
            })
            : new ClipboardJS(ref, options)

        if (onSuccess) clip.on('success', onSuccess)
        if (onError) clip.on('error', onError)

        return () => clip.destroy()
    }, [ ref ])

    return setRef
}

export async function copy(text: string) {
    const perm = await navigator.permissions.query({ name: 'clipboard-write' as PermissionName })
    if (perm.state === 'denied') {
        return
    }

    const result = await navigator.clipboard.writeText(text)
}

export function arrayEquals(a: any[], b: any[]) {
    return a.length === b.length && a.every((v, i) => v === b[i])
}

export function numhash(value: number) {
    value = ((value >> 16) ^ value) * 0x45d9f3b
    value = ((value >> 16) ^ value) * 0x45d9f3b
    value = (value >> 16) ^ value

    return value
}
