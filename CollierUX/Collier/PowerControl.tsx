import React, { Component } from 'react';
import {
    StyleSheet,
    Text,
    View,
} from 'react-native';

import Icon from 'react-native-vector-icons/FontAwesome';

class PowerControl extends React.Component {
    /*
    constructor(props) {
        super(props);

        this.state = {
            average: 0,
            last: 0
        };

        /*
        props.websocket.on("nothing", (message) => {
            this.setState(function (state, props) {
                return {
                    average: message
                }
            })
        });
*/

    //green if its running, red if its stopped, something if paused ....

    render() {
        return (
            <Icon name="power-off" size={90} color="yellow" />
        );
    }

}

export default PowerControl;
