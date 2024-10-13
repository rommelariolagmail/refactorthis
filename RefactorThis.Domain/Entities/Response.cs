using System;
using System.Collections.Generic;

namespace RefactorThis.Domain.Entities
{
    public class Response<T> where T : class
    {
        private Exception _exception;

        public Response()
        {
            IsSuccess = true;
            Data = new List<T>();
            Messages = new List<string>();
        }

        public bool IsSuccess { get; set; }
        public List<T> Data { get; set; }
        public List<string> Messages { get; set; }

        public Exception Exception { get => _exception; }

        public void SetException(Exception ex)
        {
            IsSuccess = false;
            Messages.Add(ex.Message);
            _exception = ex;
        }
    }
}