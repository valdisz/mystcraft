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
    TableFooter,
} from '@mui/material';
import { styled } from '@mui/material/styles'
import { useStore } from '../store'
import { Observer } from 'mobx-react'
import { SkillInfo } from '../game'
import { Link, Outlet } from 'react-router-dom'

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

export function SkillsTab() {
    const { stats } = useStore()

    return <Table size='small' stickyHeader={true}>
        <TableHead>
            <TableRow>
                <HeadingCell>Skill</HeadingCell>
                <HeadingCell>0</HeadingCell>
                <HeadingCell>1</HeadingCell>
                <HeadingCell>2</HeadingCell>
                <HeadingCell>3</HeadingCell>
                <HeadingCell>4</HeadingCell>
                <HeadingCell>5</HeadingCell>
                <HeadingCell>Total</HeadingCell>
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

const HeadingCell = styled(TableCell)`
    font-weight: bold;
`

export function IncomeTab() {
    const { stats } = useStore()

    return <Table size='small'>
        <TableHead>
            <TableRow sx={{ backgroundColor: 'palette.primary.main' }}>
                <HeadingCell>Turn</HeadingCell>
                { stats.stats.map(x => <HeadingCell key={x.number}>{x.number}</HeadingCell>) }
            </TableRow>
        </TableHead>
        <Observer>
            {() => <>
                <TableBody>
                    <TableRow>
                        <HeadingCell>Work</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.number}>{x.stats.income.work}</TableCell>) }
                    </TableRow>
                    <TableRow>
                        <HeadingCell>Trade</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.number}>{x.stats.income.trade}</TableCell>) }
                    </TableRow>
                    <TableRow>
                        <HeadingCell>Pillage</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.number}>{x.stats.income.pillage}</TableCell>) }
                    </TableRow>
                    <TableRow>
                        <HeadingCell>Tax</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.number}>{x.stats.income.tax}</TableCell>) }
                    </TableRow>
                </TableBody>
                <TableFooter>
                    <TableRow>
                        <HeadingCell>Total</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.number}>{x.stats.income.total}</TableCell>) }
                    </TableRow>
                </TableFooter>
            </> }
        </Observer>
    </Table>
}

export function ProductionTab() {
    const { stats } = useStore()

    return <Table size='small' stickyHeader={true}>
        <Observer>
            {() => <>
                <TableHead>
                    <TableRow sx={{ backgroundColor: 'palette.primary.main' }}>
                        <HeadingCell>Turn</HeadingCell>
                        { stats.stats.map(x => <HeadingCell key={x.number}>{x.number}</HeadingCell>) }
                    </TableRow>
                </TableHead>
                <TableBody>
                    { stats.products.map((p, i) => <TableRow key={p.code}>
                        <HeadingCell>{p.getName(2)}</HeadingCell>
                        { stats.stats.map(turn => <TableCell key={turn.number}>{turn.stats.production[i].amount}</TableCell>) }
                    </TableRow> )}
                </TableBody>
            </> }
        </Observer>
    </Table>
}

export function StatsPage() {
    const { stats } = useStore()

    React.useEffect(() => { stats.loadStats() }, [ ])

    return <Container component='main' maxWidth={false}>
        <Typography variant='h4'>Statistics</Typography>
        <Grid container>
            <Grid item xs={12}>
                <Tabs>
                    <Tab label='Skills' component={Link} to='' />
                    <Tab label='Income' component={Link} to='income' />
                    <Tab label='Production' component={Link} to='production' />
                </Tabs>
            </Grid>
            <Grid item xs={4}>
                <Outlet />
            </Grid>
        </Grid>
    </Container>
}
