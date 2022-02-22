import React from 'react'
import { Avatar, Button, TextField, Link, Box, Alert, Typography, Container } from '@mui/material'
import { LockOutlined } from '@mui/icons-material'
import { styled } from '@mui/material/styles'

import { observer, useLocalStore } from 'mobx-react-lite';
import { runInAction } from 'mobx';

function Copyright() {
    return (
        <Typography variant='body2' color='textSecondary' align='center'>
            { `Copyright ©  ${new Date().getFullYear()} ` }
            <Link color='inherit' href='https://advisor.azurewebsites.net'>Valdis Zobēla</Link>
        </Typography>
    );
}

const Form = styled('form')`
    width: 100%;
    margin-top: ${p => p.theme.spacing(1)};
`

export interface SignInProps {
    onSuccess: () => void
}

export const SignIn = observer(({ onSuccess }: SignInProps) => {
    const store = useLocalStore(() => ({
        message: '',
        severity: 'error' as ('error' | 'success'),
        setMessage: (severity: 'error' | 'success', message: string) => {
            store.message = message
            store.severity = severity
        },

        mode: 'sign-in' as ('sign-up' | 'sign-in'),
        toggleMode: () => {
            store.mode = store.mode ===  'sign-in' ? 'sign-up' : 'sign-in'
            store.message = ''
        },

        email: '',
        password: '',

        emailError: '',
        passwordError: '',

        setEmail: (e: React.ChangeEvent<HTMLInputElement>) => {
            store.email = e.target.value
            store.emailError = ''
        },
        setPassword: (e: React.ChangeEvent<HTMLInputElement>) => {
            store.password = e.target.value
            store.passwordError = ''
        },

        proceed: async (e: React.FormEvent) => {
            e.preventDefault()
            e.stopPropagation()

            const response = await fetch(store.mode === 'sign-in' ? '/account/login' : '/account/register', {
                method: 'POST',
                credentials: 'include',
                body: new FormData(e.target as any)
            })

            if (store.mode === 'sign-in' && response.ok) {
                onSuccess()
                return
            }

            if (response.ok) {
                if (store.mode === 'sign-up') {
                    runInAction(() => {
                        store.setMessage('success', 'New account created! Now you can sign in.', )
                        store.mode = 'sign-in'
                        store.email = ''
                        store.password = ''
                    })
                }

                return
            }

            switch (response.status) {
                case 400: {
                    const data = await response.json()

                    runInAction(() => {
                        store.emailError = data.Email ? data.Email[0] : ''
                        store.passwordError = data.Password ? data.Password[0] : ''
                        store.setMessage('error', data.general ?? '')
                    })

                    break
                }

                case 401: {
                    runInAction(() => {
                        store.emailError = ''
                        store.passwordError = ''
                        store.setMessage('error', 'Email or password is wrong.')
                    })
                    break
                }

                default: {
                    runInAction(() => {
                        store.emailError = ''
                        store.passwordError = ''
                        store.setMessage('error', 'An unknown error occured, try later or contact administrator.')
                    })
                    break
                }
            }
        }
    }))

    return (
        <Container component='main' maxWidth='xs'>
            <Box sx={{
                marginTop: 8,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
            }}>
                <Avatar sx={{
                    margin: 1,
                    backgroundColor: 'secondary.main',
                }}>
                    <LockOutlined />
                </Avatar>
                <Typography component='h1' variant='h5'>
                    { store.mode === 'sign-in' ? 'Sign in' : 'Sign up' }
                </Typography>
                <Form noValidate onSubmit={store.proceed}>
                    <TextField
                        variant='outlined'
                        margin='normal'
                        required
                        fullWidth
                        id='email'
                        label='Email Address'
                        name='email'
                        autoComplete='email'
                        autoFocus
                        error={!!store.emailError}
                        helperText={store.emailError}
                        value={store.email}
                        onChange={store.setEmail}
                    />
                    <TextField
                        variant='outlined'
                        margin='normal'
                        required
                        fullWidth
                        name='password'
                        label='Password'
                        type='password'
                        id='password'
                        autoComplete={ store.mode === 'sign-in' ? 'current-password' : 'new-password' }
                        error={!!store.passwordError}
                        helperText={store.passwordError}
                        value={store.password}
                        onChange={store.setPassword}
                    />
                    { store.message && <Box mt={2}>
                        <Alert severity={ store.severity }>{store.message}</Alert>
                    </Box> }

                    <Button fullWidth
                        variant='contained'
                        color='primary'
                        type='submit'
                        sx={{
                            margin: [3, 0, 2]
                        }}
                    >
                        { store.mode === 'sign-in' ? 'Sign in' : 'Sign up' }
                    </Button>

                    <Box mt={1}>
                        <Button component={'a'} fullWidth variant='contained' color='primary' sx={{ margin: [3, 0, 2] }} href='/account/login/discord'>Use DISCORD</Button>
                    </Box>

                    <Box mt={1}>
                        <Button fullWidth size='small' variant='text' onClick={store.toggleMode}>
                            { store.mode === 'sign-in' ? `Don't have an account? Sign Up` : `Already have an account? Sign in` }
                        </Button>
                    </Box>
                </Form>
            </Box>
            <Box mt={8}>
                <Copyright />
            </Box>
        </Container>
    );
})
