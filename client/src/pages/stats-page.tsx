import * as React from 'react'
import { Typography, Container, Table, TableHead, TableRow, TableCell, TableBody, Grid, Tabs, Tab, Tooltip, makeStyles } from '@material-ui/core'
import { useStore } from '../store'
import { Observer } from 'mobx-react-lite'
import { SkillInfo } from '../store/game/skill-info'

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

export function StatsPage() {
    const { stats } = useStore()
    const classes = useStyles()

    return <Container component='main' maxWidth={false}>
        <Typography variant='h4'>Statistics</Typography>
        <Grid container>
            <Grid item xs={4}>
                <Observer>
                    {() => <>
                        <Tabs value={stats.tab} onChange={(_, value) => stats.setTab(value)}>
                            <Tab value='skills' label='Skills'>
                            </Tab>
                        </Tabs>
                        <Table size='small' stickyHeader>
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
                            <TableBody>
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
                            </TableBody>
                        </Table>
                    </> }
                </Observer>
            </Grid>
        </Grid>
    </Container>
}
