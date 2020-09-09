# RssRetriever
This is a simple RSS retriever, made just for fun. It fetches articles 
from the various feeds, and posts them to slack if they haven't been 
fetched before. It honors the ttl set in the feed, or defaults to 30 
minutes if no ttl is set. 

If time allows I will also add an integration to IRC, similar to ##news 
on freenode, but there are no promises.... 


# Roadmap

* Replace sqlite with Azure SQL Database in the Serverless compute tier (https://docs.microsoft.com/en-us/azure/azure-sql/database/migrate-sqlite-db-to-azure-sql-serverless-offline-tutorial)
* Run the rss retriever in Azure somehow...  -> Docker
* Proper frontend
* Get proper test environment
* CI/CD with Azure in test
* CI/CD with Azure in prod