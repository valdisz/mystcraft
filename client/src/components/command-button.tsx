import * as React from 'react'
import { Button, Tooltip } from '@mui/material'
import { observer } from 'mobx-react-lite'
import { InterfaceCommand } from '../store/commands/move'

export interface CommandButtonProps {
    command: InterfaceCommand
}

function CommandButton({ command: { title, tooltip, canExecute, error, execute } }: CommandButtonProps) {
    const text = []
    if (tooltip) text.push(tooltip)
    if (error) text.push(`!: ${error}`)

    const btn = <Button disabled={!canExecute} onClick={execute} variant='contained'>{title}</Button>

    return text.length
        ? <Tooltip title={text.join('\n')}><span>{btn}</span></Tooltip>
        : btn
}

export default observer(CommandButton)
