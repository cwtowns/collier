//import * as React from 'react';

import React, { useEffect, useState } from 'react';
import { Text, View } from 'react-native';

import * as SignalR from "@microsoft/signalr";
import RawLog from './Collier/RawLog';
import StatsPanel from './Collier/Stats/StatsPanel';
import PowerControl from './Collier/Stats/PowerControl';

//TODO
//  1. text selection from the UX doesn't work
//  2. update notification for t-rex miner
//  3. external configuration file
//  4. update the read me and make a getting started guide
//  5. power button clickable client side instead of starting / stopping service?  

const CollierApp = () => {
  const hub_endpoint: string = 'http://localhost:9999/miner'; //TODO externalize to config

  //const connection: SignalR.HubConnection = 
  const createConnection = (): SignalR.HubConnection => {
    return new SignalR.HubConnectionBuilder()
      .withUrl(hub_endpoint)
      .configureLogging(SignalR.LogLevel.Debug)
      .build();
  };

  //const [connection, setConnection] = useState(createConnection());
  let connection: SignalR.HubConnection = createConnection();
  const [webSocket, setWebSocket] = useState(connection);

  useEffect(() => {
    let reconnectTimeout: number = 0;

    const reconnect = () => {
      if (reconnectTimeout === 0) {
        const reconnectTime: number = 5000;  //TODO externalize to config

        reconnectTimeout = +global.setTimeout(() => {
          console.log(`Scheduling reconnect event in ${reconnectTime} ms.`);
          connect();
        }, reconnectTime);
      }
    };

    const connect = () => {
      if (webSocket.state != SignalR.HubConnectionState.Disconnected) {
        console.info(`websocket already in a connected state ${webSocket.state}.  Aborting connect request.`);
        reconnect();
        return;
      }

      webSocket.start().then(() => {
        console.log(`Connected to ${hub_endpoint}`);
      }).catch(err => {
        console.log(`Error starting the connection: ${err.toString()}`);
        reconnect();
      });

      webSocket.onclose(() => {
        console.log(`Disconnected from ${hub_endpoint}`);
        reconnect();
      });
    }

    connect();

    return () => {
      clearInterval(reconnectTimeout);
      webSocket.stop();
    }
  }, []);

  return (
    <View style={{ marginBottom: 10, marginLeft: 10, marginRight: 10 }}>
      <View style={{ flexDirection: 'row', borderBottomWidth: 1, borderColor: "grey" }}>
        <View style={{ flex: 3, justifyContent: 'center', alignItems: 'center' }}>
          <PowerControl websocket={webSocket}></PowerControl>
        </View>
        <View style={{ flex: 7 }}>
          <StatsPanel websocket={webSocket}></StatsPanel>
        </View>
      </View>
      <RawLog websocket={webSocket}></RawLog>
    </View>
  );
}

export default CollierApp;