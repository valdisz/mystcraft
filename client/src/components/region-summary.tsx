import * as React from 'react'
import styled from '@emotion/styled'
import { Box, BoxProps, Typography, Button, IconButton, Grid, Stack, Tooltip, Menu, MenuItem, ClickAwayListener, Divider } from '@mui/material'
import { observer } from 'mobx-react'
import { Region, Coords, TerrainInfo, Item, ItemInfo } from '../game'
import { copy } from '../lib'
import { Item as ItemComponent2, FixedTypography } from '../components'

import MoreVertIcon from '@mui/icons-material/MoreVert'
import LocationSearchingIcon from '@mui/icons-material/LocationSearching'
import { useMapContext } from '../map'
import { useStore } from '../store'

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
            <Typography variant='caption' sx={{ color: 'text.secondary' }}>{coords.label}</Typography>
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

    const locationType = region.covered
        ? null
        : region.settlement
            ? region.settlement.size
            : 'wilderness'

    const locationName = region?.settlement?.name ?? region?.province?.name

    return <Box {...props} sx={{ display: 'flex', gap: 1, ...(sx || { }) }}>
        <ClickAwayListener onClickAway={hideMenu}>
            <Box sx={{ display: 'flex', minWidth: 0, justifyContent: 'center', alignItems: 'center' }}>
                <IconButton size='small' onClick={() => setOpen(true)} ref={anchorRef}>
                    <MoreVertIcon />
                </IconButton>
            </Box>
        </ClickAwayListener>
        <Menu open={open} anchorEl={anchorRef.current}>
            <CopyRegionDetailsMenuItem region={region} />
        </Menu>

        <Stack justifyContent='center' alignItems='flex-start' sx={{ flex: 1, minWidth: 0 }}>
            <FixedTypography variant='caption' title={locationType} sx={{ color: 'text.secondary' }}>{locationType}</FixedTypography>
            <Box sx={{ width: '100%' }}>
                <FixedTypography variant='h6' title={locationName}>{locationName}</FixedTypography>
            </Box>
        </Stack>

        <Box sx={{ display: 'flex', minWidth: 0, justifyContent: 'center', alignItems: 'center' }}>
            <Geography terrain={region.terrain} coords={region.coords} />
        </Box>
    </Box>
}

export const RegionSummary = observer(({ region }: RegionSummaryProps) => {
    const mapContext = useMapContext()
    const { game } = useStore()

    function centerAt(reg: Region) {
        mapContext.map.centerAt(reg.coords)
        game.selectRegion(reg)
    }

    return <Box m={1}>
        <Grid container spacing={1}>
            { (region.population || region.settlement) && <SpaceBetween item xs={12}>
                <FixedTypography>{ region?.province?.name }</FixedTypography>
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

                { (region.outgoingTrade.length > 0 || region.incomingTrade.length > 0) &&
                <Grid item xs={12}>
                    <Typography variant='subtitle1'>Trade Routes</Typography>
                    { region.outgoingTrade.length > 0 && <>
                        <Divider />
                        <Box>
                            <Typography variant='subtitle2'>Wanted</Typography>
                            <Box component='table' sx={{
                                width: '100%',
                                'td, th': {
                                    textAlign: 'right'
                                },
                                '.left': {
                                    textAlign: 'left'
                                },
                                '.item': {
                                    width: '50px'
                                },
                                '.shrink': {
                                    width: '1px'
                                }
                            }}>
                                <thead>
                                    <tr>
                                        <th className='left item'>Item</th>
                                        <th className='left'>Location</th>
                                        <th className='shrink' title='Move Points'>MP</th>
                                        <th className='shrink' title='Price'>Pr.</th>
                                        <th className='shrink' title='Amount'>Amnt.</th>
                                        <th className='shrink' title='Profit'>$</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    { region.outgoingTrade.map((trade, i) => <tr key={i}>
                                        <td className='left'>{trade.item.getName(trade.amount)}</td>
                                        <td className='left'>
                                            <Button variant='outlined' size='small' onClick={() => centerAt(trade.sell.region)} startIcon={<LocationSearchingIcon />}>
                                                {trade.sell.region.settlement.name}
                                                { ' ' }
                                                ({trade.sell.region.coords.toString()})
                                            </Button>
                                        </td>
                                        <td className='shrink'>{trade.distance}/{trade.cost}</td>
                                        <td className='shrink'>${trade.sell.price}</td>
                                        <td className='shrink'>{trade.amount}</td>
                                        <td className='shrink'>${trade.profit}</td>
                                    </tr>) }
                                </tbody>
                            </Box>
                        </Box>
                    </> }
                    { region.incomingTrade.length > 0 && <>
                        <Divider />
                        <Box>
                            <Typography variant='subtitle2'>For Sale</Typography>
                            <Box component='table' sx={{
                                width: '100%',
                                borderCollapse: 'collapse',
                                'td, th': {
                                    textAlign: 'right',
                                    px: 1,
                                },
                                '.left': {
                                    textAlign: 'left'
                                },
                                '.item': {
                                    width: '50px'
                                },
                                '.shrink': {
                                    width: '1px'
                                }
                            }}>
                                <thead>
                                    <tr>
                                        <th className='left item'>Item</th>
                                        <th className='left'>Location</th>
                                        <th className='shrink' title='Move Points'>MP</th>
                                        <th className='shrink' title='Price'>Pr.</th>
                                        <th className='shrink' title='Amount'>Amnt.</th>
                                        <th className='shrink' title='Profit'>$</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    { region.incomingTrade.map((trade, i) => <tr key={i}>
                                        <td className='left'>{trade.item.getName(trade.amount)}</td>
                                        <td className='left'>
                                            <Button variant='outlined' size='small' onClick={() => centerAt(trade.buy.region)} startIcon={<LocationSearchingIcon />}>
                                                {trade.buy.region.settlement.name}
                                                { ' ' }
                                                ({trade.buy.region.coords.toString()})
                                            </Button>
                                        </td>
                                        <td className='shrink'>{trade.distance}/{trade.cost}</td>
                                        <td className='shrink'>${trade.buy.price}</td>
                                        <td className='shrink'>{trade.amount}</td>
                                        <td className='shrink'>${trade.profit}</td>
                                    </tr>) }
                                </tbody>
                            </Box>
                        </Box>
                    </> }
                </Grid>
                }
            </> }
        </Grid>
    </Box>
})
