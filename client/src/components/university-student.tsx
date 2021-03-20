import * as React from 'react'
import styled from 'styled-components'
import { observer } from 'mobx-react-lite'
import { SkillCell } from './skill-cell'
import { Student, StudyLocation } from '../store'
import { Badge, Box, ButtonGroup, Typography } from '@material-ui/core'
import { CopyButton, XsButton } from './buttons'

export interface UniversityStudentProps {
    student: Student
    location: StudyLocation
}

const UnitElement = styled.div`
    display: inline-block;
`

const UnitName = styled.div<{ teaching: boolean }>`
    font-weight: ${p => p.teaching ? 'bold' : 'normal '};
`

const StudentCount = styled.em`
    font-size: 80%;
`

const Unit = observer(({ student }: { student: Student }) => {
    return <UnitElement>
        <UnitName teaching={!!student.teach.length}>{student.name} ({student.number})</UnitName>
        { student.teach.length > 0 &&
            <StudentCount>{student.teach.length} of 10 students</StudentCount>
        }
    </UnitElement>
})

export const UniversityStudent = observer(({ student, location }: UniversityStudentProps) => {
    return <tr>
        <td className='faction'>{student.factionName} ({student.factionNumber})</td>
        <td className='unit'>
            { student.teacher && <></> }
            <Unit student={student} />
            <Box ml={1} clone>
                { student.mode === ''
                    ? <ButtonGroup>
                        <XsButton variant='outlined' title='Study' onClick={student.beginStudy}>S</XsButton>
                        <XsButton variant='outlined' title='Teach' onClick={student.beginTeaching}>T</XsButton>
                        <XsButton variant='outlined' title='Clear' onClick={student.clearOrders}>X</XsButton>
                    </ButtonGroup>
                    : <XsButton variant='outlined' onClick={student.resetMode}>Done</XsButton>
                }
            </Box>
        </td>
        <td className='target'>
            { student.target
                ? <ButtonGroup title={student.target.title}>
                    <XsButton variant='outlined' onClick={student.beginTargetSelection}>{student.target.code}</XsButton>
                    <XsButton variant='outlined' className='skill-level' onClick={student.incTargetLevel}>{student.target.level}</XsButton>
                </ButtonGroup>
                : <XsButton fullWidth variant='outlined' onClick={student.beginTargetSelection}><i>none</i></XsButton>
            }
        </td>
        <td className='orders'>
            { student.ordersShort && <CopyButton fullWidth color='primary' variant='contained' text={student.ordersFull} title={student.ordersFull}>{student.ordersShort}</CopyButton> }
        </td>
        { student.skillsGroups.map((group, i) => (
            <React.Fragment key={i}>
                <td className='empty'></td>
                { group.skills.map(skill => (
                    <SkillCell key={skill.code}
                        active={student.isSkillActive(skill.code)}
                        title={skill.title}
                        days={skill.days}
                        level={skill.level}
                        isTarget={student.isTargetSkill(skill.code)}
                        missing={student.getMissingLevel(skill.code)}
                        studying={student.study == skill.code}
                        withTeacher={!!student.teacher}
                        onClick={() => student.skillClick(skill.code)}
                    />) ) }
            </React.Fragment>) ) }
    </tr>
})
