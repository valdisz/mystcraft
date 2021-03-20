import React from 'react';
import Avatar from '@material-ui/core/Avatar';
import Button from '@material-ui/core/Button';
import CssBaseline from '@material-ui/core/CssBaseline';
import TextField from '@material-ui/core/TextField';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import Checkbox from '@material-ui/core/Checkbox';
import Link from '@material-ui/core/Link';
import Grid from '@material-ui/core/Grid';
import Box from '@material-ui/core/Box';
import LockOutlinedIcon from '@material-ui/icons/LockOutlined';
import Typography from '@material-ui/core/Typography';
import { makeStyles } from '@material-ui/core/styles';
import Container from '@material-ui/core/Container';
import { Alert } from '@material-ui/lab';

function Copyright() {
    return (
        <Typography variant='body2' color='textSecondary' align='center'>
            {'Copyright © '}
            <Link color='inherit' href='https://advisor.azurewebsites.net'>Valdis Zobēla</Link>
            {' '}
            {new Date().getFullYear()}
            {'.'}
        </Typography>
    );
}

const useStyles = makeStyles((theme) => ({
    paper: {
        marginTop: theme.spacing(8),
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
    },
    avatar: {
        margin: theme.spacing(1),
        backgroundColor: theme.palette.secondary.main,
    },
    form: {
        width: '100%', // Fix IE 11 issue.
        marginTop: theme.spacing(1),
    },
    submit: {
        margin: theme.spacing(3, 0, 2),
    },
}));

export interface SignInProps {
    onSuccess: () => void
    onGoToSignUp: () => void
}

export function SignIn({ onSuccess, onGoToSignUp }: SignInProps) {
    const classes = useStyles();
    const [wrongCreds, setWrongCreds] = React.useState(false)

    const onSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        e.stopPropagation()

        const response = await fetch('/login', {
            method: 'POST',
            credentials: 'same-origin',
            body: new FormData(e.target as any)
        })

        if (response.ok) {
            onSuccess()
        }
        else {
            setWrongCreds(true)
        }
    }

    return (
        <Container component='main' maxWidth='xs'>
            <div className={classes.paper}>
                <Avatar className={classes.avatar}>
                    <LockOutlinedIcon />
                </Avatar>
                <Typography component='h1' variant='h5'>
                    Sign in
                </Typography>
                <form className={classes.form} noValidate onSubmit={onSubmit}>
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
                        autoComplete='current-password'
                    />
                    { wrongCreds && <Alert severity='error'>Username or password is incorrect!</Alert> }
                    {/* <FormControlLabel
                        control={<Checkbox value='remember' color='primary' />}
                        label='Remember me'
                    /> */}
                    <Button fullWidth
                        variant='contained'
                        color='primary'
                        type='submit'
                        className={classes.submit}>Sign In</Button>
                    <Grid container>
                        {/* <Grid item xs>
                            <Link href='#' variant='body2'>Forgot password?</Link>
                        </Grid> */}
                        <Grid item>
                            <Button size='small' variant='text' onClick={onGoToSignUp}>
                                {`Don't have an account? Sign Up`}
                            </Button>
                        </Grid>
                    </Grid>
                </form>
            </div>
            <Box mt={8}>
                <Copyright />
            </Box>
        </Container>
    );
}


export interface SignUpProps {
    onSuccess: () => void
    onGoToSignIn: () => void
}

export function SignUp({ onSuccess, onGoToSignIn }: SignUpProps) {
    const classes = useStyles();

    const onSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        e.stopPropagation()

        onSuccess()
    }

    return (
        <Container component='main' maxWidth='xs'>
            <div className={classes.paper}>
                <Avatar className={classes.avatar}>
                    <LockOutlinedIcon />
                </Avatar>
                <Typography component='h1' variant='h5'>
                    Sign up
                </Typography>
                <form className={classes.form} noValidate onSubmit={onSubmit}>
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
                        autoComplete='current-password'
                    />

                    <Button fullWidth
                        variant='contained'
                        color='primary'
                        type='submit'
                        className={classes.submit}>Sign Up</Button>

                    <Grid container>
                        <Grid item>
                            <Button size='small' variant='text' onClick={onGoToSignIn}>
                                Already have an account? Sign in
                            </Button>
                        </Grid>
                    </Grid>
                </form>
            </div>
            <Box mt={8}>
                <Copyright />
            </Box>
        </Container>
    );
  }
