import React, { useState, useEffect } from 'react';

import { MyProps } from './StatsCommon';
import StatContainer from './StatContainer';

const CrashStats = (props: MyProps) => {
  const [last, setLast] = useState(0);

  useEffect(() => {
    props.websocket.on('CurrentCrashCount', message => {
      setLast(message);
    });

    return () => {
      props.websocket.off('CurrentCrashCount');
    };
  });

  return (
    <StatContainer
      config={props.config.config.statStates.crash}
      averageValue={0}
      lastValue={last}
      appConfig={props.config}
    />
  );
};

export default CrashStats;
