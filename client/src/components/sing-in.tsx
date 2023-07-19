import React from 'react'
import { Avatar, Button, Box, TextField, Alert, Typography, Container, Stack, createSvgIcon, Divider } from '@mui/material'
import LockOutlinedIcon from '@mui/icons-material/LockOutlined'
import { observer, useLocalStore } from 'mobx-react'
import { runInAction } from 'mobx'
import { Copyright } from './copyright'

import FingerprintIcon from '@mui/icons-material/Fingerprint'
const DiscordIcon = createSvgIcon(
    <path d="M20.317 4.3698a19.7913 19.7913 0 00-4.8851-1.5152.0741.0741 0 00-.0785.0371c-.211.3753-.4447.8648-.6083 1.2495-1.8447-.2762-3.68-.2762-5.4868 0-.1636-.3933-.4058-.8742-.6177-1.2495a.077.077 0 00-.0785-.037 19.7363 19.7363 0 00-4.8852 1.515.0699.0699 0 00-.0321.0277C.5334 9.0458-.319 13.5799.0992 18.0578a.0824.0824 0 00.0312.0561c2.0528 1.5076 4.0413 2.4228 5.9929 3.0294a.0777.0777 0 00.0842-.0276c.4616-.6304.8731-1.2952 1.226-1.9942a.076.076 0 00-.0416-.1057c-.6528-.2476-1.2743-.5495-1.8722-.8923a.077.077 0 01-.0076-.1277c.1258-.0943.2517-.1923.3718-.2914a.0743.0743 0 01.0776-.0105c3.9278 1.7933 8.18 1.7933 12.0614 0a.0739.0739 0 01.0785.0095c.1202.099.246.1981.3728.2924a.077.077 0 01-.0066.1276 12.2986 12.2986 0 01-1.873.8914.0766.0766 0 00-.0407.1067c.3604.698.7719 1.3628 1.225 1.9932a.076.076 0 00.0842.0286c1.961-.6067 3.9495-1.5219 6.0023-3.0294a.077.077 0 00.0313-.0552c.5004-5.177-.8382-9.6739-3.5485-13.6604a.061.061 0 00-.0312-.0286zM8.02 15.3312c-1.1825 0-2.1569-1.0857-2.1569-2.419 0-1.3332.9555-2.4189 2.157-2.4189 1.2108 0 2.1757 1.0952 2.1568 2.419 0 1.3332-.9555 2.4189-2.1569 2.4189zm7.9748 0c-1.1825 0-2.1569-1.0857-2.1569-2.419 0-1.3332.9554-2.4189 2.1569-2.4189 1.2108 0 2.1757 1.0952 2.1568 2.419 0 1.3332-.946 2.4189-2.1568 2.4189Z"/>,
    'Discord'
)


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
    withPasskey: boolean
    onSuccess: () => void
}

function SignIn({ withPasskey, onSuccess }: SignInProps) {
    const store = useSignInStore(onSuccess)

    return (
        <Container component='main' maxWidth='xs' sx={{
            height: '100%',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'stretch',
            justifyContent: 'center',
            gap: 8
        }}>
            <Box>
                <Typography variant='h1' align='center'>Mystcraft</Typography>
                <Typography variant='subtitle1' align='right'>The Atlantis PBEM game host &amp; client</Typography>
            </Box>

            <Stack alignItems='center'>
                <Avatar sx={{
                    margin: 1,
                    backgroundColor: 'secondary.main',
                }}>
                    <LockOutlinedIcon />
                </Avatar>

                <Typography component='h1' variant='h5' mb={8}>
                    { store.mode === 'sign-in' ? 'Sign in' : 'Sign up' }
                </Typography>

                <Stack gap={2}>
                    { store.message && <Alert severity={ store.severity }>{store.message}</Alert> }

                    <Stack gap={2} mb={4}>
                        <Button size='large' variant='contained' color='primary' type='submit' startIcon={<FingerprintIcon />}>
                            { store.mode === 'sign-in' ? 'Passkey' : 'Create Passkey'}
                        </Button>

                        <Button component={'a'} size='large' variant='contained' color='primary' href='/account/login/discord' startIcon={<DiscordIcon />}>
                            Discord
                        </Button>
                    </Stack>

                    <Divider>or</Divider>

                    <Stack mb={4} component='form' noValidate onSubmit={store.proceed}>
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

                        <Box mt={2}>
                            <Button fullWidth size='large' variant='outlined' color='primary' type='submit'>
                                { store.mode === 'sign-in' ? 'Sign in' : 'Sign up' }
                            </Button>
                        </Box>
                    </Stack>

                    <Box mt={4}>
                        <Button fullWidth variant='text' onClick={store.toggleMode}>
                            { store.mode === 'sign-in' ? `Don't have an account? Sign Up` : `Already have an account? Sign in` }
                        </Button>
                    </Box>
                </Stack>
            </Stack>

            <Box>
                <Copyright />
            </Box>
        </Container>
    )
}

export default observer(SignIn)
