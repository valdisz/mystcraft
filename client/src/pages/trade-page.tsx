import * as React from 'react'
import { useStore } from '../store'
import { observer, Observer } from 'mobx-react'
import {
    Box,
    AppBar,
    Button,
    ButtonGroup,
    Container,
    IconButton,
    List,
    ListItem,
    ListItemText,
    Paper,
    TextField,
    Toolbar,
    Typography,
    Grid,
    Alert,
    AlertTitle
} from '@mui/material'

export function TradePage() {
    const { game } = useStore()
    const { university } = game

    return (
        <Container component='main' maxWidth={false}>
            <Typography variant='h4'>Trade</Typography>
            <Grid container>
                <Grid item xs={12}>
                </Grid>
            </Grid>
        </Container>
    );
}
