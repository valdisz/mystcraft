import * as React from 'react'
import styled from 'styled-components'
import { Container, List, ListSubheader, ListItem, ListItemText, Divider } from '@material-ui/core'
import { IObservableArray, runInAction } from 'mobx'
import { useObserver, useLocalStore } from 'mobx-react-lite'
import { CLIENT } from '../client'
import { GameListItem, GetGamesQuery, GetGames } from '../schema'
import { Link } from 'react-router-dom'

export function Layout(props: React.PropsWithChildren<{}>) {
    return <Container>
        {props.children}
    </Container>
}

export function HomePage() {
    const store = useLocalStore(() => ({
        games: [] as IObservableArray<GameListItem>,
        load: () => {
            CLIENT.query<GetGamesQuery>({
                query: GetGames
            }).then(response => {
                runInAction(() => {
                    store.games.replace(response.data.games.nodes)
                })
            })
        }
    }))


    React.useEffect(() => {
        store.load()
    }, [])

    return <Layout>
        <List component='nav' subheader={<ListSubheader>Games</ListSubheader>}>
            {useObserver(() => <>
                { store.games.length
                    ? store.games.map(x => <ListItem key={x.id} component={Link} to={`/game/${x.id}`}>
                        <ListItemText secondary={x.playerFactionName}>
                            <strong>{x.name}</strong>
                        </ListItemText>
                    </ListItem>)
                    : <ListItem>
                        <ListItemText>Empty</ListItemText>
                    </ListItem>
                }
            </>)}
            <Divider />
            <ListItem button>
                <ListItemText>New Game</ListItemText>
            </ListItem>
        </List>
    </Layout>
}
