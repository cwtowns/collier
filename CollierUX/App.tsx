import React, { useEffect, useState } from 'react';
import { View } from 'react-native';

import * as SignalR from '@microsoft/signalr';
import RawLog from './Collier/RawLog';
import StatsPanel from './Collier/Stats/StatsPanel';
import MiningStateControl from './Collier/Stats/MiningStateControl';

//TODO
//  1. text selection from the UX doesn't work
//  2. update notification for t-rex miner
//  3. external configuration file
//  4. update the read me and make a getting started guide
//  5. power button clickable client side instead of starting / stopping service?

const CollierApp = () => {
    const hub_endpoint: string = 'http://localhost:9999/miner'; //TODO externalize to config

    const createConnection = (): SignalR.HubConnection => {
        return new SignalR.HubConnectionBuilder()
            .withUrl(hub_endpoint)
            .configureLogging(SignalR.LogLevel.Information)
            .build();
    };

    let connection: SignalR.HubConnection = createConnection();
    const [webSocket, setWebSocket] = useState(connection); // eslint-disable-line @typescript-eslint/no-unused-vars

    useEffect(() => {
        let reconnectTimeout: number = 0;

        const reconnect = () => {
            if (reconnectTimeout === 0) {
                const reconnectTime: number = 5000; //TODO externalize to config

                reconnectTimeout = +global.setTimeout(() => {
                    reconnectTimeout = 0;
                    console.log(`Scheduling reconnect event in ${reconnectTime} ms.`);
                    connect();
                }, reconnectTime);
            }
            else
                console.log(`Skipping reconnect attempt because one is already scheduled:  ${reconnectTimeout}`);
        };

        const connect = () => {
            if (webSocket.state != SignalR.HubConnectionState.Disconnected) {
                console.info(`websocket already in a connected state ${webSocket.state}.  Aborting connect request.`);

        if (webSocket.state != SignalR.HubConnectionState.Connected)
          reconnect();
        return;
      }

            webSocket
                .start()
                .then(() => {
                    console.log(`Connected to ${hub_endpoint}`);
                })
                .catch(err => {
                    console.log(`Error starting the connection: ${err.toString()}`);
                    reconnect();
                });

            webSocket.onclose(() => {
                console.log(`Disconnected from ${hub_endpoint}`);
                reconnect();
            });
        };

        connect();

        return () => {
            clearInterval(reconnectTimeout);
            webSocket.stop();
        };
    }, []); // eslint-disable-line react-hooks/exhaustive-deps

    return (
        <View style={{ marginBottom: 10, marginLeft: 10, marginRight: 10 }}>
            <View
                style={{
                    flexDirection: 'row',
                    borderBottomWidth: 1,
                    borderColor: 'grey',
                }}>
                <View
                    style={{
                        flex: 3,
                        justifyContent: 'center',
                        alignItems: 'center',
                    }}>
                    <MiningStateControl websocket={webSocket}></MiningStateControl>
                </View>
                <View style={{ flex: 7 }}>
                    <StatsPanel websocket={webSocket} />
                </View>
            </View>
            <RawLog websocket={webSocket} />
        </View>
    );
};

export default CollierApp;
