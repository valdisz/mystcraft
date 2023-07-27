export interface FieldView {
    readonly isDisabled: boolean
    readonly isEnabled: boolean
    readonly isReadonly: boolean
    readonly isWritable: boolean
    readonly isRequired: boolean
    readonly isOptional: boolean
    readonly isDirty: boolean
    readonly isPristine: boolean
    readonly isTouched: boolean
    readonly isUntouched: boolean
    readonly isActive: boolean
    readonly isInactive: boolean
    readonly isValid: boolean
    readonly isInvalid: boolean
    readonly isErrorVisible: boolean
    readonly error: string

    reset(): void
    touch(): void
    activate(): void
    deactivate(): void
}
