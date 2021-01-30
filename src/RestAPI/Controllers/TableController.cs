using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Clients;
using RestAPI.Exceptions;
using RestAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Xml;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RestAPI.Controllers
{
	[Route("table")]
	[ApiController]
	public class TableController : ControllerBase
	{
		private readonly IDatabaseClient database;
		private readonly IEnumerable<string> BlacklistedFields;

		public TableController(IDatabaseClient database)
		{
			this.database = database;

			var fields = Environment.GetEnvironmentVariable("BLACKLISTEDFIELDS");

			if (fields != null)
			{
				BlacklistedFields = fields.Split(",").Select(x => x.ToLowerInvariant());
			}
			else
			{
				BlacklistedFields = new List<string>();
			}
		}

		private dynamic ConvertResponse(List<List<KeyValuePair<string, string>>> tableContent, string format, HttpResponse Response)
		{
			switch (format.ToLowerInvariant())
			{
				case "json":
					{
						Response.ContentType = "application/json";

						List<dynamic> content = new List<dynamic>();

						foreach (var result in tableContent)
						{
							dynamic item = new ExpandoObject();
							var dItem = item as IDictionary<string, object>;
							foreach (var attr in result)
							{
								dItem.Add(attr.Key, attr.Value);
							}

							content.Add(item);
						}

						return content;
					}
				case "xml":
					{
						Response.ContentType = "text/xml";
						var doc = new XmlDocument();

						var results = doc.CreateNode(XmlNodeType.Element, null, "results", null);

						foreach (var result in tableContent)
						{
							var res = doc.CreateNode(XmlNodeType.Element, null, "result", null);

							foreach (var attr in result)
							{
								var resEl = doc.CreateNode(XmlNodeType.Element, null, attr.Key, null);

								resEl.InnerText = attr.Value;

								res.AppendChild(resEl);
							}

							results.AppendChild(res);

							doc.AppendChild(results);
						}

						return doc;
					}
				case "html":
					{
						Response.ContentType = "text/html";

						var doc = new HtmlDocument();

						var html = doc.CreateElement("html");
						var body = doc.CreateElement("body");
						var table = doc.CreateElement("table");
						var thead = doc.CreateElement("thead");
						var theadr = doc.CreateElement("tr");

						doc.DocumentNode.AppendChild(html);
						html.AppendChild(body);
						body.AppendChild(table);
						table.AppendChild(thead);
						thead.AppendChild(theadr);

						foreach (var attr in tableContent[0])
						{
							var th = doc.CreateElement("th");
							th.InnerHtml = attr.Key;
							theadr.AppendChild(th);
						}

						var tbody = doc.CreateElement("tbody");
						table.AppendChild(tbody);

						foreach (var result in tableContent)
						{
							var row = doc.CreateElement("tr");

							foreach (var attr in result)
							{
								var resEl = doc.CreateElement("td");

								resEl.InnerHtml = attr.Value;

								row.AppendChild(resEl);
							}

							tbody.AppendChild(row);
						}

						return doc.DocumentNode.InnerHtml;
					}
				default:
					return null;
			}
		}

		[HttpGet]
		public string Get()
		{
			throw new HttpResponseException(HttpStatusCode.BadRequest, "Must enter a valid table");
		}

		[HttpGet("{table}.{format}")]
		public dynamic Get(string table, string format)
		{
			database.OpenAsync().GetAwaiter().GetResult();

			string tableLocation = table;

			if (database is NpgsqlClient)
			{
				tableLocation = $"public.{table}";
			}

			if (database.DoesTableExistAsync(tableLocation).GetAwaiter().GetResult())
			{
				var tableContent = database.GetTableAsync(tableLocation, BlacklistedFields.ToArray()).GetAwaiter().GetResult();
				dynamic response = null;

				if (tableContent != null)
				{
					response = ConvertResponse(tableContent as List<List<KeyValuePair<string, string>>>, format, Response);
				}

				if (response != null)
				{
					return response;
				}
				else
				{
					throw new HttpResponseException(HttpStatusCode.NotFound, $"Data within table \"{tableLocation}\" not found");
				}
			}
			else
			{
				throw new HttpResponseException(HttpStatusCode.NotFound, $"Table \"{tableLocation}\" not found");
			}
		}

		[HttpGet("{area}/{table}.{format}")]
		public dynamic Get(string area, string table, string format)
		{
			database.OpenAsync().GetAwaiter().GetResult();

			string tableLocation;

			if (database is NpgsqlClient)
			{
				tableLocation = $"{area}.{table}";
			}
			else
			{
				tableLocation = $"{area}/{table}"; //default assumption for now
			}

			if (database.DoesTableExistAsync(tableLocation).GetAwaiter().GetResult())
			{
				var tableContent = database.GetTableAsync(tableLocation, BlacklistedFields.ToArray()).GetAwaiter().GetResult();
				dynamic response = null;

				if (tableContent != null)
				{
					response = ConvertResponse(tableContent as List<List<KeyValuePair<string, string>>>, format, Response);
				}

				if (response != null)
				{
					return response;
				}
				else
				{
					throw new HttpResponseException(HttpStatusCode.NotFound, $"Data within table \"{tableLocation}\" not found");
				}
			}
			else
			{
				throw new HttpResponseException(HttpStatusCode.NotFound, $"Table \"{tableLocation}\" not found");
			}
		}
	}
}
