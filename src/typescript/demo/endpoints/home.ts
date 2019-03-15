import { FastifyInstance } from 'fastify';
import { Server, IncomingMessage, ServerResponse } from 'http';

export default function registerRoutes(app: FastifyInstance<Server, IncomingMessage, ServerResponse>) {
  app.get('/', async (request, reply) => {
    // var logger = request.log.child({ customerId: 42 })
    request.log.info('A message with a few suctured properties (from Typescript).');
    //request.log.warning('This is an intentional warning (from Typesscript)');

    reply.send("Hello, World. I'm a Typescript app.");
  });
}
