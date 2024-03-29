import * as React from 'react'
import styled from '@emotion/styled'
import { Typography, Button, Grid, Box, BoxProps, Tooltip, Chip } from '@mui/material'
import { Unit } from '../game'
import { useCopy } from '../lib'
import { Item } from '../game'
import { ItemInfo } from '../game'
import { Skill } from '../game'
import { SkillInfo } from '../game'

const SpaceBetween = styled(Grid)`
    display: flex;
    justify-content: space-between;
    align-items: center;
`

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

interface SkillInfoTooltipProps {
    skill: SkillInfo
}

function SkillInfoTooltip({ skill }: SkillInfoTooltipProps) {
    return <>
        <Typography variant='h6'>
            <span>{skill.name}</span>
            { ' ' }
            { skill.magic && <Chip size='small' label='Magic' /> }
        </Typography>
        { skill.description
            .map((x, i) => <Typography key={i} variant='body2'>{x}</Typography>)
        }
    </>
}

interface ItemComponentProps {
    item: Item
    className?: string
}

const WideTooltip = styled(Tooltip)`
    max-width: 500px;
`

function ItemComponent({ item, className }: ItemComponentProps) {
    return <ItemMain className={className}>
        <div className="amount">
            {item.amount}
        </div>
        <div className="name">
            <WideTooltip title={<ItemInfoTooltip info={item.info} />}>
                <span>{item.name}</span>
            </WideTooltip>
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
            <WideTooltip title={<SkillInfoTooltip skill={skill.info} />}>
            <span>{skill.name}</span>
            </WideTooltip>
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

export interface UnitSummaryProps extends BoxProps {
    unit: Unit
}

export function UnitSummary({ unit, sx, ...props }: UnitSummaryProps) {
    return <Box {...props} sx={{
        padding: 1,
        ...(sx || { })
    }}>
        <Grid container spacing={1}>
            <SpaceBetween item xs={12}>
                <Typography variant='h5'>{unit.name}</Typography>
                <UnitNumber num={unit.num} />
            </SpaceBetween>
            { !!unit.skills.size && <Grid item xs={12}>
                <strong>Skills</strong>
                <ItemTable>
                    <ItemTableBody>
                        { unit.skills.toArray()
                            .sort((a, b) => a.days - b.days)
                            .map(item => <TableSkill key={item.code} skill={item} />) }
                    </ItemTableBody>
                </ItemTable>
            </Grid> }
            { !!unit.inventory.items.size && <Grid item xs={12}>
                <strong>Items</strong>
                <ItemTable>
                    <ItemTableBody>
                        { unit.inventory.items.toArray()
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
    </Box>
}
