import React, { Component } from 'react';
import {
    Text,
    ScrollView,
} from 'react-native';

import * as SignalR from "@microsoft/signalr";

interface RawLogProps {
    websocket: SignalR.HubConnection
}

interface RawLogState {
    logArray: string[]
}

class RawLog extends React.PureComponent<RawLogProps, RawLogState>  {
    maxBacklog: number = 10

    constructor(props: RawLogProps) {
        super(props);

        props.websocket.on("Log", (message) => {
            this.updateLog(message);
        });

        this.state = {
            logArray: Array(this.maxBacklog).join(' ').split(' ')
        };
    }

    updateLog(message: string) {
        const backlog = this.maxBacklog;  //TODO how do I avoid this?

        this.setState(function (state, props) {
            let newLog;

            if (state.logArray.length < backlog) {
                newLog = state.logArray.concat(message);
            }
            else {
                newLog = [...state.logArray.slice(1, state.logArray.length), message];
            }

            return {
                logArray: newLog
            }
        });
    }

    render() {
        return (

            <ScrollView style={{ width: "100%", backgroundColor: 'purple' }}>
                {this.state.logArray.map((txt, i) => <Text key={i}>{txt}</Text>)}
            </ScrollView>
        );
    }
}

export default RawLog;