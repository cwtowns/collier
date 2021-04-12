import React, { useState, useEffect } from 'react';
import { SafeAreaView, Text, FlatList } from 'react-native';
import * as SignalR from '@microsoft/signalr';
import { v4 as uuidv4 } from 'uuid';

import { NativeScrollEvent } from 'react-native';
import { NativeSyntheticEvent } from 'react-native';

import AppConfig from './Config';

interface RawLogProps {
  websocket: SignalR.HubConnection;
}

interface LogMessage {
  id: string;
  message: string;
  timestamp: number;
}

const RawLog = (props: RawLogProps) => {
  const maxBacklogTimeInMs: number =
    AppConfig.rawLog.backlog.maxBacklogTimeInMs;
  const flatListRef: React.RefObject<FlatList> = React.createRef<FlatList>();
  const [hasUserScrolled, setHasUserScrolled] = useState(false);
  const [logArray, setLogArray] = useState([] as LogMessage[]);

  useEffect(() => {
    props.websocket.on('Log', message => {
      updateLog(message);
    });

    return () => {
      props.websocket.off('Log');
    };
  });

  const updateLog = (message: string) => {
    const now: number = Date.now();
    const cutoff: number = now - maxBacklogTimeInMs;

    let newLogArray: LogMessage[] = logArray.concat({
      id: uuidv4(),
      message: message,
      timestamp: now,
    });

    let x: number = 0;
    while (x < newLogArray.length && newLogArray[x].timestamp < cutoff) {
      x++;
    }

    if (x < newLogArray.length) {
      setLogArray(newLogArray.slice(x));
    } else {
      setLogArray(newLogArray);
    }

    flatListRef.current?.scrollToEnd();
  };

  const checkToForceScrollToBottom = (_width: number, _height: number) => {
    if (hasUserScrolled === false) {
      flatListRef.current?.scrollToEnd();
    }
  };

  const onScroll = (e: NativeSyntheticEvent<NativeScrollEvent>) => {
    let bottomCalculation: number =
      e.nativeEvent.contentOffset.y + e.nativeEvent.layoutMeasurement.height;
    let difference: number = Math.floor(
      e.nativeEvent.contentSize.height - bottomCalculation,
    );
    setHasUserScrolled(difference > 0);
  };

  const renderItem = ({ item }: { item: LogMessage }) => (
    <Text>{item.message}</Text>
  );

  return (
    <SafeAreaView>
      <FlatList
        ref={flatListRef}
        style={{ height: 150, paddingTop: 10 }}
        data={logArray}
        renderItem={renderItem}
        keyExtractor={item => item.id}
        onScroll={onScroll}
        onContentSizeChange={(width, height) =>
          checkToForceScrollToBottom(width, height)
        }
      />
    </SafeAreaView>
  );
};

export default RawLog;
