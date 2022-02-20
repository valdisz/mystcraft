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

function IncomeTab() {
    const { stats } = useStore()

    return <Table size='small' stickyHeader={true}>
        <TableHead>
            <TableRow>
                <TableCell></TableCell>
                { stats.stats.map(x => <TableCell key={x.number}>{x.number}</TableCell>) }
            </TableRow>
        </TableHead>
        <Observer>
            {() => <TableBody>
                <TableRow>
                    <TableCell>Work</TableCell>
                    { stats.stats.map(x => <TableCell key={x.number}>{x.stats.income.work}</TableCell>) }
                </TableRow>
                <TableRow>
                    <TableCell>Trade</TableCell>
                    { stats.stats.map(x => <TableCell key={x.number}>{x.stats.income.trade}</TableCell>) }
                </TableRow>
                <TableRow>
                    <TableCell>Pillage</TableCell>
                    { stats.stats.map(x => <TableCell key={x.number}>{x.stats.income.pillage}</TableCell>) }
                </TableRow>
                <TableRow>
                    <TableCell>Tax</TableCell>
                    { stats.stats.map(x => <TableCell key={x.number}>{x.stats.income.tax}</TableCell>) }
                </TableRow>
                <TableRow>
                    <TableCell>Total</TableCell>
                    { stats.stats.map(x => <TableCell key={x.number}>{x.stats.income.total}</TableCell>) }
                </TableRow>
            </TableBody> }
        </Observer>
    </Table>
}

// function ProductionTab() {
//     const { stats } = useStore()

//     return <Table size='small' stickyHeader={true}>
//         <Observer>
//             {() => <>
//             <TableHead>
//                 <TableRow>
//                     <TableCell>Faction</TableCell>
//                     { stats.allianceProducts.map(p => <TableCell key={p}>{p}</TableCell>) }
//                 </TableRow>
//             </TableHead>
//                 { stats.stats.map(turn => <TableBody key={turn.turn}>
//                     <TableRow>
//                         <TurnCell colSpan={stats.allianceProducts.length + 1}>Turn {turn.turn}</TurnCell>
//                     </TableRow>
//                     { turn.factions.map(f => <TableRow hover key={f.factionNumber}>
//                         <TableCell>{f.factionName} ({f.factionNumber})</TableCell>

//                         { f.production.map(p => <TableCell key={p.code}>{p.amount || ' '}</TableCell>) }
//                     </TableRow>)}
//                 </TableBody>)}
//             </> }
//         </Observer>
//     </Table>
// }

export function StatsPage() {
    const { stats } = useStore()
    const { path, url } = useRouteMatch()

    React.useEffect(() => { stats.loadStats() }, [ ])

    return <Container component='main' maxWidth={false}>
        <Typography variant='h4'>Statistics</Typography>
        <Grid container>
            <Grid item xs={12}>
                <Observer>
                    {() => <Tabs value={url}>
                        <Tab label='Skills' component={Link} value={url} to={url} />
                        <Tab label='Income' component={Link} value={`${url}/income`} to={`${url}/income`} />
                        {/* <Tab label='Production' component={Link} value={`${url}/production`} to={`${url}/production`} /> */}
                    </Tabs> }
                </Observer>
            </Grid>
            <Switch>
                <Route path={`${path}/income`}>
                    <Grid item xs={4}>
                        <IncomeTab />
                    </Grid>
                </Route>
                {/* <Route path={`${path}/production`}>
                    <Grid item xs={12}>
                        <ProductionTab />
                    </Grid>
                </Route> */}
                <Route path={path}>
                    <Grid item xs={4}>
                        <SkillsTab />
                    </Grid>
                </Route>
            </Switch>
        </Grid>
    </Container>
}
