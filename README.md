 
 - Kibana
 - Website
 - Logstash

# Kibana

### 1. query & match
``` 
GET capwork_alexshab_2/doc/_search
{
  "query": {
    "match": {
      "text_entry": "HORATIO"
    }
  }
}
```

### 2. complex search request
```
GET capwork_alexshab_2/doc/_search
{
  "query": {
    "match": {
      "text_entry": "HORATIO"
    }
  },
  "sort": { "speech_number": { "order": "desc" } },
  "from": 3,
  "size": 5,
  "track_scores" : true,
  "_source": ["speech_number", "text_entry"]
}
```

### 3.  match with several words
```
GET capwork_alexshab_2/doc/_search
{
  "query": {
    "match": {
      "text_entry": "love death"
    }
  }
}
```
### 4.  match with several words
```
GET capwork_alexshab_2/doc/_search
{
  "query": {
    "match_phrase": {
      "text_entry": "be or not to be"
    }
  }
}
```
### 5.  multy match
```
GET capwork_alexshab_2/doc/_search
{
  "query": {
    "multi_match" : {
      "query":    "this is a test", 
      "fields": [ "text_entry", "speaker" ] 
    }
  }
}
```

    enter code here

### 6. fuzzy match
```
GET capwork_alexshab_2/doc/_search
{
  "query": {
    "match": {
      "speaker": {
        "query":   "HORECIO",
        "fuzziness": "auto"     
      }
    }
  }
}
```

### 7. term search
```
GET capwork_alexshab_2/doc/_search
{
  "query": {
    "term" : { "text_entry" : "Horatio" } 
  }
}
```

### 8. term search
```
GET capwork_alexshab_2/doc/_search
{
  "query": {
    "term" : { "speaker" : "HORATIO" } 
  }
}
``` 
 
### 9. range
```
GET capwork_alexshab_2/doc/_search
{
	"query": {
		"range": {
			"speech_number": {
				"gte": 10,
				"lte": 30
			}
		}
	}
}
```
### 10. bool context
```
GET capwork_alexshab_2/doc/_search
{
	"query": {
		"bool": {
			"must": [
					{ "match": { "text_entry": "to be" } },
					{ "bool" : {"should": [
								{ "match": { "speaker": "HORATIO" } },
								{ "match": { "speaker": "HAMLET" } }
						]} 
					}					
				],			
			"must_not": [
					{ "match": { "text_entry": "not" } }					
				]
		}
	}
}
```

### 11. filter context
```
GET capwork_alexshab_2/doc/_search
{
	"query": {
		"bool": {
		  "filter": {
		    "term": {
		      "speaker": "HAMLET"
		    }
		  }
	  }
  }
}
```

# Website
### 1. BaseElastic
```
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using website.Configuration;
using website.DataLayer.InitStrategies;
using website.DataLayer.Models;

namespace website.DataLayer.Repository
{
    public class ElasticsearchBaseRepo<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        protected IElasticClient client;
        protected readonly ElasticConfig config;

        public ElasticsearchBaseRepo(IElasticClient elasticClient, IOptions<ElasticConfig> config)
        {
            client = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            this.config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<TEntity> Get(string id, ShardSource source)
        {
            TEntity result = null;
            var request = new GetRequest(config.PlaysIndexName, config.PlaysDocType, id);

            var res = await client.GetAsync<TEntity>(request);

            if (res.Source != null)
            {
                result = res.Source;
                result.Id = res.Id;
            }

            return result;
        }

        public async Task<TEntity> Add(TEntity item)
        {
            var res = await client.CreateAsync<TEntity>(item, idx =>
              idx.Type(config.PlaysDocType)
                  .Index(config.PlaysIndexName)
                  .Id(item.Id)
                );

            return item;
        }

        public async Task<TEntity> Update(TEntity item)
        {
            var res = await client.IndexAsync<TEntity>(item, idx =>
            idx.Type(config.PlaysDocType)
                .Index(config.PlaysIndexName)
                .Id(item.Id)
              );

            return item;
        }

        public async Task Delete(string id)
        {
            DocumentPath<TEntity> path =
                new DocumentPath<TEntity>(new Id(id)).Index(config.PlaysDocType).Type(config.PlaysIndexName);

            var res = await client.DeleteAsync(path);
        }

        public async Task Delete(TEntity item)
        {
            await Delete(item.Id);
        }

        public async Task<IEnumerable<TEntity>> GetAll(SelectSettings settings)
        {
            var res = await client.SearchAsync<TEntity>(q =>
                 q
                   .Preference(settings.Source.ToString())
                   .Type(config.PlaysDocType)
                   .Index(config.PlaysIndexName)
                   .ExecuteOnPrimary()
                   .Size(settings.Size)
                   .Sort(s => s.Field(settings.SortBy, settings.Order)));

            return res.Hits.Select(h => h.Source);
        }
    }
}
```
### 2. Config
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace website.Configuration
{
    public class ElasticConfig
    {
        public string Uri { get; set; }
        public string PlaysIndexName { get; set; }
        public string PlaysDocType { get; set; }
    }
}
```
### 2.1 Json Config
  "ElasticConfig": {
    "Uri": "http://10.6.217.106:9200",
    "PlaysIndexName": "alexshab6",
    "PlaysDocType": "playmodel"
  }
  
### 3. PlaysRepo
```
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using website.Configuration;
using website.DataLayer.InitStrategies;
using website.DataLayer.Models;
using website.Models;

namespace website.DataLayer.Repository
{
    public class PlaysRepository : ElasticsearchBaseRepo<PlayModel>
    {
        public PlaysRepository(IElasticClient elasticClient, IOptions<ElasticConfig> config) : base(elasticClient, config)
        {
        }   
    }
}
```

### 4.1 Place Services Interface
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using website.DataLayer.Models;

namespace website.Services
{
    public interface IPlaysService
    {
        Task<IEnumerable<PlayModel>> Select();
        Task Create(PlayModel model);
    }
}
```

### 4.2 Place Services Interface
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using website.DataLayer;
using website.DataLayer.Models;
using website.DataLayer.Repository;

namespace website.Services
{
    public class PlaysService : IPlaysService
    {
        private readonly ISearchableRepo<PlayModel> repository;

        public PlaysService(ISearchableRepo<PlayModel> repository)
        {
            this.repository = repository;
        }

        public async Task Create(PlayModel model)
        {
            await repository.Add(model);
        }

        public async Task<IEnumerable<PlayModel>> Select()
        {            
			return await repository.GetAll(SelectSettings.Default);            
        }
    }
}
```

### 5.1 IoC Registration
```
services.Configure<ElasticConfig>(Configuration.GetSection("ElasticConfig"));
services.AddScoped<IElasticClient, ElasticClient>(serviceProvider => {
	var config = serviceProvider.GetService<IOptions<ElasticConfig>>();
	return new ElasticClient(ComposeSettings(config.Value));
});
services.AddScoped<DataLayer.Repository.IRepository<PlayModel>, PlaysRepository>();
services.AddScoped<IPlaysService, PlaysService>();            
```

### 5.2 Elastic Client func
```
public ConnectionSettings ComposeSettings(ElasticConfig config) =>
        new ConnectionSettings(new Uri(config.Uri))
            .ThrowExceptions()
            .DefaultIndex(config.PlaysIndexName);
 ```           
### 6.1 Strategy Interface

    using Nest;
    
    namespace website.DataLayer.InitStrategies
    {
        public interface IInitStrategy
        {
            void Init(IElasticClient client, string indexName);
        }
    }

### 6.2 JustCreate Strategy

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Nest;
    
    namespace website.DataLayer.InitStrategies
    {
        public class JustCreateStrategy : IInitStrategy
        {
            private object _lock = new object();
            public virtual void Init(IElasticClient client, string indexName)
            {
                lock (_lock)
                {
                    if (CheckNeedToInit(client, indexName))
                        client.CreateIndex(indexName);
                }
            }
    
            protected  bool CheckNeedToInit(IElasticClient client, string indexName)
            {
                return !client.IndexExists(indexName).Exists;
            }
        }
    }

### 6.3 GenericType Interface

    using Nest;
    using website.DataLayer.Models;
    
    namespace website.DataLayer.InitStrategies
    {
        public interface IInitMappedStrategy<T> where T : class, IEntity
        {
            void Init(IElasticClient client, string indexName);
        }
    }

### 6.4 CreateWithMapping Strategy

    using Nest;
    using website.DataLayer.Models;
    
    namespace website.DataLayer.InitStrategies
    {
        public class CreateWithMappingStrategy : IInitMappedStrategy<PlayModel>
        {
            private object _lock = new object();
            public virtual void Init(IElasticClient client, string indexName)
            {
                lock (_lock)
                {
                    if (CheckNeedToInit(client, indexName))
                        client.CreateIndex(indexName, InitMapping);
                }
            }
    
            protected ICreateIndexRequest InitMapping(CreateIndexDescriptor descriptor) =>
                 descriptor
                    .Mappings(mapping => mapping.Map<PlayModel>(InitTypeDesciption));
    
            protected ITypeMapping InitTypeDesciption(TypeMappingDescriptor<PlayModel> typeDescriptor) =>
                typeDescriptor
                    .Properties(prop => prop.Keyword(s => s.Name(n => n.Speaker)));
    
            protected bool CheckNeedToInit(IElasticClient client, string indexName)
            {
                return !client.IndexExists(indexName).Exists;
            }
        }
    }

### 6.5 InitAndFillStrategy

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Nest;
    using Newtonsoft.Json;
    using website.DataLayer.Models;
    
    namespace website.DataLayer.InitStrategies
    {
        public class InitAndFillStrategy : IInitStrategy
        {
            const string INIT_DATA_FILENAME_TEMPLATE = "data.json";
            private object _lock = new object();
            public virtual void Init(IElasticClient client, string indexName)
            {
                lock (_lock)
                {
                    try
                    {
                        if (CheckNeedToInit(client, indexName))
                        {
                            client.CreateIndex(indexName);
                            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, INIT_DATA_FILENAME_TEMPLATE);
                            var data = JsonConvert.DeserializeObject<IEnumerable<PlayModel>>(File.ReadAllText(filePath));
                            var saveJobs = data.Select(doc => client.IndexAsync(doc, idx =>
                                 idx.Index(indexName)
                            ));
                            Task.WhenAll(saveJobs).Wait();
                        }
                    }
                    catch (Exception e)
                    {
    
                        throw;
                    }
    
                }
            }
    
            protected bool CheckNeedToInit(IElasticClient client, string indexName)
            {
                return !client.IndexExists(indexName).Exists;
            }
        }
    }


### 7. Searchable repo interface

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using website.DataLayer.Models;
    
    namespace website.DataLayer.Repository
    {
        public interface ISearchableRepo<TEntity> : IRepository<TEntity> where TEntity : IEntity
        {
            Task<IEnumerable<TEntity>> Search(string input, SelectSettings settings);
        }
    }


### 7.1. SearchRequest

 

    public override async Task<IEnumerable<PlayModel>> Search(string input, SelectSettings settings)
            {
                var res = await client.SearchAsync<PlayModel>(q =>
                {
                    var query = q
                        .Preference(settings.Source.ToString())
                        .Type(config.PlaysDocType)
                        .Index(config.PlaysIndexName)
                        .ExecuteOnPrimary()
                        .Size(settings.Size)
                        .Sort(s => s.Field(settings.SortBy, settings.Order))
                        .Query(q1 => q1
                            .Bool(boolDescriptor => boolDescriptor
                                .Should(s1 => s1
                                    .MultiMatch(mp => mp
                                        .Query(input)
                                        .Fields(f => f
                                            .Field(f1 => f1.Speaker, 2)
                                            .Fields(f2 => f2.TextEntry)))
                        )));                
                    return query;
                });
    
                return res.Hits.Select(h => h.Source);
            }

### 8. DebugRequest

    using (MemoryStream mStream = new MemoryStream())
    {
    	client.RequestResponseSerializer.Serialize(query, mStream, SerializationFormatting.None);
    	string rawQueryText = Encoding.ASCII.GetString(mStream.ToArray());
    }
    return query;

### 9. Migration

    using Nest;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using website.DataLayer.Models;
    
    namespace website.DataLayer.InitStrategies
    {
        public class MigrationStrategy : IInitStrategy
        {
            const string INIT_DATA_FILENAME_TEMPLATE = "data.json";
    
            private object _lock = new object();
            private int version = 1;
            public virtual void Init(IElasticClient client, string indexName)
            {
                lock (_lock)
                {
                    try
                    {
    
                        if (CheckNeedToInit(client, VersionedName(indexName, version)))
                        {
                            client.CreateIndex(VersionedName(indexName, version));
    
                            if (version == 1 !client.IndexExists(VersionedName(indexName, version - 1)).Exists)
                            {
                                FillByData(client, VersionedName(indexName, version));
                                UpdateAlias(client, indexName, flushOld: false);
                            }
                            else
                            {
                                MigrateData(client, indexName);
                                UpdateAlias(client, indexName, flushOld: true);
                            }
    
    
                        }
                    }
                    catch (Exception e)
                    {
    
                        throw;
                    }
    
                }
            }
    
            protected string VersionedName(string name, int version) => $"{name}-{version}";
    
            protected void MigrateData(IElasticClient client, string indexName)
            {
                var reindex = client.ReindexOnServer(r => r
                   .Source(s => s.Index(VersionedName(indexName, version - 1)))
                   .Destination(d => d.Index(VersionedName(indexName, version)))
                 );
            }
            protected void FillByData(IElasticClient client, string indexName)
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, INIT_DATA_FILENAME_TEMPLATE);
                var data = JsonConvert.DeserializeObject<IEnumerable<PlayModel>>(File.ReadAllText(filePath));
                var saveJobs = data.Select(doc => client.IndexAsync(doc, idx =>
                     idx.Index(indexName)
                ));
                Task.WhenAll(saveJobs).Wait();
            }
    
            protected void UpdateAlias(IElasticClient client, string indexName, bool flushOld)
            {
                if(flushOld)
                    client.Alias(x => x.Remove(a => a.Alias(indexName).Index(VersionedName(indexName, version - 1))));
                client.Alias(x => x.Add(a => a.Alias(indexName).Index(VersionedName(indexName, version))));
            }
    
            protected bool CheckNeedToInit(IElasticClient client, string indexName)
            {
                return !client.IndexExists(indexName).Exists;
            }
        }
    
    }

### 10. Nlog init

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using NLog.Web;
    
    namespace website
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
                try
                {
                    logger.Debug("Init web service");
                    CreateWebHostBuilder(args).Build().Run();
                }
                catch (Exception ex)
                {
                    //NLog: catch setup errors
                    logger.Error(ex, "Stopped program because of exception");
                    throw;
                }
                finally
                {
                    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                    NLog.LogManager.Shutdown();
                }
            }
    
            public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
                WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.SetBasePath(Directory.GetCurrentDirectory());
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                        config.AddCommandLine(args);
                    })
                .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(LogLevel.Trace);
                    })
                .UseNLog()
                .UseStartup<Startup>();
        }
    }

### 11. Nlog config

    <?xml version="1.0" encoding="utf-8" ?>
    <nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
          xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
          autoReload="true"
          internalLogLevel="Info"
          internalLogFile="c:\temp\internal-nlog.txt">
    
      <!-- enable asp.net core layout renderers -->
      <extensions>
        <add assembly="NLog.Web.AspNetCore"/>
      </extensions>
    
      <!-- the targets to write to -->
      <targets>
        <!-- write logs to file  -->
        <target xsi:type="File" name="allfile" fileName="c:\temp\nlog-all-${shortdate}.json"
                >
          <layout xsi:type="JsonLayout" includeAllProperties="true" excludeProperties="Comma-separated list (string)">
            <attribute name="time" layout="${longdate}" />
            <attribute name="logger" layout="${logger}" />
            <attribute name="level" layout="${level:upperCase=true}"/>
            <attribute name="message" layout="${message}" />
            <attribute name="exception" layout="${exception:format=tostring}" />
            <attribute name="event-properties" layout=" ${event-properties:item=EventId_Id}" />
          </layout>
        </target>
        <!-- layout="${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}"-->
        <!-- another file log, only own logs. Uses some ASP.NET core renderers -->
        <target xsi:type="File" name="ownFile-web" fileName="c:\temp\nlog-own-${shortdate}.log"
                layout="${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}" />
      </targets>
    
      <!-- rules to map from logger name to target -->
      <rules>
        <!--All logs, including from Microsoft-->
        <logger name="*" minlevel="Trace" writeTo="allfile" />
    
        <!--Skip non-critical Microsoft logs and so log only own logs-->
        <logger name="Microsoft.*" maxlevel="Info" final="true" />
        <!-- BlackHole without writeTo -->
        <logger name="*" minlevel="Trace" writeTo="ownFile-web" />
      </rules>
    </nlog>

# logstash


### 1. test cli

	logstash -e "input { stdin { } } output { stdout {} }"

### 2. simple config
	input { stdin { } } 
	output { 
		elasticsearch {
			hosts => ["http://localhost:9200"]
			index => "logs_alex_shab-%{+YYYY.MM}" 
		} 
	}
### 3. config with file
	input { 
		file {				
			path => [ "C:\temp\*.json" ]		
			codec => "json_lines"
			start_position => "beginning"
			sincedb_path => "C:\temp\db"
		} 
	} 
	output { 
		elasticsearch {
			hosts => ["localhost:9200"]
			index => "logs_json_alex_shab-%{+YYYY.MM}" 
		} 
	}
