import React from 'react'
import { observer } from 'mobx-react-lite'
import { Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Alert, AlertTitle, Stack, Box, Tabs, Tab } from '@mui/material'
import { LoadingButton } from '@mui/lab'
import { Forbidden, GameEngineItem, ListLayout, FileField } from '../components'
import { NewGameEngineViewModel, useStore } from '../store'
import { Role, ForRole, forRole } from '../auth'
import { GameEngineFragment } from '../schema'
import { forFileField, forTextField } from '../store/forms'

function GameEnginesPage() {
    const { engines, enginesDelete, gameEnginesPageOperation, enginesNew } = useStore()

    const actions = (
        <ForRole role={Role.GameMaster}>
            <Button color='primary' size='large' onClick={enginesNew.dialog.open}>New Engine</Button>
        </ForRole>
    )

    return (
        <>
            <ListLayout<GameEngineFragment> title='Game Engines' actions={actions} items={engines} operation={gameEnginesPageOperation}>
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
    const { dialog, form, operation } = model

    return <Dialog fullWidth maxWidth='sm' open={dialog.isOpen} onClose={dialog.autoClose}>
        <DialogTitle>New game engine</DialogTitle>
        <DialogContent>
            <Stack gap={2}>
                { operation.isFailed
                    ? <Alert severity="error">
                        <AlertTitle>Error</AlertTitle>
                        {operation.error?.message}
                    </Alert>
                    : null
                }

                <TextField label='Name' {...forTextField(form.name)} />

                <TextField label='Description' multiline rows={4} {...forTextField(form.description)} />

                <Tabs centered value={model.mode} onChange={model.setMode}>
                    <Tab label="Local" value='local' />
                    <Tab label="Remote" value='remote' />
                </Tabs>

                { form.files.isEnabled && <>
                    <FileField label='Game Engine' {...forFileField(form.files.engine)} />
                    <FileField label='Ruleset' {...forFileField(form.files.ruleset)} />
                </>}

                { form.remote.isEnabled && <>
                    <TextField label='API' select value='no' disabled>
                        <option value='no'>New Origins (http://atlantis-pbem.com)</option>
                    </TextField>
                    <TextField label='URL' type='url' {...forTextField(form.remote.url)} />
                </>}
            </Stack>
        </DialogContent>

        <DialogActions>
            <Button variant='text' disabled={operation.isLoading} onClick={dialog.close}>Cancel</Button>
            <LoadingButton color='primary' loading={operation.isLoading} onClick={model.confirm}>Add game engine</LoadingButton>
        </DialogActions>
    </Dialog>
}

const ObservableNewGameEngineDialog = observer(NewGameEngineDialog)
