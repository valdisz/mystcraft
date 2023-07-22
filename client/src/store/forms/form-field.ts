export interface FormField {
    readonly dirty: boolean
    readonly touched: boolean
    readonly valid: boolean

    readonly disabled: boolean

    touch(): void
    validate(): boolean
    reset(): void

    disable(): void
    enable(): void
}
