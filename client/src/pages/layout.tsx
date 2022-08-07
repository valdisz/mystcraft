import React from 'react'
import { Box, Button, MenuItem } from '@mui/material'
import { Link, Outlet } from 'react-router-dom'
import { Role, ForRole } from '../auth'
import { MenuIconButton } from '../components'

import ExitToAppIcon from '@mui/icons-material/ExitToApp'
import MenuIcon from '@mui/icons-material/Menu'

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
            <MenuIconButton icon={<MenuIcon />}>
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
            flexDirection: 'column'
        }}>
            <Outlet />
        </Box>
    </Box>
}
