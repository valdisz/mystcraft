function getProvinceNameFromQuest(text: string) {
    const province = (
        text.match(/^In the \w+ of (\w+)/)
        || text.match(/^Seek a token .+ of (.+)\.$/)
        || text.match(/^Build a .+ in (.+) for the glory of the Gods\.$/)
    )

    if (province) return province[1]

    return null
}
