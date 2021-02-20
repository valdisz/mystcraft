import * as React from 'react';
import { action, observable } from 'mobx';

export class NewGameStore {
    @observable isOpen = false;
    @observable name = '';

    @action open = () => {
        this.name = '';
        this.isOpen = true;
    };

    @action cancel = () => {
        this.isOpen = false;
    };

    @action setName = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.name = e.target.value;
    };
}
