import * as React from 'react';


export function useClasses(classes: { [name: string]: boolean; }): string {
    return React.useMemo(() => Object
        .keys(classes)
        .map(className => classes[className] && className)
        .filter(className => !!className)
        .join(' '),
        [Object.values(classes)]);
}
