import { NativeModules } from 'react-native';

const originalLog: any = console.log;

export function log(data: string) {
  originalLog(data);
  NativeModules.nativeLogging.info(data);
}
