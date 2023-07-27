import * as React from 'react'
import { ListItemText, TextField, Button,
    DialogTitle, DialogContent, DialogActions, Dialog, Box, Typography, Stack, FormControl,
    InputLabel, ListItemProps, Chip, Grid, Card, CardActionArea, CardContent,
    IconButton, FormHelperText, Alert, AlertTitle
} from '@mui/material'
import { LoadingButton } from '@mui/lab'
import cronstrue from 'cronstrue'
import { Observer, observer } from 'mobx-react-lite'
import { Link, LinkProps } from 'react-router-dom'
import { MapLevelItem, useStore } from '../store'
import { ListLayout, FileField, SelectField } from '../components'
import { GameEngineFragment, GameHeaderFragment } from '../schema'
import { Role, ForRole } from '../auth'
import { List, forFileField, forFormControl, forFormHelperText, forSelectField, forTextField } from '../store/forms'

import ClearIcon from '@mui/icons-material/Clear'

export function HomePage() {
    const { games, newGame } = useStore()

    const actions = (
        <ForRole role={Role.GameMaster}>
            <Button color='primary' size='large' onClick={newGame.dialog.open}>New Game</Button>
        </ForRole>
    )

    return (
        <>
            <ListLayout<GameHeaderFragment> title='Games' actions={actions} operation={games} items={games}>
                { item => <GameItem key={item.id} game={item} /> }
            </ListLayout>
            <ObservableNewGameDialog />
        </>
    )

    // React.useEffect(() => { home.load() }, [])

    // return <Container>
    //     <PageTitle title='Games'
    //         actions={<ForRole role={Role.GameMaster}>
    //                     <Button variant='outlined' color='primary' size='large' onClick={newGame.open}>New game</Button>
    //                 </ForRole>} />
    //     <Paper elevation={0}>
    //         <input type='file' ref={home.setFileUpload} style={{ display: 'none' }} onChange={home.uploadFile} />
    //         <Grid container spacing={2}>
    //             <Observer>
    //                 {() => {
    //                     const games = home.games.value

    //                     return games.length
    //                         ? <>{games.map(x => <GameItem key={x.id} game={x} />)}</>
    //                         : <EmptyListItem>
    //                             <Stack alignItems='center'>
    //                                 <Typography variant='h6'>Empty</Typography>
    //                                 <Button variant='outlined' color='primary' onClick={newGame.open}>Create first game</Button>
    //                             </Stack>
    //                         </EmptyListItem>

    //                 }}
    //             </Observer>
    //         </Grid>
    //     </Paper>
    //     <ObservableNewGameDialog />
    // </Container>
}

interface GameItemProps {
    game: GameHeaderFragment
}

function GameItem({ game }: GameItemProps) {
    const { home, gameDetails } = useStore()

    const props: ListItemProps & Partial<LinkProps> = { }

    const playerJoind = !!game.me
    const playerFactionKnown = !!game.me?.number

    const cron = game.options.schedule
        ? `${cronstrue.toString(game.options.schedule, { use24HourTimeFormat: true })} (${game.options.timeZone ?? 'UTC'})`
        : null
// onClick={() => playerJoind ? home.triggerUploadReport(game.me.id) : home.joinGame(game.id)}
    return <Grid item xs={12} sm={6} md={4}>
        <Card>
            <CardActionArea component={Link} to={`/games/${game.id}`}>
                <CardContent>
                    <Stack direction='row' justifyContent='space-between' alignItems='center'>
                        <Typography variant='h4' gutterBottom>{game.name}</Typography>
                        { playerJoind && <Chip label='Joined' size='small' color='success' /> }
                    </Stack>
                    <Typography variant='caption'>{cron}</Typography>

                </CardContent>
            </CardActionArea>
        </Card>
    </Grid>
}

/* <CardActions>
    <Observer>
        {() => playerJoind
            ? <SplitButton disabled={home.uploading} color='secondary' size='small' variant='outlined'
            onClick={() => home.triggerUploadReport(game.me.id)}
            actions={[
                { content: 'Import map', onAction: () => home.triggerImportMap(game.me.id) },
                '---',
                { content: 'Update Ruleset', onAction: () => home.triggerRuleset(game.id) },
                { content: 'Delete', onAction: () => home.deleteGame(game.id) },
            ]}>
                    Upload report
            </SplitButton>
            : null }
    </Observer>

</CardActions> */
/* <Stack flex={1} gap={2} sx={{ p: 2}}>
    <Stack direction='row' alignItems='flex-start' justifyContent='space-between' gap={2}>
        <Stack>
        </Stack>
    </Stack>

    <Box>
        <Stack flex={1} gap={2} direction='row' alignItems='center' justifyContent='space-between' minWidth={0}>
            <Stack direction='row' gap={2}>
                <Chip label='Pending' size='small' color='warning' />
                <Typography variant='overline'>Starting in 35 days</Typography>
            </Stack>
            <Button>0 Players</Button>
        </Stack>
    </Box>
</Stack> */

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
    const { newGame } = useStore()
    const { engines, form, dialog, operation } = newGame

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
                    <FileField label='game.in' {...forFileField(form.files.game)} />
                    <FileField label='players.in' {...forFileField(form.files.players)} />
                </> }

                <MapLevelEditor model={form.levels} onAdd={newGame.addLevel} onRemove={newGame.removeLevel} />

                <TextField label='Schedule' {...forTextField(form.schedule)} helperText={newGame.scheduleHelpText} />

                <SelectField label='Time Zone' items={newGame.timeZones} {...forSelectField(form.timeZone)} />
            </Stack>
        </DialogContent>

        <DialogActions>
            <Button variant='text'  disabled={operation.isLoading} onClick={dialog.close}>Cancel</Button>
            <LoadingButton loading={operation.isLoading} color='primary' onClick={newGame.confirm}>Add new game</LoadingButton>
        </DialogActions>
    </Dialog>
}

const ObservableNewGameDialog = observer(NewGameDialog)

