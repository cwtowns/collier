import React, { Component } from 'react';
import {
    Text,
    View,
} from 'react-native';

import Icon from 'react-native-vector-icons/FontAwesome';

import { MyProps, MyState } from './StatsCommon';
import StatContainer from './StatContainer';

import AppTheme from '../Theme';
import AppConfig from '../Config';

class HashStats extends React.PureComponent<MyProps, MyState> {
    constructor(props: MyProps) {
        super(props);

        this.state = {
            average: 0,
            last: 0
        };

        props.websocket.on("AverageHashRate", (message) => {
            this.setState(function (state, props) {
                return {
                    average: parseFloat(parseFloat(message).toFixed(2))
                }
            });
        });

        props.websocket.on("LastHashRate", (message) => {
            this.setState(function (state, props) {
                return {
                    last: message
                }
            });
        });
    }

    render() {
        return (
            <StatContainer config={AppConfig.statStates["hash"]} averageValue={this.state.average} lastValue={this.state.last}></StatContainer>
        );
    }
}

export default HashStats;