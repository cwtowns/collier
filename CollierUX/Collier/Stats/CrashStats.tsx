import React, { Component } from 'react';

import { MyProps, MyState } from './StatsCommon';
import StatContainer from './StatContainer';

import AppConfig from '../Config';

class CrashStats extends React.PureComponent<MyProps, MyState> {
    constructor(props: MyProps) {
        super(props);

        this.state = {
            average: 11,
            last: 22
        };

        /*
        props.websocket.on("AverageHashRate", (message) => {
            this.setState(function (state, props) {
                return {
                    average: message
                }
            })
        });

        props.websocket.on("LastHashRate", (message) => {
            this.setState(function (state, props) {
                return {
                    last: message
                }
            })
        });
        */
    }

    render() {
        return (
            <StatContainer config={AppConfig.statStates["crash"]} averageValue={this.state.average} lastValue={this.state.last}></StatContainer>
        );
    }
}

export default CrashStats;