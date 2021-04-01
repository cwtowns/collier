import React, { useState, useEffect } from 'react';
import Icon from 'react-native-vector-icons/FontAwesome';
import { MyProps } from './StatsCommon';
import AppTheme from '../Theme';
import { Color } from 'react-color';

type PowerState = 'Unknown' | 'Running' | 'Stopped' | 'Paused';

const PowerControl = (props: MyProps) => {
  let [state, setState] = useState('Unknown' as PowerState);

    useEffect(() => {
        //unfortunately we have to track mounted state here.  SignalR does not allow us to unregister our 
        //handlers and the websocket might fire events after the component is unmounted
        console.log("MiningStateControl useEffect start");
        let mounted: boolean = true;

        webSocket.onreconnected(() => {
            console.log("MiningStateControl onreconnected " + mounted);
            if (mounted)
                setState('Running');
        });

        webSocket.onclose(() => {
            console.log("MiningStateControl onclose " + mounted);
            if (mounted)
                setState('Stopped');
        });

        webSocket.onreconnecting(() => {
            console.log("MiningStateControl onreconnecting " + mounted);
            if (mounted)
                setState('Unknown');
        });

        webSocket.on("Connected", () => {
            console.log("MiningStateControl connected " + mounted);
            if (mounted)
                setState('Running');
        });

    return () => {
      props.websocket.off('MiningState');
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
