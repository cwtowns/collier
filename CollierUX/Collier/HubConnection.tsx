import * as SignalR from '@microsoft/signalr';

export type HubRemoveHandlers = {
  removeOnClose: (callback: (error?: Error) => void) => void;
  removeOnReconnecting: (callback: (error?: Error) => void) => void;
  removeOnReconnected: (callback: (connectionid?: string) => void) => void;
};

export type CollierHubConnection = SignalR.HubConnection & HubRemoveHandlers;

/*
 * The SignalR hubConnection does not provide remove handlers for onclose, onreconnecting, onreconnected.
 * This results in a memory leak in react when components that add handlers during mount cannot remove
 * them during unmount.
 *
 * So here we add our new properties on the hub with HubRemoveHandlers and create an intersection type to
 * use instead.  The default handlers on HubConnection are replaced with ours, and this function manages
 * the 'private' variables for tracking the callbacks so we can remove them later.
 *
 * It feels a little janky accomplishing this all in a function that tracks the callback arrays, but
 * HubConnection is private, so we can't extend it via javascript class system.  If we were to define
 * our own class as an adapter to HubConnection, we'd have to manage the public method definitions
 * ourselves and that seems like a lot of work for little reward.  It would also mean difficulty taking
 * upgrades to SignalR when they make breaking changes in HubConnection.
 */

export function convertHubConnection(
  hubConnection: SignalR.HubConnection,
): CollierHubConnection {
  const closedCallbacks: Array<(error?: Error) => void> = [];
  const reconnectingCallbacks: Array<(error?: Error) => void> = [];
  const reconnectedCallbacks: Array<(connectionId?: string) => void> = [];

  const myOnCloseHandler = (error?: Error): void => {
    closedCallbacks.forEach(element => {
      element(error);
    });
  };

  hubConnection.onclose(myOnCloseHandler);

  const removeOnClose = (callback: (error?: Error) => void) => {
    if (!callback) {
      return;
    }

    const index = closedCallbacks.indexOf(callback);

    if (index >= 0) {
      closedCallbacks.splice(index, 1);
    }
  };

  const myOnReconnectingHandler = (error?: Error): void => {
    reconnectingCallbacks.forEach(element => {
      element(error);
    });
  };

  hubConnection.onreconnecting(myOnReconnectingHandler);

  const removeOnReconnecting = (callback: (error?: Error) => void) => {
    if (!callback) {
      return;
    }

    const index = reconnectingCallbacks.indexOf(callback);

    if (index >= 0) {
      reconnectingCallbacks.splice(index, 1);
    }
  };

  const myOnReconnectedHandler = (connectionId?: string): void => {
    reconnectedCallbacks.forEach(element => {
      element(connectionId);
    });
  };

  hubConnection.onreconnected(myOnReconnectedHandler);

  const removeOnReconnected = (callback: (connectionid?: string) => void) => {
    if (!callback) {
      return;
    }

    const index = reconnectedCallbacks.indexOf(callback);

    if (index >= 0) {
      reconnectedCallbacks.splice(index, 1);
    }
  };

  const upgradedHubConnection: CollierHubConnection = Object.assign(
    hubConnection,
    {
      removeOnClose: removeOnClose,
      removeOnReconnecting: removeOnReconnecting,
      removeOnReconnected: removeOnReconnected,
    },
  );

  return upgradedHubConnection;
}
