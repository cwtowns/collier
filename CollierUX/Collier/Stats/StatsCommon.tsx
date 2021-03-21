import * as SignalR from "@microsoft/signalr";

export interface MyProps {
    websocket: SignalR.HubConnection
}

export interface MyState {
    average: number,
    last: number
}