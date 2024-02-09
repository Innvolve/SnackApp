using HtmlAgilityPack;
using ScrapySharp;
using ScrapySharp.Extensions;

// Send get request
var url = "cafetariabienvenue.12waiter.eu";
var httpClient = new HttpClient();
var html = httpClient.GetStringAsync(url).Result;
var document = new HtmlDocument();
document.LoadHtml(html);

document.DocumentNode.CssSelect();