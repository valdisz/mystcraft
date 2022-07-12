import React from 'react'
import { Avatar, Button, TextField, Box, Alert, Typography, Container } from '@mui/material'
import LockOutlinedIcon from '@mui/icons-material/LockOutlined'
import { observer, useLocalStore } from 'mobx-react'
import { runInAction } from 'mobx'
import { Copyright } from './copyright'
import { useTheme } from '@emotion/react'

function useSignInStore(onSuccess: () => void) {
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

    return store
}

export interface SignInProps {
    onSuccess: () => void
}

function SignIn({ onSuccess }: SignInProps) {
    const { spacing } = useTheme()
    const store = useSignInStore(onSuccess)

    return (
        <Container component='main' maxWidth='xs' sx={{
            height: '100%',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'stretch',
            justifyContent: 'center',
            gap: spacing(8)
        }}>
            <Box sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center'
            }}>
                <Avatar sx={{
                    margin: 1,
                    backgroundColor: 'secondary.main',
                }}>
                    <LockOutlinedIcon />
                </Avatar>
                <Typography component='h1' variant='h5'>
                    { store.mode === 'sign-in' ? 'Sign in' : 'Sign up' }
                </Typography>
                <Box component='form' noValidate onSubmit={store.proceed} sx={{
                    width: '100%',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: spacing(2)
                }}>
                    <TextField
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

                    { store.message && <Alert severity={ store.severity }>{store.message}</Alert> }

                    <Box sx={{ height: spacing(4) }} />

                    <Button fullWidth variant='contained' color='primary' type='submit'>
                        { store.mode === 'sign-in' ? 'Sign in' : 'Sign up' }
                    </Button>

                    { store.mode === 'sign-in' && <>
                        <Box sx={{ height: spacing(2) }} />

                        <Button component={'a'} fullWidth variant='contained' color='primary' href='/account/login/discord'>Use DISCORD</Button>
                    </> }

                    <Box sx={{ height: spacing(4) }} />

                    <Button fullWidth size='small' variant='text' onClick={store.toggleMode}>
                        { store.mode === 'sign-in' ? `Don't have an account? Sign Up` : `Already have an account? Sign in` }
                    </Button>
                </Box>
            </Box>
            <Box>
                <Copyright />
            </Box>
        </Container>
    )
}

export default observer(SignIn)
