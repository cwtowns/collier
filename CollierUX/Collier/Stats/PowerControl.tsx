import React, { useState, useEffect } from 'react';
import Icon from 'react-native-vector-icons/FontAwesome';
import { MyProps } from './StatsCommon';
import AppTheme from '../Theme';
import { Color } from 'react-color';

type PowerState = 'Unknown' | 'Running' | 'Stopped' | 'Paused';

const PowerControl = (props: MyProps) => {
  let [state, setState] = useState('Unknown' as PowerState);

  useEffect(() => {
    props.websocket.on('MiningState', message => {
      setState(message);
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

export default PowerControl;
