import React, { Component } from 'react';

import Icon from 'react-native-vector-icons/FontAwesome';

import { MyProps } from './StatsCommon';

import AppTheme from '../Theme';

import { Color } from "react-color";

interface MyState {
    state: 'Unknown' | 'Running' |  'Stopped' |  'Paused'
}

class PowerControl extends React.Component<MyProps, MyState> {
    
    constructor(props) {
        super(props);
        
        this.state = {
            state: 'Unknown'
        };

        props.websocket.on("MiningState", (message) => {
            this.setState(function (state, props) {
                return {
                    state: message
                }
            })
        });
    }

    getStateColor(): Color {            
        if (this.state.state === 'Running')
            return AppTheme.miningState.mining;
        if (this.state.state === 'Stopped' || this.state.state === 'Paused')
            return AppTheme.miningState.paused;

        return AppTheme.miningState.unknown;
    }

    componentWillUnmount() {
        this.props.websocket.off("MiningState");
    }

    render() {
        return (
            <Icon name="power-off" size={90} color={this.getStateColor().toString()} />
        );
    }

}

export default PowerControl;
