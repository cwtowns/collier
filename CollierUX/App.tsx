import React, { useEffect, useState } from 'react';
import { View } from 'react-native';

import * as SignalR from '@microsoft/signalr';
import RawLog from './Collier/RawLog';
import StatsPanel from './Collier/Stats/StatsPanel';
import MiningStateControl from './Collier/Stats/MiningStateControl';

import {
  convertHubConnection,
  CollierHubConnection,
} from './Collier/HubConnection';

import { loadConfiguration, CollierConfig } from './Collier/Config';
import { log } from './Collier/Logger';

console.log = log;

const CollierApp = () => {
  const hub_endpoint: string = 'http://localhost:9999/miner'; //TODO externalize to config

  const createConnection = (): CollierHubConnection => {
    return convertHubConnection(
      new SignalR.HubConnectionBuilder()
        .withUrl(hub_endpoint)
        .configureLogging(SignalR.LogLevel.Information)
        .build(),
    );
  };

  let connection: CollierHubConnection = createConnection();
  const [webSocket, setWebSocket] = useState(connection); // eslint-disable-line @typescript-eslint/no-unused-vars
  const [configObject, setConfigObject] = useState<CollierConfig>();

  useEffect(() => {
    loadConfiguration.then(function (config) {
      console.log('configuration loaded:  ' + JSON.stringify(config));
      setConfigObject(config);
    });
  }, [configObject]);

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
      } else {
        console.log(
          `Skipping reconnect attempt because one is already scheduled:  ${reconnectTimeout}`,
        );
      }
    };

    const onClose = () => {
      console.log(`Disconnected from ${hub_endpoint}`);
      reconnect();
    };

    const connect = () => {
      if (webSocket.state !== SignalR.HubConnectionState.Disconnected) {
        console.info(
          `websocket already in a connected state ${webSocket.state}.  Aborting connect request.`,
        );

        if (webSocket.state !== SignalR.HubConnectionState.Connected) {
          reconnect();
        }
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

      webSocket.onclose(onClose);
    };

    connect();

    return () => {
      clearInterval(reconnectTimeout);
      webSocket.removeOnClose(onClose);
      webSocket.stop();
    };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  if (configObject !== undefined) {
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
            <MiningStateControl config={configObject} websocket={webSocket} />
          </View>
          <View style={{ flex: 7 }}>
            <StatsPanel config={configObject} websocket={webSocket} />
          </View>
        </View>
        <RawLog websocket={webSocket} config={configObject} />
      </View>
    );
  } else {
    return <View />;
  }
};

export default CollierApp;
