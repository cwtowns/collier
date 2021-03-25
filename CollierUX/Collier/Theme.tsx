import { Color } from "react-color";

interface MiningStateColors {
    mining: Color
    paused: Color,
    unknown: Color
}

interface StatisticsStateColors {
    good: Color,
    caution: Color,
    danger: Color
}

interface Theme {
    statisticsState: StatisticsStateColors,
    miningState: MiningStateColors
}

const AppTheme: Theme = {
    statisticsState: {
        good: "green",
        caution: "yellow",
        danger: "red"
    },
    miningState: {
        mining: "green",
        paused: "red",
        unknown: "purple"
    }
};

export default AppTheme;