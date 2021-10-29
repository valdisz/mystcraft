import * as React from 'react'
import styled from 'styled-components'
import { Box, Typography, Button, Grid, Theme,
    Table, TableHead, TableRow, TableCell, TableBody,
    Tooltip,
    makeStyles
} from '@material-ui/core'
import { observer } from 'mobx-react-lite'
import { Region, Unit } from '../store/game/types'
import { Coords } from '../store/game/coords'
import { useCopy } from '../lib'
import { Province } from '../store/game/province'
import { TerrainInfo } from '../store/game/terrain-info'
import { Item } from '../store/game/item'
import { ItemInfo } from '../store/game/item-info'
import { Skill } from '../store/game/skill'

const SpaceBetween = styled(Grid)`
    display: flex;
    justify-content: space-between;
    align-items: center;
`

const Terrain = styled.div`
    font-size: 80%;
`

const Level = styled.div`
    font-size: 70%;
`

const Prefix = styled.div`
    display: flex;
    justify-content: center;
    align-items: flex-start;
    flex-direction: column;

    padding: ${({ theme }) => (theme as Theme).spacing(0, .5)};
    border-right: 1px solid ${({ theme }) => (theme as Theme).palette.divider};
`

const Coordinates = styled.div`
    display: flex;
    align-items: center;
    font-weight: bold;
    padding: ${({ theme }) => (theme as Theme).spacing(0, .5)};
`

const GeographyButton = styled(Button)`
    padding: 0 !important;

    .MuiButton-label {
        height: 100%;
        align-items: stretch;
    }
`

interface ProvinceNameProps {
    province: Province
}

function ProvinceName({ province }: ProvinceNameProps) {
    return <Typography variant='h5'>{province.name}</Typography>
}

interface GeographyProps {
    coords: Coords
    terrain: TerrainInfo
}

function Geography({ coords, terrain }: GeographyProps) {
    const copy = useCopy(false)

    const text = coords.toString()

    return <GeographyButton size='small' variant='outlined' ref={copy} data-clipboard-text={text}>
        <Prefix>
            <Level>{coords.label}</Level>
            <Terrain>{terrain.name}</Terrain>
        </Prefix>
        <Coordinates>{text}</Coordinates>
    </GeographyButton>
}

const ItemMain = styled.div`
    display: flex;
    flex-shrink: 1;

    .amount {

    }

    .name {
        flex: 1;
        margin-left: .5rem;
    }

    .price {
        margin-left: .5rem;
    }
`

interface ItemInfoTooltipProps {
    info: ItemInfo
}

function ItemInfoTooltip({ info }: ItemInfoTooltipProps) {
    return <>
        <Typography variant='h6'>{info.getName(1)}</Typography>
        <Typography variant='body2'>
            {info.description}
        </Typography>
    </>
}

interface ItemComponentProps {
    item: Item
    className?: string
}

const useStyles = makeStyles((theme) => ({
    wideTooltip: {
      maxWidth: 500,
    },
}))

function ItemComponent({ item, className }: ItemComponentProps) {
    const classes = useStyles()

    return <ItemMain className={className}>
        <div className="amount">
            {item.amount}
        </div>
        <div className="name">
            <Tooltip title={<ItemInfoTooltip info={item.info} />} classes={{ tooltip: classes.wideTooltip }}>
                <span>{item.name}</span>
            </Tooltip>
        </div>
        {item.price &&
        <div className="price">
            {item.price} $
        </div> }
    </ItemMain>
}

interface SkillComponentProps {
    skill: Skill
    className?: string
}

function SkillComponent({ skill, className }: SkillComponentProps) {
    return <ItemMain className={className}>
        <div className="name">
            <span>{skill.name}</span>
        </div>
        <div className="amount">
            {skill.level} ({skill.days})
        </div>
    </ItemMain>
}

const ItemTable = styled.div`
    display: table;
    width: 100%;
`

const ItemTableBody = styled.div`
    display: table-row-group;
`

const TableItem = styled(ItemComponent)`
    display: table-row;

    .amount, .name, .price {
        display: table-cell;
    }

    .amount {
        width: 40px;
    }

    .name {
        margin-left: 0;
    }

    .price {
        width: 50px;
        margin-left: .5rem;
        text-align: right;
    }
`

const TableSkill = styled(SkillComponent)`
    display: table-row;

    .amount, .name {
        display: table-cell;
    }

    .amount {
        text-align: right;
    }

    .name {
        margin-left: 0;
    }
`

function getMaxAmount(amount: number) {
    let total = amount

    let coef = 0.25
    let delta = 0
    do {
        delta = Math.floor(amount * coef)
        total += delta

        coef = coef / 2
    }
    while (delta > 0)

    return total
}

const UnitContainer = styled.div`
    padding: ${({ theme }) => theme.spacing(1)}px;
    border-top: 1px solid ${({ theme }) => theme.palette.divider};
`

interface UnitNumberProps {
    num: number
}

function UnitNumber({ num }: UnitNumberProps) {
    const copy = useCopy(false)

    const text = num.toString()

    return <Button size='small' variant='outlined' ref={copy} data-clipboard-text={text}>
        {num}
    </Button>
}

export interface UnitSummaryProps {
    unit: Unit
}

export function UnitSummary({ unit }: UnitSummaryProps) {
    return <UnitContainer>
        <Grid container spacing={1}>
            <SpaceBetween item xs={12}>
                <Typography variant='h5'>{unit.name}</Typography>
                <UnitNumber num={unit.num} />
            </SpaceBetween>
            { !!unit.skills.length && <Grid item xs={12}>
                <strong>Skills</strong>
                <ItemTable>
                    <ItemTableBody>
                        { unit.skills.all
                            .sort((a, b) => a.days - b.days)
                            .map(item => <TableSkill key={item.code} skill={item} />) }
                    </ItemTableBody>
                </ItemTable>
            </Grid> }
            { !!unit.inventory.items.length && <Grid item xs={12}>
                <strong>Items</strong>
                <ItemTable>
                    <ItemTableBody>
                        { unit.inventory.items.all
                            .sort((a, b) => {
                                if (a.isMan === b.isMan) {
                                    return a.name.localeCompare(b.name, 'en-US')
                                }

                                return (b.isMan ? 1 : -1) - (a.isMan ? 1 : -1)
                            })
                            .map(item => <TableItem key={item.code} item={item} />) }
                    </ItemTableBody>
                </ItemTable>
            </Grid> }
        </Grid>
    </UnitContainer>
}
