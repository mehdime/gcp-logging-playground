FROM golang:1.11-alpine as builder

# We'll need git to download the dependencies
RUN apk add --no-cache git

WORKDIR /src

# Restore dependencies
COPY ./go.mod ./go.sum ./
RUN go mod download

# Build. 
# Make debugging easier by disabling optimizations (-N)
# and showing full file names in error messages (-L). 
# Good for demo purposes. But think about this before 
# going to prod. 
COPY . .
RUN CGO_ENABLED=0 go build -gcflags='-N -l' -o /golanglogdemo .

# Runtime image. 
FROM alpine as release
WORKDIR /app

COPY --from=builder /golanglogdemo .
ENTRYPOINT ["./golanglogdemo"]