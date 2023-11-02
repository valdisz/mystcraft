import React from 'react'
import { observer } from 'mobx-react-lite'
import {
    styled,
    Box, Container, Stack, Alert, CircularProgress, ButtonGroup, Button, Typography, Chip,
    Avatar, Tabs, Tab, Divider, Switch, FormControlLabel, Pagination,
    List, ListItemButton, ListItemText,
} from '@mui/material'
import PageTitle from './page-title'
import { GameDetailsStore, Player, TurnState } from '../store'
import { ActionGroup, ActionItem } from './bricks'

import IconPlay from '@mui/icons-material/PlayArrow'
import IconPause from '@mui/icons-material/Pause'
import IconStop from '@mui/icons-material/Stop'
import IconNext from '@mui/icons-material/SkipNext'
import IconRepeat from '@mui/icons-material/Repeat'

import LayersIcon from '@mui/icons-material/Layers'
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth'
import MiscellaneousServicesIcon from '@mui/icons-material/MiscellaneousServices'
import GameStatusIcon from './game-status-icon'
import { blue, green, orange, red } from '@mui/material/colors'
import { BoxProps, SxProps, Theme } from '@mui/system'

const gameMasterContext = React.createContext(false)

function useGameMaster() {
    return React.useContext(gameMasterContext)
}

const Indicator = styled(Box)({
    width: '4px',
    height: '8px',
    ':hover': {
        position: 'relative',
        transform: 'scale(1.2)'
    }
})
