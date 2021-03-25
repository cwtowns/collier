import React, { useState, useEffect } from 'react';
import {
    View,
    Text,
    ScrollView,
} from 'react-native';

import * as SignalR from "@microsoft/signalr";

interface RawLogProps {
    websocket: SignalR.HubConnection
}

const RawLog = (props: RawLogProps) => {
    const maxBacklog: number = 100

    const scrollViewRef: React.RefObject<ScrollView> = React.createRef<ScrollView>();

    const [performedFirstScroll, setPerformedFirstScroll] = useState(false);
    const [logArray, setLogArray] = useState(Array(maxBacklog).join(' ').split(' '));

    useEffect(() => {
        props.websocket.on("Log", (message) => {
            updateLog(message);
        });

        return () => {
            props.websocket.off("Log");
        }
    });

    const updateLog = (message: string) => {
        const backlog = maxBacklog;  //TODO how do I avoid this?

        let newLog;

        if (logArray.length < backlog) {
            newLog = logArray.concat(message);
        }
        else {
            newLog = [...logArray.slice(1, logArray.length), message];
        }

        setLogArray(newLog);
    };

    const checkForFirstScroll = (width: number, height: number) => {
        //oddly I could not get a simple scrollToEnd in componentDidMount() to work
        if (performedFirstScroll === false) {
            scrollViewRef.current?.scrollTo({ y: height });
            setPerformedFirstScroll(true);
        }
    };

    return (
        <View style={{ height: 120 }} >
            <ScrollView ref={scrollViewRef} style={{ width: "100%", borderWidth: 1 }} onContentSizeChange={(width, height) => checkForFirstScroll(width, height)}>
                {logArray.map((txt, i) => <Text key={i}>{txt}</Text>)}
            </ScrollView>
        </View>
    );
}

export default RawLog;