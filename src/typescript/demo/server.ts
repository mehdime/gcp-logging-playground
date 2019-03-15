import fastify from 'fastify';
import config from 'config';
import registerRoutes from './endpoints';
import logger from './logger';

export default function getServer() {
  const app = fastify({
    ignoreTrailingSlash: true,
    logger: logger.child({ level: config.get('fastifyLogLevel') })
  });

  registerRoutes(app);
  return app;
}
