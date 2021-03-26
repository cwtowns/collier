//import * as React from 'react';

import React, { useEffect } from 'react';
import { Text, View } from 'react-native';

import * as SignalR from "@microsoft/signalr";
import RawLog from './Collier/RawLog';
import StatsPanel from './Collier/Stats/StatsPanel';
import PowerControl from './Collier/Stats/PowerControl';


//TODO
//  1. convert everything away from classes to function components
//  2. That lets me use hooks or useContext.  useContext (and/or hooks) might be the correct way to
//     handle passing the websocket to subcomponents and not use the context when the hotswap happens.
//  3. DONE - refresh data on connect / reconnect 
//  4. DONE - power button stat representation
//  5. text selection from the UX doesn't work
//  6. update notification for t-rex miner
//  7. external configuration file
//  8. update the read me and make a getting started guide
//  9. power button clickable client side instead of starting / stopping service?  

const CollierApp = () => {
  const hub_endpoint: string = 'http://localhost:9999/miner';

  const connection: SignalR.HubConnection = new SignalR.HubConnectionBuilder()
    .withUrl(hub_endpoint)
    .configureLogging(SignalR.LogLevel.Debug)
    .build();


  useEffect(() => {

    connection.start().then(() => {
      console.log(`Connected to ${hub_endpoint}`);
    }).catch(err => {
      console.log(`Error starting the connection: ${err.toString()}`)
    });

    connection.onclose(async () => {
      console.log(`Disconnected from ${hub_endpoint}`);
    });
  });

  return (
    <View style={{ marginBottom: 10, marginLeft: 10, marginRight: 10 }}>
      <View style={{ flexDirection: 'row', borderBottomWidth: 1, borderColor: "grey"}}>
        <View style={{ flex: 3, justifyContent: 'center', alignItems: 'center' }}>
          <PowerControl websocket={connection}></PowerControl>
        </View>
        <View style={{ flex: 7 }}>
          <StatsPanel websocket={connection}></StatsPanel>
        </View>
      </View>
      <RawLog websocket={connection}></RawLog>
    </View>
  );
}

export default CollierApp;