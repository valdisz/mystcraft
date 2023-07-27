export interface ValueOrError<T> {
    error?: string
    value?: T
}

export interface Converter<TRaw, T> {
    sanitize(value: TRaw): ValueOrError<T>
    format(value: T): TRaw
}


// const _STRING: Converter<string> = {
//     sanitzie: (value: string) => ({ value }),
//     format: value => value || ''
// }

// const _STRING_TRIM: Converter<string> = {
//     sanitzie: (value: string) => ({ value: value?.trim() || '' }),
//     format: value => value || ''
// }

// export function STRING(trim?: boolean) {
//     return trim ? _STRING_TRIM : _STRING
// }

// export const INT: Converter<number> = {
//     sanitzie: s => {
//         if (!s) {
//             return { value: null }
//         }

//         const value = parseInt(s)
//         if (isNaN(value)) {
//             return { error: 'Value is not a whole number' }
//         }

//         if (!isFinite(value)) {
//             return { error: 'Value is out of bounds' }
//         }

//         return { value }
//     },
//     format: value => value?.toString() || ''
// }

// export const REAL: Converter<number> = {
//     sanitzie: s => {
//         if (!s) {
//             return { value: null }
//         }

//         const value = parseFloat(s)
//         if (isNaN(value)) {
//             return { error: 'Value is not a fraction or whole number' }
//         }

//         if (!isFinite(value)) {
//             return { error: 'Value is out of bounds' }
//         }

//         return { value }
//     },
//     format: value => value?.toString() || ''
// }

// export const FILE: Converter<File> = {
//     sanitzie: (value: File) => ({ value }),
//     format: value => value
// }

// export const BOOLEAN: Converter<boolean> = {
//     sanitzie: (value: any) => ({ value: !!value }),
//     format: value => value
// }
