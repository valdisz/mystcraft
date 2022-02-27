import * as React from 'react'
import styled from '@emotion/styled'
import { observer } from 'mobx-react'
import { StudyLocation, useStore } from '../store'
import { UniversityStudent } from './university-student'

const LocationCell = styled.th`
    padding-top: 2rem !important;
    border: none;
    font-size: 125%;
`

export interface UniversityLocationProps {
    location: StudyLocation
}

export const UniversityLocation = observer(({ location }: UniversityLocationProps) => {
    const { game } = useStore()
    const { university } = game

    const region = location.region

    const rows = []
    let lastFaction = null
    for (const student of location.students) {
        const currentFaction = `${student.factionName} (${student.factionNumber})`
        if (lastFaction !== currentFaction) {
            lastFaction = currentFaction
            rows.push(<tr key={currentFaction}>
                <td className='faction'>{currentFaction}</td>
            </tr>)

            rows.push(<tr key={`${currentFaction}-2`}>
                <th className='unit'>Unit</th>
                <th className='target'>Target</th>
                <th className='orders'>Orders</th>

                { university.skills.map((group, i) => <React.Fragment key={i}>
                    <th className='empty'></th>
                    { group.skills.map(({ code, title }) => <th key={code} title={title}>{code}</th> ) }
                </React.Fragment> ) }
            </tr>)
        }

        rows.push(<UniversityStudent key={student.id} student={student} location={location} />)
    }


    return <tbody>
        <tr>
            <LocationCell colSpan={20}>
                {region.terrain ?? 'unknown'}
                ({region.x},{region.y},{region.z} {region.label})
                in {region.province ?? 'unknown'}
                { region.settlement ? `, contains ${region.settlement} [${region.size}]` : '' }
            </LocationCell>
        </tr>
        {rows}
        {/* <tr>
            <th className='faction'>Faction</th>
            <th className='unit'>Unit</th>
            <th className='target'>Target</th>
            <th className='orders'>Orders</th>

            { university.skills.map((group, i) => <React.Fragment key={i}>
                <th className='empty'></th>
                { group.skills.map(({ code, title }) => <th key={code} title={title}>{code}</th> ) }
            </React.Fragment> ) }
        </tr>
        { location.students.map(student => <UniversityStudent key={student.id} student={student} location={location} />)} */}
    </tbody>
})
