import React from 'react'
import { Button, ButtonProps, IconButton, IconButtonProps, Menu, MenuProps } from '@mui/material'

export interface MenuIconButtonProps {
    icon: React.ReactNode
    children: React.ReactNode
    IconButtonProps?: IconButtonProps
    ButtonProps?: ButtonProps
    MenuProps?: MenuProps
    useButton?: boolean
}

export function MenuIconButton({ icon, IconButtonProps, ButtonProps, MenuProps, useButton, children }: MenuIconButtonProps) {
    const anchorRef = React.useRef(null)
    const [ open, setOpen ] = React.useState(false)

    const toggleMenu = () => setOpen(!open)
    const closeMenu = () => setOpen(false)

    return <>
        {useButton
            ? (
                <Button ref={anchorRef} onClick={toggleMenu} {...ButtonProps || {}}>
                    {icon}
                </Button>
            )
            : (
                <IconButton ref={anchorRef} onClick={toggleMenu} {...IconButtonProps || {}}>
                    {icon}
                </IconButton>
            )
        }
        <Menu anchorEl={anchorRef.current} open={open} onClose={closeMenu} onClick={closeMenu} {...MenuProps || {}}>
            {children}
        </Menu>
    </>
}
