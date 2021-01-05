using Microsoft.AspNetCore.Mvc.Formatters;

namespace RestAPI.OutputFormatters
{
	public class HtmlOutputFormatter : StringOutputFormatter
	{
		public HtmlOutputFormatter()
		{
			SupportedMediaTypes.Add("text/html");
		}
	}
}
