import React from 'react';
import { Button, StyleSheet, Text, View } from 'react-native';
import Icon from 'react-native-vector-icons/FontAwesome';

export interface Props {
    name: string;
    enthusiasmLevel?: number;
}

const Hello = (props: Props) => {
    const [enthusiasmLevel, setEnthusiasmLevel] = React.useState(
        props.enthusiasmLevel
    );

    const onIncrement = () =>
        setEnthusiasmLevel((enthusiasmLevel || 0) + 1);
    const onDecrement = () =>
        setEnthusiasmLevel((enthusiasmLevel || 0) - 1);

    const getExclamationMarks = (numChars: number) =>
        Array(numChars + 1).join('!');
    return (
        <View style={styles.root}>
          <Icon name="bolt" size={50} color="yellow" style={{ margin: 5 }}/>
            <Text style={styles.greeting}>
                Hello{' '}
                {props.name + getExclamationMarks(enthusiasmLevel || 0)}
            </Text>
            <View style={styles.buttons}>
                <View style={styles.button}>
                    <Button
                        title="-"
                        onPress={onDecrement}
                        accessibilityLabel="decrement"
                        color="red"
                    />
                </View>
                <View style={styles.button}>
                    <Button
                        title="+"
                        onPress={onIncrement}
                        accessibilityLabel="increment"
                        color="blue"
                    />
                </View>
            </View>
        </View>
    );
};

const styles = StyleSheet.create({
    root: {
        alignItems: 'center',
        alignSelf: 'center'
    },
    buttons: {
        flexDirection: 'row',
        minHeight: 70,
        alignItems: 'stretch',
        alignSelf: 'center',
        borderWidth: 5
    },
    button: {
        flex: 1,
        paddingVertical: 0
    },
    greeting: {
        color: '#999',
        fontWeight: 'bold'
    }
});

export default Hello;