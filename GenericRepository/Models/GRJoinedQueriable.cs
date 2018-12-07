using GenericRepository.Enums;
using GenericRepository.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public class GRJoinedQueriable
    {
        public GRJoinType Type { get; set; }
        public GRQueriable Queriable { get; set; }
        public string SourcePropertyName { get; set; }
        public string TargetPropertyName { get; set; }

        public string TypeName
        {
            get
            {
                switch (Type)
                {
                    case GRJoinType.LeftJoin:
                        return "LEFT";
                    case GRJoinType.InnerJoin:
                        return "INNER";
                    case GRJoinType.RightJoin:
                        return "RIGHT";
                    case GRJoinType.FullOuterJoin:
                        return "FULL OUTER";
                    default:
                        throw new GRUnsuportedOperatorException(string.Format("{0} is not supported.", Type.ToString()));
                }
            }
        }
    }
}
