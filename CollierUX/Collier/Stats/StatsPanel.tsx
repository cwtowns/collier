import React from 'react';
import {
    View,
} from 'react-native';

import { MyProps } from './StatsCommon';

import HashStats from './HashStats';
import PowerStats from './PowerStats';
import TempStats from './TempStats';
import CrashStats from './CrashStats';

const StatsPanel = (props: MyProps) => {
    return (
        <View style={{ margin: 5, flexDirection: "row" }}>
            <View style={{ flex: 1, margin: 10, alignSelf: "stretch" }}>
                <PowerStats websocket={props.websocket}></PowerStats>
                <TempStats websocket={props.websocket}></TempStats>
            </View>
            <View style={{ flex: 1, margin: 10, alignSelf: "stretch", alignItems: "stretch" }}>
                <HashStats websocket={props.websocket}></HashStats>
                <CrashStats websocket={props.websocket}></CrashStats>
            </View>
        </View>
    );
}

export default StatsPanel;