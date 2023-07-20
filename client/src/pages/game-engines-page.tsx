import React from 'react'
import { observer } from 'mobx-react-lite'
import { Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Alert, AlertTitle, Stack } from '@mui/material'
import { LoadingButton } from '@mui/lab'
import { Forbidden, FileInputField, GameEngineItem, ListLayout } from '../components'
import { NewGameEngineViewModel, useStore } from '../store'
import { Role, ForRole, forRole } from '../auth'
import { GameEngineFragment } from '../schema'

function GameEnginesPage() {
    const { engines, enginesDelete, enginesNew } = useStore()

    const actions = (
        <ForRole role={Role.GameMaster}>
            <Button color='primary' size='large' onClick={enginesNew.open}>New Engine</Button>
        </ForRole>
    )

    return (
        <>
            <ListLayout<GameEngineFragment> title='Game Engines' actions={actions} items={engines} operation={engines}>
                {item => <GameEngineItem key={item.id} engine={item} onDelete={enginesDelete} />}
            </ListLayout>
            <ObservableNewGameEngineDialog model={enginesNew} />
        </>
    )
}

export default forRole(GameEnginesPage, Role.GameMaster, <Forbidden />)


interface NewGameEngineDialogProps {
    model: NewGameEngineViewModel
}

function NewGameEngineDialog({ model }: NewGameEngineDialogProps) {
    return <Dialog fullWidth maxWidth='sm' open={model.isOpen} onClose={model.autoClose}>
        <DialogTitle>New game engine</DialogTitle>
        <DialogContent>
            { model.error
                ? <Alert severity="error">
                    <AlertTitle>Error</AlertTitle>
                    {model.error}
                </Alert>
                : null
            }
            <Stack gap={2}>
                <TextField label='Name' value={model.name} onChange={model.setName} />

                <TextField fullWidth multiline label='Description' rows={4} value={model.description} onChange={model.setDescription} />

                <FileInputField label='Game Engine' model={model.content} />
                <FileInputField label='Ruleset' model={model.ruleset} />
            </Stack>
        </DialogContent>

        <DialogActions>
            <Button variant='text' disabled={model.inProgress} onClick={model.close}>Cancel</Button>
            <LoadingButton color='primary' loading={model.inProgress} onClick={model.confirm}>Upload game engine</LoadingButton>
        </DialogActions>
    </Dialog>
}

const ObservableNewGameEngineDialog = observer(NewGameEngineDialog)
