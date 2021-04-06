import React from 'react';
import { View } from 'react-native';
import { MyProps } from './StatsCommon';

import HashStats from './HashStats';
import PowerStats from './PowerStats';
import TempStats from './TempStats';
import CrashStats from './CrashStats';

const StatsPanel = (props: MyProps) => {
  return (
    <View style={{ flexDirection: 'row' }}>
      <View style={{ flex: 1, alignSelf: 'stretch' }}>
        <PowerStats websocket={props.websocket} />
        <TempStats websocket={props.websocket} />
      </View>
      <View style={{ flex: 1, alignSelf: 'stretch', alignItems: 'stretch' }}>
        <HashStats websocket={props.websocket} />
        <CrashStats websocket={props.websocket} />
      </View>
    </View>
  );
};

export default StatsPanel;
