using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLAPI.Domain.Models
{
    public class MLApiExperiment
    {
        public int ExperimentId { get; set; }
        public Nullable<System.Guid> ModelId { get; set; }
        public Nullable<decimal> LearningRate { get; set; }
        public Nullable<decimal> Steps { get; set; }
        public Nullable<decimal> NumberOfLayers { get; set; }
        public Nullable<decimal> Accuracy { get; set; }
    }
}
