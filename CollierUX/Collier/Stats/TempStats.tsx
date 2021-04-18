import React, { useState, useEffect } from 'react';

import { MyProps } from './StatsCommon';
import StatContainer from './StatContainer';

const TempStats = (props: MyProps) => {
  const [average, setAverage] = useState(0);
  const [last, setLast] = useState(0);

  useEffect(() => {
    props.websocket.on('AverageTemp', message => {
      setAverage(message);
    });

    props.websocket.on('LastTemp', message => {
      setLast(message);
    });

    return () => {
      props.websocket.off('AverageTemp');
      props.websocket.off('LastTemp');
    };
  });

  return (
    <StatContainer
      config={props.config.config.statStates.temp}
      appConfig={props.config}
      averageValue={average}
      lastValue={last}
    />
  );
};

export default TempStats;
