//import * as React from 'react';

import React from 'react';
import { Text, View } from 'react-native';

import * as SignalR from "@microsoft/signalr";
import RawLog from './Collier/RawLog';
import StatsPanel from './Collier/Stats/StatsPanel';
import PowerControl from './Collier/PowerControl';

export class CollierApp extends React.PureComponent {
  hub_endpoint: string = 'http://localhost:9999/miner';

  connection: SignalR.HubConnection = new SignalR.HubConnectionBuilder()
    .withUrl(this.hub_endpoint)
    .configureLogging(SignalR.LogLevel.Debug)
    .build();

  componentDidMount = () => {
    this.connection.start().then(() => {
      console.log(`Connected to ${this.hub_endpoint}`);
    }).catch(err => {
      console.log(`Error starting the connection: ${err.toString()}`)
    });

    this.connection.onclose(async () => {
      console.log(`Disconnected from ${this.hub_endpoint}`);
    });
  };

  render() {
    return (
      <View style={{margin: 10}}>
        <View style={{flexDirection: 'row'}}>
          <View style={{flex:1, justifyContent:'center', alignItems: 'center'}}>
            <PowerControl></PowerControl>
          </View>
          <View style={{flex:9}}>
            <StatsPanel websocket={this.connection}></StatsPanel>
            </View>
        </View>
        <RawLog websocket={this.connection}></RawLog>
      </View>
    );
  }
}


/*
      <View style={{ flexDirection: 'column', justifyContent: 'space-around' }}>
        <View style={{ flexDirection: 'row', justifyContent: 'space-around', margin: 10 }}>
          <StatsPanel websocket={this.connection}></StatsPanel>
        </View>
        <RawLog websocket={this.connection}></RawLog>
      </View>

      */
export default CollierApp;