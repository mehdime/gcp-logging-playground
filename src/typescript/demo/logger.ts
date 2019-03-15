import config from 'config';
import pino from 'pino';

// give access to an instance of the logger from anywhere that needs it.
// if you are logging as part of a request you are better off using the request level
// logger as this will ensure all the logs get the same request id
const logger = pino({
  level: config.get('logLevel'),
  useLevelLabels: true,

  /*
   * Use property names that Stackdriver understand.
   */
  messageKey: 'message',
  changeLevelName: 'severity',

  /*
   * Define the logging levels that Stackdriver understands.
   * Make sure to only use these levels when logging. E.g. use
   * logger.warning(...), not logger.warn(...).
   */
  customLevels: {
    debug: 100,
    info: 200,
    notice: 300,
    warning: 400,
    error: 500,
    critical: 600,
    alert: 700,
    emergency: 800
  },

  /*
   * We can't force the use of our custom Stackdriver-friendly levels because
   * fastify assumes that the default levels exist. It means that the fastify logs
   * that use levels that Stackdriver doesn't understand will be record with a
   * generic level of "Any" by Stackdriver.
   */
  useOnlyCustomLevels: false
});

logger.info('Hello, World (from Typescript)');
export default logger;
