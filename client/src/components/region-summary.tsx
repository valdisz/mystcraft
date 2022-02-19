import * as React from 'react'
import styled from '@emotion/styled'
import { Box, Typography, Button, Grid, Theme,
    Table, TableHead, TableRow, TableCell, TableBody,
    Tooltip,
} from '@mui/material'
import { observer } from 'mobx-react-lite'
import { Region } from '../game/types'
import { Coords } from '../game/coords'
import { useCopy } from '../lib'
import { Province } from '../game/province'
import { TerrainInfo } from '../game/terrain-info'
import { Item } from '../game/item'
import { ItemInfo } from '../game/item-info'


const WideTooltip = styled(Tooltip)`
    max-width: 500px;
`

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

export interface RegionSummaryProps {
    region: Region
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

function CopyRegionDetails({ region }: RegionSummaryProps) {
    const copy = useCopy(false)

    const lines = []

    lines.push(`${region.province.name}\t${region.terrain.name}\t${region.coords.toString()}`)
    lines.push('')

    lines.push(`Products`)
    lines.push(`Name\tAmount\tMax`)
    for (const p of region.products) {
        lines.push(`${p.name}\t${p.amount}\t${getMaxAmount(p.amount)}`)
    }

    lines.push('')

    lines.push(`For Sale`)
    lines.push(`Name\tAmount\tPrice`)
    for (const p of region.forSale) {
        lines.push(`${p.name}\t${p.amount}\t${p.price}`)
    }

    return <Button ref={copy} data-clipboard-text={lines.join("\n")}>Copy region details</Button>
}

export const RegionSummary = observer(({ region }: RegionSummaryProps) => {
    return <Box m={1}>
        <Grid container spacing={1}>
            <SpaceBetween item xs={12}>
                <ProvinceName province={region.province} />
                <Geography terrain={region.terrain} coords={region.coords} />
            </SpaceBetween>

            {/* <SpaceBetween item xs={12}>
                <CopyRegionDetails region={region} />
            </SpaceBetween> */}

            { (region.population || region.settlement) && <SpaceBetween item xs={12}>
                { region.settlement && <span>
                    { region.settlement && <>{region.settlement.name} {region.settlement.size}</> }
                </span> }
                { region.population && <ItemComponent item={region.population} /> }
            </SpaceBetween> }

            { region.explored && <>
                <SpaceBetween item xs={12}>
                    <div>
                        <div>
                            <strong>Ente.</strong>
                        </div>
                        {region.entertainment}
                    </div>
                    <div>
                        <div>
                            <strong>Wages</strong>
                        </div>
                        {region.wages.amount} ({region.wages.total})
                    </div>
                    <div>
                        <div>
                            <strong>Tax</strong>
                        </div>
                        {region.tax}
                    </div>
                </SpaceBetween>
                { region.products.size ? <Grid item xs={12}>
                    <strong>Products</strong>
                    <ItemTable>
                        <ItemTableBody>
                            { region.products.toArray()
                                .sort((a, b) => b.amount - a.amount)
                                .map(item => <TableItem key={item.code} item={item} />) }
                        </ItemTableBody>
                    </ItemTable>
                </Grid> : null }
                { region.forSale.size ? <Grid item xs={12}>
                    <strong>For sale</strong>
                    <ItemTable>
                        <ItemTableBody>
                            { region.forSale.toArray()
                                .sort((a, b) => a.price - b.price)
                                .map(item => <TableItem key={item.code} item={item} />) }
                        </ItemTableBody>
                    </ItemTable>
                </Grid> : null }
                { region.wanted.size ? <Grid item xs={12}>
                    <strong>Wanted</strong>
                    <ItemTable>
                        <ItemTableBody>
                            { region.wanted.toArray()
                                .sort((a, b) => a.price - b.price)
                                .map(item => <TableItem key={item.code} item={item} />) }
                        </ItemTableBody>
                    </ItemTable>
                </Grid> : null }
            </> }
        </Grid>
    </Box>
})
