import { FastifyInstance } from 'fastify';
import { Server, IncomingMessage, ServerResponse } from 'http';

import home from './home';

export default function registerRoutes(app: FastifyInstance<Server, IncomingMessage, ServerResponse>) {
  home(app);
}
