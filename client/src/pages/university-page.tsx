import * as React from 'react'
import styled from 'styled-components'
import { Link, useParams } from 'react-router-dom'
import { GameRouteParams } from './game-route-params'
import { Student, Skill, useStore } from '../store'
import { observer, Observer } from 'mobx-react-lite'
import { Box, Button, ButtonGroup, ButtonProps, Container, List, ListItem, ListItemText, makeStyles, Paper, TextField, Typography } from '@material-ui/core'
import { useCallbackRef } from '../lib'
import ClipboardJS from 'clipboard'
import { SkillCell } from '../components/skill-cell'

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

const XsButton = styled(Button).attrs({
    size: 'small',
    variant: 'outlined'
})`
    padding: 2px !important;
    min-width: 20px !important;
    font-size: 11px;
    font-family: Fira Code, Roboto Mono, monospace;
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

interface CopyButtonProps extends ButtonProps {
    text: string
}

function CopyButton({ text, ...props }: CopyButtonProps) {
    const [ref, setRef] = useCallbackRef<HTMLButtonElement>()

    React.useEffect(() => {
        if (!ref) return

        const clip = new ClipboardJS(ref)

        return () => clip.destroy()
    }, [ ref ])

    return <XsButton {...props as any} ref={setRef as any} data-clipboard-text={text} />
}

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
                            <th className='location' colSpan={20}>{x.terrain} ({x.x},{x.y},{x.z} {x.label}) in {x.province}{ x.settlement ? `, contains ${x.settlement} [${x.settlementSize.toLowerCase()}]` : '' }</th>
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
                            <td className='unit'>
                                {student.name} ({student.number})
                                <Box ml={1} clone>
                                    <ButtonGroup>
                                        <XsButton title='Study' onClick={student.beginStudy}>S</XsButton>
                                        <XsButton title='Teach'>T</XsButton>
                                        <XsButton title='Clear' onClick={student.clearOrders}>X</XsButton>
                                    </ButtonGroup>
                                </Box>
                            </td>
                            <td className='target'>
                                { student.target
                                    ? <ButtonGroup>
                                        <XsButton onClick={student.beginTargetSelection}>{student.target.code}</XsButton>
                                        <XsButton className='skill-level' onClick={student.incTargetLevel}>{student.target.level}</XsButton>
                                    </ButtonGroup>
                                    : <XsButton fullWidth onClick={student.beginTargetSelection}><i>none</i></XsButton>
                                }
                            </td>
                            <td className='orders'>
                                { student.ordersShort && <CopyButton fullWidth text={student.ordersFull}>{student.ordersShort}</CopyButton> }
                            </td>
                            { student.skillsGroups.map((group, i) => (
                                <React.Fragment key={i}>
                                    <td className='empty'></td>
                                    { group.skills.map(skill => (
                                        <SkillCell key={skill.code}
                                            active={student.isSkillActive(skill.code)}
                                            title=''
                                            days={skill.days}
                                            level={skill.level}
                                            target={student.isTargetSkill(skill.code)}
                                            missing={student.getMissingLevel(skill.code)}
                                            studying={student.study == skill.code}
                                            withTeacher={false}
                                            onClick={() => student.skillClick(skill.code)}
                                        />) ) }
                                </React.Fragment>) ) }
                        </tr>)}
                    </tbody> ) }
                </StudySchedule>
                <Members />
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

