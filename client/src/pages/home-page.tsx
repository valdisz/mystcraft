import * as React from 'react'
import { List, ListItem, ListItemText, ListItemIcon, TextField, Button, Container, Card,
    ListItemSecondaryAction, DialogTitle, DialogContent, DialogActions, Dialog, Box, Grid, Typography, Stack, Paper, FormControl, Select, InputLabel, MenuItem, ListItemProps, Accordion, AccordionSummary, CardActions, Chip
} from '@mui/material'
import { Observer, observer } from 'mobx-react'
import { Link, LinkProps } from 'react-router-dom'
import { useStore } from '../store'
import { SplitButton, PageTitle, EmptyListItem, FileInput, ExpandMore } from '../components'
import { GameHeaderFragment, PlayerHeaderFragment } from '../schema'
import cronstrue from 'cronstrue'
import { Role, ForRole } from '../auth'

import AttachFileIcon from '@mui/icons-material/AttachFile'

export function HomePage() {
    const { home, newGame } = useStore()

    React.useEffect(() => { home.load() }, [])

    return <Container>
        <PageTitle title='Games'
            actions={<ForRole role={Role.GameMaster}>
                        <Button variant='outlined' color='primary' size='large' onClick={newGame.open}>New game</Button>
                    </ForRole>} />
        <Paper elevation={0} variant='outlined'>
            <input type='file' ref={home.setFileUpload} style={{ display: 'none' }} onChange={home.uploadFile} />
            <List component='nav' dense disablePadding>
                <Observer>
                    {() => <>{ home.games.length
                        ? home.games.map(x => <GameItem key={x.id} game={x} />)
                        : <EmptyListItem>
                            <Stack alignItems='center'>
                                <Typography variant='h6'>Empty</Typography>
                                <Button variant='outlined' color='primary' onClick={newGame.open}>Create first game</Button>
                            </Stack>
                        </EmptyListItem>
                        }</>
                    }
                </Observer>
            </List>
        </Paper>
        <ObservableNewGameDialog />
    </Container>
}

interface GameItemProps {
    game: GameHeaderFragment
}

function GameItem({ game }: GameItemProps) {
    const { home } = useStore()

    const props: ListItemProps & Partial<LinkProps> = { }

    const playerJoind = !!game.me
    const playerFactionKnown = !!game.me?.number

    const cron = game.options.schedule
        ? `${cronstrue.toString(game.options.schedule, { use24HourTimeFormat: true })} (${game.options.timeZone ?? 'UTC'})`
        : null

    return <ListItem disablePadding disableGutters>
        <Stack flex={1} gap={2} sx={{ p: 2}}>
            <Stack direction='row' alignItems='flex-start' justifyContent='space-between' gap={2}>
                <Stack>
                    <Typography variant='h4'>{game.name}</Typography>
                    <Typography variant='caption'>{cron}</Typography>
                </Stack>
                <Observer>
                    {() => playerJoind
                        ? <SplitButton disabled={home.uploading} color='secondary' size='small' variant='outlined'
                            onClick={() => home.triggerUploadReport(game.me.id)}
                            actions={[
                                { content: 'Import map', onAction: () => home.triggerImportMap(game.me.id) },
                                { content: 'Delete', onAction: () => home.deleteGame(game.id) },
                                { content: 'Update Ruleset', onAction: () => home.triggerRuleset(game.id) }
                            ]}>
                                Upload report
                        </SplitButton>
                        : <Button color='primary' size='small' variant='outlined' onClick={() => home.joinGame(game.id)}>Join</Button> }
                </Observer>
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
        </Stack>
    </ListItem>
}

function NewGameDialog() {
    const { newGame, gameEngines } = useStore()

    React.useEffect(() => gameEngines.load(), [ ])

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

                <FormControl fullWidth>
                    <InputLabel>Engine</InputLabel>
                    <Select label="Engine" value={newGame.engine} onChange={newGame.setEngine}>
                        { gameEngines.engines.map(x => <MenuItem key={x.id} value={x.id}>
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
