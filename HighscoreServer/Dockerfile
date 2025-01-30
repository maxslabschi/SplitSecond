# syntax=docker/dockerfile:1

FROM golang:1.23 AS build

WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY . ./

RUN CGO_ENABLED=1 GOOS=linux go build -o /highscoreserver

# Deploy the application binary into a lean image
FROM gcr.io/distroless/base-debian12

WORKDIR /

COPY --from=build /highscoreserver /highscoreserver

EXPOSE 8080

ENTRYPOINT ["/highscoreserver"]
