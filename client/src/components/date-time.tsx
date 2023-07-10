import React, { memo, forwardRef, Ref } from 'react'
import { Typography, TypographyProps } from '@mui/material'

export interface DateTimeProps extends Intl.DateTimeFormatOptions {
    value: Date | string | number
    locale?: string
    TypographyProps?: TypographyProps
}

function toDate(value: Date | string | number): Date {
    if (typeof value === 'string' || typeof value === 'number') {
        return new Date(value)
    }

    return value
}

function DateTime({ value, locale, TypographyProps, dateStyle, timeStyle, hourCycle, ...options }: DateTimeProps, ref: Ref<HTMLDivElement>) {
    const formatter = new Intl.DateTimeFormat(locale, {
        ...options,
        dateStyle: dateStyle || 'medium',
        timeStyle: timeStyle || 'short',
        hourCycle: hourCycle || 'h24'
    })

    const props: TypographyProps = TypographyProps || {}
    delete props.children

    return <Typography ref={ref} {...props}>
        {formatter.format(toDate(value))}
    </Typography>
}

export default memo(forwardRef<HTMLDivElement, DateTimeProps>(DateTime))
