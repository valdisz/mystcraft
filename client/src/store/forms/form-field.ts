export interface FormField {
    readonly dirty: boolean
    readonly touched: boolean
    readonly valid: boolean

    touch(): void
    validate(): boolean
    reset(): void
}
