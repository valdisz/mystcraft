import * as React from 'react'
import styled, { css } from 'styled-components'

export interface SkillCellProps {
    title: string
    days?: number
    level?: number

    active: boolean
    onClick: () => void

    isTarget: boolean
    missing: number

    studying: boolean
    withTeacher: boolean
}

const CellBody = styled.div<Partial<SkillCellProps>>`
    font-size: 11px;
    font-family: Fira Code, Roboto Mono, monospace;

    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: row;

    justify-content: space-between;
    align-items: stretch;

    border-width: 2px;
    border-style: solid;
    border-color: ${props => {
        if (props.isTarget) return 'blue'
        if (props.missing) return 'orange'

        return 'white'
    }};
`

const SkillInfo = styled.div`
    display: flex;
    flex-direction: column;
    justify-content: space-between;
`

const SkillLevel = styled.div`
    font-weight: bold;
    padding: 2px;
    background-color: #e0e0e0;
`

const MissingSkillLevel = styled.div`
    font-size: 80%;
    padding: 2px;
`

const SkillDays = styled.div`
    flex: 1;

    display: flex;
    justify-content: center;
    align-items: center;

    font-size: 13px;
    padding: 2px;
`

interface CellProps {
    active: boolean
    studying: boolean
    withTeacher: boolean
}

const Cell = styled.td<CellProps>`
    padding: 0;
    height: 100%;

    background-color: ${props => {
        if (props.active) return 'lightblue';
        if (props.studying && props.withTeacher) return 'green';
        if (props.studying) return 'lightgreen'

        return 'white'
    }};

    ${props => props.active && css`cursor: pointer;`}
`

export function SkillCell({ title, onClick, ...props }: SkillCellProps) {
    return <Cell active={props.active} studying={props.studying} withTeacher={props.withTeacher}
        title={title}
        onClick={() => props.active && onClick()}>
        <CellBody {...props}>
            <SkillDays>{props.days ? props.days : ' '}</SkillDays>
            { props.level || props.missing > 0
                ? <SkillInfo>
                    { props.level ? <SkillLevel>{props.level}</SkillLevel> : <span></span> }
                    { props.missing > 0 ? <MissingSkillLevel>+{props.missing}</MissingSkillLevel> : null }
                </SkillInfo>
                : null
            }
        </CellBody>
    </Cell>
}
