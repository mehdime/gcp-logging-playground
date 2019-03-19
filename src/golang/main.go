package main

import (
	"fmt"
	"io/ioutil"
	"net/http"
	"os"
	"runtime/debug"
	"time"

	"github.com/sirupsen/logrus"
)

var log *logrus.Logger

func main() {
	/*
	 * Guide for happy logging when using Stackdriver:
	 *
	 * 1. Write logs to the standard output.
	 * 2. Write structured logs in JSON format.
	 * 3. Use those properties in your logs: 'timestamp',
	 * 'severity' and 'message'. Your life will be a lot easier.
	 */
	log = logrus.New()
	log.Level = logrus.DebugLevel
	log.Out = os.Stdout

	log.Formatter = &logrus.JSONFormatter{
		FieldMap: logrus.FieldMap{
			logrus.FieldKeyTime:  "timestamp",
			logrus.FieldKeyLevel: "severity",
			logrus.FieldKeyMsg:   "message",
		},
		TimestampFormat: time.RFC3339Nano,
	}

	log.Print("Golang Stackdriver logging demo starting...")
	http.HandleFunc("/", home)
	log.Fatal(http.ListenAndServe(":6200", nil))
}

func home(w http.ResponseWriter, r *http.Request) {
	// Enrich all log entries written during the execution of this request
	// with a few structured properties.
	requestLog := log.WithFields(logrus.Fields{
		"customerId":    42,
		"requestMethod": r.Method,
		"requestUrl":    r.RequestURI,
	})

	requestLog.Info("A message with a few structured properties (from Go).")
	requestLog.Warn("Oh no, an intentional warning! (from Go)")

	if err := fail(); err != nil {
		// If you want the Error Reporting feature of Stackdriver to pickup Go
		// errors, your error log message must include a header with the error details
		// followed by the stack trace returned by debug.Stack(). If you omit the stack trace
		// your errors won't show up on the Error Reporting page.
		requestLog.Errorf("Damn, it failed (intentionally. From Go): %v\n%v", err, string(debug.Stack()))
	}

	fmt.Fprint(w, "Hello, World. I'm a Go app. \n\nView my logs at https://console.cloud.google.com/logs/viewer?advancedFilter=golanglogdemo \n\nView my errors at https://console.cloud.google.com/errors")
}

func fail() error {
	_, err := ioutil.ReadFile("/bladibla")
	return err
}
