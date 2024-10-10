using System.Collections.Generic;

namespace RefactorThis.Domain.Entities
{
    public class Response<T> where T : class
    {
        public Response()
        {
            IsSuccess = true;
            Data = new List<T>();
        }

        public bool IsSuccess { get; set; }
        public List<T> Data { get; set; }
    }
}
