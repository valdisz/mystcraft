
export function numhash(value: number) {
    value = ((value >> 16) ^ value) * 73244475;
    value = ((value >> 16) ^ value) * 73244475;
    value = (value >> 16) ^ value;

    return value;
}
