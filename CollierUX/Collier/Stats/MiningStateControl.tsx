import React, { useState, useEffect } from 'react';
import * as SignalR from '@microsoft/signalr';
import { Button } from 'react-native-elements';

import Icon from 'react-native-vector-icons/FontAwesome';
import { MyProps } from './StatsCommon';
import { Color } from 'react-color';
import { GestureResponderEvent } from 'react-native';

type MiningState = 'Unknown' | 'Running' | 'Stopped' | 'UserPaused';
type ButtonState = 'StartRequested' | 'PauseRequested' | 'Normal';

const MiningStateControl = (props: MyProps) => {
  let [state, setState] = useState('Unknown' as MiningState);
  let [buttonState, setButtonState] = useState('Normal' as ButtonState);

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
      setButtonState('Normal');
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
    let result: Color = props.config.theme.miningState.unknown;

    if (state === 'Running') {
      result = props.config.theme.miningState.mining;
    }
    if (state === 'UserPaused') {
      result = props.config.theme.miningState.paused;
    }
    if (state === 'Stopped') {
      result = props.config.theme.miningState.stopped;
    }

    if (buttonState === 'PauseRequested') {
      result = props.config.theme.powerButtonState.pauseRequested;
    } else if (buttonState === 'StartRequested') {
      result = props.config.theme.powerButtonState.startRequested;
    }

    return result;
  };

  const pressHandler = (_e: GestureResponderEvent) => {
    console.log('press');
    if (state === 'Running') {
      setButtonState('PauseRequested');
      props.websocket.invoke('StopMiner');
    } else if (state === 'UserPaused') {
      setButtonState('StartRequested');
      props.websocket.invoke('StartMiner');
    }
  };

  return (
    <Button
      disabled={buttonState !== 'Normal'}
      type="clear"
      icon={
        <Icon name="power-off" size={90} color={getStateColor().toString()} />
      }
      onPress={pressHandler}
    />
  );
};

export default MiningStateControl;
