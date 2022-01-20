import pino, { Level } from "pino";
import pinoHttp from "pino-http";

export const logger = pino();

export const loggerMiddleware = pinoHttp({ logger });

enum LogLevel {
  ERROR = "error",
  WARN = "warn",
  INFO = "info",
  DEBUG = "debug",
}

export const boltLogger = {
  debug: logger.debug,
  info: logger.info,
  warn: logger.warn,
  error: logger.error,
  setLevel(logLevel: Level) {
    logger.level = logLevel;
  },
  getLevel() {
    if (logger.level === LogLevel.ERROR) return LogLevel.ERROR;
    if (logger.level === LogLevel.WARN) return LogLevel.WARN;
    if (logger.level === LogLevel.INFO) return LogLevel.INFO;
    if (logger.level === LogLevel.DEBUG) return LogLevel.DEBUG;
  },
  setName(name: string) {
    console.log(name);
  },
};

export interface Log {
  info: (s: string) => void;
  warn: (s: string) => void;
  error: (s: string) => void;
}
