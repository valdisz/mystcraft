import * as React from 'react'
import styled from 'styled-components'
import { observer } from 'mobx-react-lite'
import { StudyLocation, useStore } from '../store'
import { UniversityStudent } from './university-student'

const LocationCell = styled.th`
    padding-top: 2rem !important;
    border: none;
`

export interface UniversityLocationProps {
    location: StudyLocation
}

export const UniversityLocation = observer(({ location }: UniversityLocationProps) => {
    const { university } = useStore()

    return <tbody>
        <tr>
            <LocationCell colSpan={20}>
                {location.terrain}
                ({location.x},{location.y},{location.z} {location.label})
                in {location.province}
                { location.settlement ? `, contains ${location.settlement} [${location.settlementSize.toLowerCase()}]` : '' }
            </LocationCell>
        </tr>
        <tr>
            <th className='faction'>Faction</th>
            <th className='unit'>Unit</th>
            <th className='target'>Target</th>
            <th className='orders'>Orders</th>

            { university.skills.map((group, i) => <React.Fragment key={i}>
                <th className='empty'></th>
                { group.skills.map(({ code }) => <th key={code}>{code}</th> ) }
            </React.Fragment> ) }
        </tr>
        { location.students.map(student => <UniversityStudent key={student.id} student={student} location={location} />)}
    </tbody>
})
