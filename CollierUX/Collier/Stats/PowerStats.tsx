import React, { Component } from 'react';
import {
    Text,
    View,
} from 'react-native';

import Icon from 'react-native-vector-icons/FontAwesome';

import { MyProps, MyState } from './StatsCommon';
import StatContainer from './StatContainer';

class PowerStats extends React.PureComponent<MyProps, MyState> {
    constructor(props: MyProps) {
        super(props);

        this.state = {
            average: 0,
            last: 0
        };

        props.websocket.on("AveragePower", (message) => {
            this.setState(function (state, props) {
                return {
                    average: message
                }
            })
        });

        props.websocket.on("LastPower", (message) => {
            this.setState(function (state, props) {
                return {
                    last: message
                }
            })
        });
    }

    render() {
        return (
            <StatContainer unitLabel='Watts' iconName='bolt' averageValue={this.state.average} lastValue={this.state.last}></StatContainer>
        );
    }
}

export default PowerStats;