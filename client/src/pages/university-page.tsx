import * as React from 'react'
import { styled } from '@mui/material/styles'
import { Link, useParams } from 'react-router-dom'
import { GameRouteParams } from './game-route-params'
import { useStore } from '../store'
import { observer, Observer } from 'mobx-react-lite'
import {
    Box,
    AppBar,
    Button,
    ButtonGroup,
    Container,
    IconButton,
    List,
    ListItem,
    ListItemText,
    Paper,
    TextField,
    Toolbar,
    Typography,
} from '@mui/material'
import { UniversityLocation } from '../components'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'

const NewUniversity = observer(() => {
    const { university } = useStore()

    const nu = university.newUniversity

    React.useEffect(() => nu.clear(), [])

    return (
        <Container component='main' maxWidth='xs'>
            <Box sx={{
                marginTop: 8,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center'
            }}>
                <Typography component='h1' variant='h5'>Open new magic university</Typography>
                <TextField
                    variant='outlined'
                    margin='normal'
                    required
                    fullWidth
                    id='name'
                    label='University name'
                    name='name'
                    autoFocus
                    value={nu.name}
                    onChange={nu.setName}
                />
                <Button sx={{
                    margin: [3, 0, 2],
                }} fullWidth variant='contained' color='primary' onClick={nu.open}>Open</Button>
            </Box>
        </Container>
    )
})

const StudySchedule = styled('table')`
    border-collapse: collapse;
    height: 100%;

    font-size: 11px;
    font-family: Fira Code, Roboto Mono, monospace;

    td, th {
        white-space: nowrap;
    }

    td {
        border: 1px solid silver;
        text-align: right;
    }

    th {
        padding: 4px;
        text-align: left;
    }

    .empty {
        border: none;
    }

    .skill-level {
        background-color: #e0e0e0;
    }

    .skill {
        padding: 0;

        .level {
            display: inline-block;
            padding: .25rem;
            margin: 2px;
            background-color: #e0e0e0;
        }

        .days {
            display: inline-block;
            padding: .25rem;
        }
    }

    .faction {
        min-width: 100px;
        padding: 4px;
        background-color: white;
    }

    .unit {
        min-width: 100px;
        padding: 4px;
        background-color: white;
    }

    .orders {
        min-width: 50px;
        padding: 4px;
        background-color: white;
    }

    .target {
        min-width: 50px;
        padding: 4px;
        background-color: white;
    }

    .location {
        padding-top: 2rem;
        padding: 4px;
    }
`

const MemberList = observer(() => {
    const { university: { members } } = useStore()

    return <>
        { members.map(x => <ListItem key={x.player.id}>
            <ListItemText primary={`${x.player.factionName} (${x.player.factionNumber})`} secondary={x.role} />
        </ListItem>) }
    </>
})

function Members() {
    return <Paper>
        <List>
            <MemberList />
        </List>
    </Paper>
}

const University = observer(() => {
    const { university } = useStore()

    return (
        <Container component='main' maxWidth={false}>
            <AppBar position='static' color='primary'>
                <Toolbar>
                    <IconButton component={Link} to='/' edge='start' color='inherit' size="large">
                        <ArrowBackIcon />
                    </IconButton>
                    <Typography variant='h6'>{ university.name }</Typography>
                </Toolbar>
            </AppBar>
            <Box sx={{
                marginTop: 8,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'flex-start',
            }}>
                Turns:
                <ButtonGroup disableElevation>
                    { university.classes.map(x => <Button
                        size='small'
                        variant={x.id === university.selectedClassId ? 'contained' : 'outlined' }
                        key={x.id}
                        onClick={() => university.selectClass(x.id)}>{x.turnNumber}</Button> ) }
                </ButtonGroup>

                <StudySchedule>
                    { university.locations.map(location => <UniversityLocation key={location.id} location={location} />) }
                </StudySchedule>

                <Members />
            </Box>
        </Container>
    );
})

export function UniversityPage() {
    const { gameId } = useParams<GameRouteParams>()
    const { university } = useStore()

    React.useEffect(() => {
        university.load(gameId)
    }, [ gameId ])

    return <Observer>
        {() => university.loading
            ? <div>Loading...</div>
            : university.id
                ? <University />
                : <NewUniversity />
        }
    </Observer>
}

