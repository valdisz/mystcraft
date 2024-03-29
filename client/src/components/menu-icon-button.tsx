import React from 'react'
import { IconButton, IconButtonProps, Menu, MenuProps } from '@mui/material'

export interface MenuIconButtonProps {
    icon: React.ReactNode
    children: React.ReactNode
    IconButtonProps?: IconButtonProps
    MenuProps?: MenuProps
}

export function MenuIconButton({ icon, IconButtonProps, MenuProps, children }: MenuIconButtonProps) {
    const anchorRef = React.useRef(null)
    const [ open, setOpen ] = React.useState(false)

    const toggleMenu = () => setOpen(!open)
    const closeMenu = () => setOpen(false)

    return <>
        <IconButton {...IconButtonProps || {}} ref={anchorRef} onClick={toggleMenu}>
            {icon}
        </IconButton>
        <Menu {...MenuProps || {}} anchorEl={anchorRef.current} open={open} onBackdropClick={closeMenu} onClick={closeMenu}>
            {children}
        </Menu>
    </>
}
