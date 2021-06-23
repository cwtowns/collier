import React, { useState, useEffect, useMemo } from 'react';
import { SafeAreaView, Text, FlatList, ListRenderItem } from 'react-native';
import * as SignalR from '@microsoft/signalr';

import { NativeScrollEvent } from 'react-native';
import { NativeSyntheticEvent } from 'react-native';
import { NativeModules } from 'react-native';

import { CollierConfig } from './Config';

interface RawLogProps {
  websocket: SignalR.HubConnection;
  config: CollierConfig;
}

interface LogMessage {
  id: string;
  message: string;
  timestamp: number;
}

const LogElement = (props: LogMessage) => {
  return useMemo(() => {
    return <Text>{props.message}</Text>;
  }, [props.message]);
};

const RawLog = (props: RawLogProps) => {
  const maxBacklogTimeInMs: number =
    props.config.config.rawLog.backlog.maxBacklogTimeInMs;
  const flatListRef: React.RefObject<FlatList> = React.createRef<FlatList>();
  const [logArray, setLogArray] = useState([] as LogMessage[]);
  const [minerUpdateAvailable, setMinerUpdateAvailable] = useState(false);

  useEffect(() => {
    props.websocket.on('Log', message => {
      updateLog(message);
    });

    props.websocket.on('MinerUpdateAvailable', message => {
      setMinerUpdateAvailable(message.toLowerCase() === 'true');
    });

    return () => {
      props.websocket.off('Log');
      props.websocket.off('MinerUpdateAvailable');
    };
  });

  const updateLog = (message: string) => {
    const now: number = Date.now();
    const cutoff: number = now - maxBacklogTimeInMs;

    NativeModules.nativeRandom.generateGuid(function (guid: string) {
      setLogArray(arr => {
        const newMessage: LogMessage = {
          id: guid,
          message: message,
          timestamp: now,
        };

        let newArray: LogMessage[];

        //we invert the display of the list so newest messages are at the bottom.
        //this means we put the newest at the start of the array here.
        //this gives the best autoscroll behavior in flatlist (which is a little sad)
        //https://github.com/necolas/react-native-web/issues/995
        //i also didnt see a way to reverse this in custom programming because rn
        //cannot tell me if a scroll event is from the wheel or data changing

        let x: number = arr.length - 1;

        if (x === -1) {
          return [newMessage];
        }

        while (arr[x].timestamp > cutoff && x > 0) {
          x--;
        }

        if (x === 0) {
          newArray = [...arr];
        } else {
          newArray = arr.slice(0, x);
        }

        newArray.unshift(newMessage);
        return newArray;
      });
    });
  };

  const renderItem: ListRenderItem<LogMessage> = ({ item }) => {
    return <LogElement {...item} />;
  };

  const renderCallback = useMemo(() => renderItem, []);

  const keyExtractor = (item: LogMessage) => item.id;

  return (
    <SafeAreaView>
      {minerUpdateAvailable && (
        <Text
          style={{ color: props.config.theme.rawLog.updateMessage.toString() }}>
          Miner Update Available
        </Text>
      )}
      <FlatList
        ref={flatListRef}
        style={{ height: 150, paddingTop: 10 }}
        data={logArray}
        renderItem={renderCallback}
        keyExtractor={keyExtractor}
        inverted
      />
    </SafeAreaView>
  );
};

export default RawLog;
