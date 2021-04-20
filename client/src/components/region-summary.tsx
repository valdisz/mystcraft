import * as React from 'react'
import styled from 'styled-components'
import { Box, Typography, Button, Grid, Theme,
    Table, TableHead, TableRow, TableCell, TableBody,
    Tooltip,
    makeStyles
} from '@material-ui/core'
import { observer } from 'mobx-react-lite'
import { Region } from '../store/game/types'
import { Coords } from '../store/game/coords'
import { useCopy } from '../lib'
import { Province } from '../store/game/province'
import { TerrainInfo } from '../store/game/terrain-info'
import { Item } from '../store/game/item'
import { ItemInfo } from '../store/game/item-info'

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
    for (const p of region.products.all) {
        lines.push(`${p.name}\t${p.amount}\t${getMaxAmount(p.amount)}`)
    }

    lines.push('')

    lines.push(`For Sale`)
    lines.push(`Name\tAmount\tPrice`)
    for (const p of region.forSale.all) {
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

            { region.population && <SpaceBetween item xs={12}>
                <span>
                { region.settlement && <>{region.settlement.name} {region.settlement.size}</> }
                </span>
                <ItemComponent item={region.population} />
            </SpaceBetween> }

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
            { region.products.length ? <Grid item xs={12}>
                <strong>Products</strong>
                <ItemTable>
                    <ItemTableBody>
                        { region.products.all
                            .sort((a, b) => b.amount - a.amount)
                            .map(item => <TableItem key={item.code} item={item} />) }
                    </ItemTableBody>
                </ItemTable>
            </Grid> : null }
            { region.forSale.length ? <Grid item xs={12}>
                <strong>For sale</strong>
                <ItemTable>
                    <ItemTableBody>
                        { region.forSale.all
                            .sort((a, b) => a.price - b.price)
                            .map(item => <TableItem key={item.code} item={item} />) }
                    </ItemTableBody>
                </ItemTable>
            </Grid> : null }
            { region.wanted.length ? <Grid item xs={12}>
                <strong>Wanted</strong>
                <ItemTable>
                    <ItemTableBody>
                        { region.wanted.all
                            .sort((a, b) => a.price - b.price)
                            .map(item => <TableItem key={item.code} item={item} />) }
                    </ItemTableBody>
                </ItemTable>
            </Grid> : null }
        </Grid>
    </Box>
})
