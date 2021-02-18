import * as React from 'react';
import { Container } from '@material-ui/core';

export function Layout(props: React.PropsWithChildren<{}>) {
    return <Container>
        {props.children}
    </Container>;
}
