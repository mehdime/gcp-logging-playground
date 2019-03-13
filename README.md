# Google Stackdriver / GKE Logging Demo

Demo and best practices for logging from your Kubernetes-enabled app.

Includes examples designed to make the most of [Stackdriver](https://cloud.google.com/logging/). But the principles, advice and code would apply equally regardless of the logging tool used.

## Languages demoed
* C# / ASP.NET
* Go

## Pre-requisites

* [Docker Desktop](https://www.docker.com/products/docker-desktop) (enable Kubernetes in Preferences -> Kubenetes -> Enable Kubernetes)
* [skaffold](https://skaffold.dev/)

If you don't yet have a GKE cluster, you can create one like this (using [preemptible VMs](https://cloud.google.com/kubernetes-engine/docs/how-to/preemptible-vms) to keep the costs down):

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

## Build & Run

### Option 1: Build & Run locally with Docker Desktop

> In this version of the code, logs won't be forwarded to Stackdriver. Stackdriver forwarding when running locally is WIP.

Make sure that your `kubectl` context is pointing to your local cluster. Then, at the root of the repo:

```
# Build all the Docker images and deploy them to the local K8S cluster.
skaffold run

# You should now see the pods running on your K8S cluster:
kubectl get pods
```

### Option 2: Build locally & run on Google Kubernetes Engine (GKE)

With this option, you'll still need to have Docker Desktop to build the Docker images. But you don't need to have K8S running locally.

Make sure that your `kubectl` context is pointing to your GKE cluster. Then, at the root of the repo:

```
# PROJECT_ID is your GCP project ID
skaffold run --default-repo=gcr.io/PROJECT_ID

# You should now see the pods running on your GKE cluster:
kubectl get pods
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

### Option 3: Build on Google Cloud Build (GCB) & run on Google Kubernetes Engine (GKE)

With this option, you don't need to have either Docker Desktop nor Kubernetes running locally. Everything happens in the cloud.

Make sure that your `kubectl` context is pointing to your GKE cluster. Then, at the root of the repo:

```
# Build and run.
# PROJECT_ID is your GCP project ID.
skaffold run --profile gcb --default-repo=gcr.io/PROJECT_ID
```

### Option 4: Build with Kaniko on GKE and run on GKE

[TODO (the build works but pushing the built image to GCR fails for an unknown reason)]

## Logging Guide

TODO

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




