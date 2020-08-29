﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StoreWeb.Models;
using StoreLib.Models;
using StoreLib.Services;
using StoreLib.Utilities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StoreWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConvertController : ControllerBase
    {
        // GET: api/<ConvertController>
        [HttpGet]
        public async Task<string> Get(
            /*Mandatory get parameter*/ string query,
            /*Mandatory get parameter*/ string devicetoken,
            string Environment = "Production",
            string Market = "US",
            string Lang = "en",
            string Msatoken = null)
        {
            Packages packagerequest = new Packages()
            {
                id = query,
                environment = (DCatEndpoint)Enum.Parse(typeof(DCatEndpoint), Environment),
                lang = (Lang)Enum.Parse(typeof(Lang), Lang),
                market = (Market)Enum.Parse(typeof(Market), Market),
                msatoken = Msatoken
            };
            DisplayCatalogHandler dcat = new DisplayCatalogHandler(packagerequest.environment, new Locale(packagerequest.market, packagerequest.lang, true));
            if (!string.IsNullOrWhiteSpace(packagerequest.msatoken))
            {
                await dcat.QueryDCATAsync(packagerequest.id, packagerequest.type, packagerequest.msatoken);
            }
            else
            {
                await dcat.QueryDCATAsync(packagerequest.id, packagerequest.type);
            }
            if (dcat.IsFound)
            {
                if (dcat.ProductListing.Product != null) //One day ill fix the mess that is the StoreLib JSON, one day. Yeah mate just like how one day i'll learn how to fly
                {
                    dcat.ProductListing.Products = new List<Product>();
                    dcat.ProductListing.Products.Add(dcat.ProductListing.Product);
                }
                Dictionary<string, string> appinfo = new Dictionary<string, string>();
                foreach (AlternateId PID in dcat.ProductListing.Products[0].AlternateIds) //Dynamicly add any other ID(s) that might be present rather than doing a ton of null checks.
                {
                    appinfo.Add(PID.IdType, PID.Value);
                }
                appinfo.Add("ProductID", dcat.ProductListing.Products[0].ProductId);
                try
                {
                    appinfo.Add("PackageFamilyName", dcat.ProductListing.Products[0].Properties.PackageFamilyName);
                }
                catch (Exception ex) { Console.WriteLine(ex); };
                return JsonConvert.SerializeObject(appinfo);
            }
            else
            {
                return "{\"error\":\"dcat not found\"}";
            }
        }
    }
}
