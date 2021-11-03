# System design

## Table of Contents
1. [Overview](#Overview)
2. [Functional Requirements](#functional-requirements)
3. [Non Functional Requirements](#non-functional-requirements)
4. [Additional requirements](#additional-requirements)
5. [Storage and bandwidth requirements](#storage-and-bandwidth-requirements)
6. [Component Design](#component-design)
7. [Load Balancers](#load-balancers)
8. [Hosting the Services](#hosting-the-services)
9. [Data Stores](#data-stores)
10. [Additional components](#additional-components)
11. [Data Flow Design](#data-flow-design)


## Overview
This document discusses the different approaches involved in designing a word to number translator. The design elements/components are chosen from Azure's suite of cloud solutions. 

## Functional requirements
- System takes in a word as an input and returns an integer as output
- System handles multiple numbering systems, decimal values and negative values
- System records transactions performed and shows a history of transactions upon request

## Non Functional requirements
- System is highly available
- System is accessible to authorized and authenticated users only
- System is highly scalable and reliable
- System ensures data recorded is never lost
- System can service requests from various types of clients: mobile Apps, external services, web Apps
- System is deployed in one region only and should be available within that geopolitical region.

## Additional requirements
- System should be able to support addition of two numbers
- System should display user history for the current user only

## Storage and bandwidth requirements
Assuming a high threshold of 10 million translations per day and a total of 10 million users:
Each translation would need a minimum of 128+8+4 bytes (128 characters for the word, 8 characters for the userid), this amounts to 14.96 MB/second of inbound traffic. Assuming only the translated number is returned for these words, it's 0.44 MB/s outbound traffic.

Assuming one percent of the users retrieve their translation history per day, each request would need at least 8 bytes (just the user id , this results in ~100kb/s inbound traffic, each translation would need 128+4+4 bytes (the original word+translated number) and each response returns 10 records, this results in a 1.5 MB/s outbound traffic.

Storing 10 million transactions per day would need ~ 100 GB (1 transcation = 136 bytes) of data per day. Based on retention policies this number can be calculated for the period of retention.
Assuming the database only stores user IDs and not any other personal user information, instead relying on identity services to fetch user information, the system may not need to store any additional user related information.
These assumptions do not take into consideration other platform/DB specific data requirements like partition and row keys.

## Component design
The following diagram describes the various components involved. Each component must be chosen based on requirements.

![Alt text](/images/componentdiagram.jpeg?raw=true "Component Diagram")


Note: The component diagram denotes possible choices for components rather than a holistic solution

### Load balancers
There are primarily two possible choices for load balancing in Azure: Application Gateway and FrontDoor. If the system is geared towards single-user web or mobile applications with considerable static front end content, a CDN-style solution like FrontDoor is ideal. FrontDoor provides the additional benefit of load balancing across multiple regions, which necessitates the deployment of the system across multiple regions.
Application Gateway on the other hand is ideal for load balancing between different nodes deployed in a single region. 

Either solution is required for deploying a secure, scalable solution. These components can also be configured to authenticate requests via Azure/other identity providers before forwarding the requests to appropriate services. They also allow for resources to be hidden behind a private network thereby providing a secure boundary.

For the current system if the geopolitical region contains multiple Azure regions, an Azure FrontEnd that load balances between instances deployed across these regions is a good fit, else an Application Gateway would be suitable.

## Hosting the Services
Each functional unit of the system can be broken down into it's own self-contained service. This is a recommended approach to allow for scalability, ease of deployment, debuggability, etc.
For eg.: Different functional problems may need different frameworks, languages, versions, etc. Deploying these as a single service with multiple endpoints can quickly become cumbersome as the system grows. Possible Approaches:

### Azure Function Apps
For low-load, low-latency apps with low computational needs, Azure Function Apps are a great solution, it abstracts away the computational layer allowing developers to focus on the functional aspects. However, Azure function apps are billed per transaction, if the system is supposed to experience a high volume of transactions, it's better to look at more cost optimized solutions like App service and AKS.
If the current system is expected to handle a small number of transactions per day without performing any complex calculations, Azure Functions Apps are ideal.

### Azure App service
Azure App service can be scaled easily, is always on and can result in lower costs compared to Azure function apps. However, App services can host one monolithic server process per server intance making it difficult to break down the functionality into self contained units like micro services. If a system has a finite set of functionalities with little or no growth in terms of complexity, this is an ideal solution. 
If the current system is not expected to offer additional functionalities/features to the user while supporting large transaction volumes, App Service is a great fit.

### Azure Kubernetes cluster
Azure Kubernetes Service offers Kubernetes clusters that can host multiple micro services in docker containers. Containerization of solutions makes it easier to deploy solutions via docker images. However, it adds to complexity of the system. An Azure Container Registry needs to store the custom docker images, interfaces must be defined between various micro services to ensure smooth integration, etc.
If the current system is expected to grow in functionalities, features and overall complexity and scalability, it's better to adopt a micro services architecture. 

## Data Stores
### Traditional RDBMS Data store
If ACID-ness and schema integrity of data are crucial, traditional RDBMS solutions like MySQL, SQL server are best suited. They suffer from scalibility, perf and partitioning issues.

### NoSQL Key-value stores
For low-latency eventual consistency data stores, a NoSQL solution like CosmosDB is an ideal solution, CosmosDB can be tuned for consistency, made geographically redundant and supports partitioning. Choosing the right partitioning key is extremely crucial for cost and performance efficiency. CosmosDB can get expensive if used to store historical data and should be used for low latency transactional data instead

### File based data stores
HBase on HDInsights is a big-table based Azure solution that offers highly scalable file based data management solution, it has higher latencies compared to CosmosDB but has much higher cost benefits
In the current system, eventual consistency is acceptable since the users would theoretically accept their transaction history to be eventually consistent. If user history is truncated after a certain period of time, such policy can be set on CosmosDB which can become the only data store. If historical data needs to be preserved, a hybrid approach with HBase+CosmosDB or HBase only solution can be adopted, subject to latency thresholds.

## Additional components

### Messaging Queue
An Azure service bus listens to different notifications and queues them along approriate topics for subscribers. For asynchronous calls between system components, Messaging queues are perfectly suited. For Eg.: if system stores data in both low latency Azure Cosmos DB and HBase data stores, the data can be written directly to Cosmos DB and queued on the messaging queue for asynchronous writes. Similary, a cache processer can listen to the Messaging queue and react to any cache misses to update the cache based on cache policies.

### Cache
To improve efficiency, a cache stores data from the cache, a cache management micro service keeps track of "hit"/"miss" events on the service bus and keeps updating the cache accordingly. Caches themselves are distributed and may need load balancers.

### Data Analytics Engines
Data analytics is an integral part of any system in modern cloud computing. Various data points can be aggregated in real time in a Synapse-like solution for insights.

### Monitors and Alerts
Azure Application insights can be configured to listen to events related to resources in the system, these can be further configured to trigger alerts to notify system admins or even kick start self healing services if applicable.

### Azure keyVault
Components within the system must interact with eachother with appropriate tokens/certificates. The certificates to generate/verify security tokens are stored in Azure keyVaults.

## Data Flow Design
The following diagrams represent overall flow of data through the system. For brevity, diagrams are limited to the Translation calls.

Data flow when various components in the system interact to handle the /translateword API call
![Alt text](/images/translateworddfd.jpeg?raw=true "Data Flow Diagram (System Components)")

Data flow depicting the user interaction with the system
![Alt text](/images/translateworduserdfd.jpeg?raw=true "Data Flow Diagram (User Interaction)")

