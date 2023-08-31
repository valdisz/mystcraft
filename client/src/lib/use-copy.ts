import ClipboardJS from 'clipboard';
import * as React from 'react'
import useCallbackRef from './use-callback-ref';

export interface UseCopyOptions extends ClipboardJS.Options {
    onSuccess?: () => void
    onError?: () => void
}

// data-clipboard-text
export default function useCopy(content: boolean = false, { onError, onSuccess, ...options }: UseCopyOptions = { }) {
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
    }, [ ref, onSuccess, onError, options, content ])

    return setRef
}


