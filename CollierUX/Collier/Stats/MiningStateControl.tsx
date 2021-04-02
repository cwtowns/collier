import React, { useState, useEffect } from 'react';
import Icon from 'react-native-vector-icons/FontAwesome';
import { MyProps } from './StatsCommon';
import AppTheme from '../Theme';
import { Color } from 'react-color';

type PowerState = 'Unknown' | 'Running' | 'Stopped' | 'Paused';

const PowerControl = (props: MyProps) => {
  let [state, setState] = useState('Unknown' as PowerState);

    useEffect(() => {
        const reconnected = () => { setState('Running'); };
        const close = () => { setState('Stopped'); };
        const reconnecting = () => { setState('Unknown'); }; 

        webSocket.onreconnected(reconnected);
        webSocket.onclose(close);
        webSocket.onreconnecting(reconnecting);
        webSocket.on("Connected", reconnected);

        if (webSocket.state === SignalR.HubConnectionState.Connected)
            setState('Running');
        else if (webSocket.state === SignalR.HubConnectionState.Disconnected)
            setState('Stopped');
        else
            setState('Unknown');

        return () => {
            webSocket.off("Connected");
            webSocket.removeOnReconnected(reconnected);
            webSocket.removeOnClose(close);
            webSocket.removeOnReconnecting(reconnecting);
        };
    });

  const getStateColor = (): Color => {
    if (state === 'Running') {
      return AppTheme.miningState.mining;
    }
    if (state === 'Stopped' || state === 'Paused') {
      return AppTheme.miningState.paused;
    }

    return AppTheme.miningState.unknown;
  };

  return <Icon name="power-off" size={90} color={getStateColor().toString()} />;
};

export default MiningStateControl;
