import { AddressInfo } from 'net';
import getServer from './server';
import logger from './logger';

// Run the server!
const start = async () => {
  try {
    const app = getServer();
    await app.listen(6300, '::');
    app.log.info(`server listening on ${(app.server.address() as AddressInfo).port}`);
  } catch (err) {
    logger.error(err);
  }
};

start();
