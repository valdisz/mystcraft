/**
 * Hashes a number to a 32-bit integer.
 */
export default function numhash(value: number) {
    value = ((value >> 16) ^ value) * 73244475;
    value = ((value >> 16) ^ value) * 73244475;
    value = (value >> 16) ^ value;

    return value;
}
