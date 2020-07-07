using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smugger
{
    public class GetResponseStub<T>
    {
        public T Response { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }
    public class GetResponseWithExpansionStub<T,TE>
    {
        public T Response { get; set; }
        public Dictionary<string, TE> Expansions { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }
    public class PostResponseStub<T>
    {
        public T Response { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return Response.ToString();
        }
    }

    public class DeleteResponseStub
    {
        public SmugMugUri Response { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
