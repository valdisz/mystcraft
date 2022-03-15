import * as React from 'react'
import styled from '@emotion/styled'
import { Box, Typography, Button, IconButton, Grid, Theme, Tooltip, Menu, MenuItem, ClickAwayListener } from '@mui/material'
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

interface GeographyProps {
    coords: Coords
    terrain: TerrainInfo
}

function Geography({ coords, terrain }: GeographyProps) {
    const text = coords.toString()

    return <GeographyButton size='small' variant='outlined' onClick={() => copy(`(${text})`)}>
        <Prefix>
            <Typography sx={{ fontSize: '50%' }}>{coords.label}</Typography>
            <Typography sx={{ fontSize: '75%' }}>{terrain.name}</Typography>
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
    const lines = []

    if (region?.terrain) lines.push(region.terrain.name)
    if (region?.coords) lines.push(region.coords.toString())
    if (region?.province) lines.push(region.province.name)

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
            { Array.from(items).sort(itemSort) .map(item => <ItemComponent2 key={item.code} value={item} used={5} />) }
        </Box>
    </>
}

export const RegionSummary = observer(({ region }: RegionSummaryProps) => {
    const [ open, setOpen ] = React.useState(false)
    const anchorRef = React.useRef(null)

    const hideMenu = React.useCallback(() => setOpen(false), [ ])

    return <Box m={1}>
        <Grid container spacing={1}>
            <Grid item xs={12} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <ClickAwayListener onClickAway={hideMenu}>
                    <IconButton size='small' onClick={() => setOpen(true)} ref={anchorRef}>
                        <MoreVertIcon />
                    </IconButton>
                </ClickAwayListener>
                <Menu open={open} anchorEl={anchorRef.current}>
                    <CopyRegionDetailsMenuItem region={region} />
                </Menu>

                <Box sx={{ flex: 1, minWidth: 0 }}>
                    <Box sx={{ minWidth: 0 }}>
                        <Typography variant='h5' title={region?.province?.name} sx={{ overflow: 'hidden', whiteSpace: 'nowrap', textOverflow: 'ellipsis' }}>{region?.province?.name}</Typography>
                    </Box>
                </Box>

                <Geography terrain={region.terrain} coords={region.coords} />
            </Grid>

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
