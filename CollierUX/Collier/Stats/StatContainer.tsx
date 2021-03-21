import React from 'react';
import {
    View,
    Text,
} from 'react-native'

import Icon from 'react-native-vector-icons/FontAwesome';

import { Statistic } from '../Config';

import { Color } from "react-color";

interface MyProps {
    averageValue: number,
    lastValue: number,
    config: Statistic
}

interface MyState {
    state: StatState
}

type StatState = 'good' | 'caution' | 'danger';

import AppTheme from '../Theme';

class StatContainer extends React.PureComponent<MyProps, MyState> {
    constructor(props: MyProps) {
        super(props);

        this.state = {
            state: this.calculateStatState()
        };
    }

    calculateStatState(): StatState {
        if (this.props.config.direction === 'up') {
            if (this.props.averageValue <= this.props.config.states.good)
                return 'good';
            else if (this.props.averageValue <= this.props.config.states.caution)
                return 'caution';
            return 'danger'
        }

        if (this.props.averageValue <= this.props.config.states.danger)
            return 'danger';
        else if (this.props.averageValue <= this.props.config.states.caution)
            return 'caution';
        return 'good'
    }

    getStateColor(): Color {
        const state = this.calculateStatState();
        if (state === 'good')
            return AppTheme.statisticsState.good
        if (state === 'caution')
            return AppTheme.statisticsState.caution;
        if (state === 'danger')
            return AppTheme.statisticsState.danger;

        throw new Error('Unsupported state:  ' + state);
    }

    render() {
        //not sure how to do this with typescript.  
        let averageComponent;
        let lastComponent;

        if(this.props.config.hideAverage != true) {
            averageComponent = 
                <View style={{ flexDirection: 'row', borderWidth: 1 }}>
                    <Text style={{ textAlign: 'right', flex: 1 }}>Average:</Text>
                    <Text style={{ flex: 2 }} >&nbsp;&nbsp;{this.props.averageValue}&nbsp;{this.props.config.unitLabel}</Text>
                </View>;
        }

        if(this.props.config.hideLast != true) {
            lastComponent = 
                <View style={{ flexDirection: 'row' }}>
                    <Text style={{ textAlign: 'right', flex: 1 }}>Last:</Text>
                    <Text style={{ flex: 2 }}>&nbsp;&nbsp;{this.props.lastValue}&nbsp;{this.props.config.unitLabel}</Text>
                </View>;
        }

        return (
            <View style={{ flexDirection: "row" }}>
                <Icon name={this.props.config.icon.name} size={50} color={this.getStateColor().toString()} style={{ margin: 5 }} />
                <View style={{ flex: 1, flexDirection: 'column', margin: 5, alignItems: 'stretch' }}>
                    {averageComponent}
                    {lastComponent}
                </View>
            </View>
        );
    }
}

export default StatContainer;