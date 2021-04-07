import React, { useState, useEffect } from 'react';

//import { GestureResponderEvent } from 'react-native';
import * as SignalR from '@microsoft/signalr';
import { Button } from 'react-native-elements';

import Icon from 'react-native-vector-icons/FontAwesome';
import { MyProps } from './StatsCommon';
import AppTheme from '../Theme';
import { Color } from 'react-color';

type PowerState = 'Unknown' | 'Running' | 'Stopped' | 'Paused';

const MiningStateControl = (props: MyProps) => {
  let [state, setState] = useState('Unknown' as PowerState);

  useEffect(() => {
    const reconnected = () => {
      setState('Running');
    };
    const close = () => {
      setState('Stopped');
    };
    const reconnecting = () => {
      setState('Unknown');
    };

    props.websocket.onreconnected(reconnected);
    props.websocket.onclose(close);
    props.websocket.onreconnecting(reconnecting);
    props.websocket.on('Connected', reconnected);

    if (props.websocket.state === SignalR.HubConnectionState.Connected) {
      setState('Running');
    } else if (
      props.websocket.state === SignalR.HubConnectionState.Disconnected
    ) {
      setState('Stopped');
    } else {
      setState('Unknown');
    }

    return () => {
      props.websocket.off('Connected');
      props.websocket.removeOnReconnected(reconnected);
      props.websocket.removeOnClose(close);
      props.websocket.removeOnReconnecting(reconnecting);
    };
  }, [props.websocket]);

  const getStateColor = (): Color => {
    if (state === 'Running') {
      return AppTheme.miningState.mining;
    }
    if (state === 'Stopped' || state === 'Paused') {
      return AppTheme.miningState.paused;
    }

    return AppTheme.miningState.unknown;
  };

  /*
  const pressHandler = (e: GestureResponderEvent) => {
    console.log('press');
  };

  const pressInHandler = (e: GestureResponderEvent) => {
    console.log('press in ');
  };
  */

  return (
    <Button
      type="clear"
      icon={
        <Icon name="power-off" size={90} color={getStateColor().toString()} />
      }
    />
  );
};

export default MiningStateControl;
