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

interface Theme {
  statisticsState: StatisticsStateColors;
  miningState: MiningStateColors;
  powerButtonState: MiningPowerButtonColors;
  rawLog: RawLogColors;
}

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

export default AppTheme;
