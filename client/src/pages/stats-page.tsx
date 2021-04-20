import * as React from 'react'
import styled from 'styled-components'
import { Link, useParams } from 'react-router-dom'
import { useCallbackRef } from '../lib'
import { AppBar, Typography, Toolbar, IconButton, TextField, Table, TableHead, TableRow, TableCell, TableBody } from '@material-ui/core'
import { useStore } from '../store'
import { Observer, observer } from 'mobx-react-lite'
import { HexMap } from '../map'
import { Region } from "../store/game/types"
import { GameRouteParams } from './game-route-params'
import ArrowBackIcon from '@material-ui/icons/ArrowBack'
import { List } from '../store/game/list'
import { Item } from '../store/game/item'

export function StatsPage() {
    return <h2>Stats</h2>
}
