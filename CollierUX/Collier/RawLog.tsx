import React, { useState, useEffect, useMemo } from 'react';
import { SafeAreaView, Text, FlatList, ListRenderItem } from 'react-native';
import * as SignalR from '@microsoft/signalr';
import { v4 as uuidv4 } from 'uuid';

import { NativeScrollEvent } from 'react-native';
import { NativeSyntheticEvent } from 'react-native';

import AppConfig from './Config';
import Theme from './Theme';

interface RawLogProps {
  websocket: SignalR.HubConnection;
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

interface MyState {
  logArray: LogMessage[];
  counter: number;
}

const RawLog = (props: RawLogProps) => {
  const maxBacklogTimeInMs: number =
    AppConfig.rawLog.backlog.maxBacklogTimeInMs;
  const flatListRef: React.RefObject<FlatList> = React.createRef<FlatList>();
  const [hasUserScrolled, setHasUserScrolled] = useState(false);
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

  const renderItem: ListRenderItem<LogMessage> = ({ item }) => {
    return <LogElement {...item} />;
  };

  const renderCallback = useMemo(() => renderItem, []);

  const keyExtractor = (item: LogMessage) => item.id;

  return (
    <SafeAreaView>
      {minerUpdateAvailable && (
        <Text style={{ color: Theme.rawLog.updateMessage.toString() }}>
          Miner Update Available
        </Text>
      )}
      <FlatList
        ref={flatListRef}
        style={{ height: 150, paddingTop: 10 }}
        data={logArray}
        renderItem={renderCallback}
        keyExtractor={keyExtractor}
        onScroll={onScroll}
        onContentSizeChange={checkToForceScrollToBottom}
      />
    </SafeAreaView>
  );
};

export default RawLog;
