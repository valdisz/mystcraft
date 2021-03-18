import * as React from 'react'
import styled from 'styled-components'
import { Link, useParams } from 'react-router-dom'
import { GameRouteParams } from './game-route-params'
import { useStore } from '../store'
import { observer, Observer } from 'mobx-react-lite'
import { Button, ButtonGroup, Container, List, ListItem, ListItemText, makeStyles, Paper, TextField, Typography } from '@material-ui/core'
import { UniversityLocation } from '../components'

const useStyles = makeStyles((theme) => ({
    paper: {
        marginTop: theme.spacing(8),
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
    },
    paper2: {
        marginTop: theme.spacing(8),
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'flex-start',
    },
    avatar: {
        margin: theme.spacing(1),
        backgroundColor: theme.palette.secondary.main,
    },
    form: {
        width: '100%', // Fix IE 11 issue.
        marginTop: theme.spacing(1),
    },
    submit: {
        margin: theme.spacing(3, 0, 2),
    },
}));

const NewUniversity = observer(() => {
    const { university } = useStore()
    const classes = useStyles()

    const nu = university.newUniversity

    React.useEffect(() => nu.clear(), [])

    return (
        <Container component='main' maxWidth='xs'>
            <div className={classes.paper}>
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
                <Button fullWidth variant='contained' color='primary' className={classes.submit} onClick={nu.open}>Open</Button>
            </div>
        </Container>
    )
})

const StudySchedule = styled.table`
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
    }

    .unit {
        min-width: 100px;
        padding: 4px;
    }

    .orders {
        min-width: 50px;
        padding: 4px;
    }

    .target {
        min-width: 50px;
        padding: 4px;
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
    const classes = useStyles()

    return (
        <Container component='main' maxWidth={false}>
            <div className={classes.paper2}>
                <Typography component='h1' variant='h5'>{university.name}</Typography>
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
            </div>
        </Container>
    )
})

export function UniversityPage() {
    console.log('route params', useParams<GameRouteParams>())
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

