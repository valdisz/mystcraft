import * as React from 'react'
import {
    Typography, Container, Table, TableHead, TableRow, TableCell, TableBody, Tabs, Tab, Tooltip, AppBar, IconButton, Toolbar
} from '@mui/material';
import { styled } from '@mui/material/styles'
import { useStore } from '../store'
import { Observer } from 'mobx-react'
import { SkillInfo } from '../game'
import { Link, Outlet, PathMatch, useLocation, useMatch, useResolvedPath } from 'react-router-dom'

import CloseIcon from '@mui/icons-material/Close'

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

const WideTooltip = styled(Tooltip)({
    maxWidth: '500px'
})

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
                { stats.skills.map(({ skill, levels, total }) => <TableRow key={skill.code} hover>
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

const TurnCell = styled(TableCell)({
    fontWeight: 'bold'
})

const HeadingCell = styled(TableCell)({
    fontWeight: 'bold'
})

const TotalsHeadingCell = styled(HeadingCell)({
    fontSize: '1.1rem'
})

const TotalsCell = styled(TableCell)({
    fontSize: '1.1rem'
})

function usage(income: number, expenses: number) {
    if (!expenses || !income) {
        return ''
    }

    const value = Number(expenses / income)
    return value.toLocaleString(undefined, { style: 'percent' })
}

export function IncomeTab() {
    const { stats } = useStore()

    return <Table size='small'>
        <TableHead>
            <TableRow sx={{ backgroundColor: 'palette.primary.main' }}>
                <HeadingCell>Turn</HeadingCell>
                { stats.stats.map(x => <TurnCell key={x.turnNumber}>{x.turnNumber}</TurnCell>) }
            </TableRow>
        </TableHead>
        <Observer>
            {() => <>
                <TableBody>
                    <TableRow hover>
                        <HeadingCell>Borrow</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.turnNumber}>0</TableCell>) }
                    </TableRow>
                    <TableRow hover>
                        <HeadingCell>Claim</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.turnNumber}>{x.income.claim}</TableCell>) }
                    </TableRow>
                    <TableRow hover>
                        <HeadingCell>Work</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.turnNumber}>{x.income.work}</TableCell>) }
                    </TableRow>
                    <TableRow hover>
                        <HeadingCell>Entertain</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.turnNumber}>{x.income.entertain}</TableCell>) }
                    </TableRow>
                    <TableRow hover>
                        <HeadingCell>Trade (sell)</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.turnNumber}>{x.income.trade}</TableCell>) }
                    </TableRow>
                    <TableRow hover>
                        <HeadingCell>Pillage</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.turnNumber}>{x.income.pillage}</TableCell>) }
                    </TableRow>
                    <TableRow hover>
                        <HeadingCell>Tax</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.turnNumber}>{x.income.tax}</TableCell>) }
                    </TableRow>
                    <TableRow hover>
                        <TotalsHeadingCell>Total Income</TotalsHeadingCell>
                        { stats.stats.map(x => <TotalsCell key={x.turnNumber}>{x.income.total}</TotalsCell>) }
                    </TableRow>
                </TableBody>

                <TableBody>
                    <TableRow hover>
                        <HeadingCell>Trade (buy)</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.turnNumber}>{-x.expenses.trade}</TableCell>) }
                    </TableRow>
                    <TableRow hover>
                        <HeadingCell>Study</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.turnNumber}>{-x.expenses.study}</TableCell>) }
                    </TableRow>
                    <TableRow hover>
                        <HeadingCell>Consume</HeadingCell>
                        { stats.stats.map(x => <TableCell key={x.turnNumber}>{-x.expenses.consume}</TableCell>) }
                    </TableRow>
                    <TableRow hover>
                        <TotalsHeadingCell>Total Expenses</TotalsHeadingCell>
                        { stats.stats.map(x => <TotalsCell key={x.turnNumber}>{-x.expenses.total} ({usage(x?.income?.total, x?.expenses?.total)})</TotalsCell>) }
                    </TableRow>
                </TableBody>

                <TableBody>
                    <TableRow hover>
                        <TotalsHeadingCell>Balance</TotalsHeadingCell>
                        { stats.stats.map(x => <TotalsCell key={x.turnNumber}>{x.income.total - x.expenses.total}</TotalsCell>) }
                    </TableRow>
                </TableBody>
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
                        { stats.stats.map(x => <HeadingCell key={x.turnNumber}>{x.turnNumber}</HeadingCell>) }
                    </TableRow>
                </TableHead>
                <TableBody>
                    { stats.products.map((p, i) => <TableRow key={p.code} hover>
                        <HeadingCell>{p.getName(2)}</HeadingCell>
                        { stats.stats.map(turn => <TableCell key={turn.turnNumber}>{turn.production[i].amount}</TableCell>) }
                    </TableRow> )}
                </TableBody>
            </> }
        </Observer>
    </Table>
}

export function TreasuryTab() {
    const { stats } = useStore()

    return <Table size='small' stickyHeader={true}>
        <Observer>
            {() => <>
                <TableHead>
                    <TableRow sx={{ backgroundColor: 'palette.primary.main' }}>
                        <HeadingCell>Turn</HeadingCell>
                        { stats.stats.map(x => <HeadingCell key={x.turnNumber}>{x.turnNumber}</HeadingCell>) }
                    </TableRow>
                </TableHead>
                <TableBody>
                    { stats.products.map((p, i) => <TableRow key={p.code} hover>
                        <HeadingCell>{p.getName(2)}</HeadingCell>
                        { stats.stats.map(turn => <TableCell key={turn.turnNumber}>{turn.production[i].amount}</TableCell>) }
                    </TableRow> )}
                </TableBody>
            </> }
        </Observer>
    </Table>
}

export function StatsPage() {
    const { stats } = useStore()

    const routes = [
        { label: 'Treasury', path: '', pathname: '' },
        { label: 'Income', path: 'income', pathname: '' },
        { label: 'Production', path: 'production', pathname: '' },
        { label: 'Skills', path: 'skills', pathname: '' },
    ]

    let currentMatch: PathMatch<string> | null = null
    for (const route of routes) {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        const { pathname } = useResolvedPath(route.path)

        route.pathname = pathname

        // eslint-disable-next-line react-hooks/rules-of-hooks
        const match = useMatch({ path: pathname })
        if (match) {
            currentMatch = match
        }
    }

    React.useEffect(() => { stats.loadStats() }, [ ])

    return <>
        <AppBar position='static' variant='outlined' elevation={0}>
            <Toolbar>
                <IconButton component={Link} to={`..`} edge='start' color='inherit' size="large">
                    <CloseIcon />
                </IconButton>
                <Typography variant='h6'>Statistics</Typography>

                <Tabs value={currentMatch?.pathname} sx={{ ml: 8 }}>
                    { routes.map(x => <Tab key={x.label} label={x.label} value={x.pathname} component={Link} to={x.path} />)}
                </Tabs>
            </Toolbar>
        </AppBar>
        <Container component='main'>
            <Outlet />
        </Container>
    </>
}
