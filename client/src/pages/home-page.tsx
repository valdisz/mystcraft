import * as React from 'react'
import { ListItemText, TextField, Button,
    DialogTitle, DialogContent, DialogActions, Dialog, Box, Typography, Stack, FormControl,
    InputLabel, Chip,
    IconButton, FormHelperText, Alert, AlertTitle, ListItem, ListItemButton
} from '@mui/material'
import { LoadingButton } from '@mui/lab'
import cronstrue from 'cronstrue'
import { Observer, observer } from 'mobx-react-lite'
import { Link, useMatches } from 'react-router-dom'
import { MapLevelItem, useStore } from '../store'
import { ListLayout, FileField, SelectField, Confirm, GameStatusIcon } from '../components'
import { GameEngineFragment, GameHeaderFragment, GameType } from '../schema'
import { Role, ForRole } from '../auth'
import { List, forFileField, forFormControl, forFormHelperText, forSelectField, forTextField } from '../store/forms'

import ClearIcon from '@mui/icons-material/Clear'
import DeleteIcon from '@mui/icons-material/Delete'

export function HomePage() {
    const { games, gamesNew, homePageOperation, gamesDelete } = useStore()
    const matches = useMatches()

    const m = matches[matches.length - 1]
    const title = (m.handle as any).title

    console.log(matches)

    const actions = (
        <ForRole role={Role.GameMaster}>
            <Button color='primary' size='large' onClick={gamesNew.dialog.open}>New Game</Button>
        </ForRole>
    )

    return (
        <>
            <ListLayout<GameHeaderFragment> title={title} actions={actions} operation={homePageOperation} items={games}>
                { item => <GameItem key={item.id} game={item} onDelete={gamesDelete} /> }
            </ListLayout>
            <ObservableNewGameDialog />
        </>
    )
}

interface GameItemProps {
    game: GameHeaderFragment
    onDelete?: (id: string) => void
}

function GameItem({ game, onDelete }: GameItemProps) {
    const cron = game.options.schedule
        ? `${cronstrue.toString(game.options.schedule, { use24HourTimeFormat: true })} (${game.options.timeZone ?? 'UTC'})`
        : null

    const isRemote = game.type === GameType.Remote

    return <ListItem sx={{
            ':not(:last-child)': {
                borderBottomWidth: '1px',
                borderBottomStyle: 'dashed',
                borderBottomColor: 'divider',
            }
        }}
        secondaryAction={
            <ForRole role={Role.GameMaster}>
                <Confirm>
                    <IconButton onClick={() => onDelete?.(game.id)}><DeleteIcon /></IconButton>
                </Confirm>
            </ForRole>
        }
        disablePadding
    >
        <ListItemButton component={Link} to={`/games/${game.id}`}>
            <Stack gap={2} flex={1} minWidth={0}>
                <Typography variant='h6' sx={{ display: 'inline-flex', alignItems: 'center' }}>
                    <GameStatusIcon status={game.status} />
                    {' '}
                    {game.name}
                </Typography>
                <Box>
                    <Typography variant='caption'>Turn Schedule</Typography>
                    <Typography variant='body1'>{cron}</Typography>
                </Box>
            </Stack>
            <Box flex={1} minWidth={0}>
                <Chip variant={isRemote ? 'filled' : 'outlined' } label={isRemote ? 'Remote' : 'Local'} />
            </Box>
        </ListItemButton>
    </ListItem>
}

interface MapLevelItemEditorProps {
    model: MapLevelItem
    onRemove: () => void
}

function MapLevelItemEditor({ model, onRemove }: MapLevelItemEditorProps) {
    return <Observer>
        {() =>
            <Stack direction='row' gap={2}>
                <TextField label='Label' required {...forTextField(model.label)} />
                <TextField label='Width' required {...forTextField(model.width)}  />
                <TextField label='Height' required {...forTextField(model.height)}  />
                <FormControl sx={{ justifyContent: 'center' }}>
                    <IconButton color='error' onClick={onRemove}>
                        <ClearIcon />
                    </IconButton>
                </FormControl>
            </Stack>
        }
    </Observer>

}

interface MapLevelEditorProps {
    model: List<MapLevelItem>
    onAdd: () => void
    onRemove: (item: MapLevelItem, index: number) => void
}

function MapLevelEditor({ model, onAdd, onRemove }: MapLevelEditorProps) {
    return <Observer>
        {() =>
            <FormControl margin='dense' {...forFormControl(model)}>
                <InputLabel shrink>Levels</InputLabel>
                <Stack mt={2}>
                    {model.map((item, index) => <MapLevelItemEditor key={index} model={item} onRemove={() => onRemove(item, index)} />)}
                    <Box>
                        <Button variant='outlined' onClick={onAdd}>Add level</Button>
                    </Box>
                </Stack>
                <FormHelperText {...forFormHelperText(model)} />
            </FormControl>
        }
    </Observer>
}

function engineKey(engine: GameEngineFragment) {
    return engine.id
}

function NewGameDialog() {
    const { gamesNew } = useStore()
    const { engines, form, dialog, operation } = gamesNew

    const renderSelectedEngine = (engine: GameEngineFragment) => {
        if (!engine) {
            return null
        }

        return (
            <Stack direction='row' alignItems='center'>
                <ListItemText primary={engine.name} />
                <Chip size='small' variant={engine.remote ? 'filled' : 'outlined' } label={engine.remote ? 'remote' : 'local'} />
            </Stack>
        )
    }

    return <Dialog fullWidth maxWidth='sm' open={dialog.isOpen} onClose={dialog.autoClose}>
        <DialogTitle>New game</DialogTitle>
        <DialogContent>
            <Stack gap={2}>
                { operation.isFailed
                    ? <Alert severity="error">
                        <AlertTitle>Error</AlertTitle>
                        {operation.error?.message}
                    </Alert>
                    : null
                }

                <TextField autoFocus label='Name' {...forTextField(form.name)} />

                <SelectField<GameEngineFragment> label='Engine' items={engines.value} mapKey={engineKey} renderValue={renderSelectedEngine} {...forSelectField(form.engine)}>
                    { (engine: GameEngineFragment) => (
                        <>
                            <ListItemText primary={engine.name} secondary={engine.description} />
                            <Chip size='small' variant={engine.remote ? 'filled' : 'outlined' } label={engine.remote ? 'remote' : 'local'} />
                        </>
                    ) }
                </SelectField>

                { form.files.isEnabled && <>
                    <FileField label='game.in' {...forFileField(form.files.gameIn)} />
                    <FileField label='players.in' {...forFileField(form.files.playersIn)} />
                </> }

                <MapLevelEditor model={form.levels} onAdd={gamesNew.addLevel} onRemove={gamesNew.removeLevel} />

                <TextField label='Schedule' {...forTextField(form.schedule)} helperText={gamesNew.scheduleHelpText} />

                <SelectField label='Time Zone' items={gamesNew.timeZones} {...forSelectField(form.timeZone)} />
            </Stack>
        </DialogContent>

        <DialogActions>
            <Button variant='text'  disabled={operation.isLoading} onClick={dialog.close}>Cancel</Button>
            <LoadingButton loading={operation.isLoading} color='primary' onClick={gamesNew.confirm}>Add new game</LoadingButton>
        </DialogActions>
    </Dialog>
}

const ObservableNewGameDialog = observer(NewGameDialog)

