# Google Stackdriver / GKE Logging Demo

Demo and best practices for logging from your GKE-hosted app to [Stackdriver](https://cloud.google.com/logging/). Also applies to apps running on GCP AppEngine and Cloud Functions.

## Languages demoed
* C# / ASP.NET
* Go

[See below](#demo-apps-pre-requisites) for instructions on how to build and deploy the demo apps on GKE. 

## How to log to Stackdriver from your GCP-hosted app
If your app runs on GKE (with [Kubernetes Monitoring](https://cloud.google.com/monitoring/kubernetes-engine/installing) enabled), AppEngine or Cloud Functions, logs writen by your app will be automatically shipped to Stackdriver. There is no need to use the Stackdriver logging libraries. 

Use your favourite logging library and configure it to:

* Write logs to the standard output.
* Format logs in either plain-text or JSON.

If formatting logs in JSON:
* Write the log message to a top-level `message` property (pay attention to the case. It's `message`, not `Message`).
* Write the log level to a top-level `severity` property and set it to [one of the values recognized by Stackdriver](https://cloud.google.com/logging/docs/reference/v2/rest/v2/LogEntry#LogSeverity).
* Write the timestamp to a top-level `timestamp` property and format it in the [RFC3339](https://www.ietf.org/rfc/rfc3339.txt) UTC "Zulu" format, accurate to nanoseconds (e.g. `2019-02-02T15:01:23.045123456Z`).
* When logging an error, write the exception / error details in either a `message` or `exception` top-level property and follow [the format documented for the message property here](https://cloud.google.com/error-reporting/docs/formatting-error-messages#fields).

This will ensure that Stackdriver displays your logs correctly in its UI and that [Stackdriver Error Reporting](https://cloud.google.com/error-reporting/) correctly picks up your error logs.

Example Stackdriver-friendly log entry:

```
{
    "timestamp" = "2019-02-02T15:01:23.045123456Z",
    "severity" = "ERROR",
    "message" = "Failed to place order ID 123",
    "exception" = "System.ArgumentException: invalid amount. Expected: greater than 0. Got: -1.
   at Shopping.Controllers.HomeController.Foo.PlaceOrder() in /app/Controllers/HomeController.cs:line 62
   at Shopping.Controllers.HomeController.Home() in /app/Controllers/HomeController.cs:line 33",
    [...] // Anything else you feel like adding... 
}
```

## In practice

### Configuring your logger to log in a Stackdriver-friendly format

**.NET using [Serilog](https://serilog.net/)**

Serilog's [built-in JSON formatters](https://github.com/serilog/serilog/wiki/Formatting-Output#formatting-json) don't allow customizing the JSON output. In order to format your logs in the Stackdriver-friendly format shown above, you'll have to write your own Serilog JSON formatter.

Thankfully this is a straightforward task. Here's an [example Serilog JSON formatter for Stackdriver](src/dotnet/DemoApi/Logging/StackdriverJsonFormatter.cs).

You can then configure Serilog with it:

```
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    // Exclude debug logs coming from the ASP.NET runtime 
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(new StackdriverJsonFormatter())
    .CreateLogger();
```

[Working example](src/dotnet/DemoApi/Program.cs).

**Go using [Logrus](https://github.com/sirupsen/logrus)**

Logrus makes configuring it to output logs in a Stackdriver-friendly format easy:

```
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
```

[Working example](src/golang/main.go).

Unfortunately, Logrus doesn't allow customizing its log levels. You should avoid using Logrus' `Trace`, `Fatal` and `Panic` log levels as they won't be recognized by Stackdriver. Stackdriver will display logs written at those levels under a generic "Any" level and won't display them when filtering by log level.

### Logging errors
For errors to be picked up by the [Stackdriver Error Reporting feature](https://cloud.google.com/error-reporting/), they must be formatted in a way that Stackdriver understands.

**.NET with Serilog**

Log exceptions normally. Serilog will do the right thing (stringify the exception with its stack trace and output it in a `exception` property):

```
// Beware that with Serilog, the first parameter of the Error() 
// method is the exception, not the log message. This is unlike
// most other logging libraries that do it the other way around.
logger.Error(ex, "Coulnd't do the thing.");
```

**Go with Logrus**

Go errors don't include a stack trace. Since Stackdriver Error Reporting requires a stack trace  to be included (in the format returned by [runtime.Stack()](https://golang.org/pkg/runtime/debug/#Stack)), you must ensure that you always manually include it in your error logs:

```
log.Errorf("Doing the thing failed: %v\n%v", err, string(debug.Stack()))
```

Not including the stack trace in your error logs will result in errors logged by your Go app to be missing from the Stackdriver Error Reporting page.

### Misc
For everything else, log as usual. And of course, use [structured logging](https://nblumhardt.com/2016/06/structured-logging-concepts-in-net-series-1/) liberally to make querying your logs and troubleshooting issues easier.

## Demo Apps: Pre-Requisites

* [Docker Desktop](https://www.docker.com/products/docker-desktop) (enable Kubernetes in Preferences -> Kubenetes -> Enable Kubernetes)
* [skaffold](https://skaffold.dev/)

If you don't yet have a GKE cluster, create one (using [preemptible VMs](https://cloud.google.com/kubernetes-engine/docs/how-to/preemptible-vms) to keep the costs down):

```
gcloud services enable container.googleapis.com

gcloud container clusters create logdemo \
--preemptible \
--enable-autoupgrade \
--enable-autoscaling \
--min-nodes=3 \
--max-nodes=10 \
--region=europe-west1

# Check if it worked
kubectl get nodes
```

You'll also need to enable the Google Container Registry (GCR) API and configure the `docker` CLI to authenticate with GCR if you haven't already:

```
gcloud services enable containerregistry.googleapis.com

gcloud auth configure-docker
```

Finally, configure the default credentials that applications like Skaffold will use to authenticate with the Google Cloud APIs:

```
gcloud auth application-default login
```

## Demo Apps: Build & Run

### Option 1: Build & Run locally with Docker Desktop

Make sure that `kubectl` is pointing to your local cluster. Then, at the root of the repo:

```
# Build all the Docker images and deploy them to the local K8S cluster.
skaffold run

# You should now see the pods running on your K8S cluster:
kubectl get pods
```

> **NOTE**: When running locally, logs won't be forwaded to Stackdriver.
>
> To view the logs written the demo apps, use `kubectl logs` to view the logs of the running K8S pods.
> 
> Alternatively, run the apps using `skaffold run --tail` or `skaffold dev` - this will output the logs to the console.


### Option 2: Build locally & run on Google Kubernetes Engine (GKE)

With this option, you'll still need to have Docker Desktop to build the Docker images. But you don't need to have K8S running locally.

Make sure that your `kubectl` context is pointing to your GKE cluster. Then, at the root of the repo:

```
# PROJECT_ID is your GCP project ID
skaffold run --default-repo=gcr.io/PROJECT_ID

# You should now see the pods running on your GKE cluster:
kubectl get pods
```

Then, in the GCP Console:
* Go to [Stackdriver Logging](https://console.cloud.google.com/logs/viewer) and search for `dotnetlogdemo` or `golanglogdemo` to view the logs written by the demo apps.
* Go to [Stackdriver Error Reporting](https://console.cloud.google.com/errors) to view the errors logged by the demo apps.

### Option 3: Build on Google Cloud Build (GCB) & run on Google Kubernetes Engine (GKE)

With this option, you don't need to have either Docker Desktop nor Kubernetes running locally. Everything happens in the cloud.

Make sure that your `kubectl` context is pointing to your GKE cluster. Then, at the root of the repo:

```
# Build and run.
# PROJECT_ID is your GCP project ID.
skaffold run --profile gcb --default-repo=gcr.io/PROJECT_ID
```

Then, in the GCP Console:
* Go to [Stackdriver Logging](https://console.cloud.google.com/logs/viewer) and search for `dotnetlogdemo` or `golanglogdemo` to view the logs written by the demo apps.
* Go to [Stackdriver Error Reporting](https://console.cloud.google.com/errors) to view the errors logged by the demo apps.

### Option 4: Build with Kaniko on GKE and run on GKE

[TODO (the build works but pushing the built image to GCR fails for an unknown reason)]

## Skaffold tips & tricks

If this is your first time using skaffold:

```
# Builds the app and deploys (or re-deploys) it to the K8S cluster
skaffold run

# Same but also displays the logs from the app's pods in real-time
skaffold run --tail

# Stops and deletes your app from the K8S cluster
skaffold delete

# Builds and deploys the app. Automatically forwards ports exposed by
# pods to your local machine so that you can access the app locally.
#
# Then watches for any code file changes and automically re-builds 
# and re-deploys on any change. 
#
# This is what you'd typically use when developing. 
skaffold dev
```

If you don't feel like typing your GCR URL everything you run skaffold, you can set it in the `SKAFFOLD_DEFAULT_REPO` env var instead:

```
export SKAFFOLD_DEFAULT_REPO="gcr.io/PROJECT_ID"

# You might want to set it in your .bash_profile and/or .bashrc as well
echo 'export SKAFFOLD_DEFAULT_REPO="gcr.io/PROJECT_ID"' >> ~/.bash_profile

# You can now omit the --default-repo arg when running skaffold
skaffold run
```

Alternatively, you can [set the default image repo in Skaffold's global config](https://skaffold.dev/docs/concepts/#image-repository-handling), which allows you to have a different GCR URL per `kubectl` context. 


