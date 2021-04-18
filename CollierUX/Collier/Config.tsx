import { NativeModules } from 'react-native';
import { Color } from 'react-color';

interface MiningStateColors {
  mining: Color;
  paused: Color;
  stopped: Color;
  unknown: Color;
}

interface StatisticsStateColors {
  good: Color;
  caution: Color;
  danger: Color;
}

interface MiningPowerButtonColors {
  pauseRequested: Color;
  startRequested: Color;
}

interface RawLogColors {
  updateMessage: Color;
}

export type Theme = {
  statisticsState: StatisticsStateColors;
  miningState: MiningStateColors;
  powerButtonState: MiningPowerButtonColors;
  rawLog: RawLogColors;
};

const AppTheme: Theme = {
  statisticsState: {
    good: 'green',
    caution: 'yellow',
    danger: 'red',
  },
  miningState: {
    mining: 'green',
    paused: 'yellow',
    stopped: 'red',
    unknown: 'purple',
  },
  powerButtonState: {
    pauseRequested: 'palegoldenrod',
    startRequested: 'darkseagreen',
  },
  rawLog: {
    updateMessage: 'yellow',
  },
};

interface StateThresholds {
  good: number;
  caution: number;
  danger: number;
}

interface Icon {
  name: string;
}

export interface Statistic {
  icon: Icon;
  unitLabel: string;
  direction: 'up' | 'down';
  hideAverage?: boolean;
  hideLast?: boolean;
  states: StateThresholds;
}

interface StatisticCollection {
  [key: string]: Statistic;
}

interface RawLogBacklog {
  maxBacklogTimeInMs: number;
}

interface RawLog {
  backlog: RawLogBacklog;
}

interface Config {
  statStates: StatisticCollection;
  rawLog: RawLog;
}

export type CollierConfig = {
  config: Config;
  theme: Theme;
};

const AppConfig: Config = {
  statStates: {
    power: {
      icon: {
        name: 'bolt',
      },
      unitLabel: 'Watts',
      direction: 'up',
      states: {
        good: 275,
        caution: 300,
        danger: 325,
      },
    },
    hash: {
      icon: {
        name: 'calculator',
      },
      unitLabel: 'MH/s',
      direction: 'down', //down == higher is better
      states: {
        good: 80, //doesn matter, will be taken care of by previous state
        caution: 70, //upper bound, anything above it is next state
        danger: 50, //upper bound, anything above it is next state
      },
    },
    temp: {
      icon: {
        name: 'thermometer-half',
      },
      unitLabel: 'C',
      direction: 'up', //lower is better
      states: {
        good: 69, //lower bound, anything above it is next state
        caution: 74, //lower bound, anything above it is next state
        danger: 75, //doesnt matter
      },
    },
    crash: {
      icon: {
        name: 'unlink',
      },
      unitLabel: 'Crashes',
      direction: 'up', //up == lower is better
      states: {
        good: 0, //lower bound, anything above is the next state
        caution: 2, //lower bound, anything above is the next state
        danger: 3, //doesnt matter, caution will take care of it?
      },
      hideAverage: true,
    },
  },
  rawLog: {
    backlog: {
      maxBacklogTimeInMs: 1200000, //20 minutes
    },
  },
};

export const loadConfiguration: Promise<CollierConfig> = new Promise<CollierConfig>(
  function (mainResolve, mainReject) {
    const configPromise = new Promise<Config>(function (resolve, reject) {
      NativeModules.appData
        .getAppSettings('config.json', JSON.stringify(AppConfig))
        .then((result: string) => {
          const configObject: Config = {} as Config;
          console.log('configuration back from server');
          let objectData = JSON.parse(result);
          Object.assign(configObject, objectData);
          resolve(configObject);
        })
        .catch(reject);
    });

    const themePromise = new Promise<Theme>(function (resolve, reject) {
      NativeModules.appData
        .getAppSettings('theme.json', JSON.stringify(AppTheme))
        .then((result: string) => {
          const themeObject: Theme = {} as Theme;
          console.log('configuration back from server');
          let objectData = JSON.parse(result);
          Object.assign(themeObject, objectData);
          resolve(themeObject);
        })
        .catch(reject);
    });

    Promise.all([configPromise, themePromise])
      .then(data => {
        const fullConfig: CollierConfig = {
          config: data[0],
          theme: data[1],
        };
        mainResolve(fullConfig);
      })
      .catch(mainReject);
  },
);
