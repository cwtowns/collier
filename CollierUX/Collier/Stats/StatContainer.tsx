import React from 'react';
import { View, Text } from 'react-native';
import Icon from 'react-native-vector-icons/FontAwesome';
import { Statistic } from '../Config';
import { Color } from 'react-color';

interface MyProps {
  averageValue: number;
  lastValue: number;
  config: Statistic;
}

type StatState = 'good' | 'caution' | 'danger';

import AppTheme from '../Theme';

const StatContainer = (props: MyProps) => {
  const calculateStatState = (): StatState => {
    if (props.config.direction === 'up') {
      if (props.averageValue <= props.config.states.good) {
        return 'good';
      } else if (props.averageValue <= props.config.states.caution) {
        return 'caution';
      }
      return 'danger';
    }

    if (props.averageValue <= props.config.states.danger) {
      return 'danger';
    } else if (props.averageValue <= props.config.states.caution) {
      return 'caution';
    }
    return 'good';
  };

  const getStateColor = (): Color => {
    const state = calculateStatState();
    if (state === 'good') {
      return AppTheme.statisticsState.good;
    }
    if (state === 'caution') {
      return AppTheme.statisticsState.caution;
    }
    if (state === 'danger') {
      return AppTheme.statisticsState.danger;
    }

    throw new Error('Unsupported state:  ' + state);
  };

  //TODO not sure how to do this with typescript.
  let averageComponent: {};
  let lastComponent: {};

  if (props.config.hideAverage !== true) {
    averageComponent = (
      <View style={{ flexDirection: 'row', borderWidth: 1 }}>
        <Text style={{ textAlign: 'right', flex: 1 }}>Average:</Text>
        <Text style={{ flex: 2 }}>
          &nbsp;&nbsp;{props.averageValue}&nbsp;{props.config.unitLabel}
        </Text>
      </View>
    );
  } else {
    averageComponent = <View />;
  }

  if (props.config.hideLast !== true) {
    lastComponent = (
      <View style={{ flexDirection: 'row' }}>
        <Text style={{ textAlign: 'right', flex: 1 }}>Last:</Text>
        <Text style={{ flex: 2 }}>
          &nbsp;&nbsp;{props.lastValue}&nbsp;{props.config.unitLabel}
        </Text>
      </View>
    );
  } else {
    lastComponent = <View />;
  }

  return (
    <View style={{ flexDirection: 'row' }}>
      <Icon
        name={props.config.icon.name}
        size={50}
        color={getStateColor().toString()}
        style={{ margin: 5 }}
      />
      <View
        style={{
          flex: 1,
          flexDirection: 'column',
          margin: 5,
          alignItems: 'stretch',
        }}>
        {averageComponent}
        {lastComponent}
      </View>
    </View>
  );
};

export default StatContainer;
