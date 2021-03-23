import React, { Component } from 'react';

import { MyProps, MyState } from './StatsCommon';
import StatContainer from './StatContainer';

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
            });
        });

        props.websocket.on("LastTemp", (message) => {
            this.setState(function (state, props) {
                return {
                    last: message
                }
            });
        });
    }

    componentWillUnmount() {
        this.props.websocket.off("AverageTemp");
        this.props.websocket.off("LastTemp");
    }

    render() {
        return (
            <StatContainer config={AppConfig.statStates["temp"]} averageValue={this.state.average} lastValue={this.state.last}></StatContainer>
        );
    }
}

export default TempStats;