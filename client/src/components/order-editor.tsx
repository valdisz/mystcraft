import * as React from 'react'
import { styled } from '@mui/material/styles'
import { observer } from 'mobx-react'
import Editor from 'react-simple-code-editor'
import Prism from 'prismjs'
import { Typography, Box, BoxProps } from '@mui/material'
import { Ruleset } from '../game'
import { OrdersState, useStore } from '../store'
import SimpleBar from 'simplebar-react'

const OrdersEditor = styled(Editor)`
    min-height: 100%;

    textarea {
        min-height: 100%;

        &:focus-visible {
            outline: none;
        }
    }

    .comment {
        font-style: italic;
    }

    .string {
        color: blue;
    }

    .directive {
        font-weight: bold;
        color: purple;
    }

    .number {
        color: green;
    }

    .repeat {
        color: #333;
    }

    .order {
        font-weight: bold;
    }

    .error {
        text-decoration: red wavy underline;
    }
`

const language = {
    directive: /^@;(needs|route|tag)/mi,
    comment: /^@?;.*/m,
    repeat: /^@/m,
    string: /"(?:""|[^"\r\f\n])*"/i,
    number: /\d+/,
    order: {
        pattern: /^(@?)(\w+)/m,
        lookbehind: true
    }
}

const ORDER_VALIDATOR = {
    isValidOrder: (order: string) => true
}

Prism.hooks.add('wrap', env => {
    if (env.type !== 'order') {
        return
    }

    if (!ORDER_VALIDATOR.isValidOrder(env.content)) {
        env.classes.push('error')
    }
})

function highlight(ruleset: Ruleset, s: string) {
    ORDER_VALIDATOR.isValidOrder = order => {
        return ruleset.orders.includes(order.toUpperCase())
    }

    const result = Prism.highlight(s, language, null)

    return result
}

const OrdersStatusBox = styled(Box)`
    p: 1;
    text-align: center;

    &.order-state--saved {
        color: ${x => x.theme.palette.success.main};
    }

    &.order-state--saving, &.order-state--unsaved {
        background-color: ${x => x.theme.palette.info.light};
        color: ${x => x.theme.palette.info.contrastText};
    }

    &.order-state--error {
        background-color: ${x => x.theme.palette.error.main};
        color: ${x => x.theme.palette.error.contrastText};
    }
`

function renderOrderState(state: OrdersState) {
    switch (state) {
        case 'ERROR': return 'Error'
        case 'SAVED': return 'Saved'
        default: return 'Saving...'
    }
}

export interface OrdersStatusProps {
    state: OrdersState
}

export function OrdersStatus({ state }: OrdersStatusProps) {
    return <OrdersStatusBox className={`order-state--${state.toLowerCase()}`}>
        <Typography variant='caption'>{renderOrderState(state)}</Typography>
    </OrdersStatusBox>
}

interface OrdersProps extends BoxProps {
    readOnly: boolean
}

export const Orders = observer(({ readOnly, sx, ...props }: OrdersProps) => {
    const store = useStore()
    const game = store.game
    const ruleset = game.world.ruleset

    return <Box {...props} sx={{
        display: 'flex',
        alignItems: 'stretch',
        flexDirection: 'column',
        minHeight: 0,
        ...(sx || { })
    }}>
        <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', minHeight: 0 }}>
            <Typography variant='subtitle1'>Orders</Typography>
            <Box sx={{ flex: 1, border: 1, borderColor: 'divider', p: 1, mr: 1, minHeight: 0 }}>
                <Box component={SimpleBar} sx={{ height: '100%' }} autoHide={false}>
                    <OrdersEditor readOnly={readOnly} value={game.unitOrders ?? ''} onValueChange={game.setOrders} highlight={s => highlight(ruleset, s)} />
                </Box>
            </Box>
        </Box>
        <OrdersStatus state={game.ordersState} />
    </Box>
})
