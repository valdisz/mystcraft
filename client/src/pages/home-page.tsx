import * as React from 'react'
import { List, ListItem, ListItemText, TextField, Button, Container,
    DialogTitle, DialogContent, DialogActions, Dialog, Box, Typography, Stack, Paper, FormControl, Select,
    InputLabel, MenuItem, ListItemProps, Chip, Grid, Card, CardActionArea, CardContent, CardActions,
    FormGroup, FormControlLabel, Switch
} from '@mui/material'
import { Observer, observer } from 'mobx-react'
import { Link, LinkProps } from 'react-router-dom'
import { MapLevelItem, useStore } from '../store'
import { SplitButton, PageTitle, EmptyListItem, FileInput } from '../components'
import { GameHeaderFragment } from '../schema'
import cronstrue from 'cronstrue'
import { Role, ForRole } from '../auth'

import AttachFileIcon from '@mui/icons-material/AttachFile'

export function HomePage() {
    const { home, newGame } = useStore()

    // React.useEffect(() => { home.load() }, [])

    return <Container>
        <PageTitle title='Games'
            actions={<ForRole role={Role.GameMaster}>
                        <Button variant='outlined' color='primary' size='large' onClick={newGame.open}>New game</Button>
                    </ForRole>} />
        <Paper elevation={0}>
            <input type='file' ref={home.setFileUpload} style={{ display: 'none' }} onChange={home.uploadFile} />
            <Grid container spacing={2}>
                <Observer>
                    {() => {
                        const games = home.games.value

                        return games.length
                            ? <>{games.map(x => <GameItem key={x.id} game={x} />)}</>
                            : <EmptyListItem>
                                <Stack alignItems='center'>
                                    <Typography variant='h6'>Empty</Typography>
                                    <Button variant='outlined' color='primary' onClick={newGame.open}>Create first game</Button>
                                </Stack>
                            </EmptyListItem>

                    }}
                </Observer>
            </Grid>
        </Paper>
        <ObservableNewGameDialog />
    </Container>
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
                {/* <CardActions>
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

                </CardActions> */}
                {/* <Stack flex={1} gap={2} sx={{ p: 2}}>
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
                </Stack> */}
        </Card>
    </Grid>
}

interface MapLevelItemEditorProps {
    level: MapLevelItem
    onRemove: () => void
}

function _MapLevelItemEditor({ level, onRemove }: MapLevelItemEditorProps) {
    return <Stack direction='row' gap={2}>
        <TextField type='text' label='Label' value={level.label} onChange={level.setLabel} />
        <TextField type='text' label='Width' value={level.width} onChange={level.setWidth} />
        <TextField type='text' label='Height' value={level.height} onChange={level.setHeight} />
        <Button variant='outlined' color='error' onClick={onRemove}>Remove</Button>
    </Stack>
}

const MapLevelItemEditor = observer(_MapLevelItemEditor)

function _MapLevelEditor() {
    const { newGame } = useStore()

    return <Stack>
        {newGame.mapLevels.map((item, index) => <MapLevelItemEditor key={index} level={item} onRemove={() => newGame.removeMapLevel(index)} />)}
        <Button variant='outlined' color='primary' onClick={newGame.addLevel}>Add level</Button>
    </Stack>
}

const MapLevelEditor = observer(_MapLevelEditor)

function NewGameDialog() {
    const { newGame, gameEngines } = useStore()

    return <Dialog fullWidth maxWidth='sm' open={newGame.isOpen} onClose={newGame.cancel}>
        <DialogTitle>New game</DialogTitle>
        <DialogContent>
            <Stack gap={2}>
                <TextField type='text'
                    autoFocus
                    fullWidth
                    margin='dense'
                    helperText='A name of the new game'
                    value={newGame.name}
                    onChange={newGame.setName}
                    />

                <MapLevelEditor />

                <FormGroup>
                    <FormControlLabel control={<Switch checked={newGame.remote} onChange={newGame.remoteChange} />} label="Remote" />
                </FormGroup>

                { !newGame.remote && <>
                    <FormControl fullWidth>
                        <InputLabel>Engine</InputLabel>
                        <Select label="Engine" value={newGame.engine} onChange={newGame.setEngine}>
                            { gameEngines.engines.value.map(x => <MenuItem key={x.id} value={x.id}>
                                <ListItemText primary={x.name} />
                                <Typography variant='caption'>{x.createdAt}</Typography>
                            </MenuItem>) }
                        </Select>
                    </FormControl>

                    <Box sx={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between'
                    }}>
                        <Typography>
                            <span>File:{' '}</span>
                            <strong>{newGame.gameFileName}</strong>
                        </Typography>
                        <FileInput trigger={<Button variant='outlined' startIcon={<AttachFileIcon />}>Select game.in</Button>} onChange={files => newGame.setFile('game', files[0])} />
                    </Box>

                    <Box sx={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between'
                    }}>
                        <Typography>
                            <span>File:{' '}</span>
                            <strong>{newGame.playersFileName}</strong>
                        </Typography>
                        <FileInput trigger={<Button variant='outlined' startIcon={<AttachFileIcon />}>Select players.in</Button>} onChange={files => newGame.setFile('players', files[0])} />
                    </Box>
                </> }

                <TextField fullWidth multiline label='Options' rows={8} value={newGame.options} onChange={newGame.setOptions} />
            </Stack>
        </DialogContent>
        <DialogActions>
            <Button onClick={newGame.cancel}>
                Cancel
            </Button>
            <Button onClick={newGame.confirm} color='primary' variant='contained'>
                Start new game
            </Button>
        </DialogActions>
    </Dialog>
}
const ObservableNewGameDialog = observer(NewGameDialog)
