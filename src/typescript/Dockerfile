FROM node:10-alpine AS build-env

# Deal with packages up front to allow us to use docker layers efficiently
# This is a seperate stem to allow us to re-use this step for local development
WORKDIR /opt/app
COPY package.json ./
COPY yarn.lock ./
RUN yarn
# Bundle app source
COPY . .
RUN yarn build
# just use production packages
RUN rm -rf ./node_modules && yarn --production

# re-start from a blank alpine image
FROM node:10-alpine
WORKDIR /opt/app
# copy the build artifacts
COPY --from=build-env /opt/app .

EXPOSE 6300
CMD [ "yarn", "startProduction" ]

