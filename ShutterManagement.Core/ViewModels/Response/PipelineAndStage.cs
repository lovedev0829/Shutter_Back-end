using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class PipelineAndStage
    {
        public string GhlPipelineId { get; set; }
        public string GhlPipelineName { get; set; }
        public string GhlStageId { get; set; }
        public string GhlStageName { get; set; }
    }


    public class PipelineResponse
    {
        [JsonPropertyName("pipelines")]
        public List<Pipeline> Pipelines { get; set; }
    }

    public class Pipeline
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("stages")]
        public List<Stage> Stages { get; set; }
    }

    public class Stage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
