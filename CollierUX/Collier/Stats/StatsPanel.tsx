import React, { Component } from 'react';
import {
    View,
} from 'react-native';

import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

import { MyProps } from './StatsCommon';

import HashStats from './HashStats';
import PowerStats from './PowerStats';
import TempStats from './TempStats';
import CrashStats from './CrashStats';

class StatsPanel extends React.PureComponent<MyProps> {
    render() {
        return (
            <View style={{ margin: 5, flexDirection: "row" }}>
                <View style={{ flex: 1, margin: 10, alignSelf: "stretch" }}>                    
                    <PowerStats websocket={this.props.websocket}></PowerStats>
                    <TempStats websocket={this.props.websocket}></TempStats>
                </View>
                <View style={{ flex: 1, margin: 10, alignSelf: "stretch", alignItems: "stretch" }}>
                    <HashStats websocket={this.props.websocket}></HashStats>
                    <CrashStats websocket={this.props.websocket}></CrashStats>
                </View>
            </View>
        );
    }
}

export default StatsPanel;