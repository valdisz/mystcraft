import React from 'react'
import { Container, List, Alert, AlertTitle, Stack, Paper, LinearProgress } from '@mui/material'
import { Operation, OperationError, Seq } from '../store/connection'
import { PageTitle } from './page-title'
import { observer } from 'mobx-react-lite'
import { EmptyListItem } from './bricks'

interface ListLayoutProps<T> {
    title: React.ReactNode
    actions?: React.ReactNode

    operation: Operation<OperationError>
    items: Seq<T>

    children: (item: T) => React.ReactNode
}

function ListLayout<T>({ title, actions, items, operation, children }: ListLayoutProps<T>) {
    const content = items.map(children)

    return (
        <Container>
            <PageTitle title={title} actions={actions} />

            <Stack gap={6}>
                { operation.isFailed && <Alert severity="error">
                    <AlertTitle>Error</AlertTitle>
                    { operation.error.message }
                </Alert> }

                <Paper elevation={0} variant='outlined'>
                    { operation.isLoading && <LinearProgress /> }

                    <List dense disablePadding>
                        { items.isEmpty
                            ? <EmptyListItem>{operation.isLoading ? 'Loading...' : null}</EmptyListItem>
                            : content
                        }
                    </List>
                </Paper>
            </Stack>
        </Container>
    )
}

export default observer(ListLayout)
