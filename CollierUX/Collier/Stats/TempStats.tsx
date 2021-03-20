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


class TempStats extends React.PureComponent<MyProps, MyState> {
    constructor(props: MyProps) {
        super(props);

        this.state = {
            average: 0,
            last: 0
        };

        props.websocket.on("AverageTemp", (message) => {
            this.setState(function (state, props) {
                return {
                    average: message
                }
            })
        });

        props.websocket.on("LastTemp", (message) => {
            this.setState(function (state, props) {
                return {
                    last: message
                }
            })
        });
    }

    render() {
        return (
            <StatContainer config={AppConfig.statStates["temp"]} averageValue={this.state.average} lastValue={this.state.last}></StatContainer>
        );
    }
}

export default TempStats;