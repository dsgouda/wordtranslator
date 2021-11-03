# Deployed System

## Design assumptions and scope
The deployed instance makes following assumptions:
- The system will handle web requests from users only, web jobs and services will be filtered by the Application Gateway
- The system is geolocated and local to East US region only, all other traffic will be denied access
- The system will be hidden behind a private network with the only public endpoints being the Application Gateway
- A finite number of users will use the system
- Data will be stored only in the East US region (no geo redundancy)

## Technical assumptions
- A word must be correctly formed for it to be translated. Eg.: "One zero zero six", "Five-o", "1 thousand twelve" are incorrectly formed word numbers
- Only the international numbering system is supported
- Decimal and negative values are not supported
- The response is simply the translated number, or the sum in case of additions

## Implemented components
- Azure Function App instance to translate words and write translation history to data store, azure function app is hidden behind a private virtual network and is only accessible via the frontdoor
- Azure Cosmos DB with tuned consistency and partition key to store translation history, traffic is restricted to private VN and IP addresses
- An Azure Frontdoor instance load balances incoming requests, since only one instance is deployed, it essentailly just forwards the request
- An Azure WAF restricts traffic from US geopolitical region


## Partially implemented/unimplemented components
- The AddWords API has been deployed but not thoroughly tested
- The /getHistory API is not implemented
- Security is enforced on the Azure functions instance requiring users to authenticate before making the REST API call
- Azure CosmosDB instance can accept requests from a hardcoded IP address only for security purposes.

## Design Decisions
- An Azure Function app should suffice the computational needs since the system is handling a limited amount of functionality with limited number of users and limited computational time needed, for load balancing to come into picture multiple instances of Azure functions App must be deployed
- Session level consistency for Azure Cosmos DB should work since it guarantees read consistency for the same geographic region and all reads will have consistent data
- CosmosDB instance has a retention of 30 days with the assumption that only 30 days' worth of history will be stored for users.
- "UserId" which is the Identity name determined by authentication service will be used as the partition key, every user, even if they constantly use the system, won't be able to make more than 4 requests per minute, assuming they do this for nearly nine hours straight for 30 days, the size of each partition will not be more than 65k entries, which may affect performance but it is an extreme case, for most users, the partition size will be less than a thousand entries and hence easily queryable.
- The rowkey for each partition must be a unique one, for simplicity of implementation this can be the timestamp of the operation in "yyyyMMddhhmmss", since the same user cannot use the system from two locations at the same time. This avoids collisions and also allows for range queries if required. 