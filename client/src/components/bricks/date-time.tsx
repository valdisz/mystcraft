import React, { memo, forwardRef, Ref } from 'react'
import { Typography, TypographyProps, styled } from '@mui/material'

export interface DateTimeProps<TypographyComponent extends React.ElementType = 'span'> extends Intl.DateTimeFormatOptions {
    value: Date | string | number
    locale?: string
    typographyProps?: Omit<TypographyProps<TypographyComponent, { component?: TypographyComponent }>, 'children'>
}

function toDate(value: Date | string | number): Date {
    if (typeof value === 'string' || typeof value === 'number') {
        return new Date(value)
    }

    return value
}

const DateTimeValue = styled(Typography, {
    name: 'date-time'
})({ })

function DateTime({ value, locale, typographyProps, dateStyle, timeStyle, hourCycle, ...options }: DateTimeProps, ref: Ref<HTMLElement>) {
    const formatter = new Intl.DateTimeFormat(locale, {
        ...options,
        dateStyle: dateStyle || 'medium',
        timeStyle: timeStyle || 'short',
        hourCycle: hourCycle || 'h24'
    })

    const props = typographyProps || { }
    props.component = props.component || 'span'
    props.variant = props.variant || 'inherit'

    return <DateTimeValue ref={ref} {...props}>
        {formatter.format(toDate(value))}
    </DateTimeValue>
}

export default memo(forwardRef<HTMLElement, DateTimeProps>(DateTime))
