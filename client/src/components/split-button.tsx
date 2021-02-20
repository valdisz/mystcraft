import * as React from 'react'
import { Button, ButtonProps, ButtonGroup, ButtonGroupProps, MenuItem, Menu, PopoverOrigin } from '@material-ui/core'
import MoreVertIcon from '@material-ui/icons/MoreVert'

export interface SplitButtonAction {
    disabled?: boolean
    content: React.ReactNode
    onAction: () => void
}

export interface SplitButtonProps extends ButtonProps {
    actions?: SplitButtonAction[]
    moreButtonProps?: ButtonProps
    buttonGroupProps?: ButtonGroupProps
}

interface SplitButtonMenuProps {
    open: boolean
    actions: SplitButtonAction[]
    anchor: any
    onClose: (event) => void
}

const BOTTOM_RIGHT: PopoverOrigin = { vertical: 'bottom', horizontal: 'right' }
const TOP_RIGHT: PopoverOrigin = { vertical: 'top', horizontal: 'right' }

function SplitButtonMenu({ open, actions, anchor, onClose }: SplitButtonMenuProps) {
    return <Menu
        open={open}
        onClose={onClose}
        anchorEl={anchor}
        getContentAnchorEl={null}
        anchorOrigin={BOTTOM_RIGHT}
        transformOrigin={TOP_RIGHT}
        >
            {actions.map((action, i) => (
                <MenuItem key={i} disabled={action.disabled} onClick={action.onAction}>
                    {action.content}
                </MenuItem>
            ))}
    </Menu>
}

export function SplitButton({ actions, moreButtonProps, buttonGroupProps, ...props }: SplitButtonProps) {
    const showMoreButton = !!actions?.length
    const [open, setOpen] = React.useState(false)
    const anchorRef = React.useRef(null)

    const handleToggle = () => {
        setOpen((prevOpen) => !prevOpen);
    };

    const handleClose = (event) => {
        if (anchorRef.current && anchorRef.current.contains(event.target)) {
            return;
        }

        setOpen(false);
    };

    return <>
            <ButtonGroup
                variant={props?.variant}
                color={props?.color}
                aria-label='split button'
                {...(buttonGroupProps || {})}
                >
                <Button {...props} />
                { showMoreButton && <Button
                    color={props?.color}
                    size={props?.size}
                    aria-controls={open ? 'split-button-menu' : undefined}
                    aria-expanded={open ? 'true' : undefined}
                    aria-label='select merge strategy'
                    aria-haspopup='menu'
                    {...(moreButtonProps || {})}
                    ref={anchorRef}
                    onClick={handleToggle}
                >
                    <MoreVertIcon />
                </Button> }
            </ButtonGroup>
            { showMoreButton && <SplitButtonMenu
                open={open}
                anchor={anchorRef.current}
                actions={actions}
                onClose={handleClose}
            /> }
    </>
}
