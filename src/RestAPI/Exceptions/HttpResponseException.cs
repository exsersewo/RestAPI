using System;
using System.Net;

namespace RestAPI.Exceptions
{
	public class HttpResponseException : Exception
	{
		public int Status { get; set; } = 500;

		public object Value { get; set; }

		public HttpResponseException(int statusCode, object val)
		{
			Status = statusCode;
			Value = val;
		}

		public HttpResponseException(HttpStatusCode statusCode, object val) : this((int)statusCode, val)
		{

		}
	}
}
