import React from 'react'
import { Box, Button, IconButton, IconButtonProps, Menu, MenuProps, MenuItem } from '@mui/material'
import { Link, Outlet } from 'react-router-dom'
import { ForRole } from '../auth'
import { Role } from '../roles'

import ExitToAppIcon from '@mui/icons-material/ExitToApp'
import MenuIcon from '@mui/icons-material/Menu'

interface MenuIconButtonProps {
    title?: React.ReactNode
    children?: React.ReactNode
    ButtonProps?: IconButtonProps
    MenuProps?: MenuProps
}

function MenuIconButton({ title, ButtonProps, MenuProps, children }: MenuIconButtonProps) {
    const anchorRef = React.useRef(null)
    const [ open, setOpen ] = React.useState(false)

    const toggleMenu = () => setOpen(!open)
    const closeMenu = () => setOpen(false)

    return <>
        <IconButton {...ButtonProps || {}} ref={anchorRef} onClick={toggleMenu}>
            {title}
        </IconButton>
        <Menu {...MenuProps || {}} anchorEl={anchorRef.current} open={open} onBackdropClick={closeMenu}>
            {children}
        </Menu>
    </>
}

export function Layout() {
    return <Box sx={{
        display: 'flex',
        minHeight: 0,
        height: '100%',
        flexDirection: 'column'
    }}>
        <Box sx={{
            m: 2,
            minWidth: 0,
            display: 'flex',
            justifyContent: 'space-between'
        }}>
            <MenuIconButton title={<MenuIcon />}>
                <MenuItem component={Link} to='/'>Games</MenuItem>
                <ForRole role={Role.GameMaster}>
                    <MenuItem component={Link} to='/engines'>Game Engines</MenuItem>
                </ForRole>
                <ForRole role={Role.UserManager}>
                    <MenuItem component={Link} to='/users'>Users</MenuItem>
                </ForRole>
            </MenuIconButton>
            <Button component={'a'} startIcon={<ExitToAppIcon />} href='/account/logout' >Sign out</Button>
        </Box>

        <Box sx={{
            flex: 1,
            display: 'flex',
            minHeight: 0,
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center'
        }}>
            <Outlet />
        </Box>
    </Box>
}
