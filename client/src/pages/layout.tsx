import React from 'react'
import { Box, Breadcrumbs, Button, MenuItem, Link, Typography } from '@mui/material'
import { Outlet, Link as RouterLink, useMatches } from 'react-router-dom'
import { Role, ForRole } from '../auth'
import { MenuIconButton } from '../components'

import ExitToAppIcon from '@mui/icons-material/ExitToApp'
import MenuIcon from '@mui/icons-material/Menu'

export function Layout() {
    // const matches = useMatches()

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
            <Box>
                <MenuIconButton icon={<MenuIcon />}>
                    <MenuItem component={RouterLink} to='/'>Games</MenuItem>
                    <ForRole role={Role.GameMaster}>
                        <MenuItem component={RouterLink} to='/engines'>Game Engines</MenuItem>
                    </ForRole>
                    <ForRole role={Role.UserManager}>
                        <MenuItem component={RouterLink} to='/users'>Users</MenuItem>
                    </ForRole>
                </MenuIconButton>
            </Box>
            <Box>

            </Box>
            <Box>
                <Button variant='text' component={'a'} startIcon={<ExitToAppIcon />} href='/account/logout' >Sign out</Button>
            </Box>
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


// <Breadcrumbs aria-label="breadcrumb">
// { matches.map(m => <Link key={m.id} component={RouterLink} underline='hover' color='inherit' to={m.pathname}>
// {m.handle?.title ?? 'H'}
// </Link>) }
// <Link component={RouterLink} underline="hover" color="inherit" to="/">
// MUI
// </Link>
// <Link
// component={RouterLink}
// underline="hover"
// color="inherit"
// to="/material-ui/getting-started/installation/"
// >
// Core
// </Link>
// <Typography color="text.primary">Breadcrumbs</Typography>
// </Breadcrumbs>
