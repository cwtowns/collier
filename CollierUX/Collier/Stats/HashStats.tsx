import React, { Component } from 'react';

import { MyProps, MyState } from './StatsCommon';
import * as SignalR from "@microsoft/signalr";
import StatContainer from './StatContainer';

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

    componentWillUnmount() {
        this.props.websocket.off("AverageHashRate");
        this.props.websocket.off("LastHashRate");
    }

    render() {
        return (
            <StatContainer config={AppConfig.statStates["hash"]} averageValue={this.state.average} lastValue={this.state.last}></StatContainer>
        );
    }
}

export default HashStats;