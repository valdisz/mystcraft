import * as React from 'react'
import styled, { css } from 'styled-components'
import { Tooltip, Typography } from '@material-ui/core'
import { ISkill } from '../store/skill-tree'
import { StudyTarget, StudentMode } from '../store'


export interface SkillCellProps {
    skill: ISkill
    study: StudyTarget

    mode: StudentMode
    active: boolean
    onClick: () => void

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
        if (props.study?.isTarget) return 'blue'
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
    color: black;
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

    color: ${props => !props.active && props.studying && props.withTeacher ? 'white' : 'black'};

    ${props => props.active && css`cursor: pointer;`}
    ${props => {
        if (!props.active) return

        if (props.studying && props.withTeacher) return css`outline: 3px solid green;`
        if (props.studying) return css`outline: 3px solid lightgreen;`
    }}
`

function SkillTootipContent({ study }: Partial<SkillCellProps>) {
    return <>
        <Typography variant='h6'>{study.title} [{study.code}]</Typography>
        <Typography variant='body2'>
            <strong>Target Level</strong>: {study.level} ({study.days})
        </Typography>
        <Typography variant='body2'>
            <strong>Effort</strong>: {study.effort} turns
        </Typography>
    </>
}

export function SkillCell({ onClick, ...props }: SkillCellProps) {
    const { title, level, days } = props.skill
    return <Cell active={props.active} studying={props.studying} withTeacher={props.withTeacher}
        onClick={() => props.active && onClick()}>
        <Tooltip title={<SkillTootipContent {...props} />}>
            <CellBody {...props}>
                { props.mode !== 'target-selection'
                    ? <>
                        <SkillDays>{days ? days : ' '}</SkillDays>
                        { level || props.missing > 0
                            ? <SkillInfo>
                                { level ? <SkillLevel>{level}</SkillLevel> : <span></span> }
                                { props.missing > 0 ? <MissingSkillLevel>+{props.missing}</MissingSkillLevel> : null }
                            </SkillInfo>
                            : null
                        }
                    </>
                    : <SkillDays>{ props.study.effort }</SkillDays>
                }
            </CellBody>
        </Tooltip>
    </Cell>
}
