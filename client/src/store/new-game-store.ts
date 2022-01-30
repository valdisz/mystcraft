import * as React from 'react';
import { action, makeObservable, observable } from 'mobx';

export class NewGameStore {
    constructor() {
        makeObservable(this)
    }

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

    engineFile: File | null
    playersFile: File | null
    gameFile: File | null

    setFile = (type: 'engine' | 'players' | 'game', f: File) => {
        switch (type) {
            case 'engine':
                this.engineFile = f
                break
            case 'players':
                this.playersFile = f
                break
            case 'game':
                this.gameFile = f
                break
        }
    }
}
