import * as React from 'react'
import {
    Typography,
    Container,
    Table,
    TableHead,
    TableRow,
    TableCell,
    TableBody,
    Grid,
    Tabs,
    Tab,
    Tooltip,
} from '@mui/material';
import { styled } from '@mui/material/styles'
import { useStore } from '../store'
import { Observer } from 'mobx-react-lite'
import { SkillInfo } from '../game/skill-info'
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

const WideTooltip = styled(Tooltip)`
    max-width: 500px;
`

function SkillsTab() {
    const { stats } = useStore()

    return <Table size='small' stickyHeader={true}>
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
                        <WideTooltip title={<SkillInfoTooltip info={skill} level={1} />}>
                            <span>{skill.name}</span>
                        </WideTooltip>
                    </TableCell>
                    { [0, 1, 2, 3, 4, 5].map(x => <TableCell key={x}>
                        <WideTooltip title={<SkillInfoTooltip info={skill} level={x} />}>
                            <span>{levels[x]}</span>
                        </WideTooltip>
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

    return <Table size='small' stickyHeader={true}>
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
                        <TurnCell colSpan={6}>Turn {turn.turn}</TurnCell>
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

    return <Table size='small' stickyHeader={true}>
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
                        <TurnCell colSpan={stats.allianceProducts.length + 1}>Turn {turn.turn}</TurnCell>
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
    const { stats } = useStore()
    const { path, url } = useRouteMatch()

    React.useEffect(() => { stats.loadAllianceStats() }, [ ])

    return <Container component='main' maxWidth={false}>
        <Typography variant='h4'>Statistics</Typography>
        <Grid container>
            <Grid item xs={12}>
                <Observer>
                    {() => <Tabs value={url}>
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
