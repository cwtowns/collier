import React, { useState, useEffect } from 'react';
import { MyProps } from './StatsCommon';
import StatContainer from './StatContainer';

const PowerStats = (props: MyProps) => {
  const [average, setAverage] = useState(0);
  const [last, setLast] = useState(0);

  useEffect(() => {
    props.websocket.on('AveragePower', message => {
      setAverage(message);
    });

    props.websocket.on('LastPower', message => {
      setLast(message);
    });

    return () => {
      props.websocket.off('AveragePower');
      props.websocket.off('LastPower');
    };
  });

  return (
    <StatContainer
      config={props.config.config.statStates.power}
      appConfig={props.config}
      averageValue={average}
      lastValue={last}
    />
  );
};

export default PowerStats;
