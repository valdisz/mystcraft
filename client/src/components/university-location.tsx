import * as React from 'react'
import styled from '@emotion/styled'
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
    const { game } = useStore()
    const { university } = game

    const region = location.region

    return <tbody>
        <tr>
            <LocationCell colSpan={20}>
                {region.terrain.name}
                ({region.coords.x},{region.coords.y},{region.coords.z} {region.coords.label})
                in {region.province.name}
                { region.settlement ? `, contains ${region.settlement.name} [${region.settlement.size.toLowerCase()}]` : '' }
            </LocationCell>
        </tr>
        <tr>
            <th className='faction'>Faction</th>
            <th className='unit'>Unit</th>
            <th className='target'>Target</th>
            <th className='orders'>Orders</th>

            { university.skills.map((group, i) => <React.Fragment key={i}>
                <th className='empty'></th>
                { group.skills.map(({ code, title }) => <th key={code} title={title}>{code}</th> ) }
            </React.Fragment> ) }
        </tr>
        { location.students.map(student => <UniversityStudent key={student.id} student={student} location={location} />)}
    </tbody>
})
