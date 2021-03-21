import React, { Component } from 'react';

import { MyProps, MyState } from './StatsCommon';
import StatContainer from './StatContainer';

import AppConfig from '../Config';

class CrashStats extends React.PureComponent<MyProps, MyState> {

    constructor(props: MyProps) {
        super(props);

        this.state = {
            average: 0,
            last: 0
        };

        
        props.websocket.on("CurrentCrashCount", (message) => {
            this.setState(function (state, props) {
                return {
                    last: message
                }
            }) 
        });
    }
    componentWillUnmount() {
        this.props.websocket.off("CurrentCrashCount");
    }

    render() {
        return (
            <StatContainer config={AppConfig.statStates["crash"]} averageValue={this.state.average} lastValue={this.state.last}></StatContainer>
        );
    }
}

export default CrashStats;