import { Option } from './utils'
import { STRING, STRING_TRIM, INT, REAL, BOOLEAN, FILE, Converter } from "./converters"
import { Field, FieldOptions } from "./field"
import { OneOrManyValidators, makeValidator } from "./validation"

export interface BaseFieldFactoryOptions<TRaw, T> extends Omit<
    FieldOptions<TRaw, T>,
    'converter' | 'validator'
> { }

export interface StringFieldOptions extends BaseFieldFactoryOptions<Option<string>, Option<string>> {
    trim?: boolean
}

export function text(validators?: OneOrManyValidators<Option<string>>, options?: StringFieldOptions): Field<Option<string>, Option<string>>{
    const trim = options?.trim ?? false
    return new Field({
        converter: trim ? STRING_TRIM : STRING,
        validator: makeValidator(validators),
        ...options
    })
}

export interface IntFieldOptions extends BaseFieldFactoryOptions<Option<string>, Option<number>> {
}

export function int(validators?: OneOrManyValidators<Option<number>>, options?: IntFieldOptions): Field<Option<string>, Option<number>>{
    return new Field({
        converter: INT,
        validator: makeValidator(validators),
        ...options
    })
}

export interface RealFieldOptions extends BaseFieldFactoryOptions<Option<string>, Option<number>> {
}

export function real(validators?: OneOrManyValidators<Option<number>>, options?: IntFieldOptions): Field<Option<string>, Option<number>> {
    return new Field({
        converter: REAL,
        validator: makeValidator(validators),
        ...options
    })
}

export interface FileFieldOptions extends BaseFieldFactoryOptions<Option<File>, Option<File>> {
}

export function file(validators?: OneOrManyValidators<Option<File>>, options?: FileFieldOptions): Field<Option<File>, Option<File>> {
    return new Field({
        converter: FILE,
        validator: makeValidator(validators),
        ...options
    })
}

export interface BoolFieldOptions extends BaseFieldFactoryOptions<Option<string | boolean>, Option<boolean>> {
}

export function bool(validators?: OneOrManyValidators<Option<boolean>>, options?: BoolFieldOptions): Field<Option<string | boolean>, Option<boolean>> {
    return new Field({
        converter: BOOLEAN,
        validator: makeValidator(validators),
        ...options
    })
}

export interface CustomFieldOptions<TRaw, T> extends BaseFieldFactoryOptions<TRaw, T> {
}

export function custom<TRaw, T = TRaw>(converter: Converter<TRaw, T>, validators?: OneOrManyValidators<T>, options?: CustomFieldOptions<TRaw, T>): Field<TRaw, T> {
    return new Field<TRaw, T>({
        converter,
        validator: makeValidator(validators),
        ...options
    })
}

