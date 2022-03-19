import * as React from 'react'
import styled from '@emotion/styled'
import { Box, BoxProps, Typography, Button, IconButton, Grid, Theme, Tooltip, Menu, MenuItem, ClickAwayListener } from '@mui/material'
import { observer } from 'mobx-react'
import { Region, Coords, TerrainInfo, Item, ItemInfo } from '../game'
import { copy } from '../lib'
import { Item as ItemComponent2 } from '../components'
import MoreVertIcon from '@mui/icons-material/MoreVert'

const WideTooltip = styled(Tooltip)`
    max-width: 500px;
`

const SpaceBetween = styled(Grid)`
    display: flex;
    justify-content: space-between;
    align-items: center;
`

interface GeographyProps {
    coords: Coords
    terrain: TerrainInfo
}

function Geography({ coords, terrain }: GeographyProps) {
    const text = coords.toString()

    return <Button variant='outlined' color='inherit' onClick={() => copy(`(${text})`)} sx={{ p: 0 }}>
        <Box sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'flex-start',
            flexDirection: 'column',
            px: 1, py: 0,
            borderRight: 1,
            borderColor: 'divider'
        }}>
            <Typography variant='caption'>{coords.label}</Typography>
            <Typography variant='body2'>{terrain.name}</Typography>
        </Box>
        <Box sx={{
            display: 'flex',
            alignItems: 'center',
            px: 1, py: 0,
        }}>
            <Typography sx={{ fontWeight: 'bold' }}>{text}</Typography>
        </Box>
    </Button>
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

export interface RegionSummaryProps {
    region: Region
}

interface CopyRegionDetailsMenuItemProps {
    region: Region
}

function formatRegionInfo(region: Region) {
    if (!region) {
        return null
    }

    const lines = []

    if (region.terrain) lines.push(region.terrain.name)
    if (region.coords) lines.push(region.coords.toString())
    if (region.province) lines.push(region.province.name)

    lines.push('')
    lines.push(`Tax\t${region.tax}`)
    lines.push(`Pillage\t${region.tax * 2}`)
    lines.push(`Wages\t${region.wages.amount}`)
    lines.push(`Total Wages\t${region.wages.total}`)
    lines.push(`Entertainment\t${region.entertainment}`)

    lines.push('')
    lines.push(`Taxers\t${Math.ceil(region.tax / 50)}`)
    lines.push(`Pillagers\t${Math.ceil(region.tax / 100)}`)
    lines.push(`Workers\t${Math.ceil(region.wages.total / region.wages.amount)}`)
    lines.push(`Entertainers\t${Math.ceil(region.entertainment / 30)}`)

    if (region.products.size > 0) {
        lines.push('')

        lines.push(`Products`)
        lines.push(`Name\tAmount\tMax`)
        for (const p of region.products) {
            lines.push(`${p.name}\t${p.amount}\t${getMaxAmount(p.amount)}`)
        }
    }

    if (region.forSale.size > 0) {
        lines.push('')

        lines.push(`For Sale`)
        lines.push(`Name\tAmount\tPrice`)
        for (const p of region.forSale) {
            lines.push(`${p.name}\t${p.amount}\t${p.price}`)
        }
    }

    if (region.wanted.size > 0) {
        lines.push('')

        lines.push(`Wanted`)
        lines.push(`Name\tAmount\tPrice`)
        for (const p of region.wanted) {
            lines.push(`${p.name}\t${p.amount}\t${p.price}`)
        }
    }

    const text = lines.join("\n")
    return text
}

function CopyRegionDetailsMenuItem({ region }: CopyRegionDetailsMenuItemProps) {
    return <MenuItem onClick={() => copy(formatRegionInfo(region)) }>Copy region details</MenuItem>
}

type RegionEconomicsProps = Pick<Region, 'entertainment' | 'wages' | 'tax'>

function RegionEconomics({ entertainment, tax, wages }: RegionEconomicsProps) {
    return <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box sx={{ display: 'flex', flexDirection: 'column' }}>
            <strong>Ente.</strong>
            {entertainment}
        </Box>
        <Box sx={{ display: 'flex', flexDirection: 'column', textAlign: 'center' }}>
            <strong>Wages</strong>
            {wages.amount} ({wages.total})
        </Box>
        <Box sx={{ display: 'flex', flexDirection: 'column', textAlign: 'right' }}>
            <strong>Tax</strong>
            {tax}
        </Box>
    </Box>
}

function getItemCategoryValue(item: Item) {
    const { category } = item.info
    if (category === 'food') return -1;
    if (category === 'trade') return 1;

    return 0
}

function itemSort(a: Item, b: Item) {
    const aV = getItemCategoryValue(a)
    const bV = getItemCategoryValue(b)

    if (aV === bV) {
        if (a.price > 0) {
            return a.price - b.price
        }

        return a.amount - b.amount
    }

    return aV - bV
}

interface ResourcesColumnProps {
    title: string
    items: Iterable<Item>
}

function ResourcesColumn({ title, items }: ResourcesColumnProps) {
    return <>
        <Box sx={{ textAlign: 'center', mb: 1 }}>
            <Typography variant='body2'>{title}</Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1, flexDirection: 'column' }}>
            { Array.from(items).sort(itemSort).map(item => <ItemComponent2 key={item.code} value={item} />) }
        </Box>
    </>
}

export interface RegionHeaderProps extends BoxProps {
    region: Region
}

export function RegionHeader({ region, sx, ...props }: RegionHeaderProps) {
    const [ open, setOpen ] = React.useState(false)
    const anchorRef = React.useRef(null)

    const hideMenu = React.useCallback(() => setOpen(false), [ ])

    return <Box {...props} sx={{ display: 'flex', gap: 1, ...(sx || { }) }}>
        <ClickAwayListener onClickAway={hideMenu}>
            <IconButton size='small' onClick={() => setOpen(true)} ref={anchorRef}>
                <MoreVertIcon />
            </IconButton>
        </ClickAwayListener>
        <Menu open={open} anchorEl={anchorRef.current}>
            <CopyRegionDetailsMenuItem region={region} />
        </Menu>

        <Box sx={{ flex: 1, minWidth: 0, display: 'flex', justifyContent: 'flex-start', alignItems: 'center' }}>
            <Typography variant='h6' title={region?.province?.name} sx={{ overflow: 'hidden', whiteSpace: 'nowrap', textOverflow: 'ellipsis' }}>{region?.province?.name}</Typography>
        </Box>

        <Geography terrain={region.terrain} coords={region.coords} />
    </Box>
}

export const RegionSummary = observer(({ region }: RegionSummaryProps) => {

    return <Box m={1}>
        <Grid container spacing={1}>
            { (region.population || region.settlement) && <SpaceBetween item xs={12}>
                { region.settlement && <span>
                    { region.settlement && <>{region.settlement.name} {region.settlement.size}</> }
                </span> }
                { region.population && <ItemComponent item={region.population} /> }
            </SpaceBetween> }

            { region.explored && <>
                <Grid item xs={12}>
                    <RegionEconomics entertainment={region.entertainment} tax={region.tax} wages={region.wages} />
                </Grid>
                { region.products.size > 0 && <Grid item xs={4}>
                    <ResourcesColumn title='Products' items={region.products} />
                </Grid> }
                { region.forSale.size > 0 && <Grid item xs={4}>
                    <ResourcesColumn title='For sale' items={region.forSale} />
                </Grid> }
                { region.wanted.size > 0 && <Grid item xs={4}>
                    <ResourcesColumn title='Wanted' items={region.wanted} />
                </Grid> }
            </> }
        </Grid>
    </Box>
})
