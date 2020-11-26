import pino from "pino";
import pinoHttp from "pino-http";

export const logger = pino();

export const loggerMiddleware = pinoHttp({ logger });

export interface Log {
  info: (s: string) => void;
  warn: (s: string) => void;
  error: (s: string) => void;
}
