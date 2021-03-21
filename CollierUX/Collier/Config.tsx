interface StateThresholds {
    good: number,
    caution: number,
    danger: number
};

interface Icon {
    name: string
}

export interface Statistic {
    icon: Icon,
    unitLabel: string,
    direction: "up" | "down",
    hideAverage?: boolean,
    hideLast?: boolean,
    states: StateThresholds
}

interface StatisticCollection {
    [key: string]: Statistic
}

interface RawLogBacklog {
    condencedLimit: number
    fullLimit: number
}

interface RawLog {
    backlog: RawLogBacklog
}

interface CollierConfig {
    statStates: StatisticCollection,
    rawLog: RawLog
}

const AppConfig: CollierConfig = {
    statStates: {
        power: {
            icon: {
                name: "bolt"
            },
            unitLabel: "Watts",
            direction: "up",
            states: {
                good: 275,
                caution: 300,
                danger: 325
            }
        },
        hash: {
            icon: {
                name: "calculator"
            },
            unitLabel: "MH/s",
            direction: "down",  //down == higher is better
            states: {
                good: 80,  //doesn matter, will be taken care of by previous state
                caution: 70,  //upper bound, anything above it is next state
                danger: 50,  //upper bound, anything above it is next state
            }
        },
        temp: {
            icon: {
                name: "thermometer-half"
            },
            unitLabel: "C",
            direction: "up",  //lower is better
            states: {
                good: 69,  //lower bound, anything above it is next state
                caution: 74,  //lower bound, anything above it is next state
                danger: 75  //doesnt matter            
            }
        },
        crash: {
            icon: {
                name: "unlink"
            },
            unitLabel: "Crashes",
            direction: "up",  //up == lower is better
            states: {
                good: 0,  //lower bound, anything above is the next state
                caution: 2,  //lower bound, anything above is the next state
                danger: 3  //doesnt matter, caution will take care of it?
            },
            hideAverage: true
        }
    },
    rawLog: {
        backlog: {
            condencedLimit: 10,
            fullLimit: 100
        }
    }
};

export default AppConfig;
