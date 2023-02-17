using ElasticSearchApi.BLL.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchApi.BLL.DTO
{
    public class NewsDTO : ElasticEntity
    {
        public int NewsID { get; set; }
        public string Title { get; set; }
        public string Spot { get; set; }
        public string FullNews { get; set; }
        public int isActive { get; set; }
        public string PhotoURL { get; set; }
    }
}
