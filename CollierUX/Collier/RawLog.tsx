import React, { Component } from 'react';
import {
    View,
    Text,
    ScrollView,
} from 'react-native';

import * as SignalR from "@microsoft/signalr";

interface RawLogProps {
    websocket: SignalR.HubConnection
}

interface RawLogState {
    logArray: string[],
    performedFirstScroll: boolean
}

class RawLog extends React.PureComponent<RawLogProps, RawLogState>  {
    maxBacklog: number = 100

    scrollViewRef : React.RefObject<ScrollView>;

    constructor(props: RawLogProps) {
        super(props);

        props.websocket.on("Log", (message) => {
            this.updateLog(message);
        });

        this.state = {
            logArray: Array(this.maxBacklog).join(' ').split(' '),
            performedFirstScroll: false
        };

        this.scrollViewRef = React.createRef<ScrollView>();
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

    checkForFirstScroll(width: number, height: number) {
        //oddly I could not get a simple scrollToEnd in componentDidMount() to work
        if(!this.state.performedFirstScroll) {
            this.scrollViewRef.current?.scrollTo({y:height});
            this.setState(() => {
                return {
                    performedFirstScroll: true
                }
            })
        }
    }

    componentWillUnmount() {
        this.props.websocket.off("Log");
    }

    render() {
        return (
            <View style={{height: 120}} >
                <ScrollView ref={this.scrollViewRef} style={{ width: "100%", borderWidth: 1 }} onContentSizeChange={(width,height) => this.checkForFirstScroll(width, height)}> 
                    {this.state.logArray.map((txt, i) => <Text key={i}>{txt}</Text>)}
                </ScrollView>
            </View>
        );
    }
}

export default RawLog;