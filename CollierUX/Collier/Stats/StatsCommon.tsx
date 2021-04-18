import { CollierHubConnection } from '../HubConnection';
import { CollierConfig } from '../Config';

export interface MyProps {
  websocket: CollierHubConnection;
  config: CollierConfig;
}
