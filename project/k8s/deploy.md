# Crash application deplyoment

## 1. Create namespace

```kubectl create namespace crash```

## 2. Install postgres

```helm repo add bitnami https://charts.bitnami.com/bitnami```

```helm install db -n crash -f ./postgres/values.yml bitnami/postgresql```

## 3. Install kafka

```helm install brocker -n crash -f ./kafka/values.yml bitnami/kafka```

## 4. Install redis

```helm install cache bitnami/redis```

## 5. Deploy application

```kubectl apply -f ./app/deployment.yml```
