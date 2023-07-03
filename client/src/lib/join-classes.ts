
export function joinClasses(...classes: string[]) {
    return classes.filter(x => !!x).join(' ');
}
