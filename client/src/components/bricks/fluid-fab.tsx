import * as React from 'react'
import { Fab, FabProps } from '@mui/material'

export interface FluidFabProps extends FabProps {
    icon: React.ReactNode
}

export function FluidFab({ onMouseOver, onMouseOut, icon, children, ...props }: FluidFabProps) {
    const [ variant, setVariant ] = React.useState<'circular' | 'extended'>('circular')

    return <Fab {...props} variant='extended'
        onMouseOver={e => {
            setVariant('extended')
        }}
        onMouseOut={e => {
            setVariant('circular')
        }}
    >
        { icon }
        { variant === 'extended' && children }
    </Fab>
}
