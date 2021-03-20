import React from 'react';
import {
    View,
    Text,
} from 'react-native';

import Icon from 'react-native-vector-icons/FontAwesome';

interface MyProps {
    iconName: string,
    unitLabel: string,
    averageValue: number,
    lastValue: number
}

const StatContainer = (props: MyProps) => {
    return (
        <View style={{ flexDirection: "row"}}>
            <Icon name={props.iconName} size={50} color="yellow" style={{ margin: 5 }}/>
            <View style={{ flex: 1, flexDirection: 'column', margin: 5, alignItems: 'stretch' }}>
                <View style={{ flexDirection: 'row' }}>
                    <Text style={{ textAlign: 'right', flex: 1 }}>Average:</Text>
                    <Text style={{ flex: 2 }} >&nbsp;&nbsp;{props.averageValue}&nbsp;{props.unitLabel}</Text>
                </View>
                <View style={{ flexDirection: 'row' }}>
                    <Text style={{ textAlign: 'right', flex: 1 }}>Last:</Text>
                    <Text style={{ flex: 2 }}>&nbsp;&nbsp;{props.lastValue}&nbsp;{props.unitLabel}</Text>
                </View>
            </View>
        </View>
    );    
}

export default StatContainer;