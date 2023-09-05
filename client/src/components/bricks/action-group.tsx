import * as React from 'react'
import { Button, ButtonProps, ButtonGroup, ButtonGroupProps, MenuItem, Menu, PopoverOrigin, Divider, SxProps, Theme, MenuProps, MenuItemProps } from '@mui/material'
import { MoreVert } from '@mui/icons-material'

export interface ActionGroupSlotProps {
    action?: ButtonProps
    trigger?: ButtonProps
    menu?: MenuProps
    item?: MenuItemProps
}

export interface Action {
    disabled?: boolean
    content: React.ReactNode
    onAction: () => void
}

export type ActionItem = '---' | Action

export interface ActionGroupProps extends Omit<ButtonGroupProps, 'children'> {
    actions?: ActionItem[]
    additionalActions?: ActionItem[]
    slotProps?: ActionGroupSlotProps
}

const BOTTOM_RIGHT: PopoverOrigin = { vertical: 'bottom', horizontal: 'right' }
const TOP_RIGHT: PopoverOrigin = { vertical: 'top', horizontal: 'right' }

export default function ActionGroup({ actions, additionalActions, slotProps, ...props }: ActionGroupProps) {
    const [open, setOpen] = React.useState(false)
    const anchorRef = React.useRef(null)

    const handleToggle = React.useCallback(() => {
        setOpen((prevOpen) => !prevOpen);
    }, [ setOpen ])

    const handleClose = React.useCallback((event) => {
        if (anchorRef.current && anchorRef.current.contains(event.target)) {
            return;
        }

        setOpen(false);
    }, [anchorRef, setOpen])

    const showMoreButton = additionalActions?.length > 0

    return (
        <ButtonGroup
            aria-label='split button'
            {...props}
        >
            {actions?.map((action, i) => (
                action === '---'
                    ? <Divider key={i} orientation='vertical' />
                    : <Button key={i} disabled={action.disabled} onClick={action.onAction} {...slotProps?.action}>{action.content}</Button>
            ))}
            { showMoreButton && (
                <Button
                    ref={anchorRef}
                    aria-controls={open ? 'split-button-menu' : undefined}
                    aria-expanded={open ? 'true' : undefined}
                    aria-label='select merge strategy'
                    aria-haspopup='menu'
                    onClick={handleToggle}
                    {...slotProps?.trigger}
                >
                    <MoreVert />
                </Button>
            )}
            {showMoreButton && (
                <Menu
                    open={open}
                    onClose={handleClose}
                    anchorEl={anchorRef.current}
                    anchorOrigin={BOTTOM_RIGHT}
                    transformOrigin={TOP_RIGHT}
                    {...slotProps?.menu}
                >
                    {additionalActions.map((action, i) => (
                        action === '---'
                            ? <Divider key={i} />
                            : <MenuItem key={i} disabled={action.disabled} onClick={action.onAction} {...slotProps?.item}>{action.content}</MenuItem>
                    ))}
                </Menu>
            )}
        </ButtonGroup>
    )
}
