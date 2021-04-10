import React, { useState, useEffect } from 'react';
import * as SignalR from '@microsoft/signalr';
import { Button } from 'react-native-elements';

import Icon from 'react-native-vector-icons/FontAwesome';
import { MyProps } from './StatsCommon';
import AppTheme from '../Theme';
import { Color } from 'react-color';
import { GestureResponderEvent } from 'react-native';

type MiningState = 'Unknown' | 'Running' | 'Stopped' | 'UserPaused';

const MiningStateControl = (props: MyProps) => {
  let [state, setState] = useState('Unknown' as MiningState);

  useEffect(() => {
    const close = () => {
      setState('Unknown');
    };

    const reconnecting = () => {
      setState('Unknown');
    };

    const miningStateChanged = (miningState: MiningState) => {
      console.log(`miningStateChanged ${miningState}`);
      if (miningState === 'Running') {
        setState('Running');
      } else if (miningState === 'UserPaused') {
        setState('UserPaused');
      } else if (miningState === 'Stopped') {
        setState('Stopped');
      } else {
        setState('Unknown');
      }
    };

    props.websocket.onclose(close);
    props.websocket.onreconnecting(reconnecting);
    props.websocket.on('MiningState', miningStateChanged);

    if (props.websocket.state !== SignalR.HubConnectionState.Connected) {
      setState('Unknown');
    } else {
      props.websocket.invoke('SendMinerState');
    }

    return () => {
      props.websocket.off('MiningState');
      props.websocket.removeOnClose(close);
      props.websocket.removeOnReconnecting(reconnecting);
    };
  }, [props.websocket]);

  const getStateColor = (): Color => {
    if (state === 'Running') {
      return AppTheme.miningState.mining;
    }
    if (state === 'UserPaused') {
      return AppTheme.miningState.paused;
    }
    if (state === 'Stopped') {
      return AppTheme.miningState.stopped;
    }

    return AppTheme.miningState.unknown;
  };

  const pressHandler = (_e: GestureResponderEvent) => {
    if (state === 'Running') {
      props.websocket.invoke('StopMiner');
    } else if (state === 'UserPaused') {
      props.websocket.invoke('StartMiner');
    }
  };

  return (
    <Button
      type="clear"
      icon={
        <Icon name="power-off" size={90} color={getStateColor().toString()} />
      }
      onPress={pressHandler}
    />
  );
};

export default MiningStateControl;
