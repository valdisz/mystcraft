
export async function copy(text: string) {
    const perm = await navigator.permissions.query({ name: 'clipboard-write' as PermissionName });
    if (perm.state === 'denied') {
        return;
    }

    const result = await navigator.clipboard.writeText(text);
}
