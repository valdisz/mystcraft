import * as React from 'react'
import styled from 'styled-components'
import { Typography, Container, Table, TableHead, TableRow, TableCell, TableBody, Grid, Tabs, Tab, Tooltip, makeStyles } from '@material-ui/core'
import { useStore } from '../store'
import { Observer } from 'mobx-react-lite'
import { SkillInfo } from '../store/game/skill-info'
import { Link, useParams, Switch, Route, useLocation, useRouteMatch } from 'react-router-dom'

interface SkillInfoTooltipProps {
    info: SkillInfo
    level: number
}

function SkillInfoTooltip({ info, level }: SkillInfoTooltipProps) {
    return <>
        <Typography variant='h6'>{info.name}</Typography>
        <Typography variant='body2'>
            <strong>Level: {level}</strong>
        </Typography>
        { level
            ? <Typography variant='body2'>
                {info.description[level - 1]}
            </Typography>
            : null
        }
    </>
}

const useStyles = makeStyles((theme) => ({
    wideTooltip: {
      maxWidth: 500,
    },
}))

function SkillsTab() {
    const { stats } = useStore()
    const classes = useStyles()

    return <Table size='small' stickyHeader>
        <TableHead>
            <TableRow>
                <TableCell>Skill</TableCell>
                <TableCell>0</TableCell>
                <TableCell>1</TableCell>
                <TableCell>2</TableCell>
                <TableCell>3</TableCell>
                <TableCell>4</TableCell>
                <TableCell>5</TableCell>
                <TableCell>Total</TableCell>
            </TableRow>
        </TableHead>
        <Observer>
            {() => <TableBody>
                { stats.skills.map(({ skill, levels, total }) => <TableRow key={skill.code}>
                    <TableCell>
                        <Tooltip title={<SkillInfoTooltip info={skill} level={1} />} classes={{ tooltip: classes.wideTooltip }}>
                            <span>{skill.name}</span>
                        </Tooltip>
                    </TableCell>
                    { [0, 1, 2, 3, 4, 5].map(x => <TableCell key={x}>
                        <Tooltip title={<SkillInfoTooltip info={skill} level={x} />} classes={{ tooltip: classes.wideTooltip }}>
                            <span>{levels[x]}</span>
                        </Tooltip>
                    </TableCell>) }
                    <TableCell>{total}</TableCell>
                </TableRow>) }
            </TableBody> }
        </Observer>
    </Table>
}

const TurnCell = styled(TableCell)`
    font-weight: bold;
`

function AllianceIncome() {
    const { stats } = useStore()

    return <Table size='small' stickyHeader>
        <TableHead>
            <TableRow>
                <TableCell>Faction</TableCell>
                <TableCell>Work</TableCell>
                <TableCell>Trade</TableCell>
                <TableCell>Pillage</TableCell>
                <TableCell>Tax</TableCell>
                <TableCell>Total</TableCell>
            </TableRow>
        </TableHead>
        <Observer>
            {() => <>
                { stats.allianceStats.map(turn => <TableBody key={turn.turn}>
                    <TableRow>
                        <TurnCell variant='head' colSpan={6}>Turn {turn.turn}</TurnCell>
                    </TableRow>
                    { turn.factions.map(f => <TableRow hover key={f.factionNumber}>
                        <TableCell>{f.factionName} ({f.factionNumber})</TableCell>
                        <TableCell>{f.income?.work}</TableCell>
                        <TableCell>{f.income?.trade}</TableCell>
                        <TableCell>{f.income?.pillage}</TableCell>
                        <TableCell>{f.income?.tax}</TableCell>
                        <TableCell>{f.income?.total}</TableCell>
                    </TableRow>)}
                </TableBody>)}
            </> }
        </Observer>
    </Table>
}

function AllianceProduction() {
    const { stats } = useStore()

    return <Table size='small' stickyHeader>
        <Observer>
            {() => <>
            <TableHead>
                <TableRow>
                    <TableCell>Faction</TableCell>
                    { stats.allianceProducts.map(p => <TableCell key={p}>{p}</TableCell>) }
                </TableRow>
            </TableHead>
                { stats.allianceStats.map(turn => <TableBody key={turn.turn}>
                    <TableRow>
                        <TurnCell variant='head' colSpan={stats.allianceProducts.length + 1}>Turn {turn.turn}</TurnCell>
                    </TableRow>
                    { turn.factions.map(f => <TableRow hover key={f.factionNumber}>
                        <TableCell>{f.factionName} ({f.factionNumber})</TableCell>

                        { f.production.map(p => <TableCell key={p.code}>{p.amount || ' '}</TableCell>) }
                    </TableRow>)}
                </TableBody>)}
            </> }
        </Observer>
    </Table>
}

export function StatsPage() {
    const { game, stats } = useStore()
    const { path, url } = useRouteMatch()

    React.useEffect(() => { stats.loadAllianceStats() }, [ ])

    return <Container component='main' maxWidth={false}>
        <Typography variant='h4'>Statistics</Typography>
        <Grid container>
            <Grid item xs={12}>
                <Observer>
                    {() => <Tabs value={stats.tab} onChange={(_, value) => stats.setTab(value)}>
                        <Tab label='Skills' component={Link} value={url} to={url} />
                        <Tab label='Alliance income' component={Link} value={`${url}/alliance-income`} to={`${url}/alliance-income`} />
                        <Tab label='Alliance production' component={Link} value={`${url}/alliance-production`} to={`${url}/alliance-production`} />
                    </Tabs> }
                </Observer>
            </Grid>
            <Switch>
                <Route path={`${path}/alliance-income`}>
                    <Grid item xs={4}>
                        <AllianceIncome />
                    </Grid>
                </Route>
                <Route path={`${path}/alliance-production`}>
                    <Grid item xs={12}>
                        <AllianceProduction />
                    </Grid>
                </Route>
                <Route path={path}>
                    <Grid item xs={4}>
                        <SkillsTab />
                    </Grid>
                </Route>
            </Switch>
        </Grid>
    </Container>
}
