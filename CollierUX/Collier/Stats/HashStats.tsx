import React, { useState, useEffect } from 'react';

import { MyProps } from './StatsCommon';
import StatContainer from './StatContainer';

import AppConfig from '../Config';

const HashStats = (props: MyProps) => {
    const [average, setAverage] = useState(0);
    const [last, setLast] = useState(0);

    useEffect(() => {
        props.websocket.on("AverageHashRate", (message) => {
            setAverage(parseFloat(parseFloat(message).toFixed(2)));
        });

        props.websocket.on("LastHashRate", (message) => {
            setLast(message);
        });

        return () => {
            props.websocket.off("AverageHashRate");
            props.websocket.off("LastHashRate");
        }
    });

    return (
        <StatContainer config={AppConfig.statStates["hash"]} averageValue={average} lastValue={last}></StatContainer>
    );
}

export default HashStats;