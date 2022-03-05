import '@emotion/react'
import { Theme as MuiTheme } from '@mui/material/styles'

declare module '@emotion/react' {
    export interface Theme extends MuiTheme {
    }
}

declare module "*.ttf" {
    const value: any;
    export default value;
}

declare module "*.txt" {
    const value: any;
    export default value;
}
declare module "*.png" {
    const value: any;
    export default value;
}
