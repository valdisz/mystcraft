import * as React from 'react'
import { SvgIconProps } from '@mui/material'
import FiberNewIcon from '@mui/icons-material/FiberNew'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import PauseIcon from '@mui/icons-material/Pause'
import LockIcon from '@mui/icons-material/Lock'
import StopIcon from '@mui/icons-material/Stop'

import { GameStatus } from '../schema'

export interface GameStatusIconProps extends SvgIconProps {
    status: GameStatus
}

export default function GameStatusIcon({ status, ...props }: GameStatusIconProps) {
    switch (status) {
        case GameStatus.New: return <FiberNewIcon color='info' {...props} />
        case GameStatus.Running: return <PlayArrowIcon color='success' {...props} />
        case GameStatus.Paused: return <PauseIcon color='secondary' {...props} />
        case GameStatus.Locked: return <LockIcon color='warning' {...props} />
        case GameStatus.Stoped: return <StopIcon color='disabled' {...props} />
        default: return null
    }
}
