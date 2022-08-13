import { createTheme } from '@mui/material/styles'

const heading = {
    fontFamily: 'Almendra, serif'
}

const theme = createTheme({
    palette: {
        // mode: 'dark'
    },
    typography: {
        fontFamily: 'Fira Code, monospace',
        fontSize: 12,
        h1: heading,
        h2: heading,
        h3: heading,
        h4: heading,
    },
    shape: {
        borderRadius: 2
    },
    spacing: 4,
    components: {
        MuiList: {
            defaultProps: {
                dense: true,
            }
        },
        MuiMenuItem: {
            defaultProps: {
                dense: true,
            }
        },
        MuiTable: {
            defaultProps: {
                size: 'small',
                stickyHeader: true
            }
        },
        MuiButton: {
            defaultProps: {
                size: 'small',
            }
        },
        MuiButtonGroup: {
            defaultProps: {
                size: 'small',
            }
        },
        MuiCheckbox: {
            defaultProps: {
                size: 'small',
            }
        },
        MuiFab: {
            defaultProps: {
                size: 'small',
            }
        },
        MuiFormControl: {
            defaultProps: {
                margin: 'dense',
                size: 'small',
            }
        },
        MuiFormHelperText: {
            defaultProps: {
                margin: 'dense',
            }
        },
        MuiIconButton: {
            defaultProps: {
                size: 'small',
            }
        },
        MuiInputBase: {
            defaultProps: {
                margin: 'dense',
            }
        },
        MuiInputLabel: {
            defaultProps: {
                margin: 'dense',
            }
        },
        MuiRadio: {
            defaultProps: {
                size: 'small',
            }
        },
        MuiSwitch: {
            defaultProps: {
                size: 'small',
            }
        },
        MuiTextField: {
            defaultProps: {
                margin: 'dense',
                size: 'small',
            }
        },
        MuiAppBar: {
            defaultProps: {
                color: 'default',
            }
        },
    }
})

export default theme
