import pino from "pino";
import pinoHttp from "pino-http";

export const logger = pino();

export const loggerMiddleware = pinoHttp({ logger });
