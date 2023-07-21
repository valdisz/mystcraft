import * as React from 'react'
import { ListItemText, TextField, Button, Container,
    DialogTitle, DialogContent, DialogActions, Dialog, Box, Typography, Stack, FormControl, Select,
    InputLabel, MenuItem, ListItemProps, Chip, Grid, Card, CardActionArea, CardContent,
    FormControlLabel, Switch, IconButton, FormHelperText
} from '@mui/material'
import { Observer, observer } from 'mobx-react-lite'
import { Link, LinkProps } from 'react-router-dom'
import { MapLevelItem, NewGameEngineViewModel, useStore } from '../store'
import { ListLayout, FileField } from '../components'
import { GameEngineFragment, GameHeaderFragment } from '../schema'
import cronstrue from 'cronstrue'
import { Role, ForRole } from '../auth'

import AttachFileIcon from '@mui/icons-material/AttachFile'
import ClearIcon from '@mui/icons-material/Clear'
import { FieldList } from '../store/forms'

export function HomePage() {
    const { games, newGame } = useStore()

    const actions = (
        <ForRole role={Role.GameMaster}>
            <Button color='primary' size='large' onClick={newGame.open}>New Game</Button>
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
                <TextField label='Label' required {...model.label.forTextField} />
                <TextField label='Width' required {...model.width.forTextField}  />
                <TextField label='Height' required {...model.height.forTextField}  />
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
    model: FieldList<MapLevelItem>
    onAdd: () => void
    onRemove: (item: MapLevelItem, index: number) => void
}

function MapLevelEditor({ model, onAdd, onRemove }: MapLevelEditorProps) {
    return <Observer>
        {() =>
            <FormControl margin='dense' {...model.forFormControl}>
                <InputLabel shrink>Levels</InputLabel>
                <Stack mt={2}>
                    {model.map((item, index) => <MapLevelItemEditor key={index} model={item} onRemove={() => onRemove(item, index)} />)}
                    <Box>
                        <Button variant='outlined' onClick={onAdd}>Add level</Button>
                    </Box>
                </Stack>
                <FormHelperText {...model.forFormHelperText} />
            </FormControl>
        }
    </Observer>
}

function NewGameDialog() {
    const { newGame, engines } = useStore()
    const form = newGame.form

    const renderSelectedEngine = (engineId: any) => <ListItemText primary={engines.find(x => x.id === engineId).name} />
    const renderEngine = (engine: GameEngineFragment) => <ListItemText primary={engine.name} secondary={engine.description} />

    return <Dialog fullWidth maxWidth='sm' open={newGame.isOpen} onClose={newGame.cancel}>
        <DialogTitle>New game</DialogTitle>
        <DialogContent>
            <Stack gap={2}>
                <TextField autoFocus label='Name' {...form.name.forTextField} />

                <FormControl {...form.engine.forFormControl}>
                    <InputLabel {...form.engine.forInputLabel}>Engine</InputLabel>
                    <Select label='Engine' renderValue={renderSelectedEngine} {...form.engine.forSelect}>
                        { engines.value.map(engine => <MenuItem key={engine.id} value={engine.id}>
                            { renderEngine(engine) }
                        </MenuItem>) }
                    </Select>
                    <FormHelperText {...form.engine.forFormHelperText} />
                </FormControl>

                <FormControlLabel control={<Switch checked={newGame.remote} onChange={newGame.remoteChange} />} label="Remote" />

                { !newGame.remote && <>
                    <FileField label='game.in' {...form.gameFile.forFileField} />
                    <FileField label='players.in' {...form.playersFile.forFileField} />
                </> }

                <MapLevelEditor model={form.levels} onAdd={newGame.addLevel} onRemove={newGame.removeLevel} />

                <TextField label='Schedule' {...form.schedule.forTextField} />

                <FormControl {...form.timeZone.forFormControl}>
                    <InputLabel id='timeZoneLabel' {...form.timeZone.forInputLabel}>Time Zone</InputLabel>
                    <Select labelId='timeZoneLabel' label='Time Zone' {...form.timeZone.forSelect}>
                        { newGame.timeZones.map((x, i) => <MenuItem key={i} value={x}>{x}</MenuItem>) }
                    </Select>
                    <FormHelperText {...form.timeZone.forFormHelperText} />
                </FormControl>
            </Stack>
        </DialogContent>

        <DialogActions>
            <Button variant='text' onClick={newGame.cancel}>Cancel</Button>
            <Button color='primary' onClick={newGame.confirm}>Add new game</Button>
        </DialogActions>
    </Dialog>
}

const ObservableNewGameDialog = observer(NewGameDialog)
