# EShop application deplyoment

## 1. Create namespace

```kubectl create namespace eshop```

## 2. Install postgres

```helm repo add bitnami https://charts.bitnami.com/bitnami```

```helm install db -n eshop -f ./postgres/values.yml bitnami/postgresql```

## 3. Install kafka

```helm install brocker -n eshop -f ./kafka/values.yml bitnami/kafka```

## 4. Apply configuration

```kubectl apply -f ./app/config.yml```

## 5. Deploy application

```kubectl apply -f ./app/deployment.yml```
