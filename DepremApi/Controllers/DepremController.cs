using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace DepremApi.Controllers
{
    public class DepremController : ApiController
    {

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        private long ConvertToTimestamp(DateTime value)
        {
            TimeSpan elapsedTime = value - Epoch;
            return (long)elapsedTime.TotalSeconds;
        }




        [HttpGet]
        public JObject api(int limit = 0)
        {

            string urlAddress = "http://www.koeri.boun.edu.tr/scripts/lst0.asp";


            Encoding iso = Encoding.GetEncoding("iso-8859-9");
            HtmlWeb web = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = iso,
            };

            HtmlDocument document = web.Load(urlAddress);
            HtmlNode node = document.DocumentNode.SelectSingleNode("//pre");

            string htmlData = node.InnerText;

            htmlData = htmlData.Trim().Split(new string[] { "--------------" }, StringSplitOptions.None)[2].Trim();
            var depremList = htmlData.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            if (limit != 0)
            {

                if (limit > depremList.Length || limit <0)
                    limit = depremList.Length;
 

            }
            else
            {
                limit = depremList.Length;


            }


            JObject objDepremApi = new JObject();
            JArray arrDepremApi = new JArray();

            if (depremList.Length > 0)
            {

                objDepremApi.Add("success", true);
            }
            else
            {

                objDepremApi.Add("success", false);



                return objDepremApi;
            }


            for (int i = 0; i <limit; i++)
            {


                var depremRow = depremList[i].Trim();




                //Verideki birden fazla olan boşlukları tek boşluğa indirme
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                depremRow = regex.Replace(depremRow, " ");

                var depremArgs = depremRow.Split(' ');


                var depremArgs8 = depremRow.Split(new string[] { depremArgs[8] }, StringSplitOptions.None);
                var depremYeri = depremArgs[8] + depremArgs8[depremArgs8.Length - 1].Split(new string[] { "İlksel" }, StringSplitOptions.None)[0];
                depremYeri = depremYeri.Split(new string[] { "REVIZE01" }, StringSplitOptions.None)[0].Trim();

                string tarih_saat = depremArgs[0] + " " + depremArgs[1];
                DateTime oDate = DateTime.ParseExact(tarih_saat, "yyyy.MM.dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                var nitelik = depremRow.Split(new string[] { depremYeri }, StringSplitOptions.None)[1].Trim().Split(' ')[0];



                JObject objDeprem = new JObject();

                objDeprem.Add("Id", i + 1);
                objDeprem.Add("Tarih", depremArgs[0]);
                objDeprem.Add("Saat", depremArgs[1]);
                objDeprem.Add("Unix_Time", ConvertToTimestamp(oDate));
                objDeprem.Add("Enlem", depremArgs[2]);
                objDeprem.Add("Boylam", depremArgs[3]);
                objDeprem.Add("Derinlik", depremArgs[4]);

                JObject objBuyukluk = new JObject();
                objBuyukluk.Add("MD", depremArgs[5].Replace("-.-", "0"));
                objBuyukluk.Add("ML", depremArgs[6].Replace("-.-", "0"));
                objBuyukluk.Add("Mw", depremArgs[7].Replace("-.-", "0"));

                objDeprem.Add("Buyukluk", objBuyukluk);
                objDeprem.Add("Yer", depremYeri);
                objDeprem.Add("Nitelik", nitelik);


                arrDepremApi.Add(objDeprem);


            }

            objDepremApi.Add("depremler", arrDepremApi);


            return objDepremApi;




        }



    }


}
