import React, { useState, useEffect } from 'react';

import { MyProps } from './StatsCommon';
import StatContainer from './StatContainer';

import AppConfig from '../Config';

const CrashStats = (props: MyProps) => {
    const [last, setLast] = useState(0);

    useEffect(() => {
        props.websocket.on("CurrentCrashCount", (message) => {
            setLast(message);
        });

        return () => {
            props.websocket.off("CurrentCrashCount");
        }
    });

    return (
        <StatContainer config={AppConfig.statStates["crash"]} averageValue={0} lastValue={last}></StatContainer>
    );
}

export default CrashStats;