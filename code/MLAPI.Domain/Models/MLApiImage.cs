using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLAPI.Domain.Models
{
    public class MLApiImage
    {
        public int Id { get; set; }
        public Nullable<System.Guid> ModelId { get; set; }
        public string ImagePath { get; set; }
    }
}
