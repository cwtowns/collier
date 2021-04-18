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
        <PowerStats config={props.config} websocket={props.websocket} />
        <TempStats config={props.config} websocket={props.websocket} />
      </View>
      <View style={{ flex: 1, alignSelf: 'stretch', alignItems: 'stretch' }}>
        <HashStats config={props.config} websocket={props.websocket} />
        <CrashStats config={props.config} websocket={props.websocket} />
      </View>
    </View>
  );
};

export default StatsPanel;
