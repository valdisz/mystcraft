import * as React from 'react'
import styled from 'styled-components'
import { Link, useParams } from 'react-router-dom'
import { GameRouteParams } from './game-route-params'
import { Student, useStore } from '../store'
import { observer, Observer } from 'mobx-react-lite'
import { Button, Container, makeStyles, TextField, Typography } from '@material-ui/core'

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

    font-size: 75%;
    font-family: Fira Code, Roboto Mono, monospace;

    td, th {
        padding: 4px;
        white-space: nowrap;
    }

    td {
        border: 1px solid silver;
    }

    th {
        text-align: left;
    }

    .empty {
        border: none;
    }

    .skill {
        padding: 0;

        .level {
            display: inline-block;
            padding: .25rem;
            margin: 2px;
            background-color: silver;
        }

        .days {
            display: inline-block;
            padding: .25rem;
        }
    }

    .faction {
        min-width: 100px;
    }

    .unit {
        min-width: 100px;
    }

    .orders {
        min-width: 50px;
    }

    .target {
        min-width: 50px;
    }
`

const University = observer(() => {
    const { university } = useStore()
    const classes = useStyles()

    return (
        <Container component='main' maxWidth={false}>
            <div className={classes.paper2}>
                <Typography component='h1' variant='h5'>{university.name}</Typography>
                Turns:
                <ul>
                    { university.classes.map(x => <li key={x.id}>{x.turnNumber}</li> ) }
                </ul>

                <StudySchedule>
                    { university.locations.map(x => <tbody key={x.id}>
                        <tr>
                            <th colSpan={20}>{x.terrain} ({x.x},{x.y},{x.z} {x.label}) in {x.province}{ x.settlement ? `, contains ${x.settlement} [${x.settlementSize.toLowerCase()}]` : '' }</th>
                        </tr>
                        <tr>
                            <th className='faction'>Faction</th>
                            <th className='unit'>Unit</th>
                            <th className='target'>Target</th>
                            <th className='orders'>Orders</th>

                            { university.skills.map((group, i) => <React.Fragment key={i}>
                                <th className='empty'></th>
                                { group.skills.map(({ code }) => <th key={code}>{code}</th> ) }
                            </React.Fragment> ) }
                        </tr>
                        { x.students.map(student => <tr key={student.id}>
                            <td className='faction'>{student.factionName} ({student.factionNumber})</td>
                            <td className='unit'>{student.name} ({student.number})</td>
                            <td className='target'>
                                { student.target && <>
                                        <div className='level'>{student.target.level}</div>
                                        <div className='days'>{student.target.code}</div>
                                    </>}
                            </td>
                            <td className='orders'>{student.orders}</td>
                            { student.skills.map((group, i) => <React.Fragment key={i}>
                                <td className='empty'></td>
                                { group.skills.map(({ code, level, days }) => <td key={code} className='skill'>
                                    { days && <>
                                        <div className='level'>{level}</div>
                                        <div className='days'>{days}</div>
                                    </> }
                                </td> ) }
                            </React.Fragment> ) }
                        </tr>)}
                    </tbody> ) }
                </StudySchedule>
            </div>
        </Container>
    )
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

